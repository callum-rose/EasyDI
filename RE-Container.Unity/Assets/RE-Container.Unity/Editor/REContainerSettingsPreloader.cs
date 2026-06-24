#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace REContainer.Unity.Editor
{
	[InitializeOnLoad]
	internal class REContainerSettingsPreloader : IPreprocessBuildWithReport
	{
		public int callbackOrder => 0;
		
		static REContainerSettingsPreloader()
		{
			EditorApplication.delayCall += AddSettingsToPreloadedAssetsIfNotThereYet;
		}
		
		public void OnPreprocessBuild(BuildReport report)
		{
			AddSettingsToPreloadedAssetsIfNotThereYet();
		}

		private static void AddSettingsToPreloadedAssetsIfNotThereYet()
		{
			Debug.Log($"[REContainer] Ensuring {nameof(REContainerSettings)} is in Preloaded Assets...");
			var existingAssets = AssetDatabase.FindAssets($"t:{nameof(REContainerSettings)}");
        
			if (existingAssets.Length is 0)
			{
				Debug.LogWarning($"[REContainer] No {nameof(REContainerSettings)} asset found. Please create one via [Assets -> Create -> REContainer -> Settings], or via right click in the Project Window [Create -> REContainer -> Settings].");
				return;
			}

			var preloadedAssets = PlayerSettings.GetPreloadedAssets();
        
			if (preloadedAssets.OfType<REContainerSettings>().Any())
			{
				Debug.Log($"[REContainer] {nameof(REContainerSettings)} already in Preloaded Assets.");
				return;
			}
			
			var assetPath = AssetDatabase.GUIDToAssetPath(existingAssets[0]);
			var settings = AssetDatabase.LoadAssetAtPath<REContainerSettings>(assetPath);
			
			preloadedAssets = preloadedAssets.Append(settings).ToArray();
			
			PlayerSettings.SetPreloadedAssets(preloadedAssets);
			Debug.Log($"[REContainer] {nameof(REContainerSettings)} added to Preloaded Assets.");
		}
		
	}
}
#endif