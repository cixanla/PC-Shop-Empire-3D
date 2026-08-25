#!/bin/bash

set -euo pipefail

usage() {
  echo "Usage: $0 <repository> <checkpoint-package> <canonical-evidence-directory> [canonical|issue66|issue68|issue71]" >&2
  exit 64
}

[[ $# -eq 3 || $# -eq 4 ]] || usage

repository=$1
package=$2
evidence_source=$3
evidence_contract=${4:-canonical}

case "$evidence_contract" in
  canonical|issue66|issue68|issue71) ;;
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
if [[ "$package_name" != .incoming-* ]]; then
  incoming_residue_count=$(find "$package_parent" -maxdepth 1 -mindepth 1 -name '.incoming-*' -print | wc -l | tr -d ' ')
  [[ "$incoming_residue_count" == "0" ]] || {
    echo "ERROR incoming residue remains beside final package: $incoming_residue_count" >&2
    exit 1
  }
  sibling_appledouble_count=$(find "$package_parent" -maxdepth 1 -mindepth 1 -name '._*' -print | wc -l | tr -d ' ')
  [[ "$sibling_appledouble_count" == "0" ]] || {
    echo "ERROR sibling AppleDouble residue remains beside final package: $sibling_appledouble_count" >&2
    exit 1
  }
fi

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
cmp -s \
  <(printf 'Source/docs commit: %s\nSource/docs tree: %s\n' "$source_commit" "$source_tree") \
  "$package/SOURCE_COMMIT.txt" || {
  echo "ERROR SOURCE_COMMIT.txt must contain only the exact commit/tree receipt" >&2
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

if [[ "$evidence_contract" == "issue66" ||
      "$evidence_contract" == "issue68" ||
      "$evidence_contract" == "issue71" ]]; then
  contract_issue=${evidence_contract#issue}
  contract_build_log=build-il2cpp-d3d11.log
  contract_evidence_count=14
  if [[ "$evidence_contract" == "issue66" ]]; then
    contract_build_log=build-il2cpp-d3d11-rerun.log
    contract_evidence_count=9
  fi
  {
    printf '%s\n' \
      binary-manifest.json \
      "$contract_build_log" \
      editmode.xml \
      macos-build.log \
      macos-runtime.log \
      playmode.xml \
      runtime-d3d11.log \
      runtime-summary.json \
      source-receipt.json
    if [[ "$evidence_contract" == "issue68" ||
          "$evidence_contract" == "issue71" ]]; then
      printf '%s\n' \
        build-procedure.ps1 \
        launch-procedure.ps1 \
        procedure-manifest.json \
        runtime-procedure.ps1 \
        task-receipt.json
    fi
  } | LC_ALL=C sort > "$contract_evidence_paths"
  cmp -s "$canonical_evidence_paths" "$contract_evidence_paths" || {
    echo "ERROR Issue #$contract_issue canonical evidence does not equal the exact $contract_evidence_count-file contract" >&2
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
if [[ "$evidence_contract" == "issue66" ||
      "$evidence_contract" == "issue68" ||
      "$evidence_contract" == "issue71" ]]; then
  contract_issue=${evidence_contract#issue}
  contract_evidence_count=14
  if [[ "$evidence_contract" == "issue66" ]]; then
    contract_evidence_count=9
  fi
  if [[ "$evidence_rows" != "$contract_evidence_count" ]]; then
    echo "ERROR Issue #$contract_issue requires exactly $contract_evidence_count evidence files: actual=$evidence_rows" >&2
    exit 1
  fi
fi

if [[ "$evidence_contract" == "issue71" ]]; then
  evidence_root="$package/EVIDENCE"
  technical_commit=11683c8b567ad6edcd6777610875aeebd0e509ef
  technical_tree=6890157f3f3625661314b34700259e0933ff2677
  binary_manifest="$evidence_root/binary-manifest.json"
  procedure_manifest="$evidence_root/procedure-manifest.json"
  runtime_summary="$evidence_root/runtime-summary.json"
  task_receipt="$evidence_root/task-receipt.json"
  source_receipt="$evidence_root/source-receipt.json"

  for json_receipt in \
    "$binary_manifest" \
    "$procedure_manifest" \
    "$runtime_summary" \
    "$task_receipt" \
    "$source_receipt"; do
    jq -e . "$json_receipt" >/dev/null || {
      echo "ERROR Issue #71 evidence receipt is not valid JSON: ${json_receipt##*/}" >&2
      exit 1
    }
  done

  git -C "$repository" cat-file -e "$technical_commit^{commit}"
  [[ "$(git -C "$repository" rev-parse "$technical_commit^{tree}")" == "$technical_tree" ]] || {
    echo "ERROR Issue #71 technical commit/tree contract does not resolve" >&2
    exit 1
  }
  git -C "$repository" merge-base --is-ancestor "$technical_commit" "$source_commit" || {
    echo "ERROR packaged source/docs commit does not descend from the Issue #71 technical source" >&2
    exit 1
  }

  jq -e \
    --arg commit "$technical_commit" \
    --arg tree "$technical_tree" '
      .sourceCommit == $commit and
      .sourceTree == $tree and
      .target == "StandaloneWindows64" and
      .architecture == "x64" and
      .scriptingBackend == "IL2CPP" and
      .graphicsApi == "Direct3D11" and
      .developmentBuild == true and
      .strictMode == true and
      .buildReportBytes > 0 and
      .buildMarkerCount == 1 and
      .buildForbiddenTokenCount == 0 and
      .buildForbiddenPolicy == "issue71-hardened-v2" and
      (.binaries | length) == 3 and
      ([.binaries[].path] | sort) ==
        (["GameAssembly.dll", "PC Shop Empire 3D.exe", "UnityPlayer.dll"] | sort) and
      all(.binaries[]; .bytes > 0 and (.sha256 | test("^[0-9a-f]{64}$")))
    ' "$binary_manifest" >/dev/null || {
    echo "ERROR Issue #71 binary manifest does not satisfy the hardened Windows build contract" >&2
    exit 1
  }

  jq -e \
    --arg commit "$technical_commit" \
    --arg tree "$technical_tree" '
      .schemaVersion == 1 and
      .sourceCommit == $commit and
      .sourceTree == $tree and
      (.procedures | length) == 3 and
      ([.procedures[].path] | sort) ==
        (["build-procedure.ps1", "launch-procedure.ps1", "runtime-procedure.ps1"] | sort) and
      all(.procedures[]; .bytes > 0 and (.sha256 | test("^[0-9a-f]{64}$")))
    ' "$procedure_manifest" >/dev/null || {
    echo "ERROR Issue #71 procedure manifest does not satisfy the exact three-procedure contract" >&2
    exit 1
  }

  jq -e \
    --arg commit "$technical_commit" \
    --arg tree "$technical_tree" '
      .sourceCommit == $commit and
      .sourceTree == $tree and
      .platform == "WindowsPlayer" and
      .requiredGraphicsApi == "Direct3D11" and
      .forceD3D11 == true and
      .playerExitCode == 0 and
      .gracefulShutdown == true and
      .timedOut == false and
      .hostMarkerCount == 1 and
      .forceD3D11LineCount == 1 and
      .d3d11VersionLineCount == 1 and
      .rendererLineCount == 1 and
      (.renderer | startswith("Renderer: Intel(R) Iris(R) Xe Graphics")) and
      .readinessMarkerCount == 1 and
      .readinessTokensOk == true and
      .readinessMissingTokenCount == 0 and
      .successMarkerCount == 1 and
      .forbiddenTokenCount == 0 and
      .shutdownMarkerCount == 1 and
      .accepted == true
    ' "$runtime_summary" >/dev/null || {
    echo "ERROR Issue #71 runtime summary does not satisfy the accepted D3D11 contract" >&2
    exit 1
  }

  jq -e \
    --arg commit "$technical_commit" \
    --arg tree "$technical_tree" '
      .sourceCommit == $commit and
      .sourceTree == $tree and
      .logonType == "Interactive" and
      .lastTaskResult == 0 and
      .taskFinishedWithinDeadline == true and
      .runtimeAccepted == true and
      .gracefulShutdown == true and
      .taskDeleted == true and
      .cleanupWasRequired == false and
      .playerResidueAfterCleanup == 0 and
      .cleanAfterRuntime == true
    ' "$task_receipt" >/dev/null || {
    echo "ERROR Issue #71 task receipt does not satisfy the interactive cleanup contract" >&2
    exit 1
  }

  jq -e \
    --arg commit "$technical_commit" \
    --arg tree "$technical_tree" '
      .schemaVersion == 1 and
      .issue == 71 and
      .sourceCommit == $commit and
      .sourceTree == $tree and
      .repositoryCleanAfterAllLocalGates == true and
      .gitDiffCheck == true and
      .technicalGuard.conclusion == "success" and
      .tests.editMode.total == 677 and
      .tests.editMode.passed == 677 and
      .tests.editMode.failed == 0 and
      .tests.editMode.skipped == 0 and
      .tests.editMode.inconclusive == 0 and
      .tests.playMode.total == 81 and
      .tests.playMode.passed == 81 and
      .tests.playMode.failed == 0 and
      .tests.playMode.skipped == 0 and
      .tests.playMode.inconclusive == 0 and
      .macOS.readinessMarkerCount == 1 and
      .macOS.successMarkerCount == 1 and
      .macOS.failureMarkerCount == 0 and
      .windows.checkout == "detached-clean" and
      .windows.target == "StandaloneWindows64" and
      .windows.architecture == "x64" and
      .windows.scriptingBackend == "IL2CPP" and
      .windows.graphicsApi == "Direct3D11" and
      .windows.buildForbiddenPolicy == "issue71-hardened-v2" and
      .windows.projectSettingsByteExact == true and
      .windows.playerExitCode == 0 and
      .windows.gracefulShutdown == true and
      .windows.readinessMarkerCount == 1 and
      .windows.successMarkerCount == 1 and
      .windows.forbiddenTokenCount == 0 and
      .windows.scheduledTaskDeleted == true and
      .windows.cleanupWasRequired == false and
      .windows.playerResidueAfterCleanup == 0 and
      .windows.cleanAfterRuntime == true and
      (.promotedArtifacts | length) == 13
    ' "$source_receipt" >/dev/null || {
    echo "ERROR Issue #71 source receipt does not satisfy the promoted technical evidence contract" >&2
    exit 1
  }

  actual_promoted="$temp_dir/issue71-actual-promoted.tsv"
  receipt_promoted="$temp_dir/issue71-receipt-promoted.tsv"
  (
    cd "$evidence_root"
    for artifact in *; do
      [[ "$artifact" == "source-receipt.json" ]] && continue
      printf '%s\t%s\t%s\n' \
        "$(shasum -a 256 "$artifact" | awk '{print $1}')" \
        "$(file_size "$artifact")" \
        "$artifact"
    done
  ) | LC_ALL=C sort -t $'\t' -k3,3 > "$actual_promoted"
  jq -r '
    .promotedArtifacts |
    sort_by(.path)[] |
    [.sha256, (.bytes | tostring), .path] |
    @tsv
  ' "$source_receipt" > "$receipt_promoted"
  cmp -s "$actual_promoted" "$receipt_promoted" || {
    echo "ERROR Issue #71 promoted artifact receipt does not equal the other 13 evidence files" >&2
    exit 1
  }

  actual_procedures="$temp_dir/issue71-actual-procedures.tsv"
  receipt_procedures="$temp_dir/issue71-receipt-procedures.tsv"
  (
    cd "$evidence_root"
    for procedure in build-procedure.ps1 launch-procedure.ps1 runtime-procedure.ps1; do
      printf '%s\t%s\t%s\n' \
        "$(shasum -a 256 "$procedure" | awk '{print $1}')" \
        "$(file_size "$procedure")" \
        "$procedure"
    done
  ) | LC_ALL=C sort -t $'\t' -k3,3 > "$actual_procedures"
  jq -r '
    .procedures |
    sort_by(.path)[] |
    [.sha256, (.bytes | tostring), .path] |
    @tsv
  ' "$procedure_manifest" > "$receipt_procedures"
  cmp -s "$actual_procedures" "$receipt_procedures" || {
    echo "ERROR Issue #71 procedure manifest does not equal the three evidence procedures" >&2
    exit 1
  }

  binary_manifest_sha=$(shasum -a 256 "$binary_manifest" | awk '{print $1}')
  binary_manifest_bytes=$(file_size "$binary_manifest")
  procedure_manifest_sha=$(shasum -a 256 "$procedure_manifest" | awk '{print $1}')
  procedure_manifest_bytes=$(file_size "$procedure_manifest")
  jq -e \
    --arg binary_sha "$binary_manifest_sha" \
    --argjson binary_bytes "$binary_manifest_bytes" \
    --arg procedure_sha "$procedure_manifest_sha" \
    --argjson procedure_bytes "$procedure_manifest_bytes" \
    --slurpfile binaries "$binary_manifest" \
    --slurpfile procedures "$procedure_manifest" '
      .binaryManifestSha256 == $binary_sha and
      .binaryManifestBytes == $binary_bytes and
      .procedureManifestSha256 == $procedure_sha and
      .procedureManifestBytes == $procedure_bytes and
      .binaryReadback == $binaries[0].binaries and
      .procedureReadback == $procedures[0].procedures
    ' "$runtime_summary" >/dev/null || {
    echo "ERROR Issue #71 runtime summary manifest/readback bindings do not match evidence" >&2
    exit 1
  }
  jq -e \
    --arg binary_sha "$binary_manifest_sha" \
    --arg procedure_sha "$procedure_manifest_sha" \
    --slurpfile procedures "$procedure_manifest" '
      .binaryManifestSha256 == $binary_sha and
      .procedureManifestSha256 == $procedure_sha and
      .procedureReadback == $procedures[0].procedures
    ' "$task_receipt" >/dev/null || {
    echo "ERROR Issue #71 task receipt manifest/readback bindings do not match evidence" >&2
    exit 1
  }

  build_success_count=$(tr '\r' '\n' < "$evidence_root/build-il2cpp-d3d11.log" | grep -Ec '^STAGE_A_BUILD_OK target=StandaloneWindows64 .* scripting-backend=IL2CPP graphics-api=Direct3D11 settings-restored=ok project-settings=byte-exact$' || true)
  [[ "$build_success_count" == "1" ]] || {
    echo "ERROR Issue #71 build log must contain exactly one accepted IL2CPP/D3D11 marker" >&2
    exit 1
  }
  build_fatal_count=$(grep -Eic 'error CS[0-9]{4}|BuildFailedException|Scripts have compiler errors|Compilation failed|Burst internal compiler error|Burst\.Compiler\.IL\.Aot\.AotLinkerException|The native link step failed|Win32 IO returned 232|Error while executing command:.*burst-lld|Aborting batchmode due to failure|Build completed with a result of .Failed.|Windows IL2CPP/D3D11 build and settings restore both failed|errors=[1-9][0-9]*' "$evidence_root/build-il2cpp-d3d11.log" || true)
  [[ "$build_fatal_count" == "0" ]] || {
    echo "ERROR Issue #71 hardened build log contains fatal tokens: $build_fatal_count" >&2
    exit 1
  }

  processor_success_marker='GARAGE_PROCESSOR_BUILD_KIT_RUNTIME_SMOKE work-ticket=ok prerequisite=motherboard-staged processor-pickup=exact physical-identity=stable carry=ok input=keyboard+mouse custody-guards=ok rotation=ok placement=ok progress=2/10 reservation=alive custody=processor-build-kit receipts=ok revisions=ok assembly=untouched processor-socket=untouched no-duplicate-loss=ok replay=ok invariants=ok'
  runtime_success_count=$(tr '\r' '\n' < "$evidence_root/runtime-d3d11.log" | grep -Fxc "$processor_success_marker" || true)
  [[ "$runtime_success_count" == "1" ]] || {
    echo "ERROR Issue #71 runtime log must contain the exact CPU BuildKit success marker once" >&2
    exit 1
  }
  runtime_fatal_count=$(grep -Eic 'GARAGE_PROCESSOR_BUILD_KIT_RUNTIME_SMOKE build-kit-flow=failed|code=smoke\.|Assertion failed|AssertionException|NullReferenceException|MissingReferenceException|Unhandled Exception|ArgumentException|InvalidOperationException|StackOverflowException|AccessViolationException|Crash!!!|PlayerLoop called recursively|JobTempAlloc has allocations|A Native Collection has not been disposed' "$evidence_root/runtime-d3d11.log" || true)
  [[ "$runtime_fatal_count" == "0" ]] || {
    echo "ERROR Issue #71 runtime log contains forbidden tokens: $runtime_fatal_count" >&2
    exit 1
  }
fi

echo "CHECKPOINT_PACKAGE_OK manifest=$manifest_rows bytes=$manifest_bytes git_source=$git_rows evidence=$evidence_rows commit=$source_commit tree=$source_tree"
