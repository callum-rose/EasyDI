using System;
using System.Linq;
using REContainer.Unity.LifetimeScopes;
using UnityEditor;
using UnityEngine;

namespace REContainer.Unity.Editor
{
	[CustomEditor(typeof(LifetimeScope), true)]
	public class LifetimeScopeEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			GUI.enabled = !EditorApplication.isPlaying;

			var lifetimeScope = (LifetimeScope)target;

			switch (lifetimeScope)
			{
				case ApplicationLifetimeScope:
					EditorGUILayout.HelpBox("This is the root scope", MessageType.Info);
					break;
				
				case SessionLifetimeScope:
					EditorGUILayout.HelpBox("This parents to the " + nameof(ApplicationLifetimeScope),
						MessageType.Info);
					break;
				
				case GameLifetimeScope:
					EditorGUILayout.HelpBox("This parents to the " + nameof(SessionLifetimeScope), MessageType.Info);
					break;
				
				case SceneLifetimeScope:
				{
					var parentScopeProperty = serializedObject.FindProperty("parentScopeName");

					if (EditorApplication.isPlaying)
					{
						var parentScopeType = NameHelper.GetTypeByName(parentScopeProperty.stringValue);
						var parentScope = FindAnyObjectByType(parentScopeType) as LifetimeScope;

						if (parentScope != null)
						{
							GUI.enabled = false;
							EditorGUILayout.ObjectField("Found Scope", parentScope, typeof(LifetimeScope), true);
							GUI.enabled = !EditorApplication.isPlaying;

							// Line
							EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
						}
					}

					var scopeNames = NameHelper.ParentableLifetimeScopeNames.ToArray();
					var currentIndex = Array.IndexOf(scopeNames, parentScopeProperty.stringValue);
					var newIndex = EditorGUILayout.Popup("Parent Scope", currentIndex, scopeNames);

					if (newIndex != currentIndex)
					{
						parentScopeProperty.stringValue = scopeNames[newIndex];
					}
					
					EditorGUILayout.PropertyField(serializedObject.FindProperty("primaryInstaller"));

					break;
				}
				
				default:
					EditorGUILayout.HelpBox("Unknown LifetimeScope type", MessageType.Warning);
					break;
			}
			
			if (lifetimeScope is SceneLifetimeScope)
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("testingBackupInstaller"), true);
			}
			else
			{
				EditorGUILayout.PropertyField(serializedObject.FindProperty("installer"), true);
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}