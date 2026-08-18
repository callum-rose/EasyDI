#!/usr/bin/env bash
#
# Tags the commit that each package version was built from.
#
# A version on nuget.org is immutable, so it is already something a consumer can pin to.
# What they can't do without these tags is get from a pinned version back to the source:
# nothing in the repo records which commit produced 1.1.0. SourceLink puts the commit SHA
# in the package, and these tags are the same fact from the other direction — readable
# from a git clone.
#
# For the Unity package the tag isn't a second copy of that fact, it's the only copy. It
# isn't on nuget.org: Unity clones this repo and reads the version as a literal string out
# of package.json, so no build step ever stamps one in and no registry holds an immutable
# copy. A UPM git URL pins by revision, which makes this tag the whole of what a consumer
# has to point at.
#
# Tag names are <project>-v<version>, e.g. EasyDI-v1.1.0, because the packages version
# independently — there is no single repo-wide version to tag.
#
#   ./scripts/tag-release.sh            # create tags locally, show what would be pushed
#   ./scripts/tag-release.sh --push     # create and push them
#   ./scripts/tag-release.sh --ref SHA  # tag a commit other than HEAD
#
# publish.yml runs this after a successful nuget.org push. A Unity-only release has nothing
# to publish, so run it yourself with --push once package.json is bumped and CI is green on
# the commit — don't wait for a nuget release to carry the tag out.
#
# Existing tags are never moved: a tag that already exists is reported and skipped, so
# re-running this after a partial release is safe. Don't work around that by deleting and
# re-tagging — a Unity consumer's packages-lock.json records the commit a tag resolved to,
# so moving one leaves the tag quietly describing something they haven't got.

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

PROJECTS=(
    EasyDI
    EasyDI.Analyzers
    EasyDI.Configuration
    EasyDI.LifecycleHooks
    EasyDI.Godot.Core
)

# The Unity package, kept out of PROJECTS because it has no csproj to read a version from.
UNITY_PROJECT=EasyDI.Unity
UNITY_PACKAGE_JSON=EasyDI.Unity/Assets/EasyDI.Unity/package.json

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

for project in "${PROJECTS[@]}" "$UNITY_PROJECT"; do
    if [[ "$project" == "$UNITY_PROJECT" ]]; then
        # sed rather than jq, which nothing else in this repo depends on. "version" is a
        # top-level string and no other key in a UPM manifest is named that, so the first
        # match is the right one — and a miss leaves this empty and fails below rather than
        # tagging a version nobody ships.
        version="$(sed -n 's/^[[:space:]]*"version"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' \
            "$UNITY_PACKAGE_JSON" | head -1)"
    else
        # PackageVersion rather than Version: some projects set one, some the other, and the SDK
        # derives PackageVersion from Version, so this reads correctly either way.
        version="$(dotnet msbuild "$project/$project.csproj" \
            -getProperty:PackageVersion \
            -nologo)"
    fi
    version="${version//[$'\t\r\n ']/}"

    if [[ -z "$version" ]]; then
        echo "error: could not read a version for $project" >&2
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
