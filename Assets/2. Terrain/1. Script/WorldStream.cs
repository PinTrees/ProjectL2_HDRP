using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using Unity.EditorCoroutines.Editor;
#endif


public class _Editor_StreamScene
{
    public string code;
    public Scene? scene;
}

public class WorldStream : MonoBehaviour
{
    public GameObject aa;

    public float BaseDistanceLODO = 100;
    public float BaseDistanceStatic = 500;
    public float BaseDistanceImpostor = 1000;
    public float UnLoadDistancePercent = 0.3f;

    public bool load_background = true;

    public static int BaseChunkCount = 2;
    public static int ImpostorChunkCount = 9;

    [SerializeField] List<Biome> biomeChunkRoots = new List<Biome>();
    public Dictionary<string, bool> ChunksExportExist = new();
    Dictionary<string, bool> scene_lod0_load = new();
    Dictionary<string, bool> scene_impostor_load = new();
    Dictionary<string, SceneInstance?> scene_impostor = new();
    Dictionary<string, SceneInstance?> scene_lod0 = new();

    // Editor Use
#if UNITY_EDITOR
    [Header("Editor")]
    [SerializeField] List<string> _editor_chunk_export_exist = new();
    [SerializeField] List<string> _editor_scene_lod0_load = new();
    [SerializeField] List<string> _editor_scene_impostor_load = new();
    [SerializeField] List<_Editor_StreamScene> _editor_scene_lod0 = new();
    [SerializeField] List<_Editor_StreamScene> _editor_scene_impostor = new();
    [SerializeField] EditorCoroutine _editor_stream;
#endif

    [SerializeField][HideInInspector] Transform streamTarget;

    void Start()
    {
        Init();
        StartCoroutine(ChunkStream());
    }

    public void Init()
    {
        DynamicResolutionHandler.SetDynamicResScaler(() => 80, DynamicResScalePolicyType.ReturnsPercentage);

        scene_lod0_load.Clear();
        scene_impostor_load.Clear();
        scene_lod0.Clear();
        scene_impostor.Clear();

        streamTarget = PlayerManager.Instance.Player.transform;
        biomeChunkRoots = GameObject.FindObjectsOfType<Biome>().ToList();
        biomeChunkRoots.ForEach(e => e.Init());
        foreach (var k in ChunksExportExist.Keys)
        {
            scene_lod0_load[k] = false;
            scene_impostor_load[k] = false;
            scene_lod0[k] = null;
            scene_impostor[k] = null;
        }
    }

