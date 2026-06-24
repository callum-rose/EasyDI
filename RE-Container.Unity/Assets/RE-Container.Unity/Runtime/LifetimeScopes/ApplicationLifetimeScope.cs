using System;
using System.Diagnostics.CodeAnalysis;
using REContainer.Registering;
using UnityEngine;

namespace REContainer.Unity.LifetimeScopes
{
	public sealed class ApplicationLifetimeScope : LifetimeScope
	{
		[SerializeField] private MonoInstaller installer = null!;

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
}