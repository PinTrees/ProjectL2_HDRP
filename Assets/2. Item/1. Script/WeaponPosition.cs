using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


#if UNITY_EDITOR
using UnityEditor;
#endif


public class WeaponPosition : MonoBehaviour
{
    [Space(10)]
    public WEAPON_POSITION_TYPE PosType;
    Weapon owner;

    public void Init()
    {
        owner = GetComponentInParent<Weapon>();
    }

    public void SetPosition()
    {
        Transform targetTransform = null;

        if (PosType == WEAPON_POSITION_TYPE.EQUIP) targetTransform = owner.controller.GetEquipTransform(owner.EquipOnTransform);
        if (PosType == WEAPON_POSITION_TYPE.UNEQUIP) targetTransform = owner.controller.GetEquipTransform(owner.EquipOffTransform);
        if (targetTransform == null) return;

        transform.parent.SetParent(targetTransform, true);
        transform.parent.localPosition = Vector3.zero;
        transform.parent.eulerAngles = Vector3.zero;

        // Z 단일축 사용 시
        if (VectorExtentions.IsAxisUsedZ(transform.localEulerAngles))
        {
            transform.parent.localEulerAngles = new Vector3(0, 0, 360 - transform.localEulerAngles.z);
        }

        // XY 축 사용 시
        if (VectorExtentions.IsAxisUsedXY(transform.localEulerAngles))
        {
            if (transform.localEulerAngles.x >= 250)
                transform.parent.localEulerAngles = new Vector3(0, 360 - transform.localEulerAngles.x, 360 - transform.localEulerAngles.y);
            else
                transform.parent.localEulerAngles = new Vector3(0, transform.localEulerAngles.x, transform.localEulerAngles.y);
        }

        var direction = transform.parent.position - transform.position;
        transform.parent.position = targetTransform.position + direction;
    }

#if UNITY_EDITOR
    public void _Editor_Init()
    {
        owner = GetComponentInParent<Weapon>();
    }

    public void _Editor_SetPosition()
    {
        _Editor_Init();

        Transform targetTransform = null;

        if (PosType == WEAPON_POSITION_TYPE.EQUIP) targetTransform = owner.controller.GetEquipTransform(owner.EquipOnTransform);
        if (PosType == WEAPON_POSITION_TYPE.UNEQUIP) targetTransform = owner.controller.GetEquipTransform(owner.EquipOffTransform);
        if (targetTransform == null) return;

        transform.parent.SetParent(targetTransform, true);
        transform.parent.localPosition = Vector3.zero;
        transform.parent.localEulerAngles = Vector3.zero;

        // Z 단일축 사용 시
        if (VectorExtentions.IsAxisUsedZ(transform.localEulerAngles))
        {
            transform.parent.localEulerAngles = new Vector3(0, 0, 360 - transform.localEulerAngles.z);
        }

        // XY 축 사용 시
        if (VectorExtentions.IsAxisUsedXY(transform.localEulerAngles))
        {
            if(transform.localEulerAngles.x >= 250)
                transform.parent.localEulerAngles = new Vector3(0, 360 - transform.localEulerAngles.x, 360 - transform.localEulerAngles.y);
            else
                transform.parent.localEulerAngles = new Vector3(0, transform.localEulerAngles.x, transform.localEulerAngles.y);
        }

        var direction = transform.parent.position - transform.position;
        transform.parent.position = targetTransform.position + direction;
    }
#endif

    public void OnDrawGizmos()
    {
        if (PosType == WEAPON_POSITION_TYPE.EQUIP)
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        else if(PosType == WEAPON_POSITION_TYPE.UNEQUIP)
            Gizmos.color = new Color(0f, 0f, 1f, 0.3f);

        Gizmos.matrix = transform.localToWorldMatrix; 

        Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.3f);
        //Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.3f);
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(WeaponPosition))]
public class WeaponPositionEditor : Editor
{
    WeaponPosition owner;
    public void OnEnable()
    {
        owner = (WeaponPosition)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        DrawDefaultInspector();

        GUILayout.Space(10);
        GUILayout.Label("Parent");
        GUILayout.Label($"World Angle: {owner.transform.parent.eulerAngles}");
        GUILayout.Label($"Local Euler Angle: {owner.transform.parent.localEulerAngles}");
        GUILayout.Space(10);
        GUILayout.Label($"World Angle: {owner.transform.eulerAngles}");
        GUILayout.Label($"Local Euler Angle: {owner.transform.localEulerAngles}");

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