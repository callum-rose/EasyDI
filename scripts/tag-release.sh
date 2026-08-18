#!/usr/bin/env bash
#
# Tags the commit that each package version was built from.
#
# A version on nuget.org is immutable, so it is already something a consumer can pin to.
# What they can't do without these tags is get from a pinned version back to the source:
# nothing in the repo records which commit produced 1.1.0. SourceLink puts the commit SHA
# in the package, and these tags are the same fact from the other direction — readable
# from a git clone, and what a Unity consumer pins their UPM git URL to.
#
# Tag names are <project>-v<version>, e.g. EasyDI-v1.1.0, because the packages version
# independently — there is no single repo-wide version to tag.
#
#   ./scripts/tag-release.sh            # create tags locally, show what would be pushed
#   ./scripts/tag-release.sh --push     # create and push them
#   ./scripts/tag-release.sh --ref SHA  # tag a commit other than HEAD
#
# Existing tags are never moved: a tag that already exists is reported and skipped, so
# re-running this after a partial release is safe.

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

PROJECTS=(
    EasyDI
    EasyDI.Analyzers
    EasyDI.Configuration
    EasyDI.LifecycleHooks
    EasyDI.Godot.Core
)

PUSH=false
REF=HEAD

while [[ $# -gt 0 ]]; do
    case "$1" in
        --push) PUSH=true; shift ;;
        --ref) REF="${2:?--ref needs a commit}"; shift 2 ;;
        *) echo "error: unknown argument: $1" >&2; exit 1 ;;
    esac
done

cd "$REPO_ROOT"

COMMIT="$(git rev-parse --verify "$REF^{commit}")"

created=()
skipped=()

for project in "${PROJECTS[@]}"; do
    # PackageVersion rather than Version: some projects set one, some the other, and the SDK
    # derives PackageVersion from Version, so this reads correctly either way.
    version="$(dotnet msbuild "$project/$project.csproj" \
        -getProperty:PackageVersion \
        -nologo)"
    version="${version//[$'\t\r\n ']/}"

    if [[ -z "$version" ]]; then
        echo "error: could not read PackageVersion from $project" >&2
        exit 1
    fi

    tag="$project-v$version"

    if git rev-parse -q --verify "refs/tags/$tag" >/dev/null; then
        skipped+=("$tag")
        continue
    fi

    git tag -a "$tag" "$COMMIT" -m "$project $version"
    created+=("$tag")
done

for tag in "${skipped[@]:-}"; do
    [[ -n "$tag" ]] && echo "already tagged, left alone: $tag"
done

if [[ ${#created[@]} -eq 0 ]]; then
    echo "==> Nothing new to tag"
    exit 0
fi

echo "==> Tagged ${COMMIT:0:9}"
for tag in "${created[@]}"; do
    echo "    $tag"
done

if [[ "$PUSH" == true ]]; then
    echo "==> Pushing"
    git push origin "${created[@]}"
else
    echo
    echo "Not pushed. To publish these tags:"
    echo "    git push origin ${created[*]}"
fi
