using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using UnityEditor.SceneTemplate;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;
using UnityEngine.Splines;
using static UnityEngine.UI.GridLayoutGroup;
using UnityEngine.ResourceManagement.ResourceProviders;




#if UNITY_EDITOR
using UnityEditor;
#endif



public class TerrainExtentionsX
{
    public static TerrainCollider GetNearTerrainCollider(Transform target)
    {
        var terrainColliders = GameObject.FindObjectsOfType<TerrainCollider>();

        foreach(var t in terrainColliders)
        {
            if(target.position.x > t.transform.position.x && target.position.z > t.transform.position.z
                && target.position.x < t.transform.position.x + 4000 && target.position.z < t.transform.position.z + 4000)
            {
                return t;
            }
        }

        return null;
    }
    public static TerrainCollider GetNearTerrainCollider(Vector3 position)
    {
        var terrainColliders = GameObject.FindObjectsOfType<TerrainCollider>();

        foreach (var t in terrainColliders)
        {
            if (position.x > t.transform.position.x && position.z > t.transform.position.z
                && position.x < t.transform.position.x + 4000 && position.z < t.transform.position.z + 4000)
            {
                return t;
            }
        }

        return null;
    }
}


[System.Serializable]
public class SpawnedObjects
{
    public string biome_uid;
    [SerializeField]
    public List<GameObject> spawnObjects = new();
}


[System.Serializable]
public class BiomeSpawnData
{
    [SerializeField]
    public BiomeData data;
    public Vector3 offset;
    public float strength = 1f;
    public float slope_min = 0f;
    public float slope_max = 45f;
    public float height_min = 1;
    public float height_max = 1;
    public float width_min = 1;
    public float width_max = 1;
    public float distribution_center = 0;
    public float slope_aliment = 0f;
    public bool isRandomRotationY;
}

public class BiomeSpawner : MonoBehaviour
{
    [SerializeField]
    [HideInInspector]
    public string ExportDataId = "";

    [SerializeField]
    public bool IsExported = false;

    // 기즈모 드로우 사이즈
    public static float EditorSamplingScale = 3f;

    [SerializeField]
    [HideInInspector]
    public List<BiomeSpawnData> spawn_object_datas = new();

    #region edite data value
    [HideInInspector]
    public Vector3 size;
    public bool IsStaticObject = false;
    #endregion

    [SerializeField]
    [HideInInspector]
    private List<SpawnedObjects> spawnedObjects = new();

    public string AssetPath_StreamSceneLocal { get => $"Assets/2. Terrain/StreamScene_Local/"; }
    public string AssetPath_StreamSceneWorld { get => $"Assets/2. Terrain/StreamScene_World/"; }
    public string AssetFileName { get => $"{ExportDataId}.unity";  }

    public SceneInstance? scene_impostor_instance;
    public SceneInstance? scene_lod0_instance;

    public Scene? scene_impostor;
    public Scene? scene_lod0;

    [HideInInspector]
    public bool IsLoadSceneLod0 = false;
    [HideInInspector]
    public bool IsLoadSceneImpostor = false;

    [HideInInspector]
    public bool IsLoadSceneLod0_Editor = false;
    [HideInInspector]
    public bool IsLoadSceneImpostor_Editor = false;

    void Start()
    {
        scene_lod0 = null;
        scene_impostor = null;
    }

    private bool IsPrefab()
    {
        foreach(var g in spawn_object_datas)
        {
            if (PrefabUtility.GetPrefabAssetType(g.data.Object) == PrefabAssetType.NotAPrefab)
                return false;
        }

        return true;
    }

    public int GetSpawnedObejctCount()
    {
        var count = 0;

        spawnedObjects.ForEach(e =>
        {
            count += e.spawnObjects.Count;
        });

        return count;
    }


    #region Private Func
    /// <summary>
    /// 이 함수는 소환할 오브젝트의 초기 위치를 생성하고 반환합니다.
    /// </summary>
    /// <returns></returns>
    Vector3 GetRandomSpawnPositon()
    {
        var spawn_position = new Vector3(Random.Range(-(size.x - 1), size.x - 1), 1000, Random.Range(-(size.z - 1), size.z - 1));
        return transform.rotation * spawn_position;
    }

