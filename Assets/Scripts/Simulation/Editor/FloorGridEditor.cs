using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RobotSimulation
{
    [CustomEditor(typeof(FloorGrid))]
    public class FloorGridEditor : Editor
    {
        SerializedProperty m_paths;
        SerializedProperty m_partition;
        SerializedProperty m_defaultLightness;

        void OnEnable()
        {
            m_paths = serializedObject.FindProperty("paths");
            m_partition = serializedObject.FindProperty("partition");
            m_defaultLightness = serializedObject.FindProperty("defaultLightness");
        }

        void CreateGridIfEmpty()
        {
            var path = target as FloorGrid;
            if (!path.partition)
            {
                var savePath = EditorUtility.SaveFilePanelInProject("Save Path Asset", "path", "asset", "");
                Debug.Log(savePath);
                if (string.IsNullOrEmpty(savePath))
                {
                    return;
                }

                if (File.Exists(savePath))
                {
                    path.partition = AssetDatabase.LoadAssetAtPath<MeshGrid>(savePath);
                    if (!path.partition)
                    {
                        Debug.LogError("please choose a different save path");
                        return;
                    }
                }

                if (!path.partition)
                {
                    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<MeshGrid>(), savePath);
                    path.partition = AssetDatabase.LoadAssetAtPath<MeshGrid>(savePath);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_paths, true);
            EditorGUILayout.PropertyField(m_defaultLightness);
            EditorGUILayout.PropertyField(m_partition);
            serializedObject.ApplyModifiedProperties();

            var partition = (target as FloorGrid).partition;
            if (!partition)
            {
                if (GUILayout.Button("Create"))
                {
                    CreateGridIfEmpty();
                }
                return;
            }

            int cellX = EditorGUILayout.IntField("x", partition.numCellX);
            partition.numCellX = Mathf.Max(cellX, 1);

            int cellY = EditorGUILayout.IntField("y", partition.numCellY);
            partition.numCellY = Mathf.Max(cellY, 1);

            var nonEmptyCells = partition.cells.Select(x => x.numTriangles).Where(x => x > 0).DefaultIfEmpty();
            int maxTri = nonEmptyCells.Max();
            int minTri = nonEmptyCells.Min();
            float avgTri = (float)nonEmptyCells.Average();

            EditorGUILayout.LabelField("Max Triangles: " + maxTri);
            EditorGUILayout.LabelField("Min Triangles: " + minTri);
            EditorGUILayout.LabelField("Avg Triangles: " + avgTri);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Build"))
            {
                Build();
            }
            if (GUILayout.Button("Add Selected Paths"))
            {
                AddSelectedPaths();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void AddSelectedPaths()
        {
            var grid = target as FloorGrid;
            var paths = new List<FloorGrid.PathInfo>(grid.paths ?? new FloorGrid.PathInfo[0]);
            foreach (var obj in Selection.GetFiltered<MeshFilter>(SelectionMode.Editable | SelectionMode.ExcludePrefab))
            {
                if (!paths.Any(x => x.meshObject == obj.gameObject))
                {
                    paths.Add(new FloorGrid.PathInfo {
                        meshObject = obj.gameObject
                    });
                }
            }
            grid.paths = paths.ToArray();
            // TODO: we assume selected objects and the grid are in the active scene
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private struct ModelSavedState
        {
            public ModelImporter importer;
            public bool readable;
        }

        private void Build()
        {
            var floor = (FloorGrid)target;
            if (floor.paths == null || floor.paths.Length == 0)
            {
                Debug.LogError("mesh object not specified", target);
                return;
            }

            CreateGridIfEmpty();
            if (!floor.partition)
            {
                return;
            }

            var meshObjects = floor.meshObjects.ToArray();
            var meshSavedStates = new Dictionary<string, ModelSavedState>();

            for (int i = 0; i < meshObjects.Length; ++i)
            {
                var filter = meshObjects[i].GetComponent<MeshFilter>();
                if (!filter)
                {
                    Debug.LogError("mesh not found", meshObjects[i]);
                    return;
                }

                var fbxPath = AssetDatabase.GetAssetPath(filter.sharedMesh);
                if (!string.IsNullOrEmpty(fbxPath) && !meshSavedStates.ContainsKey(fbxPath))
                {
                    var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
                    var state = new ModelSavedState();
                    state.importer = importer;
                    state.readable = importer.isReadable;
                    importer.isReadable = true;
                    importer.SaveAndReimport();
                    meshSavedStates.Add(fbxPath, state);
                }
                else if (!filter.sharedMesh.isReadable)
                {
                    Debug.LogError("mesh is not readable", meshObjects[i]);
                    return;
                }
            }

            floor.partition.Build(meshObjects);
            EditorUtility.SetDirty(floor.partition);

            // restore readable state
            foreach (var mesh in meshSavedStates.Values)
            {
                mesh.importer.isReadable = mesh.readable;
                mesh.importer.SaveAndReimport();
            }
        }

    }
}
