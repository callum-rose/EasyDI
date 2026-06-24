#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EasyDI.Unity.Editor
{
	[InitializeOnLoad]
	internal class EasyDISettingsPreloader : IPreprocessBuildWithReport
	{
		public int callbackOrder => 0;
		
		static EasyDISettingsPreloader()
		{
			EditorApplication.delayCall += AddSettingsToPreloadedAssetsIfNotThereYet;
		}
		
		public void OnPreprocessBuild(BuildReport report)
		{
			AddSettingsToPreloadedAssetsIfNotThereYet();
		}

		private static void AddSettingsToPreloadedAssetsIfNotThereYet()
		{
			Debug.Log($"[EasyDI] Ensuring {nameof(EasyDISettings)} is in Preloaded Assets...");
			var existingAssets = AssetDatabase.FindAssets($"t:{nameof(EasyDISettings)}");
        
			if (existingAssets.Length is 0)
			{
				Debug.LogWarning($"[EasyDI] No {nameof(EasyDISettings)} asset found. Please create one via [Assets -> Create -> EasyDI -> Settings], or via right click in the Project Window [Create -> EasyDI -> Settings].");
				return;
			}

			var preloadedAssets = PlayerSettings.GetPreloadedAssets();
        
			if (preloadedAssets.OfType<EasyDISettings>().Any())
			{
				Debug.Log($"[EasyDI] {nameof(EasyDISettings)} already in Preloaded Assets.");
				return;
			}
			
			var assetPath = AssetDatabase.GUIDToAssetPath(existingAssets[0]);
			var settings = AssetDatabase.LoadAssetAtPath<EasyDISettings>(assetPath);
			
			preloadedAssets = preloadedAssets.Append(settings).ToArray();
			
			PlayerSettings.SetPreloadedAssets(preloadedAssets);
			Debug.Log($"[EasyDI] {nameof(EasyDISettings)} added to Preloaded Assets.");
		}
		
	}
}
#endif