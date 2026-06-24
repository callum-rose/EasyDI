using Godot;
namespace REContainer.Godot.Extensions;

public static class IGlobalRootExtensions
{
	public static Node? InstantiateChildNode(this IGlobalRoot globalRoot, PackedScene? packedScene)
	{
		return globalRoot.InstantiateChild<Node>(packedScene);
	}
}