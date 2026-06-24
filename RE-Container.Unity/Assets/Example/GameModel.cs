using System;

namespace REContainer.Unity.Example
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