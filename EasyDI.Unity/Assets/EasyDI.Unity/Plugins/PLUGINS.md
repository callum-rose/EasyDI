# Plugins

The DLLs in this folder are **NuGet packages**, not source. They're installed and managed by [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity), so **don't edit or hand-copy them**.

## What's here

| Package | Version |
|---------|:-------:|
| `EasyDI` | 1.0.1 |
| `EasyDI.LifecycleHooks` | 1.0.0 |
| `EasyDI.Analyzers` | 1.0.3 |

Versions are pinned in [`Assets/packages.config`](../../packages.config). The install location and sources are configured in [`Assets/NuGet.config`](../../NuGet.config) (`repositoryPath` points NuGetForUnity at this folder).

## Updating a package

1. In Unity, open **NuGet → Manage NuGet Packages**.
2. Find the package and install/upgrade to the desired version (or edit the version in `packages.config` and let NuGetForUnity restore it).
3. Commit the updated `packages.config` and the changed files under `Plugins/`.

Do **not** drag a newer `.dll` in manually — the version in `packages.config` is the source of truth, and a manual copy will drift from it and get overwritten on the next restore.
