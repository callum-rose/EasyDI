#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace REContainer.Unity.Editor
{
	internal class REContainerSettingsEditorMenu
	{
		[MenuItem("Assets/Create/REContainer/Settings")]
		private static void CreateSettingsAssetMenu()
		{
			REContainerSettings asset;
			
			var existingAssets = AssetDatabase.FindAssets($"t:{nameof(REContainerSettings)}");
			
			if (existingAssets.Length > 0)
			{
				var assetPath = AssetDatabase.GUIDToAssetPath(existingAssets[0]);
				Debug.LogWarning($"{nameof(REContainerSettings)} asset already exists at: {assetPath}");
				asset = AssetDatabase.LoadAssetAtPath<REContainerSettings>(assetPath);
			}
			else
			{
				asset = ScriptableObject.CreateInstance<REContainerSettings>();
				var path = Path.Combine(GetCurrentDirectory(), $"{nameof(REContainerSettings)}.asset");
				AssetDatabase.CreateAsset(asset, path);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				Debug.Log($"Created {nameof(REContainerSettings)} asset at: {path}");
			}
			
			Selection.activeObject = asset;
			EditorGUIUtility.PingObject(asset);
		}

		private static string GetCurrentDirectory()
		{
			string selectedPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);

			return Directory.Exists(selectedPath)
				? selectedPath
				: File.Exists(selectedPath)
					? Path.GetDirectoryName(selectedPath) ?? "Assets"
					: "Assets";
		}
	}
}
#endif