    /// <summary>
    /// 이 함수는 소환된 오브젝트의 크기를 계산하고 반환합니다.
    /// </summary>
    /// <param name="scale_width"></param>
    /// <returns></returns>
    Vector3 GetRandomScale(BiomeSpawnData data)
    {
        var width_scale_factor = Random.Range(data.width_min, data.width_max);
        var height_scale_factor = Random.Range(data.height_min, data.height_max);

        if(data.data.IsAdjustRatio)
        {
            return Vector3.one * height_scale_factor;
        }
        else
        {
            return new Vector3(width_scale_factor, height_scale_factor, width_scale_factor);
        }
    }
    #endregion


    #region EDITOR FUNC
    /// <summary>
    /// 에디터 함수
    /// 지형 오브젝트를 생성합니다.
    /// </summary>
    public void _Editor_SpawnObject()
    {
        TerrainCollider terrain = TerrainExtentionsX.GetNearTerrainCollider(transform);

        /// 소환할 오브젝트 정보가 프리팹인지 확인합니다.
        if (!IsPrefab())
        {
            Debug.LogError("[Object Spawner] Not Prefab Object. Make Original Prefab");
            return;
        }

        /// 에디터 오브젝트의 위치를 재조정합니다.
        Ray ray = new Ray(new Vector3(transform.position.x, 1000, transform.position.z), Vector3.down);
        RaycastHit hit;
        if (terrain.Raycast(ray, out hit, Mathf.Infinity))
        {
            transform.position = hit.point;
        }

        int x = (int)size.x;
        int z = (int)size.z;

        for(int i = (int)(size.x * -0.5f); i < (int)(size.x * 0.5f); ++i)
        {
            for(int j = (int)(size.z * -0.5f); j < (int)(size.z * 0.5f); ++j)
            {
                var position = transform.position + transform.rotation * new Vector3(i, 0, j);
                spawn_object_datas.ForEach(e =>
                {
                    _SpawnObject(e, position, e.strength);
                });
            }
        }
    }



    // 추가 작업 사항
    // 가장자리 슬로프
    // 중앙 집중 분포
    // 그리드 정렬 생성
    // 패턴 생성 (곡률, 펄린 노이즈)
    // 높이 기반 생성 제한
    // 이미 소환된 오브젝트 존재시 확률 하락
    public void _SpawnObject(BiomeSpawnData data, Vector3 position, float percent)
    {
        if (data.distribution_center > 0)
        {
            float distanceToCenterX = transform.position.x - position.x;
            distanceToCenterX = Mathf.Abs(distanceToCenterX);
            float allLengthX = size.x / 2;

            float distanceToCenterZ = transform.position.z - position.z;
            distanceToCenterZ = Mathf.Abs(distanceToCenterZ);
            float allLengthZ = size.z / 2;

            float amount = (distanceToCenterX / allLengthX) + (distanceToCenterZ / allLengthZ);
            amount = amount * 0.5f;

            percent = percent * (amount * amount) * data.distribution_center;
        }

        if (percent < 0) return;

        if (Random.Range(0f, 1f) < percent)
        {
            TerrainCollider terrain = TerrainExtentionsX.GetNearTerrainCollider(position);

            int attempts = 0;

            while (attempts < 128)
            {
                /// 초기 위치 설정
                Vector3 spawnPosition = position + new Vector3(Random.Range(-0.3f, 0.3f), 2000, Random.Range(-0.3f, 0.3f));
    
                /// 정확한 지형 배치 위치를 계산합니다.
                Ray ray_spawn = new Ray(spawnPosition, Vector3.down);
                RaycastHit hit_spawn;
                if (terrain.Raycast(ray_spawn, out hit_spawn, Mathf.Infinity))
                {
                    /// 지형의 소환 가능 경사를 확인합니다.
                    if (Vector3.Angle(Vector3.up, hit_spawn.normal) > data.slope_max
                        || Vector3.Angle(Vector3.up, hit_spawn.normal) < data.slope_min)
                    {
                        ++attempts;
                        continue;
                    }

                    /// 오브젝트 프리팹 연결 소환
                    var spawn = GameObjectExtensions.InstantiatePrefab(data.data.Object, transform);

                    spawn.transform.localScale = GetRandomScale(data);
                    spawn.transform.position = spawnPosition;
                    spawn.transform.position = hit_spawn.point + data.offset;
                    if (data.isRandomRotationY) spawn.transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);

                    if (data.slope_aliment > 0)
                    {
                        var max_angle = Vector3.Angle(Vector3.up, hit_spawn.normal);
                        float alignmentAngle = Mathf.Lerp(0f, max_angle, 1f);

                        Vector3 currentRotation = spawn.transform.eulerAngles;

                        currentRotation.x = alignmentAngle;
                        currentRotation.z = alignmentAngle;

                        spawn.transform.eulerAngles = currentRotation;
                    }

                    /// 오브젝트의 스태틱 플래그를 설정합니다.
                    if (IsStaticObject)
                    {
                        StaticEditorFlags staticFlags = StaticEditorFlags.ContributeGI | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.BatchingStatic | StaticEditorFlags.BatchingStatic | StaticEditorFlags.ReflectionProbeStatic;
                        GameObjectUtility.SetStaticEditorFlags(spawn, staticFlags);
                    }
                    /// 오브젝트의 스태틱 플래그를 해제합니다.
                    else
                    {
                        StaticEditorFlags staticFlags = 0; // StaticEditorFlags.ContributeGI | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.BatchingStatic | StaticEditorFlags.BatchingStatic | StaticEditorFlags.ReflectionProbeStatic;
                        GameObjectUtility.SetStaticEditorFlags(spawn, staticFlags);
                    }

                    /// 레이어의 하위계층으로 추가합니다.
                    spawn.transform.SetParent(transform, true);

                    /// 관리되는 오브젝트 목록에 추가합니다.
                    var listAll = spawnedObjects.Where(e => e.biome_uid.Equals(data.data.ID));
                    SpawnedObjects list = null;
                    if (listAll.Count() > 0) list = listAll.First();

                    if (list == null)
                    {
                        var spawnData = new SpawnedObjects();
                        spawnData.biome_uid = data.data.ID;
                        spawnData.spawnObjects.Add(spawn);

                        spawnedObjects.Add(spawnData);
                    }
                    else
                    {
                        list.spawnObjects.Add(spawn);
                    }

                    break;
                }
                else
                {
                    ++attempts;
                }
            }
        }

