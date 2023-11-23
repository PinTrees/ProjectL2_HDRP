using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class NpcGroup : MonoBehaviour
{
    [SerializeField] GameObject SpawnObject;

    [Range(1, 100)]
    public float BasSpawnCount;

    [Range(1, 50)]
    public float spwan_range;
    [Range(10, 200)]
    public float MoveAbleRange;

    public bool spawn_random_rotation;

    public List<Npc> npcs = new();




    public void _EditorSpawnObject()
    {
        TerrainCollider terrain = GameObject.FindWithTag("Terrain").GetComponent<TerrainCollider>();
        Ray ray = new Ray(new Vector3(transform.position.x, 1000, transform.position.z), Vector3.down);
        RaycastHit hit;
        if (terrain.Raycast(ray, out hit, Mathf.Infinity))
        {
            transform.position = hit.point;
        }


        for (int i = 0; i < BasSpawnCount; ++i)
        {
            var spawnAniamll = GameObjectExtensions.InstantiatePrefab(SpawnObject, transform);
            var spawn2dPosition = transform.position + new Vector3(Random.Range(-(spwan_range - 1), spwan_range - 1), 1000, Random.Range(-(spwan_range - 1), spwan_range - 1));
          
            Ray ray_spawn = new Ray(spawn2dPosition, Vector3.down);
            RaycastHit hit_spawn;
            if (terrain.Raycast(ray_spawn, out hit_spawn, Mathf.Infinity))
            {
                spawnAniamll.transform.position = hit_spawn.point;
            }

            if (spawn_random_rotation)
            {
                spawnAniamll.transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
            }

            spawnAniamll.transform.SetParent(transform, true);
            npcs.Add(spawnAniamll.GetComponent<Npc>());
        }
    }

    public void _EditorRemoveObject()
    {
        npcs.ForEach(e =>
        {
            if (e != null)
            {
                DestroyImmediate(e.gameObject);
            }
        });
        npcs.Clear();
    }

    public void _EditorRefeshObject()
    {
        TerrainCollider terrain = GameObject.FindWithTag("Terrain").GetComponent<TerrainCollider>();

        Ray ray = new Ray(new Vector3(transform.position.x, 1000, transform.position.z), Vector3.down);
        RaycastHit hit;
        if (terrain.Raycast(ray, out hit, Mathf.Infinity))
        {
            transform.position = hit.point;
        }

        npcs.ForEach(e =>
        {
            Ray ray = new Ray(new Vector3(e.transform.position.x, 1000, e.transform.position.z), Vector3.down);
            RaycastHit hit;
            if (terrain.Raycast(ray, out hit, Mathf.Infinity))
            {
                e.transform.position = hit.point;
            }
        });
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(NpcGroup))]
public class NpcGroupEditor : Editor
{
    NpcGroup value;

    private void OnEnable()
    {
        value = target as NpcGroup;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();

        EditorGUILayout.Space(20);

        BuildSpawnerButton();

        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }


    public void BuildSpawnerButton()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();

        // Remove Spawner
        if (GUILayout.Button("Remove Npc"))
        {
            value._EditorRemoveObject();
        }

        EditorGUILayout.Space();

        // Create Spawner
        EditorGUI.BeginChangeCheck(); // 이 위치에서 변경 사항 체크를 시작

        if (GUILayout.Button("Create Npc"))
        {
            value._EditorSpawnObject();
        }

        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Refresh"))
        {
            value._EditorRefeshObject();
        }

        EditorGUILayout.EndVertical();
    }


    private void OnSceneGUI()
    {
        // 테두리 그리기
        Handles.color = Color.green;
        Handles.DrawWireDisc(value.transform.position, Vector3.up, value.spwan_range);

        // 내부를 채우기 위해 다른 색상으로 설정
        Handles.color = new Color(0.0f, 1.0f, 0.0f, 0.1f); // 초록색으로 채움

        // 원형 기즈모 채우기
        Handles.DrawSolidDisc(value.transform.position, Vector3.up, value.spwan_range);

        // 테두리 그리기
        Handles.color = Color.blue;
        Handles.DrawWireDisc(value.transform.position, Vector3.up, value.MoveAbleRange);
    }
}
#endif