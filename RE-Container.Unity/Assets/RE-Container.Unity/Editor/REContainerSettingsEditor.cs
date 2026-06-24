#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using REContainer.Unity.LifetimeScopes;

namespace REContainer.Unity.Editor
{
    [CustomEditor(typeof(REContainerSettings))]
    public class REContainerSettingsEditor : UnityEditor.Editor
    {
        private SerializedProperty _applicationScopePrefabProperty;
        private SerializedProperty _sessionLifetimeScopeProperty;
        private SerializedProperty _gameLifetimeScopeProperty;
        
        private readonly List<ApplicationLifetimeScope> _availableApplicationPrefabs = new();
        private readonly List<string> _applicationPrefabNames = new();
        private int _selectedApplicationPrefabIndex;
        
        private readonly List<SessionLifetimeScope> _availableSessionPrefabs = new();
        private readonly List<string> _sessionPrefabNames = new();
        private int _selectedSessionPrefabIndex;
        
        private readonly List<GameLifetimeScope> _availableGamePrefabs = new();
        private readonly List<string> _gamePrefabNames = new();
        private int _selectedGamePrefabIndex;

        private void OnEnable()
        {
            _applicationScopePrefabProperty = serializedObject.FindProperty(REContainerSettings.ApplicationLifetimeScopePropertyName);
            _sessionLifetimeScopeProperty = serializedObject.FindProperty(REContainerSettings.SessionLifetimeScopePropertyName);
            _gameLifetimeScopeProperty = serializedObject.FindProperty(REContainerSettings.GameLifetimeScopePropertyName);
            
            RefreshAvailablePrefabs();
            AutoAssignIfEmpty();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawScopeSection<ApplicationLifetimeScope>(
                ObjectNames.NicifyVariableName(REContainerSettings.ApplicationLifetimeScopePropertyName),
                _availableApplicationPrefabs,
                _applicationPrefabNames,
                _applicationScopePrefabProperty,
                MessageType.Error,
                CreateApplicationScopePrefab,
                false,
                ref _selectedApplicationPrefabIndex
            );

            EditorGUILayout.Space();

            DrawScopeSection<SessionLifetimeScope>(
                ObjectNames.NicifyVariableName(REContainerSettings.SessionLifetimeScopePropertyName),
                _availableSessionPrefabs,
                _sessionPrefabNames,
                _sessionLifetimeScopeProperty,
                MessageType.Info,
                CreateSessionScopePrefab,
                true,
                ref _selectedSessionPrefabIndex
            );

            EditorGUILayout.Space();

            DrawScopeSection<GameLifetimeScope>(
                ObjectNames.NicifyVariableName(REContainerSettings.GameLifetimeScopePropertyName),
                _availableGamePrefabs,
                _gamePrefabNames,
                _gameLifetimeScopeProperty,
                MessageType.Info,
                CreateGameScopePrefab,
                true,
                ref _selectedGamePrefabIndex
            );

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawScopeSection<T>(
            string sectionTitle,
            List<T> availablePrefabs,
            List<string> prefabNames,
            SerializedProperty property,
            MessageType missingPrefabMessageType,
            Action createAction,
            bool isOptional,
            ref int selectedPrefabIndex) where T : Component
        {
            EditorGUILayout.LabelField(sectionTitle, EditorStyles.boldLabel);
            
            if (availablePrefabs.Count == 0)
            {
                EditorGUILayout.HelpBox($"No {typeof(T).Name} prefabs found in the project.", missingPrefabMessageType);
                
                if (GUILayout.Button($"Create {typeof(T).Name} Prefab"))
                {
                    createAction();
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                
                IEnumerable<string> dropdownOptions;
                int displayIndex;

                if (isOptional)
                {
                    dropdownOptions = prefabNames.Prepend("None");
                    displayIndex = selectedPrefabIndex + 1; // Offset by 1 because of "None" option
                }
                else
                {
                    dropdownOptions = prefabNames;
                    displayIndex = selectedPrefabIndex;
                }
                
                int newIndex = EditorGUILayout.Popup(displayIndex, dropdownOptions.ToArray());
                
                if (newIndex != displayIndex)
                {
                    selectedPrefabIndex = isOptional ? newIndex - 1 : newIndex;
                    property.objectReferenceValue = selectedPrefabIndex >= 0 && selectedPrefabIndex < availablePrefabs.Count
                        ? availablePrefabs[selectedPrefabIndex]
                        : null;
                }
                
                if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent("d_Refresh").image, "Refresh prefabs list"), GUILayout.Width(25), GUILayout.Height(18)))
                {
                    RefreshAvailablePrefabs();
                }
                
                if (GUILayout.Button(new GUIContent("New", $"Create a new {typeof(T).Name} prefab"), GUILayout.Width(40)))
                {
                    createAction();
                }
                
                EditorGUILayout.EndHorizontal();
                
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(property, GUIContent.none);
                EditorGUI.EndDisabledGroup();
            }
        }
                
        private void RefreshAvailablePrefabs()
        {
            RefreshPrefabs(_availableApplicationPrefabs, _applicationPrefabNames);
            RefreshPrefabs(_availableSessionPrefabs, _sessionPrefabNames);
            RefreshPrefabs(_availableGamePrefabs, _gamePrefabNames);
            
            UpdateSelectedIndices();
        }

        private void UpdateSelectedIndices()
        {
            _selectedApplicationPrefabIndex = GetSelectedIndex(_availableApplicationPrefabs, _applicationScopePrefabProperty);
            _selectedSessionPrefabIndex = GetSelectedIndex(_availableSessionPrefabs, _sessionLifetimeScopeProperty);
            _selectedGamePrefabIndex = GetSelectedIndex(_availableGamePrefabs, _gameLifetimeScopeProperty);
        }