        if(percent > 1f)
        {
            var nextPercent = percent - 1f;
            _SpawnObject(data, position, nextPercent);
            return;
        }
    }


    /// <summary>
    /// 생성목록에 등록된 오브젝트의 랜덤한 1개를 배치하고 관리되는 항목으로 추가합니다.
    /// </summary>
    public void _Editor_SpawnObjectOne()
    {

    }

    /// <summary>
    /// 에디터 함수
    /// 소환된 오브젝트를 제거합니다.
    /// </summary>
    public void _Editor_RemoveObject()
    {
       
        spawnedObjects.ForEach(e =>
        {
            e.spawnObjects.ForEach(s =>
            {
                if (s != null)
                {
                    /// 에디터 타임에서 오브젝트를 제거합니다.
                    /// 에디터 메모리 누수 제거
                    DestroyImmediate(s.gameObject);
                }
            });
            e.spawnObjects.Clear();
        });

        spawnedObjects.Clear();
    }

    /// <summary>
    /// 에디터 함수
    /// 소환된 오브젝트의 위치를 재조정 합니다.
    /// 관리되는 오브젝트 목록만 해당합니다.
    /// </summary>
    public void _Editor_RefreshObject()
    {
        TerrainCollider terrain = TerrainExtentionsX.GetNearTerrainCollider(transform);

        Ray ray = new Ray(new Vector3(transform.position.x, 1000, transform.position.z), Vector3.down);
        RaycastHit hit;
        if (terrain.Raycast(ray, out hit, Mathf.Infinity))
        {
            transform.position = hit.point;
        }

        spawnedObjects.ForEach(e =>
        {
            e.spawnObjects.ForEach(s =>
            {
                Ray ray = new Ray(new Vector3(s.transform.position.x, 1000, s.transform.position.z), Vector3.down);
                RaycastHit hit;
                if (terrain.Raycast(ray, out hit, Mathf.Infinity))
                {
                    s.transform.position = hit.point; // + Offset;
                }
            });
        });
    }

    public void _Editor_ExportWorld()
    {
        if(ExportDataId.Length < 5)
        {
            Debug.LogError("[Biome Spawner] Create Uid First");
            return;
        }

        if (!Directory.Exists(AssetPath_StreamSceneWorld))
        {
            Directory.CreateDirectory(AssetPath_StreamSceneWorld);
        }

        List<GameObject> gameobjects = new();
        spawnedObjects.ForEach(e =>
        {
            e.spawnObjects.ForEach(s =>
            {
                gameobjects.Add(s);
            });
        });

        SceneManagerExtensions.CreateSceneWithTerrainObject_World_Editor(gameobjects, AssetPath_StreamSceneWorld, ExportDataId);
        IsExported = true;
    }

    public void _Editor_ExportLocal()
    {

        if (ExportDataId.Length < 5)
        {
            Debug.LogError("[Biome Spawner] Create Uid First");
            return;
        }

        if (!Directory.Exists(AssetPath_StreamSceneLocal))
        {
            Directory.CreateDirectory(AssetPath_StreamSceneLocal);
        }

        List<GameObject> gameobjects = new();
        spawnedObjects.ForEach(e =>
        {
            e.spawnObjects.ForEach(s =>
            {
                gameobjects.Add(s);
            });
        });

        SceneManagerExtensions.CreateSceneWithTerrainObject_Local_Editor(gameobjects, AssetPath_StreamSceneLocal, ExportDataId, true);
        IsExported = true;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length > 1)
        {
            return;
        }

        if(selectedObjects.Contains(transform.parent.gameObject))
        {
            return;
        } 

        TerrainCollider terrain = TerrainExtentionsX.GetNearTerrainCollider(transform);

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(Vector3.zero, size);


        for (int x = (int)(size.x * -0.5f); x < (int)(size.x * 0.5f); ++x)
        {
            for (int z = (int)(size.z * -0.5f); z < (int)(size.z * 0.5f); ++z)
            {
                if(x % BiomeSpawner.EditorSamplingScale == 0 && z % BiomeSpawner.EditorSamplingScale == 0)
                {
                    Vector3 pos = new Vector3(x, 0, z);

                    Quaternion rotatedRay = Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up);
                    Vector3 rayOrigin = transform.position + rotatedRay * new Vector3(x, 1000, z);

                    RaycastHit hit;
                    Ray ray = new Ray(rayOrigin, Vector3.down);
                    
                    if (terrain.Raycast(ray, out hit, Mathf.Infinity))
                    {
                        pos.y = transform.InverseTransformPoint(hit.point).y;
                    }

                    Gizmos.color = new Color(1f, 0f, 0f, 1f);
                    Gizmos.DrawWireCube(pos, Vector3.one * BiomeSpawner.EditorSamplingScale);
                    Gizmos.color = new Color(1.0f, 1.0f, 1.0f, 0.3f);
                    Gizmos.DrawCube(pos, Vector3.one * BiomeSpawner.EditorSamplingScale);
                }
            }
        }

        /*for (int x = (int)(size.x * -0.5f); x < (int)(size.x * 0.5f); ++x)
        {
            Vector3 pos1 = new Vector3(x, 0, size.z * -0.5f);
            Vector3 pos2 = new Vector3(x, 0, size.z * 0.5f);

            Gizmos.DrawLine(pos1, pos2);
        }

        for (int z = (int)(size.z * -0.5f); z < (int)(size.z * 0.5f); ++z)
        {
            Vector3 pos1 = new Vector3(size.x * -0.5f, 0, z);
            Vector3 pos2 = new Vector3(size.x * 0.5f, 0, z);

            Gizmos.DrawLine(pos1, pos2);
        }*/
    }
