using System;

namespace EasyDI.Unity.LifetimeScopes
{
	/// <summary>
	/// Base class for a scope nested inside <typeparamref name="TParent"/>. Services registered in
	/// <typeparamref name="TParent"/> are resolvable from this scope, but not the other way around.
	/// </summary>
	/// <typeparam name="TParent">
	/// The scope this one nests inside. A single instance of it is expected to exist when this scope awakes.
	/// </typeparam>
	public abstract class LifetimeScope<TParent> : LifetimeScope where TParent : LifetimeScope
	{
		protected internal sealed override Type? ParentScopeType => typeof(TParent);
	}
}
