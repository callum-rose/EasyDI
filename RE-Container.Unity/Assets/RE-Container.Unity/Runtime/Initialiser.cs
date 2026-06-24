using UnityEngine;

namespace REContainer.Unity
{
	internal static class Initialiser
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialise()
		{
			var applicationScope = Object.Instantiate(REContainerSettings.ApplicationLifetimeScope);
			Object.DontDestroyOnLoad(applicationScope);
		}
	}
}