#endif
#endregion
}


#if UNITY_EDITOR
[CustomEditor(typeof(BiomeSpawner))]
public class ObjectSpawnerEditor : Editor
{
    BiomeSpawner owner;

    private void OnEnable()
    {
        owner = target as BiomeSpawner;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();

        GUILayout.Space(10);
        GUILayout.Label($"Biome Spawn Area Size");
        owner.size = EditorGUILayout.Vector3Field("", owner.size);
        
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Label($"UID: {owner.ExportDataId}");
        if (owner.ExportDataId.Length < 5)
        {
            if(GUILayout.Button("Create UID"))
            {
                owner.ExportDataId = System.Guid.NewGuid().ToString();
                EditorUtility.SetDirty(target);
                Debug.Log("[Biome Spawned] Create UID");
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label($"Biome Spawn Cell Size: 1m X 1m");

        GUILayout.Space(5);
        GUILayout.BeginVertical("Biome Datas", "window");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Biome Data"))
        {
            owner.spawn_object_datas.Add(new BiomeSpawnData());
            EditorUtility.SetDirty(target);
        }
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < owner.spawn_object_datas.Count; ++i)
        {
            EditorGUILayout.Space(10);
            var biomeData = owner.spawn_object_datas[i];
            EditorGUILayout.BeginHorizontal();
            owner.spawn_object_datas[i].data = (BiomeData)EditorGUILayout.ObjectField(owner.spawn_object_datas[i].data, typeof(BiomeData));
            if (GUILayout.Button("Remove"))
            {
                owner.spawn_object_datas.RemoveAt(i);
                EditorUtility.SetDirty(target);
            }
            GUILayout.EndHorizontal();

            owner.spawn_object_datas[i].isRandomRotationY = EditorGUILayout.Toggle("Random Rotation Y", owner.spawn_object_datas[i].isRandomRotationY);
            owner.spawn_object_datas[i].strength = EditorGUILayout.FloatField("Spawn Strength", owner.spawn_object_datas[i].strength);
            owner.spawn_object_datas[i].offset = EditorGUILayout.Vector3Field("Offset", owner.spawn_object_datas[i].offset);
            EditorGUILayout.MinMaxSlider("Slope", ref owner.spawn_object_datas[i].slope_min, ref owner.spawn_object_datas[i].slope_max, 0, 100);
            EditorGUILayout.MinMaxSlider("Height (0.5 ~ 4.0)", ref owner.spawn_object_datas[i].height_min, ref owner.spawn_object_datas[i].height_max, 0.5f, 4f);
            EditorGUILayout.MinMaxSlider("Width   (0.5 ~ 4.0)", ref owner.spawn_object_datas[i].width_min, ref owner.spawn_object_datas[i].width_max, 0.5f, 4f);
            owner.spawn_object_datas[i].distribution_center = EditorGUILayout.Slider("중앙분포   (0 ~ 1)", owner.spawn_object_datas[i].distribution_center, 0f, 1f);
            owner.spawn_object_datas[i].slope_aliment = EditorGUILayout.Slider("경사맞춤   (0 ~ 1)", owner.spawn_object_datas[i].slope_aliment, 0f, 1f);
        }
        GUILayout.EndVertical();

        EditorGUILayout.Space(20);
        GUILayout.Label($"Spawned Object Count: {owner.GetSpawnedObejctCount()}");

        GUILayout.BeginVertical();

        EditorGUILayout.Space(20);

        GUILayout.BeginHorizontal();
        // Remove Spawner
        if (GUILayout.Button("Remove Object"))
        {
            owner._Editor_RemoveObject();
        }
        // Remove Spawner
        if (GUILayout.Button("Spawn Object"))
        {
            owner._Editor_SpawnObject();
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Spawn Object Only Once"))
        {
        }
        EditorGUILayout.Space(4);
        // Refresh Spawner
        if (GUILayout.Button("Refresh"))
        {
            owner._Editor_RefreshObject();
        }

        GUILayout.Space(10);
        GUILayout.Label($"Export Scene Options");
        GUILayout.Label($"Biome Chunk 250m X 250m");
        GUILayout.Label($"Export World: {owner.AssetPath_StreamSceneWorld}");
        GUILayout.Label($"Export Local: {owner.AssetPath_StreamSceneLocal}");
        GUILayout.Label($"월드 기반 내보내기 기능은 테스트중이므로 가급적 미사용");
      
        GUILayout.Space(10);
        GUILayout.Label($"Export By Local");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Export By Local Area"))
        {
            owner._Editor_ExportLocal();
            EditorUtility.SetDirty(target);
        }
        if (GUILayout.Button("Remove Local File"))
        {
            FileUtilExtentions._Editor_DeleteFileOrDirectory($"2. Terrain/StreamScene_Local/{owner.ExportDataId}.unity");
            FileUtilExtentions._Editor_DeleteFileOrDirectory($"2. Terrain/StreamScene_Local/{owner.ExportDataId}_impostor.unity");

            SceneManagerExtensions._Editor_BuildSceneRemove(owner.AssetPath_StreamSceneLocal + owner.ExportDataId + ".unity");
            SceneManagerExtensions._Editor_BuildSceneRemove(owner.AssetPath_StreamSceneLocal + owner.ExportDataId + "_impostor.unity");

            AssetDatabase.Refresh();
            EditorUtility.SetDirty(target);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(20);
        if (GUILayout.Button("Remove UID"))
        {
            owner.ExportDataId = "";
            Debug.Log("[Biome Spawned] Remove UID"); 
        }
        GUILayout.EndVertical();

        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
#endif