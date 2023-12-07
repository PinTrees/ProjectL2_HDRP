using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


[System.Serializable]
public class AstarCell
{
    public Vector3 position;
    public int cost;
}


// Runtime Build Navigation LOD
// 모든 경로를 처리하는 네비게이션이 아님
// 건물 내부, 동굴, 이중 복층 지형에 대헤 네비게이션을 처리할 수 없습니다.

// 플레이어의 이동 범위 주위로 실시간 지형 네비게이션을 처리합니다.
public class NavigationManager : Singleton<NavigationManager>   
{
    public Vector3 NavigationBuildScale;
    public Vector3 NavigationCellScale;
    public Transform target;

    [Range(0, 90)]
    public float Slope = 5;

    // 250,000
    [SerializeField]
    public AstarCell[,] navigation_build_data = new AstarCell[500, 500];

    public void Start()
    {
        StartCoroutine(StartBuildNavigation());
    }

    IEnumerator StartBuildNavigation()
    {
        while (true)
        {
            BuildNavigation();
            yield return new WaitForSeconds(5);
        }
    }

    public void BuildNavigation()
    {
        transform.position = target.position;
        transform.eulerAngles = Vector3.zero;

        target = GameObject.FindAnyObjectByType<Player>().transform;
        var terrain = TerrainExtentionsX.GetNearTerrainCollider(transform);

        for (int x = 0; x < 500; ++x)
        {
            for (int z = 0; z < 500; ++z)
            {
                if (x % BiomeSpawner.EditorSamplingScale == 0 && z % BiomeSpawner.EditorSamplingScale == 0)
                {
                    Vector3 rayOrigin = transform.position + new Vector3(0, 1000, 0) + new Vector3(x, 0, z) * 0.5f + new Vector3(-250, 0, -250) * 0.5f;

                    RaycastHit hit;
                    Ray ray = new Ray(rayOrigin, Vector3.down);

                    if (terrain.Raycast(ray, out hit, Mathf.Infinity))
                    {
                        navigation_build_data[x, z] = new AstarCell();
                        navigation_build_data[x, z].position = hit.point;

                        /// 지형의 소환 가능 경사를 확인합니다.
                        if (Vector3.Angle(Vector3.up, hit.normal) > Slope)
                        {
                            navigation_build_data[x, z].cost = 100;
                        }
                        else navigation_build_data[x, z].cost = 1;
                    }
                }
            }
        }
    }

#if UNITY_EDITOR
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 1f, 0.3f);
        Gizmos.DrawCube(transform.position, NavigationBuildScale);

        Gizmos.color = new Color(0, 0, 1, 1f);
        Gizmos.DrawWireCube(transform.position, NavigationBuildScale);

        for (int i = 0; i < 500; ++i)
        {
            for (int j = 0; j < 500; ++j)
            {
                if (navigation_build_data[i, j] == null) continue;

                if (navigation_build_data[i, j].cost > 10)
                    Gizmos.color = new Color(1, 0, 0, 1f);
                else
                    Gizmos.color = new Color(0, 0, 1, 1f);

                Gizmos.DrawCube(navigation_build_data[i, j].position, Vector3.one * 0.35f);
            }
        }
    }
#endif
}


#if UNITY_EDITOR
[CustomEditor(typeof(NavigationManager))]
public class NavigationManagerEditor : Editor
{
    NavigationManager owner;
    private void OnEnable()
    {
        owner = (NavigationManager)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();

        GUILayout.Space(10);
        GUILayout.Label("Editor Build Setting");
        if(GUILayout.Button("Build Navigation"))
        {
            owner.BuildNavigation();
        }

        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();
        if (GUI.changed || EditorGUI.EndChangeCheck())
        {
            Debug.Log("[Navigation] Save");
        }
    }
}
#endif
