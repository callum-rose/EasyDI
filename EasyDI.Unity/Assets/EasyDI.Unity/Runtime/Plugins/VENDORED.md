# Vendored plugins

The DLLs in this folder are **generated build output**, committed on purpose. They are not
managed by NuGet, NuGetForUnity, or any package manager — this Unity package is self-contained,
so installing it is all a consumer needs to do.

| File | Built from | Target |
|---|---|---|
| `EasyDI.dll` | [`EasyDI/`](../../../../../EasyDI) | `netstandard2.1` |
| `EasyDI.LifecycleHooks.dll` | [`EasyDI.LifecycleHooks/`](../../../../../EasyDI.LifecycleHooks) | `netstandard2.1` |
| `EasyDI.Analyzers.dll` | [`EasyDI.Analyzers/`](../../../../../EasyDI.Analyzers) | `netstandard2.0` |

The `.pdb` files are portable symbols, shipped so you can step into EasyDI from Unity.

The same libraries are published to nuget.org for .NET and Godot consumers. Unity gets them this
way instead because `Library/PackageCache` is read-only, so nothing can restore into a package
that was installed by git URL.

## Do not hand-edit anything in this folder

Changing core means editing the C# in the projects above and then re-running the vendor script
from the repo root:

```bash
./scripts/vendor-unity-dlls.sh
```

Commit the changed DLLs alongside the source change. CI rebuilds them and fails the build if the
committed bytes don't match, so a source change without a re-vendor won't merge.

The `.meta` files are hand-authored and committed, and the script deliberately leaves them alone.
Their GUIDs are referenced by every project that uses this package, and the `RoslynAnalyzer` label
on `EasyDI.Analyzers.dll.meta` is what makes Unity run the analyzer at all — if you regenerate
them, both break.
