using Godot;

namespace REContainer.Godot;

public partial class Initialiser : Node
{
	public override void _EnterTree()
	{
		GlobalNode.Instantiate(REContainerSettings.ApplicationScopePrefab);
	}
}