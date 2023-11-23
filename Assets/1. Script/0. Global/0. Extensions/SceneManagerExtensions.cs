using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Linq;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
using Unity.VisualScripting;






public class SceneManagerExtensions
{
    public static Scene CreateSceneWithGameObject_Editor(List<GameObject> objects, string fullpath)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        EditorSceneManager.SetActiveScene(scene);

        foreach (var go in objects)
        {
            var prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(go);
            var spawn = GameObjectExtensions.InstantiatePrefab(prefab, null);
            TransformExtentions.Overwrite(spawn, go.transform);
            EditorSceneManager.MoveGameObjectToScene(spawn, scene);
        }

        EditorSceneManager.SaveScene(scene, fullpath);
        EditorSceneManager.CloseScene(scene, true);
        return scene;
    }

    public static Scene CreateSceneWithTerrainObject_Local_Editor(List<GameObject> objects, string path, string uid, bool export_impostor=false)
    {
        var fullpath = $"{path}{uid}.unity";
      
        try
        {
            Scene scene = EditorSceneManager.OpenScene(fullpath, OpenSceneMode.Additive);

            var games = scene.GetRootGameObjects().ToList();
            games.ForEach(s =>
            {
                if (s != null)
                {
                    GameObject.DestroyImmediate(s.gameObject);
                }
            });

            foreach (var go in objects)
            {
                var prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(go);
                var spawn = GameObjectExtensions.InstantiatePrefab(prefab, null);

                TransformExtentions.Overwrite(spawn, go.transform);
                EditorSceneManager.MoveGameObjectToScene(spawn, scene);
            }

            EditorSceneManager.SaveScene(scene, fullpath);
            EditorSceneManager.CloseScene(scene, true);

            AddresableExtentions.SetActiveAddresableFile(path, uid + ".unity");
            SceneManagerExtensions._Editor_BuildSceneAppend(fullpath);
        }
        catch (Exception ex)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            EditorSceneManager.SetActiveScene(scene);

            foreach (var go in objects)
            {
                var prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(go);
                var spawn = GameObjectExtensions.InstantiatePrefab(prefab, null);

                TransformExtentions.Overwrite(spawn, go.transform);
                EditorSceneManager.MoveGameObjectToScene(spawn, scene);
            }

            EditorSceneManager.SaveScene(scene, fullpath);
            EditorSceneManager.CloseScene(scene, true);

            AddresableExtentions.SetActiveAddresableFile(path, uid + ".unity");
            SceneManagerExtensions._Editor_BuildSceneAppend(fullpath);
        }

        if(export_impostor)
        {
            fullpath = $"{path}{uid}_impostor.unity";
            try
            {
                Scene scene = EditorSceneManager.OpenScene(fullpath, OpenSceneMode.Additive);

                var games = scene.GetRootGameObjects().ToList();
                games.ForEach(s =>
                {
                    if (s != null)
                    {
                        GameObject.DestroyImmediate(s.gameObject);
                    }
                });

                foreach (var go in objects)
                {
                    var prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(go);
                    var spawn = GameObjectExtensions.InstantiateLastLOD(prefab);

                    if (spawn == null) continue;

                    StaticEditorFlags staticFlags = StaticEditorFlags.ContributeGI | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic 
                        | /*StaticEditorFlags.BatchingStatic StaticEditorFlags.BatchingStatic*/ StaticEditorFlags.ReflectionProbeStatic;

                    GameObjectUtility.SetStaticEditorFlags(spawn, staticFlags);

                    TransformExtentions.Overwrite(spawn, go.transform);
                    EditorSceneManager.MoveGameObjectToScene(spawn, scene);
                }

                EditorSceneManager.SaveScene(scene, fullpath);
                EditorSceneManager.CloseScene(scene, true);

                AddresableExtentions.SetActiveAddresableFile(path, uid + "_impostor.unity");
                SceneManagerExtensions._Editor_BuildSceneAppend(fullpath);
            }
            catch (Exception ex)
            {
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                EditorSceneManager.SetActiveScene(scene);

                foreach (var go in objects)
                {
                    var prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(go);
                    var spawn = GameObjectExtensions.InstantiateLastLOD(prefab);

                    if (spawn == null) continue;

                    StaticEditorFlags staticFlags = StaticEditorFlags.ContributeGI | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.ReflectionProbeStatic;
                    GameObjectUtility.SetStaticEditorFlags(spawn, staticFlags);

                    TransformExtentions.Overwrite(spawn, go.transform);
                    EditorSceneManager.MoveGameObjectToScene(spawn, scene);
                }

                EditorSceneManager.SaveScene(scene, fullpath);
                EditorSceneManager.CloseScene(scene, true);

                AddresableExtentions.SetActiveAddresableFile(path, uid + "_impostor.unity");
                SceneManagerExtensions._Editor_BuildSceneAppend(fullpath);
            }
        }

        return new Scene();
    }
    public static Scene CreateSceneWithTerrainObject_World_Editor(List<GameObject> objects, string fullpath, string rootUid)
    {
        Dictionary<Vector2Int, List<GameObject>> chunkObjects = new();

        objects.ForEach(e =>
        {
            var x = (int)(e.transform.position.x / 250);
            var z = (int)(e.transform.position.z / 250);
            var key = new Vector2Int(x, z);

            if(chunkObjects.ContainsKey(key))
            {
                chunkObjects[key].Add(e);
            }
            else
            {
                chunkObjects[key] = new List<GameObject> { e };
            }
        });

        foreach(var key in chunkObjects.Keys)
        {
            var gameObjects = chunkObjects[key];
            var scene_file_path = fullpath + $"chunk_{key.x.ToString()}_{key.y.ToString()}.unity";

            try
            {
                Scene scene = EditorSceneManager.OpenScene(scene_file_path, OpenSceneMode.Additive);

                var games = scene.GetRootGameObjects().ToList();
                games.ForEach(s =>
                {
                    if (s != null && s.name == rootUid)
                    {
                        GameObject.DestroyImmediate(s.gameObject);
                    }
                });

                var rootGameObject = new GameObject();
                rootGameObject.transform.position = Vector3.zero;
                rootGameObject.name = rootUid;

                foreach (var go in gameObjects)
                {
                    var prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(go);
                    var spawn = GameObjectExtensions.InstantiatePrefab(prefab, rootGameObject.transform);
                    TransformExtentions.Overwrite(spawn, go.transform);
                }

                EditorSceneManager.MoveGameObjectToScene(rootGameObject, scene);
                EditorSceneManager.SaveScene(scene, scene_file_path);
                EditorSceneManager.CloseScene(scene, true);
            }
            catch (Exception ex)
            {
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                EditorSceneManager.SetActiveScene(scene);

                var rootGameObject = new GameObject();
                rootGameObject.transform.position = Vector3.zero;
                rootGameObject.name = rootUid;

                foreach (var go in objects)
                {
                    var prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(go);
                    var spawn = GameObjectExtensions.InstantiatePrefab(prefab, rootGameObject.transform);
                    TransformExtentions.Overwrite(spawn, go.transform);
                }

                EditorSceneManager.MoveGameObjectToScene(rootGameObject, scene);
                EditorSceneManager.SaveScene(scene, scene_file_path);
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        return new Scene();
    }

#if UNITY_EDITOR
    public static Scene _Editor_LoadScene(string name) { return EditorSceneManager.OpenScene(name, OpenSceneMode.Additive); }
    public static void _Editor_UnloadScene(Scene scene) { EditorSceneManager.CloseScene(scene, true); }
    public static void _Editor_BuildSceneAppend(string fullpath, bool enable=false)
    {
        EditorBuildSettingsScene ebss = new EditorBuildSettingsScene();
        ebss.path = fullpath;
        ebss.enabled = enable;

        var ebssArray = EditorBuildSettings.scenes.ToList();

        var data = ebssArray.Find(e => { return e.path == fullpath; });
        if(data != null)
        {
            Debug.Log($"[Build Setting] Exist Scene."); 
            return;
        }
        else
        {
            ebssArray.Add(ebss);
            EditorBuildSettings.scenes = ebssArray.ToArray();
        }

        Debug.Log($"[Build Setting] Appended Scene {fullpath}");
    }
    public static void _Editor_BuildSceneRemove(string fullpath, bool enable = false)
    {
        EditorBuildSettingsScene ebss = new EditorBuildSettingsScene();
        ebss.path = fullpath;
        ebss.enabled = enable;

        var ebssArray = EditorBuildSettings.scenes.ToList();
        var target = ebssArray.Find(e => { if (e.path == fullpath) return true; return false; });

        if(target != null)
        {
            ebssArray.Remove(target);
        }

        EditorBuildSettings.scenes = ebssArray.ToArray();

        Debug.Log($"[Build Setting] Remove Scene {fullpath}");
    }

#endif

    /// <summary>
    /// 확정
    /// 씬을 런타임 중에 비동기 로드후 현재 씬에 추가합니다.
    /// Addressable을 사용하여 유니티에서 자체관리되는 메모리 시스템을 사용합니다.
    /// </summary>
    /// <param name="fullpath"></param>
    /// <param name="complete"></param>
    public static void LoadSceneAsync_Addressables(string fullpath, Action<AsyncOperationHandle<SceneInstance>> complete = null)
    {
        var handle = Addressables.LoadSceneAsync(fullpath, LoadSceneMode.Additive, true);
        if (complete != null)
            handle.Completed += complete;
    }

    /// <summary>
    /// 테스트
    /// 런타임중 로드된 씬을 비동기 해제합니다.
    /// Addressable을 사용하여 유니티에서 자체관리되는 메모리 시스템을 사용합니다.
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="complete"></param>
    public static void UnloadSceneAsync_Addressables(SceneInstance? instance, Action<AsyncOperationHandle<SceneInstance>> complete = null)
    {
        if (instance == null)
            return;

        var handle = Addressables.UnloadSceneAsync(instance.Value, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
        if (complete != null)
            handle.Completed += complete;
    }
}
