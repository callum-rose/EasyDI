using System;
using EasyDI.Registering;
using UnityEngine;

namespace EasyDI.Unity.LifetimeScopes
{
	/// <summary>
	/// A scope that lives in a scene and parents to whichever scope is named in its inspector.
	/// </summary>
	public sealed class SceneLifetimeScope : LifetimeScope
	{
		[SerializeField] private string parentScopeName = string.Empty;

		[SerializeField, Tooltip("When a parent scope can't be found, use to this to substitute any missing services.")]
		private MonoInstaller? testingBackupInstaller;

	#if UNITY_EDITOR
		internal static string ParentScopeNamePropertyName => nameof(parentScopeName);
	#endif

		protected override bool DoParentTransformToParentScope => false;

		protected internal override Type? ParentScopeType
		{
			get
			{
				if (string.IsNullOrEmpty(parentScopeName))
				{
					throw new InvalidOperationException(
						$"'{name}' has no parent scope selected. Set the Parent Scope field of its " +
						$"{nameof(SceneLifetimeScope)} to one of: " +
						$"{string.Join(", ", NameHelper.ParentableLifetimeScopeNames)}.");
				}

				return NameHelper.GetTypeByName(parentScopeName);
			}
		}

		protected override void Configure(IObjectRegistry registry)
		{
			if (IsMissingParentScope())
			{
				testingBackupInstaller?.Install(registry);
			}
		}
	}
}
