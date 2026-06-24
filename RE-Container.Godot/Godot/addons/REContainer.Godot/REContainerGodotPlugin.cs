#if TOOLS
using System.Collections.Generic;
using Godot;
using REContainer.Godot.LifetimeScopes;

namespace REContainer.Godot;

[Tool]
public partial class REContainerGodotPlugin : EditorPlugin
{
	private readonly HashSet<string> _customAutoloadedNodeNames = [];

	public override void _EnterTree()
	{
		CreateAllAutoLoadNodes();
	}

	public override void _ExitTree()
	{
		foreach (var name in _customAutoloadedNodeNames)
		{
			RemoveAutoloadSingleton(name);
		}
		
		_customAutoloadedNodeNames.Clear();
	}

	private void CreateAllAutoLoadNodes()
	{
		CreateAutoLoadNode(
            nameof(GlobalRoot),
			$"res://addons/REContainer.Godot/Runtime/{nameof(GlobalRoot)}.cs");

		CreateAutoLoadNode(
			nameof(Initialiser),
			$"res://addons/REContainer.Godot/Runtime/{nameof(Initialiser)}.cs");
	}

	private void CreateAutoLoadNode(string name, string autoloadPath)
	{
		if (!_customAutoloadedNodeNames.Add(name))
		{
			GD.PrintErr($"Autoload node {name} is already registered!");
			return;
		}

		AddAutoloadSingleton(name, autoloadPath);
	}
}
#endif