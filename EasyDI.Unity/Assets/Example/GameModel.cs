using System;

namespace EasyDI.Unity.Example
{
	public class GameModel
	{
		public event Action Updated;
		
		public void Update()
		{
			// Simulating some domain level event
			Updated?.Invoke();
		}
	}
}