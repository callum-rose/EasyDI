using System;
using System.Diagnostics.CodeAnalysis;
using Godot;
using REContainer.Registering;

namespace REContainer.Godot.LifetimeScopes;

[GlobalClass]
public partial class ApplicationLifetimeScope : LifetimeScope
{
	[Export] private NodeInstaller installer = null!;

	protected override bool DoParentTransformToParentScope => true;

	protected override bool RequiresParentScope([NotNullWhen(true)] out Type? type)
	{
		type = null;
		return false;
	}

	protected override void Configure(IObjectRegistry registry)
	{
		installer.Install(registry);
	}
}