using Godot;

namespace EasyDI.Godot;

public partial class Initialiser : Node
{
	public override void _EnterTree()
	{
		GlobalNode.Instantiate(EasyDISettings.ApplicationScopePrefab);
	}
}