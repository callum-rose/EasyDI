using System;
using System.Diagnostics.CodeAnalysis;
using REContainer.Registering;
using UnityEngine;

namespace REContainer.Unity.LifetimeScopes
{
	public sealed class SceneLifetimeScope : LifetimeScope
	{
		[SerializeField] private string parentScopeName = NameHelper.GetName<ApplicationLifetimeScope>();
		[SerializeField] private MonoInstaller primaryInstaller = null!;

		[SerializeField, Tooltip("When a parent scope can't be found, use to this to substitute any missing services.")]
		private MonoInstaller? testingBackupInstaller;

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
}