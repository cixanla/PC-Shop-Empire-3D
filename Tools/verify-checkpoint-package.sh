#!/bin/bash

set -euo pipefail

usage() {
  echo "Usage: $0 <repository> <checkpoint-package> <canonical-evidence-directory> [canonical|issue66]" >&2
  exit 64
}

[[ $# -eq 3 || $# -eq 4 ]] || usage

repository=$1
package=$2
evidence_source=$3
evidence_contract=${4:-canonical}

case "$evidence_contract" in
  canonical|issue66) ;;
  *) echo "ERROR unsupported evidence contract: $evidence_contract" >&2; usage ;;
esac

[[ -d "$repository/.git" ]] || { echo "ERROR repository is not a Git checkout: $repository" >&2; exit 1; }
[[ -d "$package" ]] || { echo "ERROR checkpoint package is missing: $package" >&2; exit 1; }
[[ -d "$evidence_source" ]] || { echo "ERROR canonical evidence directory is missing: $evidence_source" >&2; exit 1; }

for required in SOURCE EVIDENCE SOURCE_COMMIT.txt MANIFEST.tsv MANIFEST.sha256; do
  [[ -e "$package/$required" ]] || { echo "ERROR missing package entry: $required" >&2; exit 1; }
done
[[ -d "$package/SOURCE" && -d "$package/EVIDENCE" ]] || { echo "ERROR SOURCE and EVIDENCE must be directories" >&2; exit 1; }
[[ -f "$package/SOURCE_COMMIT.txt" && -f "$package/MANIFEST.tsv" && -f "$package/MANIFEST.sha256" ]] || {
  echo "ERROR package metadata entries must be regular files" >&2
  exit 1
}

root_entries=("$package"/* "$package"/.[!.]* "$package"/..?*)
for root_entry in "${root_entries[@]}"; do
  [[ -e "$root_entry" || -L "$root_entry" ]] || continue
  root_name=${root_entry##*/}
  case "$root_name" in
    SOURCE|EVIDENCE|SOURCE_COMMIT.txt|MANIFEST.tsv|MANIFEST.sha256) ;;
    *) echo "ERROR unexpected package-root entry: $root_name" >&2; exit 1 ;;
  esac
done

symlink_count=$(find "$package" -type l -print | wc -l | tr -d ' ')
[[ "$symlink_count" == "0" ]] || { echo "ERROR symlinks are forbidden in checkpoint packages: $symlink_count" >&2; exit 1; }

appledouble_count=$(find "$package" -name '._*' -print | wc -l | tr -d ' ')
[[ "$appledouble_count" == "0" ]] || { echo "ERROR internal AppleDouble files found: $appledouble_count" >&2; exit 1; }

package_parent=$(dirname "$package")
package_name=$(basename "$package")
[[ ! -e "$package_parent/._$package_name" ]] || { echo "ERROR sibling AppleDouble sidecar found: ._$package_name" >&2; exit 1; }

for forbidden_name in .git Library Temp Logs UserSettings Obj Build Builds node_modules .DS_Store .env; do
  forbidden_count=$(find "$package" -name "$forbidden_name" -print | wc -l | tr -d ' ')
  [[ "$forbidden_count" == "0" ]] || { echo "ERROR forbidden package path '$forbidden_name': $forbidden_count" >&2; exit 1; }
done

for forbidden_glob in '*.p12' '*.pfx' '*.pem' '*.key' '*.mobileprovision' '.env.*'; do
  forbidden_count=$(find "$package" -name "$forbidden_glob" -print | wc -l | tr -d ' ')
  [[ "$forbidden_count" == "0" ]] || { echo "ERROR forbidden package pattern '$forbidden_glob': $forbidden_count" >&2; exit 1; }
done

set +e
rg --text -l --no-messages \
  '(-----BEGIN [A-Z ]*PRIVATE KEY-----|AKIA[0-9A-Z]{16}|ghp_[A-Za-z0-9]{20,}|github_pat_[A-Za-z0-9_]{20,})' \
  "$package" >/dev/null
secret_scan_status=$?
set -e
case "$secret_scan_status" in
  0)
    echo "ERROR credential or private-key pattern found in package" >&2
    exit 1
    ;;
  1) ;;
  *)
    echo "ERROR secret scan failed: rg status=$secret_scan_status" >&2
    exit 1
    ;;
esac

