using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Rendering.HighDefinition;


#if UNITY_EDITOR
using UnityEditor;
#endif


[System.Serializable]
public class BiomeBrushContainer
{
    public NatureSpawnData NatureData;
    public bool IsSelected = false;
}


[System.Serializable]
public class BiomeChunkContainer
{
    public string export;
    [SerializeField]
    public List<BiomeObject_Spwaned> objects = new();
}


[System.Serializable]
public class BiomeObject_Spwaned
{
    public BiomeData data;
    public GameObject Object;
}

// 해당 클래스는 매우 많은 양의 오브젝트를 생성할 수 있는 에디터 스크립트 입니다.
// 월드 식생 군계 오브젝트를 생성하며 최소 10만개에서 최대 100만개 이상의 오브젝트를 생성하고
// 별도의 파일로 저장후 월드 스트림에 추가합니다.

// 에디터 코드에서 모든 소환오브젝트가 관리되어야 합니다
// 다량의 오브젝트가 생성후 관리되지 못하면 매우 큰 문제가 발생할 수 있습니다.
// 코드 변경 또는 오류 확인시 신중히 수정하세요.
public class Biome : MonoBehaviour
{
    public static int BiomeSpawnAreaSise = 100;
    public static int StreamChunkSize = 100;

    [SerializeField][HideInInspector] public List<BiomeBrushContainer> biomeBrushs = new();
    [SerializeField] List<BiomeChunkContainer> spawnedBiomes = new();
    [SerializeField] List<string> exportChunk = new();
    bool[,] exportChunkIndex = new bool[40, 40]; 

    public static string AssetPath_StreamSceneWorld = $"Assets/Terrain/StreamScene_World/";
    public static string AssetPath_StreamSceneWorld_LOD1 = $"Assets/Terrain/StreamScene_World_Lod1/";
    public static string AssetPath_StreamSceneWorld_Impostor = $"Assets/Terrain/StreamScene_World_Impostor/";

    void Start()
    {
        
    }

    public void Init()
    {
        WorldStream worldstream = GameObject.FindObjectOfType<WorldStream>();
        exportChunk.ForEach(e => worldstream.ChunksExportExist[e] = true);
    }

    public static string GetChunkExportId(Vector3 pos)
    {
        var gridPos = GetNearChunkPosition(pos);
        return $"{(int)gridPos.x}_{(int)gridPos.z}.unity";
    }

    public static Vector3 GetNearChunkPosition(Vector3 pos)
    {
        return new Vector3(
              pos.x - pos.x % StreamChunkSize
            , pos.y
            , pos.z - pos.z % StreamChunkSize);
    }

#if UNITY_EDITOR
    public void _Editor_ExportChunkIndexInit()
    {
        exportChunk.ForEach(e =>
        {
            var x = int.Parse(e.Split(".").First().Split("_").First()) - (int)transform.position.x;
            var z = int.Parse(e.Split(".").First().Split("_").Last()) - (int)transform.position.x;
            exportChunkIndex[x != 0 ? x / StreamChunkSize : x
                , z != 0 ? z / StreamChunkSize : z] = true;
        });
    }
    public void _Editor_AddBiomeSpawnObject(BiomeData data, GameObject spawn)
    {
        var chunk_key = GetChunkExportId(spawn.transform.position);
        var containers = spawnedBiomes.Where(e => e.export == chunk_key);

        var spawned_data = new BiomeObject_Spwaned();
        spawned_data.data = data;
        spawned_data.Object = spawn;

        if (containers == null)
        {
            var container = new BiomeChunkContainer();
            container.export = chunk_key;
            container.objects.Add(spawned_data);
            spawnedBiomes.Add(container);
        }
        else if(containers.Count() < 1)
        {
            var container = new BiomeChunkContainer();
            container.export = chunk_key;
            container.objects.Add(spawned_data);
            spawnedBiomes.Add(container);
        }
        else
        {
            containers.First().objects.Add(spawned_data);
        }
    }

    public Vector3? _Editor_Gizemo_Viewport(bool draw=true)
    {
        TerrainCollider terrain = TerrainExtentionsX.GetNearTerrainCollider(transform.position + Vector3.one);

        var camera = EditorExtensions._Editor_GetSceneCamera();
        Vector3 rayOrigin = camera.transform.position; 

        RaycastHit hit;
        Ray ray = new Ray(rayOrigin, camera.transform.TransformDirection(Vector3.forward));

        if (terrain.Raycast(ray, out hit, Mathf.Infinity))
        {
            int gridscale = BiomeSpawnAreaSise / StreamChunkSize;
            int gridscale_half = Mathf.FloorToInt(gridscale * 0.5f);

            if(!draw) return hit.point;

            for (int i = -gridscale_half; i < Mathf.CeilToInt(gridscale * 0.5f); ++i)
            {
                for (int j = -gridscale_half; j < Mathf.CeilToInt(gridscale * 0.5f); ++j)
                {
                    Vector3 ray_grid_origin = GetNearChunkPosition(hit.point)
                        + new Vector3(StreamChunkSize * i, 1000, StreamChunkSize * j) 
                        + new Vector3(StreamChunkSize, 0, StreamChunkSize) * 0.5f;

                    RaycastHit hit_grid;
                    Ray ray_grid = new Ray(ray_grid_origin, Vector3.down);

                    if (terrain.Raycast(ray_grid, out hit_grid, Mathf.Infinity))
                    {
                        Gizmos.color = new Color(0, 0, 1, 0.3f); 
                        Gizmos.DrawCube(hit_grid.point, Vector3.one * StreamChunkSize);
                    }
                }
            }

            return hit.point;
        }

        return null;
    }

