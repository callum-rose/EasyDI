using System;
using System.Collections.Generic;
using Godot;

namespace REContainer.Godot;

/// <summary>
/// Used by nodes that have been created outside the scene tree but still need to be placed within it.
/// </summary>
public partial class GlobalRoot : Node
{
	private static GlobalRoot? _instance;

	private static readonly List<Node> DeferredNodes = [];

	public override void _EnterTree()
	{
		if (_instance != null)
		{
			GD.PushError("Multiple instances of GlobalNode detected! There should only be one GlobalNode in the scene tree.");
			QueueFree();
			return;
		}

		_instance = this;

		foreach (var node in DeferredNodes)
		{
			AddChild(node);
		}

		DeferredNodes.Clear();
	}

	public static Node? InstantiateChild(PackedScene? packedScene)
	{
		return InstantiateChild<Node>(packedScene);
	}

	public static T? InstantiateChild<T>(PackedScene? packedScene) where T : Node
	{
		if (packedScene == null)
		{
			GD.PushError($"Failed to instantiate {typeof(T).Name} in the Global node. Argument {nameof(packedScene)} is null!");
			return null;
		}
		var node = packedScene.Instantiate<T>();
		AddChildNowOrDeferred(node);
		return node;
	}

	public static void AddChildNowOrDeferred(Node node)
	{
		if (_instance == null)
		{
			DeferredNodes.Add(node);
		}
		else
		{
			_instance.AddChild(node);
		}
	}
}