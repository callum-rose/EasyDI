#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace REContainer.Unity.Editor
{
	internal class REContainerSettingsPostprocessor : AssetPostprocessor
	{
		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			foreach (string deletedAssetPath in deletedAssets)
			{
				if (AssetDatabase.GetMainAssetTypeAtPath(deletedAssetPath) == typeof(REContainerSettings))
				{
				    RemoveFromPreloadedAssets(deletedAssetPath);
				}
			}

			if (PlayerSettings.GetPreloadedAssets().OfType<REContainerSettings>().Any())
			{
				return;
			}
			
			foreach (string importedAssetPath in importedAssets)
			{
				if (AssetDatabase.GetMainAssetTypeAtPath(importedAssetPath) == typeof(REContainerSettings))
				{
					AddToPreloadAssets(importedAssetPath);
				}
			}
		}
	
		private static void RemoveFromPreloadedAssets(string path)
		{
			var preloadedAssets = PlayerSettings.GetPreloadedAssets();

			var remainingAssets = preloadedAssets
				.Where(asset => asset != null && AssetDatabase.GetAssetPath(asset) != path)
				.ToArray();
			
			PlayerSettings.SetPreloadedAssets(remainingAssets);
		}

		private static void AddToPreloadAssets(string path)
		{
			var asset = AssetDatabase.LoadAssetAtPath<REContainerSettings>(path);
			
			var preloadedAssets = PlayerSettings.GetPreloadedAssets();
			
			preloadedAssets = preloadedAssets.Append(asset).ToArray();
			
			PlayerSettings.SetPreloadedAssets(preloadedAssets);
			
			Debug.Log($"[REContainer] {nameof(REContainerSettings)} added to Preloaded Assets.");
		}
	}
}
#endif