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
    [SerializeField]
    SPAWN_OBJECT_TYPE Type;

    [SerializeField]
    public string ID = "";

    [SerializeField]
    public bool IsRandomRotation;

    [SerializeField]
    public bool IsAdjustRatio;

    [SerializeField]
    public int limitCount;

    [SerializeField] public GameObject Object;
    [SerializeField] public GameObject StaticObject;

    [Range(1, 4)]
    [SerializeField]
    public float ScaleWidth = 1;

    [SerializeField]
    public float ImpostorActivePer = 1;
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