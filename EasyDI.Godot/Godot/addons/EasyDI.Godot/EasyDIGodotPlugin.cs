#if TOOLS
using System.Collections.Generic;
using Godot;
using EasyDI.Godot.LifetimeScopes;

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
		
		_customAutoloadedNodeNames.Clear();
	}

	private void CreateAllAutoLoadNodes()
	{
		CreateAutoLoadNode(
            nameof(GlobalRoot),
			$"res://addons/EasyDI.Godot/Runtime/{nameof(GlobalRoot)}.cs");

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