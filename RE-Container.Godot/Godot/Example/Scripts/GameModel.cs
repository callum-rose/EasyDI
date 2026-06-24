using System;
namespace REContainer.Godot.Example;

public class GameModel
{
    public event Action? Updated;

    public void Update()
    {
        // Simulating some domain level event
        Updated?.Invoke();
    }
}