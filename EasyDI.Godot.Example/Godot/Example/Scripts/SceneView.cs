using System;
using System.Threading;
using Godot;

namespace EasyDI.Godot.Example.Scripts;

public interface ISceneView
{
	event Action ButtonPressed;

	void Jump(CancellationToken cancellationToken);
	void SetSecondsElapsed(float seconds);
}
internal partial class SceneView : Node, ISceneView
{
	[Export] private Label secondsElapsedText = null!;
	[Export] private Node3D jumper = null!;
	[Export] private Button button = null!;

	public event Action? ButtonPressed;

	public override void _Ready()
	{
		button.Pressed += OnButtonPressed;
	}

	public async void Jump(CancellationToken cancellationToken)
	{
		for(float time = 0; time < 1; time += (float)GetProcessDeltaTime())
		{
			float y = 4 * (time - time * time);
			jumper.Position = Vector3.Up * y;

			if (cancellationToken.IsCancellationRequested)
			{
				return;
			}

			await ToSignal(GetTree(), "process_frame");
		}
	}

	public void SetSecondsElapsed(float seconds)
	{
		secondsElapsedText.Text = $"{seconds:0.00} seconds elapsed";
	}

	private void OnButtonPressed()
	{
		ButtonPressed?.Invoke();
	}
}