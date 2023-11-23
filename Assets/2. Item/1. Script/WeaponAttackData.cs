using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditorInternal;


#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
[CreateAssetMenu(fileName = "Weapon Attack Data", menuName = "Scriptable Object/Weapon Attack Data", order = int.MaxValue)]
public class WeaponAttackData : ScriptableObject 
{
    public bool IsProjectile;
    public Animation AnimationData;

    [HideInInspector] public float CancelStartDelay;
    [HideInInspector] public float ExitDelay;

    [HideInInspector] public float SuperAmmorStartDelay;
    [HideInInspector] public float SuperAmmorExitDelay;

    [HideInInspector] public float NextInputStartDelay;
    [HideInInspector] public float NextInputExitDelay;

    [SerializeField][HideInInspector]
    public List<tHitBoxData> tHitBoxDatas = new();

    [Header("Npc Nead Data")]
    [SerializeField]
    public tMonAttackRequiedData RequiedData;
    public float DistanceToTarget;
}


#if UNITY_EDITOR
[CustomEditor(typeof(WeaponAttackData)), CanEditMultipleObjects]
public class WeaponAttackDataEditor : Editor
{
    private WeaponAttackData owner;

    private void OnEnable()
    {
        owner = (WeaponAttackData)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();

        owner.ExitDelay = EditorGUILayout.Slider("Exit Delay", owner.ExitDelay, 0, 1f);
        owner.CancelStartDelay = EditorGUILayout.Slider("Cancel Delay", owner.CancelStartDelay, 0, 1f);
        EditorGUILayout.MinMaxSlider("Super Ammor", ref owner.SuperAmmorStartDelay, ref owner.SuperAmmorExitDelay, 0, 1f);
        EditorGUILayout.MinMaxSlider("Next Input", ref owner.NextInputStartDelay, ref owner.NextInputExitDelay, 0, 1f);

        GUILayout.Space(10);
        GUILayout.BeginVertical("HITBOXS", "window");
        if(GUILayout.Button("+ Hitbox Data"))
        {
            owner.tHitBoxDatas.Add(new tHitBoxData());
        }
        for(int i = 0; i < owner.tHitBoxDatas.Count; ++i)
        {
            var hitboxData = owner.tHitBoxDatas[i];

            GUILayout.Space(10);
            hitboxData.ForceType = (HIT_FORCE_TYPE)EditorGUILayout.EnumPopup("Force Direction", hitboxData.ForceType);
            hitboxData.ParryedType = (PARRYED_TYPE)EditorGUILayout.EnumPopup("Parryed Event", hitboxData.ParryedType);
            EditorGUILayout.MinMaxSlider("Spawn Delay", ref hitboxData.StartDelay, ref hitboxData.ExitDelay, 0, 1f);
            hitboxData.ForceStrength = EditorGUILayout.Slider("Force Strength", hitboxData.ForceStrength, 0, 5f);
            hitboxData.ParryExitDelay = EditorGUILayout.Slider("Parry Exit Delay", hitboxData.ParryExitDelay, 0, 0.5f);
            hitboxData.HitboxScale = EditorGUILayout.Vector3Field("Scale", hitboxData.HitboxScale);
            hitboxData.HitboxOfset = EditorGUILayout.Vector3Field("Offset", hitboxData.HitboxOfset);
            if (GUILayout.Button("Delete"))
            {
                owner.tHitBoxDatas.RemoveAt(i);
            }
        }
        GUILayout.EndVertical();

        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed || EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
            Debug.Log("[Weapon Attack Data] Save ScriptableObject");
        }
    }
}
#endif