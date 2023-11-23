using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


public enum ARMOR_TYPE
{
    HEAD,
    BODY,
    ARM,
    GLOVES,
    PANTS,
    SHOSE,
}


[System.Serializable]
[CreateAssetMenu(fileName = "Armor Data", menuName = "Scriptable Object/Armor Data", order = int.MaxValue)]
public class ArmorData : ItemData
{
    [Header("Armor Data")]
    public ARMOR_TYPE ArmorType;
}


#if UNITY_EDITOR
[CustomEditor(typeof(ArmorData))]
public class ArmorDataEditor : Editor
{
    ArmorData owner;
    private void OnEnable()
    {
        owner = (ArmorData)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Label($"UID: {owner.UID}");
        if (owner.UID.Length < 5)
        {
            if (GUILayout.Button("Create UID"))
            {
                owner.UID = System.Guid.NewGuid().ToString();
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        DrawDefaultInspector();

        // save --------------------------------------
        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();
        if (GUI.changed || EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
            Debug.Log("[ArmorData] Save ScriptableObject");
        }
    }
}
#endif