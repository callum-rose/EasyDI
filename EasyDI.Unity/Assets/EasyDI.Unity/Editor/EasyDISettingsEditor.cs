#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using EasyDI.Unity.LifetimeScopes;

namespace EasyDI.Unity.Editor
{
    [CustomEditor(typeof(EasyDISettings))]
    public class EasyDISettingsEditor : UnityEditor.Editor
    {
        private sealed class ScopePrefab
        {
            public ScopePrefab(LifetimeScope scope, string assetPath)
            {
                Scope = scope;
                AssetPath = assetPath;
                DisplayName = scope.name;
            }

            public LifetimeScope Scope { get; }
            public string AssetPath { get; }
            public string DisplayName { get; set; }
        }

        private SerializedProperty _rootLifetimeScopeProperty;
        private SerializedProperty _scopePrefabsProperty;

        private readonly List<ScopePrefab> _rootPrefabs = new();
        private readonly List<ScopePrefab> _childPrefabs = new();

        private int _selectedRootPrefabIndex;

        private static IReadOnlyList<Type> RootScopeTypes => NameHelper.ParentableLifetimeScopeTypes
            .Where(t => typeof(RootLifetimeScope).IsAssignableFrom(t))
            .OrderBy(t => t.Name)
            .ToArray();

        private static IReadOnlyList<Type> ChildScopeTypes => NameHelper.ParentableLifetimeScopeTypes
            .Where(t => !typeof(RootLifetimeScope).IsAssignableFrom(t))
            .OrderBy(t => t.Name)
            .ToArray();

        private void OnEnable()
        {
            _rootLifetimeScopeProperty = serializedObject.FindProperty(EasyDISettings.RootLifetimeScopePropertyName);
            _scopePrefabsProperty = serializedObject.FindProperty(EasyDISettings.ScopePrefabsPropertyName);

            RefreshPrefabs();
            AutoAssignRootIfEmpty();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawRootSection();

            EditorGUILayout.Space();

            DrawScopePrefabsSection();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawRootSection()
        {
            EditorGUILayout.LabelField(
                ObjectNames.NicifyVariableName(EasyDISettings.RootLifetimeScopePropertyName),
                EditorStyles.boldLabel);

            if (_rootPrefabs.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    $"No root scope prefabs found in the project. Create one from a script deriving from " +
                    $"{nameof(RootLifetimeScope)}; it is instantiated before the first scene loads.",
                    MessageType.Error);

                EditorGUILayout.BeginHorizontal();
                DrawRefreshButton();
                DrawNewRootPrefabButton();
                EditorGUILayout.EndHorizontal();

                return;
            }

            EditorGUILayout.BeginHorizontal();

            var newIndex = EditorGUILayout.Popup(
                _selectedRootPrefabIndex,
                _rootPrefabs.Select(p => p.DisplayName).ToArray());

            if (newIndex != _selectedRootPrefabIndex && newIndex >= 0 && newIndex < _rootPrefabs.Count)
            {
                _selectedRootPrefabIndex = newIndex;
                _rootLifetimeScopeProperty.objectReferenceValue = _rootPrefabs[newIndex].Scope;
            }

            DrawRefreshButton();
            DrawNewRootPrefabButton();

            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(_rootLifetimeScopeProperty, GUIContent.none);
            EditorGUI.EndDisabledGroup();

            DrawRootValidation();
        }

        private void DrawRootValidation()
        {
            var assignedRoot = _rootLifetimeScopeProperty.objectReferenceValue as LifetimeScope;

            if (assignedRoot == null)
            {
                EditorGUILayout.HelpBox(
                    "Choose the scope to instantiate before the first scene loads.",
                    MessageType.Error);
            }
            else if (_rootPrefabs.All(p => p.Scope != assignedRoot))
            {
                EditorGUILayout.HelpBox(
                    $"'{assignedRoot.name}' parents to another scope, so it can't be instantiated as the root. " +
                    $"Choose a scope deriving from {nameof(RootLifetimeScope)}.",
                    MessageType.Error);
            }
        }

        private void DrawScopePrefabsSection()
        {
            EditorGUILayout.LabelField(
                ObjectNames.NicifyVariableName(EasyDISettings.ScopePrefabsPropertyName),
                EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                $"Prefabs listed here can be looked up by type with {nameof(EasyDISettings)}.GetScopePrefab<T>().",
                MessageType.None);

            EditorGUILayout.PropertyField(_scopePrefabsProperty, GUIContent.none, true);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent(
                    "Auto-populate",
                    "Register every scope prefab in the project that isn't registered yet")))
            {
                AutoPopulateScopePrefabs();
            }

