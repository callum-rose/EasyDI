using System;
using System.Diagnostics.CodeAnalysis;
using Godot;
using EasyDI.Registering;

namespace EasyDI.Godot.LifetimeScopes;

[GlobalClass]
public partial class GameLifetimeScope : LifetimeScope
{
	[Export] private NodeInstaller installer = null!;

	protected override bool DoParentTransformToParentScope => true;

	protected override bool RequiresParentScope([NotNullWhen(true)] out Type? type)
	{
		type = typeof(SessionLifetimeScope);
		return true;
	}

	protected override void Configure(IObjectRegistry registry)
	{
		installer.Install(registry);
	}
}