#!/usr/bin/env bash

set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repo_root"

fail()
{
    printf 'REPOSITORY_GUARD_FAILED: %s\n' "$1" >&2
    exit 1
}

sha256_file()
{
    if command -v sha256sum >/dev/null 2>&1; then
        sha256sum "$1" | awk '{print $1}'
    else
        shasum -a 256 "$1" | awk '{print $1}'
    fi
}

required_files=(
    PROJECT_BIBLE.md
    CONTRIBUTING.md
    CHANGELOG.md
    LICENSE.md
    SECURITY.md
    Docs/DEVELOPER-HANDOFF.md
    Docs/REPOSITORY-GOVERNANCE.md
    Docs/ProjectBible/00_OKU_BENI.md
    Docs/ProjectBible/01_GAME_DESIGN_BIBLE.md
    Docs/ProjectBible/05_GELISTIRME_YOL_HARITASI.md
    Docs/ProjectBible/06_PROJE_HAFIZASI.md
    LegacyReference/PC-Shop-Empire-1.1.6/CANONICAL-MANIFEST.tsv
)

for required_file in "${required_files[@]}"; do
    test -f "$required_file" || fail "Required file is missing: $required_file"
done

project_version=$(awk -F ': ' '/^m_EditorVersion:/{print $2}' ProjectSettings/ProjectVersion.txt)
test "$project_version" = '6000.3.21f1' || fail "Unexpected Unity version: $project_version"

tracked_forbidden=$(git ls-files | grep -E '(^|/)(Library|Temp|Logs|UserSettings|Obj|Builds?|node_modules)(/|$)|(^|/)(\.env($|\.)|\.DS_Store|\._)|\.(p12|pfx|pem|key|mobileprovision)$' || true)
test -z "$tracked_forbidden" || fail "Generated, secret-prone or build paths are tracked:\n$tracked_forbidden"

secret_matches=$(git grep -I -l -E '(-----BEGIN (RSA |EC |OPENSSH )?PRIVATE KEY-----|AKIA[0-9A-Z]{16}|ghp_[A-Za-z0-9]{20,}|github_pat_[A-Za-z0-9_]{20,})' -- . ':(exclude)Tools/verify-repository.sh' || true)
test -z "$secret_matches" || fail "Possible credential material found in tracked files:\n$secret_matches"

core_asmdef='Assets/Scripts/Core/Runtime/PSE.Core.asmdef'
grep -q '"name": "PSE.Core"' "$core_asmdef" || fail 'PSE.Core assembly name changed unexpectedly.'
grep -q '"noEngineReferences": true' "$core_asmdef" || fail 'PSE.Core must not reference Unity engine assemblies.'

legacy_root='LegacyReference/PC-Shop-Empire-1.1.6/Source'
legacy_manifest='LegacyReference/PC-Shop-Empire-1.1.6/CANONICAL-MANIFEST.tsv'
legacy_count=0

while IFS=$'\t' read -r expected_hash expected_size relative_path; do
    test -n "$relative_path" || fail 'Legacy manifest contains an empty path.'
    source_file="$legacy_root/$relative_path"
    test -f "$source_file" || fail "Legacy file is missing: $relative_path"

    actual_hash=$(sha256_file "$source_file")
    actual_size=$(wc -c < "$source_file" | tr -d ' ')

    test "$actual_hash" = "$expected_hash" || fail "Legacy hash mismatch: $relative_path"
    test "$actual_size" = "$expected_size" || fail "Legacy size mismatch: $relative_path"
    legacy_count=$((legacy_count + 1))
done < "$legacy_manifest"

test "$legacy_count" -eq 26 || fail "Legacy manifest expected 26 files, found $legacy_count."

actual_legacy_count=$(find "$legacy_root" -type f ! -name '.DS_Store' ! -name '._*' | wc -l | tr -d ' ')
test "$actual_legacy_count" -eq 26 || fail "Legacy source expected 26 files, found $actual_legacy_count."

project_bible_count=$(find Docs/ProjectBible -maxdepth 1 -type f -name '*.md' | wc -l | tr -d ' ')
test "$project_bible_count" -ge 11 || fail "Project Bible package is incomplete: $project_bible_count documents."

printf 'REPOSITORY_GUARD_OK unity=%s legacy=%s project_bible_docs=%s tracked=%s\n' \
    "$project_version" \
    "$legacy_count" \
    "$project_bible_count" \
    "$(git ls-files | wc -l | tr -d ' ')"
