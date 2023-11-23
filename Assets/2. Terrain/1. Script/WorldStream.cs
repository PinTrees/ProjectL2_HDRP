using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;


#if UNITY_EDITOR
#endif


public class WorldStream : MonoBehaviour
{
    public float BaseDistanceLODO = 100;
    public float BaseDistanceImpostor = 500;

    public float UnLoadDistancePercent = 0.3f;

    [SerializeField]
    [HideInInspector]
    List<BiomeSpawner> biomes = new();

    Transform player;

    public int BiomeCount { get => biomes.Count; }

    void Start()
    {
        player = PlayerManager.Instance.Player.transform;
        InitBiomes();
    }

    public void InitBiomes()
    { 
        //biomes = biomeParentTransform.GetComponentsInChildren<BiomeSpawner>().ToList();
        biomes = GameObject.FindObjectsOfType<BiomeSpawner>().ToList();
    }

    void FixedUpdate()
    {
        var current_position = player.position;

        biomes.ForEach(biome =>
        {
            var distance = Vector3.Distance(current_position, biome.transform.position);
            if (!biome.IsLoadSceneLod0 && biome.IsExported && distance < BaseDistanceLODO)
            {
                biome.IsLoadSceneLod0 = true;
                SceneManagerExtensions.LoadSceneAsync_Addressables(biome.AssetPath_StreamSceneLocal + biome.ExportDataId + ".unity"
                    , (AsyncOperationHandle<SceneInstance> instane) =>
                    {
                        biome.scene_lod0_instance = instane.Result;
                    });

                if (biome.IsLoadSceneImpostor)
                {
                    biome.IsLoadSceneImpostor = false;
                    if (biome.scene_impostor_instance != null)
                        SceneManagerExtensions.UnloadSceneAsync_Addressables(biome.scene_impostor_instance.Value);

                    biome.scene_impostor_instance = null;
                }
            }

            if (!biome.IsLoadSceneImpostor && biome.IsExported
            && distance < BaseDistanceImpostor && distance > BaseDistanceLODO)
            {
                biome.IsLoadSceneImpostor = true;
                SceneManagerExtensions.LoadSceneAsync_Addressables(biome.AssetPath_StreamSceneLocal + biome.ExportDataId + "_impostor.unity"
                     , (AsyncOperationHandle<SceneInstance> instane) =>
                     {
                         biome.scene_impostor_instance = instane.Result;
                     });

                if (biome.IsLoadSceneLod0)
                {
                    biome.IsLoadSceneLod0 = false;
                    if (biome.scene_lod0_instance != null)
                        SceneManagerExtensions.UnloadSceneAsync_Addressables(biome.scene_lod0_instance.Value);

                    biome.scene_lod0_instance = null;
                }
            }

            if (biome.IsLoadSceneImpostor && distance > BaseDistanceImpostor * (1 + UnLoadDistancePercent))
            {
                biome.IsLoadSceneImpostor = false;
                if (biome.scene_impostor_instance != null)
                    SceneManagerExtensions.UnloadSceneAsync_Addressables(biome.scene_impostor_instance.Value);

                biome.scene_impostor_instance = null;
            }

            if (biome.IsLoadSceneLod0 && distance > BaseDistanceLODO * (1 + UnLoadDistancePercent))
            {
                biome.IsLoadSceneLod0 = false;
                if (biome.scene_lod0_instance != null)
                    SceneManagerExtensions.UnloadSceneAsync_Addressables(biome.scene_lod0_instance.Value);

                biome.scene_lod0_instance = null;
            }
        });
    }

    public void _Editor_UpdateWorldStream()
    {
        var cameras = SceneView.GetAllSceneCameras();
        if (cameras == null) return;
        if (cameras.Length < 1) return;

        var view = cameras.First();
        var current_position = view.transform.position;

        biomes.ForEach(biome =>
        {
            var distance = Vector3.Distance(current_position, biome.transform.position);
            if(!biome.IsLoadSceneLod0_Editor && biome.IsExported && distance < BaseDistanceLODO)
            {
                biome.IsLoadSceneLod0_Editor = true;
                biome.scene_lod0 = SceneManagerExtensions._Editor_LoadScene(biome.AssetPath_StreamSceneLocal + biome.ExportDataId + ".unity");

                if (biome.IsLoadSceneImpostor_Editor)
                {
                    biome.IsLoadSceneImpostor_Editor = false;
                    if (biome.scene_impostor != null)
                        SceneManagerExtensions._Editor_UnloadScene(biome.scene_impostor.Value);
                    biome.scene_impostor = null;
                }
            }

            if(!biome.IsLoadSceneImpostor_Editor && biome.IsExported 
            && distance < BaseDistanceImpostor && distance > BaseDistanceLODO)
            {
                biome.IsLoadSceneImpostor_Editor = true;
                biome.scene_impostor = SceneManagerExtensions._Editor_LoadScene(biome.AssetPath_StreamSceneLocal + biome.ExportDataId + "_impostor.unity");

                if (biome.IsLoadSceneLod0_Editor)
                {
                    biome.IsLoadSceneLod0_Editor = false;
                    if (biome.scene_lod0 != null)
                        SceneManagerExtensions._Editor_UnloadScene(biome.scene_lod0.Value);
                    biome.scene_lod0 = null;
                }
            }

            if(biome.IsLoadSceneImpostor_Editor && distance > BaseDistanceImpostor * (1 + UnLoadDistancePercent))
            {
                biome.IsLoadSceneImpostor_Editor = false;
                if (biome.scene_impostor != null)
                    SceneManagerExtensions._Editor_UnloadScene(biome.scene_impostor.Value);
                biome.scene_impostor = null;
            }

            if (biome.IsLoadSceneLod0_Editor && distance > BaseDistanceLODO * (1 + UnLoadDistancePercent))
            {
                biome.IsLoadSceneLod0_Editor = false;
                if (biome.scene_lod0 != null)
                    SceneManagerExtensions._Editor_UnloadScene(biome.scene_lod0.Value);
                biome.scene_lod0 = null;
            }
        });
    }

    public void _Editor_ExitWorldStream()
    {
        biomes.ForEach(e => 
        {
            if (e.scene_lod0 != null)
                SceneManagerExtensions._Editor_UnloadScene(e.scene_lod0.Value);
            if (e.scene_impostor != null)
                SceneManagerExtensions._Editor_UnloadScene(e.scene_impostor.Value);

            e.IsLoadSceneLod0_Editor = false;
            e.IsLoadSceneImpostor_Editor = false;
        });
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(WorldStream))]
public class WorldStreamEditor : Editor
{
    WorldStream owner;

    private void OnEnable()
    {
        owner = target as WorldStream;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();

        GUILayout.Space(10);

        GUILayout.Label($"Biome Count ({owner.BiomeCount})");
        if (GUILayout.Button("Auto Fine Biomes"))
        {
            owner.InitBiomes();
        }
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Start Stream (Editor)"))
        {
            Debug.Log("[World Stream] Start In Editor");
            EditorApplication.update = () =>
            {
                owner._Editor_UpdateWorldStream();
            };
        }
        if (GUILayout.Button("Stop Stream (Editor)"))
        {
            owner._Editor_ExitWorldStream();
            Debug.Log("[World Stream] Exit In Editor");
            EditorApplication.update = null;
        }
        GUILayout.EndHorizontal();

        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
#endif
