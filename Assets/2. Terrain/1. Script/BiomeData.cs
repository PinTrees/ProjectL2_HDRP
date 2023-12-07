using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public enum SPAWN_OBJECT_TYPE
{ 
    GRASS,
    TREE,
    ROCK,

    BUILLDING
}


[System.Serializable]
[CreateAssetMenu(fileName = "SpawnObject Data", menuName = "Scriptable Object/SpawnObject Data", order = int.MaxValue)]
public class BiomeData : ScriptableObject
{
    [SerializeField] public string ID = "";
    [SerializeField] public SPAWN_OBJECT_TYPE Type;

    [SerializeField] public bool IsRandomRotation;
    [SerializeField] public bool IsUsedObstacle = false;
    [SerializeField] public bool IsStaticObject = false;

    [SerializeField] public bool IsAdjustRatio;

    [Header("Impostor 빌드 손실율")]
    [SerializeField][Range(0, 1)] public float ImpostorLossRate;

    [Header("Obstacle 겹침율 (0(겸침) ~ 1(겹침 불가))")]
    [SerializeField] public Vector3 ObstacleScale;
    [SerializeField][Range(0, 1)] public float ObstacleRate;

    [Header("Perfab Data")]
    [SerializeField] public GameObject Object;
    [SerializeField] public GameObject StaticObject;
    [SerializeField] public GameObject ImpostorObject;

    [Header("Ranmdom Width")]
    [SerializeField][Range(1, 4)] public float WidthMin;
    [SerializeField][Range(1, 4)] public float WidthMax;

    [Header("Ranmdom Height")]
    [SerializeField][Range(1, 4)] public float HeightMin;
    [SerializeField][Range(1, 4)] public float HeightMax;

    [Header("생성 가능 경사도")]
    [SerializeField][Range(0, 90)] public float SlopMin;
    [SerializeField][Range(0, 90)] public float SlopMax;

    [Header("오브젝트 경사 맞춤")]
    [SerializeField][Range(0, 1)] public float SlopeAliment = 0;

    // Editor Func
    public Vector3 GetRandomSpawnPositon()
    {
        if (IsUsedObstacle)
            return new Vector3(Random.Range(0, ObstacleScale.x), 1000, Random.Range(0, ObstacleScale.z));
        return new Vector3(Random.Range(0, 1), 1000, Random.Range(0, 1));
    }
    public Vector3 GetRandomSpawnRotation()
    {
        if (!IsRandomRotation) return Vector3.zero;
        return new Vector3(0, Random.Range(0, 360), 0);
    }
    public Vector3 GetRandomScale()
    {
        var width_scale_factor = Random.Range(WidthMin, WidthMax);
        var height_scale_factor = Random.Range(HeightMin, HeightMax);

        if (IsAdjustRatio) return Vector3.one * height_scale_factor;
        else return new Vector3(width_scale_factor, height_scale_factor, width_scale_factor);
    }
    public Vector3 GetSlopeRotation(Vector3 normal, Vector3 current_rotation)
    {
        if (SlopeAliment > 0)
        {
            var max_angle = Vector3.Angle(Vector3.up, normal);
            float alignmentAngle = Mathf.Lerp(0f, max_angle, 1f);

            Vector3 currentRotation = current_rotation;

            currentRotation.x = alignmentAngle;
            currentRotation.z = alignmentAngle;
            return currentRotation;
        }

        return current_rotation;
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(BiomeData)), CanEditMultipleObjects]
public class BiomeDataEditor : Editor
{
    BiomeData owner;

    public void OnEnable()
    {
        owner = (BiomeData)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        GUILayout.Space(10);
        GUILayout.Label($"UID: {owner.ID}");

        GUILayout.Space(10);
        GUILayout.Label($"잔디 오브젝트의 경우 임포스터 손실률 50% 이상으로 설정");
        GUILayout.Label($"정점 개수 제한 300 미만");

        GUILayout.Space(10);
        DrawDefaultInspector();

        GUILayout.Space(10);
        GUILayout.BeginVertical();
        if (GUILayout.Button("Create UID"))
        {
            owner.ID = System.Guid.NewGuid().ToString();
        }

        GUILayout.EndVertical();

        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed || EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
            Debug.Log("[Biome Data] Saved");
        }
    }
}
#endif