source_commit=$(sed -nE 's/^Source\/docs commit: ([0-9a-f]{40})$/\1/p' "$package/SOURCE_COMMIT.txt")
source_tree=$(sed -nE 's/^Source\/docs tree: ([0-9a-f]{40})$/\1/p' "$package/SOURCE_COMMIT.txt")
[[ $(printf '%s\n' "$source_commit" | sed '/^$/d' | wc -l | tr -d ' ') == "1" ]] || {
  echo "ERROR SOURCE_COMMIT.txt must contain exactly one full Source/docs commit" >&2
  exit 1
}
[[ $(printf '%s\n' "$source_tree" | sed '/^$/d' | wc -l | tr -d ' ') == "1" ]] || {
  echo "ERROR SOURCE_COMMIT.txt must contain exactly one full Source/docs tree" >&2
  exit 1
}

git -C "$repository" cat-file -e "$source_commit^{commit}"
actual_tree=$(git -C "$repository" rev-parse "$source_commit^{tree}")
[[ "$actual_tree" == "$source_tree" ]] || { echo "ERROR source tree mismatch: expected=$source_tree actual=$actual_tree" >&2; exit 1; }

expected_manifest_sha=$(tr -d '[:space:]' < "$package/MANIFEST.sha256")
[[ "$expected_manifest_sha" =~ ^[0-9a-f]{64}$ ]] || { echo "ERROR MANIFEST.sha256 must contain one lowercase SHA-256" >&2; exit 1; }
actual_manifest_sha=$(shasum -a 256 "$package/MANIFEST.tsv" | awk '{print $1}')
[[ "$actual_manifest_sha" == "$expected_manifest_sha" ]] || { echo "ERROR MANIFEST.tsv SHA-256 mismatch" >&2; exit 1; }

temp_dir=$(mktemp -d "${TMPDIR:-/tmp}/pcshop-checkpoint-verify.XXXXXX")
cleanup() {
  rm -rf "$temp_dir"
}
trap cleanup EXIT

manifest_paths="$temp_dir/manifest-paths.txt"
payload_paths="$temp_dir/payload-paths.txt"
git_paths="$temp_dir/git-paths.txt"
source_paths="$temp_dir/source-paths.txt"
evidence_paths="$temp_dir/evidence-paths.txt"
canonical_evidence_paths="$temp_dir/canonical-evidence-paths.txt"
contract_evidence_paths="$temp_dir/contract-evidence-paths.txt"

LC_ALL=C awk -F '\t' '
  NF != 3 { exit 10 }
  $1 !~ /^[0-9a-f]{64}$/ { exit 11 }
  $2 !~ /^[0-9]+$/ { exit 12 }
  $3 == "" || $3 ~ /^\// || $3 ~ /(^|\/)\.\.($|\/)/ || $3 ~ /\\/ { exit 13 }
  previous != "" && $3 <= previous { exit 14 }
  { previous=$3; print $3 }
' "$package/MANIFEST.tsv" > "$manifest_paths" || {
  status=$?
  echo "ERROR malformed, duplicate or non-deterministic MANIFEST.tsv (status=$status)" >&2
  exit 1
}

(
  cd "$package"
  find SOURCE EVIDENCE -type f -print
  printf '%s\n' SOURCE_COMMIT.txt
) | LC_ALL=C sort > "$payload_paths"
cmp -s "$manifest_paths" "$payload_paths" || { echo "ERROR manifest path set does not equal package payload" >&2; exit 1; }

file_size() {
  if stat -f '%z' "$1" >/dev/null 2>&1; then
    stat -f '%z' "$1"
  else
    stat -c '%s' "$1"
  fi
}

manifest_rows=0
manifest_bytes=0
while IFS=$'\t' read -r expected_sha expected_size relative_path; do
  payload_file="$package/$relative_path"
  [[ -f "$payload_file" && ! -L "$payload_file" ]] || { echo "ERROR manifest target is not a regular file: $relative_path" >&2; exit 1; }
  actual_size=$(file_size "$payload_file")
  actual_sha=$(shasum -a 256 "$payload_file" | awk '{print $1}')
  [[ "$actual_size" == "$expected_size" ]] || { echo "ERROR size mismatch: $relative_path" >&2; exit 1; }
  [[ "$actual_sha" == "$expected_sha" ]] || { echo "ERROR SHA-256 mismatch: $relative_path" >&2; exit 1; }
  manifest_rows=$((manifest_rows + 1))
  manifest_bytes=$((manifest_bytes + expected_size))
