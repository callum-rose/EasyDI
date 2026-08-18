using EasyDI.Unity.LifetimeScopes;

namespace EasyDI.Unity.Example
{
	/// <summary>
	/// The example's root scope, instantiated before the first scene loads and kept for the lifetime of the game.
	/// </summary>
	public sealed class ApplicationLifetimeScope : RootLifetimeScope
	{
	}
}
