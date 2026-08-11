#!/usr/bin/env bash
#
# Builds the .NET libraries that the Unity package ships and copies the resulting
# assemblies into the package's vendored Plugins folder.
#
# The Unity package is self-contained: it carries these DLLs directly rather than
# restoring them through NuGet. Run this after any change to EasyDI, EasyDI.LifecycleHooks
# or EasyDI.Analyzers, and commit the result.
#
# The .meta files next to the DLLs are hand-authored and committed. This script never
# touches them — their GUIDs must stay stable or every Unity project referencing the
# package breaks.

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DEST="$REPO_ROOT/EasyDI.Unity/Assets/EasyDI.Unity/Runtime/Plugins"
BUILD_ROOT="$REPO_ROOT/artifacts/vendor"

# These flags make the output byte-reproducible, which the CI drift check depends on:
#
#   PathMap                            rewrites the source paths embedded in the PDB, so the
#                                      bytes don't depend on where the repo is checked out.
#   EnableSourceControlManagerQueries  together, these stop the SDK asking git for the repo
#   EnableSourceLink                   root and commit SHA and baking both into the PDB. That
#                                      would change the bytes on every single commit, so the
#                                      drift check could never pass.
#
# SourceLink still applies to the nuget.org packages, which is where it earns its keep. These
# DLLs sit in the same repo as their source, so there's nothing for it to buy here.
#
# The build is also kept out of the projects' own bin/ and obj/. That cuts both ways: an
# ordinary `dotnet build` can't leave stale output that this script then skips rebuilding
# (MSBuild's incremental check doesn't notice the changed properties above), and this
# SourceLink-free build can't end up in a package pushed to nuget.org.
#
# Use --artifacts-path, not BaseOutputPath/BaseIntermediateOutputPath. Those are global
# properties, so they propagate into ProjectReferences and every project in the graph ends
# up sharing one obj/ — which breaks restore as soon as the referencing project and the
# referenced one target different frameworks (NETSDK1005).
build() {
    local project="$1"
    dotnet build "$REPO_ROOT/$project/$project.csproj" \
        --configuration Release \
        --nologo \
        --artifacts-path "$BUILD_ROOT" \
        "-p:PathMap=$REPO_ROOT/=/_/" \
        -p:EnableSourceControlManagerQueries=false \
        -p:EnableSourceLink=false \
        -p:GeneratePackageOnBuild=false
}

echo "==> Building"
rm -rf "$BUILD_ROOT"
build EasyDI
build EasyDI.LifecycleHooks
build EasyDI.Analyzers

# netstandard2.1 is the TFM Unity consumes. The analyzer is netstandard2.0 because
# Roslyn analyzers have to be.
ARTEFACTS=(
    "$BUILD_ROOT/bin/EasyDI/release_netstandard2.1/EasyDI.dll"
    "$BUILD_ROOT/bin/EasyDI/release_netstandard2.1/EasyDI.pdb"
    "$BUILD_ROOT/bin/EasyDI.LifecycleHooks/release_netstandard2.1/EasyDI.LifecycleHooks.dll"
    "$BUILD_ROOT/bin/EasyDI.LifecycleHooks/release_netstandard2.1/EasyDI.LifecycleHooks.pdb"
    "$BUILD_ROOT/bin/EasyDI.Analyzers/release/EasyDI.Analyzers.dll"
)

# Verify everything exists before copying anything, so a missing artefact can't leave
# the package half-updated.
for artefact in "${ARTEFACTS[@]}"; do
    if [[ ! -f "$artefact" ]]; then
        echo "error: expected build output not found: $artefact" >&2
        exit 1
    fi
done

echo "==> Vendoring into ${DEST#"$REPO_ROOT/"}"
mkdir -p "$DEST"
for artefact in "${ARTEFACTS[@]}"; do
    cp "$artefact" "$DEST/"
    echo "    $(basename "$artefact")"
done

echo "==> Done"