    public void _Editor_Spawn_Viewport()
    {
        // 현재 뷰포트 확인
        var spawn_center = _Editor_Gizemo_Viewport(draw: false);
        if (spawn_center == null) return;

        TerrainCollider terrain = TerrainExtentionsX.GetNearTerrainCollider(transform.position + Vector3.one);

        // 그리드 사이즈 확인
        int gridscale = BiomeSpawnAreaSise / StreamChunkSize;
        int gridscale_half = Mathf.FloorToInt(gridscale * 0.5f);

        for (int i = -gridscale_half; i < Mathf.CeilToInt(gridscale * 0.5f); ++i)
        {
            for (int j = -gridscale_half; j < Mathf.CeilToInt(gridscale * 0.5f); ++j)
            {
                // 청크 중앙 위치 
                Vector3 ray_grid_origin = GetNearChunkPosition(spawn_center.Value)
                + new Vector3(StreamChunkSize * i, 1000, StreamChunkSize * j)
                + new Vector3(StreamChunkSize, 0, StreamChunkSize) * 0.5f;

                RaycastHit hit_grid;
                Ray ray_grid = new Ray(ray_grid_origin, Vector3.down);

                // 식생 소환
                if (terrain.Raycast(ray_grid, out hit_grid, Mathf.Infinity))
                {
                    biomeBrushs.ForEach(n =>
                    {
                        if (!n.IsSelected) return;
                        n.NatureData._Editor_Spawn(this, hit_grid.point);
                    });
                }
            }
        }
    }

    public void _Editor_Export_SpawnObject()
    {
        if (!Directory.Exists(AssetPath_StreamSceneWorld)) Directory.CreateDirectory(AssetPath_StreamSceneWorld);
        if (!Directory.Exists(AssetPath_StreamSceneWorld_Impostor)) Directory.CreateDirectory(AssetPath_StreamSceneWorld_Impostor);

        spawnedBiomes.ForEach(n =>
        {
            SceneManagerExtensions._Editor_CreateSceneWithTerrainObject_ByWorld_Original(n, AssetPath_StreamSceneWorld);
            //SceneManagerExtensions._Editor_CreateSceneWithTerrainObject_ByWorld_LOD1(n, AssetPath_StreamSceneWorld_LOD1);
            SceneManagerExtensions._Editor_CreateSceneWithTerrainObject_ByWorld_Impostor(n, AssetPath_StreamSceneWorld_Impostor);
            exportChunk.Remove(n.export);
            exportChunk.Add(n.export);
        });
    }

    public void _Editor_Remove_SpawnObject()
    {
        foreach(var s in spawnedBiomes)
        {
            s.objects.ForEach(e => DestroyImmediate(e.Object));
            s.objects.Clear();
        }
        spawnedBiomes.Clear();
    }

    public int _Editor_GetSpawnObjectCount()
    {
        int count = 0;
        foreach (var s in spawnedBiomes)
            count += s.objects.Count();

        return count;
    }

    public void OnDrawGizmosSelected()
    {
        _Editor_Gizemo_Viewport();

        Gizmos.color = Color.red;
        TerrainCollider terrain = TerrainExtentionsX.GetNearTerrainCollider(transform.position + Vector3.one);

        for (int i = 0; i < 40; ++i)
            for (int j = 0; j < 40; ++j)
            {
                Vector3 rayOrigin = transform.position + new Vector3(100 * i, 1000, 100 * j) + new Vector3(100, 0, 100) / 2;

                RaycastHit hit;
                Ray ray = new Ray(rayOrigin, Vector3.down);

                if (terrain.Raycast(ray, out hit, Mathf.Infinity))
                {
                    if (exportChunkIndex[i, j]) Gizmos.color = Color.green;
                    else Gizmos.color = Color.red;

                    Gizmos.DrawWireCube(hit.point, Vector3.one * 100);
                }
            }
    }
#endif
}


#if UNITY_EDITOR
[CustomEditor(typeof(Biome)), CanEditMultipleObjects]
public class BiomeEditor : Editor
{
    Biome owner;

    public void OnEnable()
    {
        owner = (Biome)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        DrawDefaultInspector();

        GUILayout.Space(10);
        GUILayout.BeginVertical("Biome Brushs", "window");
        if(GUILayout.Button("Add Brush"))
        {
            owner.biomeBrushs.Add(new BiomeBrushContainer());
        }
        for(int i = 0; i < owner.biomeBrushs.Count; ++i)
        {
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            owner.biomeBrushs[i].IsSelected = EditorGUILayout.Toggle(owner.biomeBrushs[i].IsSelected);
            owner.biomeBrushs[i].NatureData = (NatureSpawnData)EditorGUILayout.ObjectField(owner.biomeBrushs[i].NatureData, typeof(NatureSpawnData));
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                owner.biomeBrushs.RemoveAt(i);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        
        GUILayout.Space(10);
        GUILayout.BeginHorizontal(); 
        if(GUILayout.Button("Spawn Vieport Chunk"))
        {
            owner._Editor_Spawn_Viewport();
        }

        if (GUILayout.Button("Export Spawnd Chunk"))
        {
            owner._Editor_Export_SpawnObject();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label($"Spawned Object Count: {owner._Editor_GetSpawnObjectCount()}");

        if (GUILayout.Button("Remove All Object"))
        {
            owner._Editor_Remove_SpawnObject();
        }

        GUILayout.Space(10);
        if(GUILayout.Button("Refresh"))
        {
            owner._Editor_ExportChunkIndexInit();
        }

        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed || EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
        }
    }
}
#endif