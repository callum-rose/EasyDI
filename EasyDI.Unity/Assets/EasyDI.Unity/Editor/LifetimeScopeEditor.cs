using System;
using System.Linq;
using EasyDI.Unity.LifetimeScopes;
using UnityEditor;
using UnityEngine;

namespace EasyDI.Unity.Editor
{
	[CustomEditor(typeof(LifetimeScope), true)]
	public class LifetimeScopeEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			GUI.enabled = !EditorApplication.isPlaying;

			serializedObject.Update();

			if (target is SceneLifetimeScope)
			{
				DrawParentScopeName();

				DrawPropertiesExcluding(serializedObject, "m_Script", SceneLifetimeScope.ParentScopeNamePropertyName);
			}
			else
			{
				DrawParentScopeType();

				DrawPropertiesExcluding(serializedObject, "m_Script");
			}

			serializedObject.ApplyModifiedProperties();
		}

		private void DrawParentScopeType()
		{
			var parentScopeType = ((LifetimeScope)target).GetParentScopeType();

			EditorGUILayout.HelpBox(
				parentScopeType == null
					? "This is a root scope, so it has no parent."
					: $"This parents to the {ObjectNames.NicifyVariableName(parentScopeType.Name)}.",
				MessageType.Info);
		}

		private void DrawParentScopeName()
		{
			var parentScopeNameProperty = serializedObject.FindProperty(SceneLifetimeScope.ParentScopeNamePropertyName);
			var parentScopeName = parentScopeNameProperty.stringValue;
			var scopeNames = NameHelper.ParentableLifetimeScopeNames.ToArray();

			DrawFoundParentScope(parentScopeName);

			var currentIndex = Array.IndexOf(scopeNames, parentScopeName);
			var newIndex = EditorGUILayout.Popup("Parent Scope", currentIndex, scopeNames);

			if (newIndex != currentIndex && newIndex >= 0)
			{
				parentScopeNameProperty.stringValue = scopeNames[newIndex];
				parentScopeName = scopeNames[newIndex];
				currentIndex = newIndex;
			}

			if (string.IsNullOrEmpty(parentScopeName))
			{
				EditorGUILayout.HelpBox(
					"Choose the scope this one parents to. Without it, entering this scene will fail.",
					MessageType.Warning);
			}
			else if (currentIndex < 0)
			{
				EditorGUILayout.HelpBox(
					$"'{parentScopeName}' isn't the name of a scope in this project; it was probably renamed or " +
					$"deleted. Choose one of: {string.Join(", ", scopeNames)}.",
					MessageType.Error);
			}
		}

		private void DrawFoundParentScope(string parentScopeName)
		{
			if (!EditorApplication.isPlaying || !NameHelper.TryGetTypeByName(parentScopeName, out var parentScopeType))
			{
				return;
			}

			if (FindAnyObjectByType(parentScopeType) is not LifetimeScope parentScope)
			{
				return;
			}

			GUI.enabled = false;
			EditorGUILayout.ObjectField("Found Scope", parentScope, typeof(LifetimeScope), true);
			GUI.enabled = !EditorApplication.isPlaying;

			// Line
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
		}
	}
}
