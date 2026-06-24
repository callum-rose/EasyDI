using UnityEngine;

namespace EasyDI.Unity
{
	internal static class Initialiser
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialise()
		{
			var applicationScope = Object.Instantiate(EasyDISettings.ApplicationLifetimeScope);
			Object.DontDestroyOnLoad(applicationScope);
		}
	}
}