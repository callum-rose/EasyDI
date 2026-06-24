using System;

namespace EasyDI.Godot.Example.Scripts;

public class GameModel
{
    public event Action? Updated;

    public void Update()
    {
        // Simulating some domain level event
        Updated?.Invoke();
    }
}