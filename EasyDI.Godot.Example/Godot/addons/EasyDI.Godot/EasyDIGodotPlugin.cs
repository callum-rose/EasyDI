#if TOOLS
using System.Collections.Generic;
using Godot;

namespace EasyDI.Godot;

[Tool]
public partial class EasyDIGodotPlugin : EditorPlugin
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
	}

	private void CreateAllAutoLoadNodes()
	{
		CreateAutoLoadNode(
            nameof(GlobalNode),
			$"res://addons/EasyDI.Godot/Runtime/{nameof(GlobalNode)}.cs");

		CreateAutoLoadNode(
			nameof(Initialiser),
			$"res://addons/EasyDI.Godot/Runtime/{nameof(Initialiser)}.cs");
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