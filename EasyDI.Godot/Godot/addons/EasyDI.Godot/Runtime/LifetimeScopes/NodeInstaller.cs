using Godot;
using EasyDI.Registering;

namespace EasyDI.Godot.LifetimeScopes;

public abstract partial class NodeInstaller : Node
{
	public abstract void Install(IObjectRegistry registry);
}