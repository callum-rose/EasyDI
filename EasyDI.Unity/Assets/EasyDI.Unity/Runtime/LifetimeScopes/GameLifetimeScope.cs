using System;
using System.Diagnostics.CodeAnalysis;
using EasyDI.Registering;
using UnityEngine;

namespace EasyDI.Unity.LifetimeScopes
{
	public sealed class GameLifetimeScope : LifetimeScope
	{
		[SerializeField] private MonoInstaller installer = null!;

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
}