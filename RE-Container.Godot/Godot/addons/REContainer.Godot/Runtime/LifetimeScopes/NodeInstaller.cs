using Godot;
using REContainer.Registering;

namespace REContainer.Godot.LifetimeScopes;

public abstract partial class NodeInstaller : Node
{
	public abstract void Install(IObjectRegistry registry);
}