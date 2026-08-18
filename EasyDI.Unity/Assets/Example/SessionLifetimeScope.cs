using EasyDI.Unity.LifetimeScopes;

namespace EasyDI.Unity.Example
{
	/// <summary>
	/// The example's middle layer, holding services that live for as long as a player's session.
	/// </summary>
	public sealed class SessionLifetimeScope : LifetimeScope<ApplicationLifetimeScope>
	{
	}
}