done < "$package/MANIFEST.tsv"

if git -C "$repository" ls-tree -r "$source_commit" | awk '$2 != "blob" { bad=1 } END { exit bad }'; then
  :
else
  echo "ERROR source commit contains a non-blob recursive tree entry" >&2
  exit 1
fi

git -C "$repository" ls-tree -r --name-only "$source_commit" | LC_ALL=C sort > "$git_paths"
(
  cd "$package/SOURCE"
  find . -type f -print | sed 's#^\./##'
) | LC_ALL=C sort > "$source_paths"
cmp -s "$git_paths" "$source_paths" || { echo "ERROR SOURCE path set does not equal recorded Git commit" >&2; exit 1; }

git_rows=0
while IFS= read -r relative_path; do
  expected_size=$(git -C "$repository" cat-file -s "$source_commit:$relative_path")
  expected_sha=$(git -C "$repository" cat-file blob "$source_commit:$relative_path" | shasum -a 256 | awk '{print $1}')
  source_file="$package/SOURCE/$relative_path"
  actual_size=$(file_size "$source_file")
  actual_sha=$(shasum -a 256 "$source_file" | awk '{print $1}')
  [[ "$actual_size" == "$expected_size" ]] || { echo "ERROR Git-source size mismatch: $relative_path" >&2; exit 1; }
  [[ "$actual_sha" == "$expected_sha" ]] || { echo "ERROR Git-source SHA-256 mismatch: $relative_path" >&2; exit 1; }
  git_rows=$((git_rows + 1))
done < "$git_paths"

(
  cd "$package/EVIDENCE"
  find . -type f -print | sed 's#^\./##'
) | LC_ALL=C sort > "$evidence_paths"
(
  cd "$evidence_source"
  evidence_entries=(* .[!.]* ..?*)
  for entry in "${evidence_entries[@]}"; do
    [[ -e "$entry" || -L "$entry" ]] || continue
    [[ -f "$entry" && ! -L "$entry" ]] || {
      echo "ERROR canonical evidence entry must be a top-level regular file: $entry" >&2
      exit 1
    }
    printf '%s\n' "$entry"
  done
) | LC_ALL=C sort > "$canonical_evidence_paths"
cmp -s "$evidence_paths" "$canonical_evidence_paths" || { echo "ERROR EVIDENCE allowlist does not equal canonical evidence directory" >&2; exit 1; }

if [[ "$evidence_contract" == "issue66" ]]; then
  printf '%s\n' \
    binary-manifest.json \
    build-il2cpp-d3d11-rerun.log \
    editmode.xml \
    macos-build.log \
    macos-runtime.log \
    playmode.xml \
    runtime-d3d11.log \
    runtime-summary.json \
    source-receipt.json \
    | LC_ALL=C sort > "$contract_evidence_paths"
  cmp -s "$canonical_evidence_paths" "$contract_evidence_paths" || {
    echo "ERROR Issue #66 canonical evidence does not equal the exact 9-file contract" >&2
    exit 1
  }
fi

evidence_rows=0
while IFS= read -r relative_path; do
  canonical_file="$evidence_source/$relative_path"
  package_file="$package/EVIDENCE/$relative_path"
  expected_size=$(file_size "$canonical_file")
  expected_sha=$(shasum -a 256 "$canonical_file" | awk '{print $1}')
  actual_size=$(file_size "$package_file")
  actual_sha=$(shasum -a 256 "$package_file" | awk '{print $1}')
  [[ "$actual_size" == "$expected_size" ]] || { echo "ERROR evidence size mismatch: $relative_path" >&2; exit 1; }
  [[ "$actual_sha" == "$expected_sha" ]] || { echo "ERROR evidence SHA-256 mismatch: $relative_path" >&2; exit 1; }
  evidence_rows=$((evidence_rows + 1))
done < "$evidence_paths"

[[ "$evidence_rows" -gt 0 ]] || {
  echo "ERROR canonical evidence contract must contain at least one file" >&2
  exit 1
}
if [[ "$evidence_contract" == "issue66" && "$evidence_rows" != "9" ]]; then
  echo "ERROR Issue #66 requires exactly 9 evidence files: actual=$evidence_rows" >&2
  exit 1
fi

echo "CHECKPOINT_PACKAGE_OK manifest=$manifest_rows bytes=$manifest_bytes git_source=$git_rows evidence=$evidence_rows commit=$source_commit tree=$source_tree"
