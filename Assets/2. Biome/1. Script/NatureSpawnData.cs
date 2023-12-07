using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif


[System.Serializable]
public class BiomeSpawnData
{
    [SerializeField]
    public BiomeData data;
    public Vector3 offset;
    public float strength = 1f;
    public float distribution_center = 0;

    public bool IsSplopeOverride;
    public float SlopeMin;
    public float SlopeMax;
}


[System.Serializable]
[CreateAssetMenu(fileName = "Nature Spawn Data", menuName = "Scriptable Object/Nature Spawn Data", order = int.MaxValue)]
public class NatureSpawnData : ScriptableObject
{
    [SerializeField][HideInInspector]
    public List<BiomeSpawnData> SpawnObjectData = new();

#if UNITY_EDITOR
    public void _Editor_Spawn(Biome biome, Vector3 chunkCenter)
    {
        SpawnObjectData.ForEach(e =>
        {
            var nature = e.data;
            int grid_x = nature.IsUsedObstacle ? (int)(Biome.StreamChunkSize / nature.ObstacleScale.x) : Biome.StreamChunkSize;
            int grid_z = nature.IsUsedObstacle ? (int)(Biome.StreamChunkSize / nature.ObstacleScale.z) : Biome.StreamChunkSize;

            int count = 0;
            for (int x = -(int)(grid_x * 0.5f); x < (int)(grid_x * 0.5f); ++x)
            {
                for (int z = -(int)(grid_z * 0.5f); z < (int)(grid_z * 0.5f); ++z)
                {
                    if (count % 128 == 0)
                    {
                        Debug.Log($"spawned { count }");
                        count = 0;
                    }

                    var spawn_center = chunkCenter;
                    if (nature.IsUsedObstacle) spawn_center += new Vector3(x * nature.ObstacleScale.x, 0, z * nature.ObstacleScale.z) + nature.ObstacleScale * 0.5f;
                    else spawn_center += new Vector3(x, 0, z) + Vector3.one * 0.5f;

                    // �Ļ� ������Ʈ ����
                    ++count; 
                    _Editor_SpawnBiome(biome, e, spawn_center, e.strength);
                }
            }
        });
    }

    // ���� ������Ʈ ��ȯ �Լ�
    public void _Editor_SpawnBiome(Biome biome, BiomeSpawnData spawn_data, Vector3 spawn_center, float spawn_percent)
    {
        var biome_data = spawn_data.data;
        if (spawn_percent < 0) return;

        if (Random.Range(0f, 1f) < spawn_percent)
        {
            var terrain = TerrainExtentionsX.GetNearTerrainCollider(spawn_center);

            int attempts = 0;
            while (attempts < 128)
            {
                // ��ȯ ���� ���� ���ø� ��ġ�� �����մϴ�.
                Vector3 spawnPosition = spawn_center;
                if(biome_data.IsUsedObstacle) spawnPosition += new Vector3(
                      Random.Range(biome_data.ObstacleScale.x * -0.7f, biome_data.ObstacleScale.x * 0.7f)
                    , 2000
                    , Random.Range(biome_data.ObstacleScale.z * -0.7f, biome_data.ObstacleScale.z * 0.7f));
                else spawnPosition += new Vector3(Random.Range(-0.3f, 0.3f), 2000, Random.Range(-0.3f, 0.3f));

                Ray ray_spawn = new Ray(spawnPosition, Vector3.down);
                RaycastHit hit_spawn;
                if (terrain.Raycast(ray_spawn, out hit_spawn, Mathf.Infinity))
                {
                    // ������ ��ȯ ���� ��縦 Ȯ���մϴ�.
                    if (Vector3.Angle(Vector3.up, hit_spawn.normal) > biome_data.SlopMax
                        || Vector3.Angle(Vector3.up, hit_spawn.normal) < biome_data.SlopMin)
                    {
                        ++attempts;
                        continue;
                    }

                    // ������Ʈ ������ ���� ��ȯ
                    var spawn = GameObjectExtensions.InstantiatePrefab(spawn_data.data.Object);

                    spawn.transform.position = hit_spawn.point + spawn_data.offset;
                    spawn.transform.localScale = biome_data.GetRandomScale();
                    spawn.transform.eulerAngles = biome_data.GetRandomSpawnRotation();
                    spawn.transform.eulerAngles = biome_data.GetSlopeRotation(hit_spawn.normal, spawn.transform.eulerAngles);
                    
                    // ������Ʈ�� ����ƽ �÷��׸� �����մϴ�.
                    if (spawn_data.data.IsStaticObject)
                    {
                        StaticEditorFlags staticFlags = StaticEditorFlags.ContributeGI | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.BatchingStatic | StaticEditorFlags.BatchingStatic | StaticEditorFlags.ReflectionProbeStatic;
                        GameObjectUtility.SetStaticEditorFlags(spawn, staticFlags);
                    }
                    // ������Ʈ�� ����ƽ �÷��׸� �����մϴ�.
                    else
                    {
                        StaticEditorFlags staticFlags = 0;
                        GameObjectUtility.SetStaticEditorFlags(spawn, staticFlags);
                    }

                    biome._Editor_AddBiomeSpawnObject(spawn_data.data, spawn);
                    break;
                }

                // ���̰� ������ ����� ���
                // ��κ��� ��쿡�� ������� �ʽ��ϴ�.
                else ++attempts;
            }
        }

        // Ȯ���� �ִ�ġ �̻��� ��� �ݵ�� ��ȯ�� �����մϴ�.
        if (spawn_percent > 1f)
        {
            var nextPercent = spawn_percent - 1f;
            _Editor_SpawnBiome(biome, spawn_data, spawn_center, nextPercent);
            return;
        }
    }
#endif
}


