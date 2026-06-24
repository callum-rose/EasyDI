using System;
using System.Diagnostics.CodeAnalysis;
using Godot;
using REContainer.Registering;

namespace REContainer.Godot.LifetimeScopes;

[GlobalClass]
public partial class SceneLifetimeScope : LifetimeScope
{
	[Export] private string parentScopeName = NameHelper.GetName<ApplicationLifetimeScope>();
	[Export] private NodeInstaller primaryInstaller = null!;

	[Export] // Tooltip("When a parent scope can't be found, use to this to substitute any missing services.")
	private NodeInstaller? testingBackupInstaller;

	protected override bool DoParentTransformToParentScope => false;

	protected override bool RequiresParentScope([NotNullWhen(true)] out Type? type)
	{
		type = NameHelper.GetTypeByName(parentScopeName);
		return true;
	}

	protected override void Configure(IObjectRegistry registry)
	{
		primaryInstaller.Install(registry);

		if (IsMissingParentScope())
		{
			testingBackupInstaller?.Install(registry);
		}
	}
}