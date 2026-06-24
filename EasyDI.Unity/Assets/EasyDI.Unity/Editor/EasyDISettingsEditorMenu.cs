#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EasyDI.Unity.Editor
{
	internal class EasyDISettingsEditorMenu
	{
		[MenuItem("Assets/Create/EasyDI/Settings")]
		private static void CreateSettingsAssetMenu()
		{
			EasyDISettings asset;
			
			var existingAssets = AssetDatabase.FindAssets($"t:{nameof(EasyDISettings)}");
			
			if (existingAssets.Length > 0)
			{
				var assetPath = AssetDatabase.GUIDToAssetPath(existingAssets[0]);
				Debug.LogWarning($"{nameof(EasyDISettings)} asset already exists at: {assetPath}");
				asset = AssetDatabase.LoadAssetAtPath<EasyDISettings>(assetPath);
			}
			else
			{
				asset = ScriptableObject.CreateInstance<EasyDISettings>();
				var path = Path.Combine(GetCurrentDirectory(), $"{nameof(EasyDISettings)}.asset");
				AssetDatabase.CreateAsset(asset, path);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				Debug.Log($"Created {nameof(EasyDISettings)} asset at: {path}");
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