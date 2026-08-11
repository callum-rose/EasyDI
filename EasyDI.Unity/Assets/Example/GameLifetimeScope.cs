using EasyDI.Unity.LifetimeScopes;

namespace EasyDI.Unity.Example
{
	/// <summary>
	/// The example's innermost layer, holding services that live for as long as a single game.
	/// </summary>
	public sealed class GameLifetimeScope : LifetimeScope<SessionLifetimeScope>
	{
	}
}
