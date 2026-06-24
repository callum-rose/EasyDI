#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EasyDI.Unity.Editor
{
	internal class EasyDISettingsPostprocessor : AssetPostprocessor
	{
		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			foreach (string deletedAssetPath in deletedAssets)
			{
				if (AssetDatabase.GetMainAssetTypeAtPath(deletedAssetPath) == typeof(EasyDISettings))
				{
				    RemoveFromPreloadedAssets(deletedAssetPath);
				}
			}

			if (PlayerSettings.GetPreloadedAssets().OfType<EasyDISettings>().Any())
			{
				return;
			}
			
			foreach (string importedAssetPath in importedAssets)
			{
				if (AssetDatabase.GetMainAssetTypeAtPath(importedAssetPath) == typeof(EasyDISettings))
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
			var asset = AssetDatabase.LoadAssetAtPath<EasyDISettings>(path);
			
			var preloadedAssets = PlayerSettings.GetPreloadedAssets();
			
			preloadedAssets = preloadedAssets.Append(asset).ToArray();
			
			PlayerSettings.SetPreloadedAssets(preloadedAssets);
			
			Debug.Log($"[EasyDI] {nameof(EasyDISettings)} added to Preloaded Assets.");
		}
	}
}
#endif