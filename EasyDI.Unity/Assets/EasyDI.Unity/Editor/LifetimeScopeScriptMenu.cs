#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EasyDI.Unity.Editor
{
	internal static class LifetimeScopeScriptMenu
	{
		private const string RootTemplateFileName = "RootLifetimeScope.cs.txt";
		private const string ChildTemplateFileName = "ChildLifetimeScope.cs.txt";

		[MenuItem("Assets/Create/EasyDI/Root Lifetime Scope Script")]
		private static void CreateRootLifetimeScopeScript()
		{
			CreateScriptFromTemplate(RootTemplateFileName, "NewRootLifetimeScope.cs");
		}

		[MenuItem("Assets/Create/EasyDI/Child Lifetime Scope Script")]
		private static void CreateChildLifetimeScopeScript()
		{
			CreateScriptFromTemplate(ChildTemplateFileName, "NewChildLifetimeScope.cs");
		}

		private static void CreateScriptFromTemplate(string templateFileName, string defaultScriptName)
		{
			var templatePath = FindTemplatePath(templateFileName);

			if (templatePath == null)
			{
				Debug.LogError($"Couldn't find the script template '{templateFileName}' in the project.");
				return;
			}

			ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, defaultScriptName);
		}

		private static string FindTemplatePath(string templateFileName)
		{
			return AssetDatabase.FindAssets("LifetimeScope t:TextAsset")
				.Select(AssetDatabase.GUIDToAssetPath)
				.FirstOrDefault(path => path.EndsWith(templateFileName, StringComparison.Ordinal));
		}
	}
}
#endif