            DrawRefreshButton();

            EditorGUILayout.EndHorizontal();

            DrawScopePrefabsValidation();
            DrawCreateMissingPrefabButtons();
        }

        private void DrawScopePrefabsValidation()
        {
            var registeredScopes = new List<LifetimeScope>();
            var emptyEntryCount = 0;

            for (int i = 0; i < _scopePrefabsProperty.arraySize; i++)
            {
                if (_scopePrefabsProperty.GetArrayElementAtIndex(i).objectReferenceValue is LifetimeScope scope)
                {
                    registeredScopes.Add(scope);
                }
                else
                {
                    emptyEntryCount++;
                }
            }

            if (emptyEntryCount > 0)
            {
                EditorGUILayout.HelpBox(
                    $"{emptyEntryCount} entr{(emptyEntryCount == 1 ? "y is" : "ies are")} empty. Assign a prefab to " +
                    "them or remove them.",
                    MessageType.Error);
            }

            var duplicatedTypeNames = registeredScopes
                .GroupBy(s => s.GetType())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key.Name)
                .ToArray();

            if (duplicatedTypeNames.Length > 0)
            {
                EditorGUILayout.HelpBox(
                    $"More than one prefab is registered for: {string.Join(", ", duplicatedTypeNames)}. Looking a " +
                    "scope up by type can't choose between them.",
                    MessageType.Error);
            }

            var sceneScopeNames = registeredScopes
                .Where(s => s is SceneLifetimeScope)
                .Select(s => s.name)
                .ToArray();

            if (sceneScopeNames.Length > 0)
            {
                EditorGUILayout.HelpBox(
                    $"{string.Join(", ", sceneScopeNames)} {(sceneScopeNames.Length == 1 ? "is a" : "are")} " +
                    $"{nameof(SceneLifetimeScope)}{(sceneScopeNames.Length == 1 ? "" : "s")}, which belong in a scene " +
                    "rather than in this list.",
                    MessageType.Error);
            }

            if (_rootLifetimeScopeProperty.objectReferenceValue is LifetimeScope root &&
                registeredScopes.Any(s => s == root))
            {
                EditorGUILayout.HelpBox(
                    $"'{root.name}' is the root scope, which is always searched. Listing it here as well is redundant.",
                    MessageType.Warning);
            }
        }

        private void DrawCreateMissingPrefabButtons()
        {
            var typesWithPrefabs = _rootPrefabs
                .Concat(_childPrefabs)
                .Select(p => p.Scope.GetType())
                .ToHashSet();

            foreach (var scopeType in ChildScopeTypes.Where(t => !typesWithPrefabs.Contains(t)))
            {
                if (GUILayout.Button($"Create {ObjectNames.NicifyVariableName(scopeType.Name)} Prefab"))
                {
                    CreateScopePrefab(scopeType, null);
                }
            }
        }

        private void DrawRefreshButton()
        {
            if (GUILayout.Button(
                    new GUIContent(EditorGUIUtility.IconContent("d_Refresh").image, "Refresh prefabs list"),
                    GUILayout.Width(25),
                    GUILayout.Height(18)))
            {
                RefreshPrefabs();
            }
        }

        private void DrawNewRootPrefabButton()
        {
            var rootScopeTypes = RootScopeTypes;

            EditorGUI.BeginDisabledGroup(rootScopeTypes.Count == 0);

            if (GUILayout.Button(
                    new GUIContent(
                        "New",
                        rootScopeTypes.Count == 0
                            ? $"No {nameof(RootLifetimeScope)} scripts exist in the project"
                            : "Create a new root scope prefab"),
                    GUILayout.Width(40)))
            {
                if (rootScopeTypes.Count == 1)
                {
                    CreateScopePrefab(rootScopeTypes[0], _rootLifetimeScopeProperty);
                }
                else
                {
                    var menu = new GenericMenu();

                    foreach (var scopeType in rootScopeTypes)
                    {
                        var capturedScopeType = scopeType;

                        menu.AddItem(
                            new GUIContent(ObjectNames.NicifyVariableName(scopeType.Name)),
                            false,
                            () => CreateScopePrefab(capturedScopeType, _rootLifetimeScopeProperty));
                    }

                    menu.ShowAsContext();
                }
            }

            EditorGUI.EndDisabledGroup();
        }

        private void AutoPopulateScopePrefabs()
        {
            var root = _rootLifetimeScopeProperty.objectReferenceValue as LifetimeScope;

            var registeredScopes = Enumerable.Range(0, _scopePrefabsProperty.arraySize)
                .Select(i => _scopePrefabsProperty.GetArrayElementAtIndex(i).objectReferenceValue as LifetimeScope)
                .Where(s => s != null)
                .ToList();

            foreach (var candidate in _childPrefabs)
            {
                if (candidate.Scope == root ||
                    registeredScopes.Any(s => s == candidate.Scope || s.GetType() == candidate.Scope.GetType()))
                {
                    continue;
                }

                AppendScopePrefab(candidate.Scope);
                registeredScopes.Add(candidate.Scope);
            }
        }

        private void AppendScopePrefab(LifetimeScope scope)
        {
            _scopePrefabsProperty.arraySize++;
            _scopePrefabsProperty.GetArrayElementAtIndex(_scopePrefabsProperty.arraySize - 1).objectReferenceValue =
                scope;
        }

        private void AutoAssignRootIfEmpty()
        {
            if (_rootLifetimeScopeProperty.objectReferenceValue != null || _rootPrefabs.Count != 1)
            {
                return;
            }

            _rootLifetimeScopeProperty.objectReferenceValue = _rootPrefabs[0].Scope;
            _selectedRootPrefabIndex = 0;

            serializedObject.ApplyModifiedProperties();
        }

        private void CreateScopePrefab(Type scopeType, SerializedProperty propertyToAssign)
        {
            var tempGameObject = new GameObject(scopeType.Name, scopeType);

            var prefabAsset = SaveAsPrefab(ObjectNames.NicifyVariableName(scopeType.Name), tempGameObject);

            DestroyImmediate(tempGameObject);

            RefreshPrefabs();

            serializedObject.Update();

            var scope = (LifetimeScope)prefabAsset.GetComponent(scopeType);

            if (propertyToAssign != null)
            {
                propertyToAssign.objectReferenceValue = scope;
            }
            else
            {
                AppendScopePrefab(scope);
            }

            serializedObject.ApplyModifiedProperties();

            _selectedRootPrefabIndex = IndexOfAssignedRootPrefab();

            Selection.activeObject = prefabAsset;
            EditorGUIUtility.PingObject(prefabAsset);
        }

        private void RefreshPrefabs()
        {
            _rootPrefabs.Clear();
            _childPrefabs.Clear();

            foreach (var guid in AssetDatabase.FindAssets("a:Assets t:Prefab"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null)
                {
                    continue;
                }

                var scope = prefab.GetComponent<LifetimeScope>();

                // Scene scopes are placed in scenes and read their parent from the scene, so they belong in neither list.
                if (scope == null || scope is SceneLifetimeScope)
                {
                    continue;
                }

                (IsRootScope(scope) ? _rootPrefabs : _childPrefabs).Add(new ScopePrefab(scope, path));
            }

            SetDisplayNames();

            _selectedRootPrefabIndex = IndexOfAssignedRootPrefab();
        }

        private static bool IsRootScope(LifetimeScope scope)
        {
            try
            {
                return scope.GetParentScopeType() == null;
            }
            catch (Exception)
            {
                // A scope that can't report its parent can't be trusted to be a root one.
                return false;
            }
        }

        private void SetDisplayNames()
        {
            var allPrefabs = _rootPrefabs.Concat(_childPrefabs).ToArray();

            bool prefabsSpanDirectories = allPrefabs
                .Select(p => Path.GetDirectoryName(p.AssetPath))
                .Distinct()
                .Count() > 1;

            if (!prefabsSpanDirectories)
            {
                return;
            }

            foreach (var prefab in allPrefabs)
            {
                var directoryName = Path.GetFileName(Path.GetDirectoryName(prefab.AssetPath));

                prefab.DisplayName = string.IsNullOrEmpty(directoryName) || directoryName == "Assets"
                    ? $"Assets / {prefab.Scope.name}"
                    : $"{directoryName} / {prefab.Scope.name}";
            }
        }

        private int IndexOfAssignedRootPrefab()
        {
            var assignedRoot = _rootLifetimeScopeProperty.objectReferenceValue as LifetimeScope;

            return assignedRoot == null ? -1 : _rootPrefabs.FindIndex(p => p.Scope == assignedRoot);
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
    }
}
#endif
