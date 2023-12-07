using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets;
#endif


public class SceneManagerExtensions
{
#if UNITY_EDITOR
    public static Scene _Editor_CreateSceneWithGameObject(List<GameObject> objects, string fullpath)
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
    public static Scene _Editor_CreateSceneWithTerrainObject_Local(List<GameObject> objects, string path, string uid, bool export_impostor = false)
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

        if (export_impostor)
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
    public static void _Editor_CreateSceneWithTerrainObject_ByWorld_Original(BiomeChunkContainer container, string path)
    {
        var scene_file_path = path + container.export;

        try
        {
            Scene scene = EditorSceneManager.OpenScene(scene_file_path, OpenSceneMode.Additive);

            var games = scene.GetRootGameObjects().ToList();
            games.ForEach(s =>
            {
                if (s != null && s.name == container.export)
                    GameObject.DestroyImmediate(s.gameObject);
            });

            var rootGameObject = new GameObject(container.export);
            rootGameObject.transform.position = Vector3.zero;

            foreach (var go in container.objects)
            {
                var prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(go.Object);
                var spawn = GameObjectExtensions.InstantiatePrefab(prefab, rootGameObject.transform);

                StaticEditorFlags staticFlags = 0; //StaticEditorFlags.ContributeGI | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.ReflectionProbeStatic;
                GameObjectUtility.SetStaticEditorFlags(spawn, staticFlags);

                TransformExtentions.Overwrite(spawn, go.Object.transform);
            }

            //var processing = PrefabUtility.GetCorrespondingObjectFromOriginalSource(GameObject.FindObjectOfType<WorldStream>().aa);
            //var processing_spawn = GameObjectExtensions.InstantiatePrefab(processing, rootGameObject.transform);

            EditorSceneManager.MoveGameObjectToScene(rootGameObject, scene);
            EditorSceneManager.SaveScene(scene, scene_file_path);
            EditorSceneManager.CloseScene(scene, true);

            AddresableExtentions.SetActiveAddresableFile(path, container.export);
            SceneManagerExtensions._Editor_BuildSceneAppend(scene_file_path);
        }
        catch (Exception ex)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            EditorSceneManager.SetActiveScene(scene);

            var rootGameObject = new GameObject(container.export);
            rootGameObject.transform.position = Vector3.zero;

            foreach (var go in container.objects)
            {
                var prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(go.Object);
                var spawn = GameObjectExtensions.InstantiatePrefab(prefab, rootGameObject.transform);

                StaticEditorFlags staticFlags = 0;
                GameObjectUtility.SetStaticEditorFlags(spawn, staticFlags);

                TransformExtentions.Overwrite(spawn, go.Object.transform);
            }

            //var processing = PrefabUtility.GetCorrespondingObjectFromOriginalSource(GameObject.FindObjectOfType<WorldStream>().aa);
            //var processing_spawn = GameObjectExtensions.InstantiatePrefab(processing, rootGameObject.transform);

            EditorSceneManager.MoveGameObjectToScene(rootGameObject, scene);
            EditorSceneManager.SaveScene(scene, scene_file_path);
            EditorSceneManager.CloseScene(scene, true);

            AddresableExtentions.SetActiveAddresableFile(path, container.export);
            SceneManagerExtensions._Editor_BuildSceneAppend(scene_file_path);
        }
    }
    public static void _Editor_CreateSceneWithTerrainObject_ByWorld_LOD1(BiomeChunkContainer container, string path)
    {
        var scene_file_path = path + container.export.Split(".").First() + "_lod1.unity";

        try
        {
            Scene scene = EditorSceneManager.OpenScene(scene_file_path, OpenSceneMode.Additive);

            var games = scene.GetRootGameObjects().ToList();
            games.ForEach(s =>
            {
                if (s != null && s.name == container.export)
                    GameObject.DestroyImmediate(s.gameObject);
            });

            var rootGameObject = new GameObject(container.export);
            rootGameObject.transform.position = Vector3.zero;

            foreach (var go in container.objects)
            {
                var prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(go.Object);
                var spawn = GameObjectExtensions.InstantiatePrefab(prefab, rootGameObject.transform);
                TransformExtentions.Overwrite(spawn, go.Object.transform);
            }

            EditorSceneManager.MoveGameObjectToScene(rootGameObject, scene);
            EditorSceneManager.SaveScene(scene, scene_file_path);
            EditorSceneManager.CloseScene(scene, true);

            AddresableExtentions.SetActiveAddresableFile(path, container.export);
            SceneManagerExtensions._Editor_BuildSceneAppend(scene_file_path);
        }
        catch (Exception ex)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            EditorSceneManager.SetActiveScene(scene);

            var rootGameObject = new GameObject(container.export);
            rootGameObject.transform.position = Vector3.zero;

            foreach (var go in container.objects)
            {
                var prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(go.Object);
                var spawn = GameObjectExtensions.InstantiatePrefab(prefab, rootGameObject.transform);
                TransformExtentions.Overwrite(spawn, go.Object.transform);
            }

            EditorSceneManager.MoveGameObjectToScene(rootGameObject, scene);
            EditorSceneManager.SaveScene(scene, scene_file_path);
            EditorSceneManager.CloseScene(scene, true);

            AddresableExtentions.SetActiveAddresableFile(path, container.export);
            SceneManagerExtensions._Editor_BuildSceneAppend(scene_file_path);
        }
    }
    public static void _Editor_CreateSceneWithTerrainObject_ByWorld_Impostor(BiomeChunkContainer container, string path)
    {
        var filename = container.export.Split(".").First() + "_impostor.unity";
        var scene_file_path = path + filename;

        try
        {
            Scene scene = EditorSceneManager.OpenScene(scene_file_path, OpenSceneMode.Additive);

            var games = scene.GetRootGameObjects().ToList();
            games.ForEach(s =>
            {
                if (s != null && s.name == container.export)
                    GameObject.DestroyImmediate(s.gameObject);
            });

            var rootGameObject = new GameObject(container.export);
            rootGameObject.transform.position = Vector3.zero;

            foreach (var go in container.objects)
            {
                // 임포스터 빌드시 잔디 제외
                if (go.data.Type == SPAWN_OBJECT_TYPE.GRASS) continue;

                var prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(go.Object);
                var spawn = GameObjectExtensions.InstantiateLastLOD(prefab, rootGameObject.transform);
                TransformExtentions.Overwrite(spawn, go.Object.transform);
            }

            EditorSceneManager.MoveGameObjectToScene(rootGameObject, scene);
            EditorSceneManager.SaveScene(scene, scene_file_path);
            EditorSceneManager.CloseScene(scene, true);

            AddresableExtentions.SetActiveAddresableFile(path, filename);
            SceneManagerExtensions._Editor_BuildSceneAppend(scene_file_path);
        }
        catch (Exception ex)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            EditorSceneManager.SetActiveScene(scene);

            var rootGameObject = new GameObject(container.export);
            rootGameObject.transform.position = Vector3.zero;

            foreach (var go in container.objects)
            {
                if (go.data.Type == SPAWN_OBJECT_TYPE.GRASS) continue;

                var prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(go.Object);
                var spawn = GameObjectExtensions.InstantiateLastLOD(prefab, rootGameObject.transform);
                TransformExtentions.Overwrite(spawn, go.Object.transform);
            }

            EditorSceneManager.MoveGameObjectToScene(rootGameObject, scene);
            EditorSceneManager.SaveScene(scene, scene_file_path);
            EditorSceneManager.CloseScene(scene, true);

            AddresableExtentions.SetActiveAddresableFile(path, filename);
            SceneManagerExtensions._Editor_BuildSceneAppend(scene_file_path);
        }
    }

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
