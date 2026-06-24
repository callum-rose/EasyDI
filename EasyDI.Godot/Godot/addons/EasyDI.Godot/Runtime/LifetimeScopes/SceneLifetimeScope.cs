using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Godot;
using EasyDI.Registering;

namespace EasyDI.Godot.LifetimeScopes;

[GlobalClass]
public partial class SceneLifetimeScope : LifetimeScope
{
	[Export] private ParentScopeType parentScopeType;
	[Export] private NodeInstaller primaryInstaller = null!;
	
	[Export] //("When a parent scope can't be found, use to this to substitute any missing services.")
	private NodeInstaller? testingBackupInstaller;

	protected override bool DoParentTransformToParentScope => false;

	protected override bool RequiresParentScope([NotNullWhen(true)] out Type? type)
	{
		type = parentScopeType switch
		{
			ParentScopeType.Application => typeof(ApplicationLifetimeScope),
			ParentScopeType.Session => typeof(SessionLifetimeScope),
			ParentScopeType.Game => typeof(GameLifetimeScope),
			_ => throw new ArgumentOutOfRangeException()
		};
		
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