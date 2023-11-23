using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


[System.Serializable]
public enum EQUIP_TRANSFORM_PARENT_TYPE
{
    SPINE_01,
    SPINE_02,
    SPINE_03,
    RIGHT_HAND,
    LEFT_HAND,
    LEFT_ARM,
}


[System.Serializable]
public enum EQUIP_TRANSFORM_TYPE
{
    RightHand_GreatSword,
    RightHand_Katana,
    LeftHand_GreatSword,

    Back_1,
    Belt_1,

    Back_Bow,
    LeftHand_Bow,
    LeftHand_Shield
}


public class EquipPosition : MonoBehaviour
{
    [Space(10)]
    public EQUIP_TRANSFORM_PARENT_TYPE ParentTransformType;
    public EQUIP_TRANSFORM_TYPE EquipTransformType;

    void Start()
    {
    }

    public void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
      
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawCube(Vector3.zero, new Vector3(0.02f, 0.1f, 0.3f));
        Gizmos.color = new Color(1f, 1f, 1f, 1f);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.02f, 0.1f, 0.3f));
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(EquipPosition))]
public class EquipPositionEditor : Editor
{
    EquipPosition owner;
    public void OnEnable()
    {
        owner = (EquipPosition)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        DrawDefaultInspector();

        GUILayout.Space(10);
        GUILayout.Label("Z 무기의 칼 끝");
        GUILayout.Label("X 무기의 넓은 면");
        GUILayout.Label("Y 무기의 날카로운 날");
        EditorGUILayout.BeginVertical();
        EditorGUILayout.EndVertical();

        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed || EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
        }
    }
}
#endif