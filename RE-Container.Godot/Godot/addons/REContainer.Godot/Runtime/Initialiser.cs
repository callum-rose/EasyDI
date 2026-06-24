using Godot;

namespace REContainer.Godot;

public partial class Initialiser : Node
{
    public override void _EnterTree()
    {
        GlobalRoot.InstantiateChild(REContainerSettings.ApplicationScopePrefab);
    }
}