    // 전투가 진행중일 경우 임포스터 청크 로드 금지 - 프레임 드랍 방지
    // 가까운 거리의 청크만 로드
    IEnumerator ChunkStream()
    {
        Application.backgroundLoadingPriority = load_background ? ThreadPriority.Low : ThreadPriority.High;

        while (true)
        {
            for (int x = -ImpostorChunkCount; x < ImpostorChunkCount; ++x)
            {
                for (int z = -ImpostorChunkCount; z < ImpostorChunkCount; ++z)
                {
                    if(Mathf.Abs(x) <= BaseChunkCount && Mathf.Abs(z) <= BaseChunkCount)
                    {
                        var currentpos = streamTarget.position + new Vector3(x, 0, z) * Biome.StreamChunkSize;
                        string chunk_key = Biome.GetChunkExportId(currentpos);

                        if (ChunksExportExist.ContainsKey(chunk_key)
                            && ChunksExportExist[chunk_key] && !scene_lod0_load[chunk_key] && scene_lod0[chunk_key] == null)
                        {
                            scene_lod0_load[chunk_key] = true;
                            yield return new WaitForSeconds(0.3f);
                            SceneManagerExtensions.LoadSceneAsync_Addressables(Biome.AssetPath_StreamSceneWorld
                                + chunk_key, (AsyncOperationHandle<SceneInstance> instane) =>
                                {
                                    scene_lod0[chunk_key] = instane.Result;
                                });

                            // 씬 로드까지 기다린후 제거
                            if (scene_impostor_load[chunk_key] && scene_impostor[chunk_key] != null)
                            {
                                scene_impostor_load[chunk_key] = false;
                                yield return new WaitForSeconds(0.3f);
                                SceneManagerExtensions.UnloadSceneAsync_Addressables(scene_impostor[chunk_key].Value, (AsyncOperationHandle<SceneInstance> instane) =>
                                {
                                    scene_impostor[chunk_key] = null;
                                });
                            }
                        }
                    }
                    else
                    {
                        var currentpos = streamTarget.position + new Vector3(x, 0, z) * Biome.StreamChunkSize;
                        string chunk_key = Biome.GetChunkExportId(currentpos);

                        if (ChunksExportExist.ContainsKey(chunk_key)
                            && ChunksExportExist[chunk_key] && !scene_impostor_load[chunk_key] && scene_impostor[chunk_key] == null)
                        {
                            scene_impostor_load[chunk_key] = true;
                            yield return new WaitForSeconds(0.3f);
                            SceneManagerExtensions.LoadSceneAsync_Addressables(Biome.AssetPath_StreamSceneWorld_Impostor
                                + chunk_key.Split(".").First() + "_impostor.unity", (AsyncOperationHandle<SceneInstance> instane) =>
                                {
                                    scene_impostor[chunk_key] = instane.Result;
                                });
                            
                            // 씬 로드까지 기다린 후 제거
                            if (scene_lod0_load[chunk_key] && scene_lod0[chunk_key] != null)
                            {
                                scene_lod0_load[chunk_key] = false;
                                yield return new WaitForSeconds(0.3f);
                                SceneManagerExtensions.UnloadSceneAsync_Addressables(scene_lod0[chunk_key].Value, (AsyncOperationHandle<SceneInstance> instane) =>
                                {
                                    scene_lod0[chunk_key] = null;
                                });
                            }
                        }
                    }
                }
            }

            yield return new WaitForSeconds(0.3f);
        }
    }

#if UNITY_EDITOR
    IEnumerator _Editor_ChunkStream()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);

            var cameras = SceneView.GetAllSceneCameras();
            if (cameras == null) continue;
            if (cameras.Length < 1) continue;

            var view = cameras.First();
            var current_position = view.transform.position;

            for (int x = -ImpostorChunkCount; x < ImpostorChunkCount; ++x)
            {
                for (int z = -ImpostorChunkCount; z < ImpostorChunkCount; ++z)
                {
                    if (Mathf.Abs(x) <= BaseChunkCount && Mathf.Abs(z) <= BaseChunkCount)
                    {
                        var currentpos = current_position + new Vector3(x, 0, z) * Biome.StreamChunkSize;
                        string chunk_key = Biome.GetChunkExportId(currentpos);

                        if (_editor_chunk_export_exist.Contains(chunk_key) && !_editor_scene_lod0_load.Contains(chunk_key))
                        {
                            _editor_scene_lod0_load.Add(chunk_key);
                            var scene = SceneManagerExtensions._Editor_LoadScene(Biome.AssetPath_StreamSceneWorld + chunk_key);

                            var stream = new _Editor_StreamScene();
                            stream.code = chunk_key;
                            stream.scene = scene;
                            _editor_scene_lod0.Add(stream);

                            if (_editor_scene_impostor_load.Contains(chunk_key))
                            {
                                _editor_scene_impostor_load.Remove(chunk_key);
                                var container = _editor_scene_impostor.Where(e => e.code == chunk_key).ToList();
                                container.ForEach(e => {
                                    if (e != null) SceneManagerExtensions._Editor_UnloadScene(e.scene.Value);
                                    _editor_scene_impostor.Remove(e);
                                });
                            }
                        }
                    }
                    else
                    {
                        var currentpos = current_position + new Vector3(x, 0, z) * Biome.StreamChunkSize;
                        string chunk_key = Biome.GetChunkExportId(currentpos);

                        if (_editor_chunk_export_exist.Contains(chunk_key) &&  !_editor_scene_impostor_load.Contains(chunk_key))
                        {
                            _editor_scene_impostor_load.Add(chunk_key);
                            var scene = SceneManagerExtensions._Editor_LoadScene(Biome.AssetPath_StreamSceneWorld_Impostor + chunk_key.Split(".").First() + "_impostor.unity");
                         
                            var stream = new _Editor_StreamScene();
                            stream.code = chunk_key;
                            stream.scene = scene;
                            _editor_scene_impostor.Add(stream);

                            if (_editor_scene_lod0_load.Contains(chunk_key))
                            {
                                _editor_scene_lod0_load.Remove(chunk_key);
                                var container = _editor_scene_lod0.Where(e => e.code == chunk_key).ToList();
                                container.ForEach(e => {
                                    if (e != null) SceneManagerExtensions._Editor_UnloadScene(e.scene.Value);
                                    _editor_scene_lod0.Remove(e);
                                });
                            }
                        }
                    }
                }
            }
        }
    }
 
    public void _Editor_UpdateWorldStream()
    {
        _Editor_ExitWorldStream();

        streamTarget = GameObject.FindObjectOfType<Player>().transform;
        GameObject.FindObjectsOfType<Biome>().ToList().ForEach(e => e.Init());

        _editor_chunk_export_exist.Clear();
        foreach (var k in ChunksExportExist.Keys) _editor_chunk_export_exist.Add(k);

        _editor_stream = EditorCoroutineUtility.StartCoroutine(_Editor_ChunkStream(), this);
    }

    public void _Editor_ExitWorldStream()
    {
        if(_editor_stream != null)
            EditorCoroutineUtility.StopCoroutine(_editor_stream);

        _editor_scene_lod0_load.Clear();
        _editor_scene_impostor_load.Clear();

        _editor_scene_lod0.ForEach(e => { if (e.scene != null) SceneManagerExtensions._Editor_UnloadScene(e.scene.Value); });
        _editor_scene_impostor.ForEach(e => { if (e.scene != null) SceneManagerExtensions._Editor_UnloadScene(e.scene.Value); });
        _editor_scene_lod0.Clear();
        _editor_scene_impostor.Clear();
    }
#endif
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

        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Start Stream (Editor)"))
        {
            Debug.Log("[World Stream] Start In Editor");
            owner._Editor_UpdateWorldStream();
            //EditorApplication.update = () => {};
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
