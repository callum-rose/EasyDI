using System;
using REContainer.Unity.LifetimeScopes;
using UnityEngine;

namespace REContainer.Unity
{
	public class REContainerSettings : ScriptableObject
	{
		[SerializeField] private ApplicationLifetimeScope applicationLifetimeScope = null!;
		[SerializeField] private SessionLifetimeScope? sessionLifetimeScope;
		[SerializeField] private GameLifetimeScope? gameLifetimeScope;
		
	#if UNITY_EDITOR
		internal static string ApplicationLifetimeScopePropertyName => nameof(applicationLifetimeScope);
		internal static string SessionLifetimeScopePropertyName => nameof(sessionLifetimeScope);
		internal static string GameLifetimeScopePropertyName => nameof(gameLifetimeScope);
	#endif
		
		public static ApplicationLifetimeScope ApplicationLifetimeScope => Instance.applicationLifetimeScope;
		public static SessionLifetimeScope? SessionLifetimeScope => Instance.sessionLifetimeScope;
		public static GameLifetimeScope? GameLifetimeScope => Instance.gameLifetimeScope;

		private static REContainerSettings Instance => _instance != null
			? _hasSingleInstance
				? _instance
				: throw new Exception($"Multiple instances of {nameof(REContainerSettings)} found. There must be only one instance of {nameof(REContainerSettings)} in the project.")
			: throw new Exception($"{nameof(REContainerSettings)} asset not found. Please create one via [Assets -> Create -> REContainer -> Settings], or via right click in the Project Window [Create -> REContainer -> Settings].");
		
		private static REContainerSettings _instance = null!;
		private static bool _hasSingleInstance;

		private void OnEnable()
		{
			if (_instance == null)
			{
				_instance = this;
				_hasSingleInstance = true;
			}
			else if (_instance != this)
			{
				_hasSingleInstance = false;
#if UNITY_EDITOR
				Debug.LogError($"Instance of {nameof(REContainerSettings)} already exists at {UnityEditor.AssetDatabase.GetAssetPath(_instance)}.");
#else
				Debug.LogError($"Multiple instances of {nameof(REContainerSettings)} found. There must be only one instance of {nameof(REContainerSettings)} in the project.");
#endif
			}
		}
	}
}