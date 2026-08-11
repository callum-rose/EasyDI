using System;

namespace EasyDI.Unity.LifetimeScopes
{
	/// <summary>
	/// Base class for a scope that sits at the top of a scope chain and has no parent.
	/// </summary>
	/// <remarks>
	/// The root scope of a project is spawned automatically before the first scene loads; assign its prefab to the
	/// Root Lifetime Scope field of the EasyDISettings asset.
	/// </remarks>
	public abstract class RootLifetimeScope : LifetimeScope
	{
		protected internal sealed override Type? ParentScopeType => null;
	}
}
