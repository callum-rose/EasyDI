using Godot;
using EasyDI.Registering;
namespace EasyDI.Godot;

public interface IGlobalRoot
{
	T? InstantiateChild<T>(PackedScene? packedScene) where T : Node;

	void AddChildNowOrDeferred(Node node);
	
	IDisposable? InstantiateLifetimeScope(PackedScene scene, Action<IObjectRegistry> registration);
}