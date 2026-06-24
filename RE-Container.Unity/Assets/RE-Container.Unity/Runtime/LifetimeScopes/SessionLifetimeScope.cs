using System;
using System.Diagnostics.CodeAnalysis;
using REContainer.Registering;
using UnityEngine;

namespace REContainer.Unity.LifetimeScopes
{
	public sealed class SessionLifetimeScope : LifetimeScope
	{
		[SerializeField] private MonoInstaller installer = null!;
		
		protected override bool DoParentTransformToParentScope => true;

		protected override bool RequiresParentScope([NotNullWhen(true)] out Type? type)
		{
			type = typeof(ApplicationLifetimeScope);
			return true;
		}

		protected override void Configure(IObjectRegistry registry)
		{
			installer.Install(registry);
		}
	}
}