        private void AutoAssignIfEmpty()
        {
            bool modified = false;

            if (_applicationScopePrefabProperty.objectReferenceValue == null && _availableApplicationPrefabs.Count > 0)
            {
                _applicationScopePrefabProperty.objectReferenceValue = _availableApplicationPrefabs[0];
                _selectedApplicationPrefabIndex = 0;
                modified = true;
            }

            if (_sessionLifetimeScopeProperty.objectReferenceValue == null && _availableSessionPrefabs.Count > 0)
            {
                _sessionLifetimeScopeProperty.objectReferenceValue = _availableSessionPrefabs[0];
                _selectedSessionPrefabIndex = 0;
                modified = true;
            }

            if (_gameLifetimeScopeProperty.objectReferenceValue == null && _availableGamePrefabs.Count > 0)
            {
                _gameLifetimeScopeProperty.objectReferenceValue = _availableGamePrefabs[0];
                _selectedGamePrefabIndex = 0;
                modified = true;
            }

            if (modified)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void CreateApplicationScopePrefab()
        {
            CreateScopePrefab<ApplicationLifetimeScope>(ObjectNames.NicifyVariableName(nameof(ApplicationLifetimeScope)), _applicationScopePrefabProperty);
            _selectedApplicationPrefabIndex = GetSelectedIndex(_availableApplicationPrefabs, _applicationScopePrefabProperty);
        }

        private void CreateSessionScopePrefab()
        {
            CreateScopePrefab<SessionLifetimeScope>(ObjectNames.NicifyVariableName(nameof(SessionLifetimeScope)), _sessionLifetimeScopeProperty);
            _selectedSessionPrefabIndex = GetSelectedIndex(_availableSessionPrefabs, _sessionLifetimeScopeProperty);
        }

        private void CreateGameScopePrefab()
        {
            CreateScopePrefab<GameLifetimeScope>(ObjectNames.NicifyVariableName(nameof(GameLifetimeScope)), _gameLifetimeScopeProperty);
            _selectedGamePrefabIndex = GetSelectedIndex(_availableGamePrefabs, _gameLifetimeScopeProperty);
        }

        private void CreateScopePrefab<T>(string prefabName, SerializedProperty property) where T : Component
        {
            var tempGameObject = new GameObject(typeof(T).Name, typeof(T));
            
            var prefabAsset = SaveAsPrefab(prefabName, tempGameObject);
            
            DestroyImmediate(tempGameObject);
            
            RefreshAvailablePrefabs();
            
            serializedObject.Update();
            property.objectReferenceValue = prefabAsset.GetComponent<T>();
            serializedObject.ApplyModifiedProperties();
            
            Selection.activeObject = prefabAsset;
            EditorGUIUtility.PingObject(prefabAsset);
        }

        private static int GetSelectedIndex<T>(List<T> availablePrefabs, SerializedProperty property) where T : Component
        {
            var currentPrefab = property.objectReferenceValue as T;

            int index;

            if (currentPrefab != null)
            {
                index = availablePrefabs.IndexOf(currentPrefab);

                if (index < 0)
                {
                    index = 0;
                }
            }
            else
            {
                index = -1;
            }

            return index;
        }

        private static GameObject SaveAsPrefab(string prefabName, GameObject gameObject)
        {
            var directory = AssetDatabase.GetAssetPath(Selection.activeInstanceID);

            if (!Directory.Exists(directory))
            {
                if (File.Exists(directory))
                {
                    directory = Path.GetDirectoryName(directory);
                }
                
                if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                {
                    directory = Path.Combine("Assets", "LifetimeScopes");
                    
                    if (!AssetDatabase.IsValidFolder(directory))
                    {
                        AssetDatabase.CreateFolder("Assets", "LifetimeScopes");
                        AssetDatabase.Refresh();
                    }
                }
            }
            
            var path = Path.Combine(directory, $"{prefabName}.prefab");
            
            var prefabPath = AssetDatabase.GenerateUniqueAssetPath(path);
            
            return PrefabUtility.SaveAsPrefabAsset(gameObject, prefabPath);
        }

        private static void RefreshPrefabs<T>(List<T> availablePrefabs, List<string> displayNames) where T : Component
        {
            availablePrefabs.Clear();
            var prefabPaths = new List<string>();

            string[] guids = AssetDatabase.FindAssets("a:Assets t:Prefab");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null)
                {
                    continue;
                }

                var scope = prefab.GetComponent<T>();

                if (scope == null)
                {
                    continue;
                }

                availablePrefabs.Add(scope);
                prefabPaths.Add(path);
            }

            // Check if prefabs are in different directories
            var uniqueDirectories = prefabPaths
                .Select(Path.GetDirectoryName)
                .Distinct()
                .Count();

            displayNames.Clear();

            for (int i = 0; i < availablePrefabs.Count; i++)
            {
                string prefabName = availablePrefabs[i].name;

                if (uniqueDirectories > 1)
                {
                    string directoryPath = Path.GetDirectoryName(prefabPaths[i]);
                    string directoryName = Path.GetFileName(directoryPath);

                    if (string.IsNullOrEmpty(directoryName) || directoryName == "Assets")
                    {
                        displayNames.Add($"Assets / {prefabName}");
                    }
                    else
                    {
                        displayNames.Add($"{directoryName} / {prefabName}");
                    }
                }
                else
                {
                    displayNames.Add(prefabName);
                }
            }
        }
    }
}
#endif