#if UNITY_EDITOR
[CustomEditor(typeof(NatureSpawnData))]
public class NatureSpawnDataEditor : Editor
{
    NatureSpawnData owner;
    private void OnEnable()
    {
        owner = (NatureSpawnData)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        GUILayout.Label($"�ش� ��ü�� �̸� ���ǵ� ������Ʈ��" +
            $"\n���� ������ �����ϴ� ������ Ŭ���� �Դϴ�." +
            $"\n\n�ʹ� ���� ������Ʈ�� �ϳ��� �����ͷ�" +
            $"\n������ ��� ������ ������ ����� �� �ֽ��ϴ�." +
            $"\n\nMeadow, Forest ó�� �������� ���ǰ� �ƴ�" +
            $"\nmeadow_small_rock, meadow_dry_tree�� ����" +
            $"\n�ұԸ� ������ �߰��� ��õ�մϴ�.");

        GUILayout.Space(10);
        DrawDefaultInspector();

        GUILayout.Space(10);
        GUILayout.Label($"Biome Spawn Cell Size: 1m X 1m");
        GUILayout.Space(10);
        GUILayout.BeginVertical("Biome Datas", "window");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Biome Data"))
        {
            owner.SpawnObjectData.Add(new BiomeSpawnData());
            EditorUtility.SetDirty(target);
        }
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < owner.SpawnObjectData.Count; ++i)
        {
            EditorGUILayout.Space(10);
            GUILayout.Label("Biome");
            var biomeData = owner.SpawnObjectData[i];
            EditorGUILayout.BeginHorizontal();
            owner.SpawnObjectData[i].data = (BiomeData)EditorGUILayout.ObjectField(owner.SpawnObjectData[i].data, typeof(BiomeData));
            if (GUILayout.Button("Remove"))
            {
                owner.SpawnObjectData.RemoveAt(i);
                EditorUtility.SetDirty(target);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            GUILayout.Label("Override");
            owner.SpawnObjectData[i].IsSplopeOverride = EditorGUILayout.Toggle("Spole", owner.SpawnObjectData[i].IsSplopeOverride);

            if (owner.SpawnObjectData[i].IsSplopeOverride)
            {
                EditorGUILayout.MinMaxSlider("Slope", ref owner.SpawnObjectData[i].SlopeMin, ref owner.SpawnObjectData[i].SlopeMax, 0f, 90f);
            }

            GUILayout.Space(5);
            GUILayout.Label("Data");
            owner.SpawnObjectData[i].strength = EditorGUILayout.FloatField("Spawn Strength", owner.SpawnObjectData[i].strength);
            owner.SpawnObjectData[i].offset = EditorGUILayout.Vector3Field("Offset", owner.SpawnObjectData[i].offset);
        }
        GUILayout.EndVertical();

        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed || EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
            Debug.Log("[Nature Spawn Data] Saved");
        }
    }
}
#endif
