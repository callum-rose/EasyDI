using Godot;
using REContainer.Registering;
namespace REContainer.Godot;

public interface IGlobalRoot
{
	T? InstantiateChild<T>(PackedScene? packedScene) where T : Node;

	void AddChildNowOrDeferred(Node node);
	
	IDisposable? InstantiateLifetimeScope(PackedScene scene, Action<IObjectRegistry> registration);
}