using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;




// Editor Main
[CustomEditor(typeof(AnimallData))]
public class AnimallDataEditor : Editor
{
    AnimallData value;

    private void OnEnable()
    {
        value = (AnimallData)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        BuildPatternDataList();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

    public void BuildPatternDataList()
    {
        EditorGUILayout.Space();

        GUILayout.BeginVertical();
        EditorGUILayout.Space(4);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Attack Patten Data List", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        if (GUILayout.Button("Pattern +", GUILayout.MaxWidth(100)))
        {
            value.AttackPatternDatas.Add(new tMonPatternData());
            Debug.Log("Attack + button clicked!");
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        for (int i = 0; i < value.AttackPatternDatas.Count; ++i)
        {
            var pattenData = value.AttackPatternDatas[i];
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical($"PATTERN [{i}]", "window");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space(4);
            EditorGUILayout.BeginVertical();

            // Create Header
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Attack +", GUILayout.MaxWidth(100)))
            {
                pattenData.AttackDatas.Add(new tMonAttackData());
                Debug.Log("[AnimallData] Add Attack Data");
            }
            EditorGUILayout.Space();
            if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
            {
                value.AttackPatternDatas.RemoveAt(i);
                Debug.Log("[AnimallData] Delete Attack Data");
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Active Percent", GUILayout.MaxWidth(100));
            EditorGUILayout.Space();
            pattenData.ActivePercent = EditorGUILayout.Slider(pattenData.ActivePercent, 0f, 1f);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Cool Time", GUILayout.MaxWidth(100));
            EditorGUILayout.Space();
            pattenData.CoolTime = EditorGUILayout.IntField((int)pattenData.CoolTime);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Requied Range In", GUILayout.MaxWidth(100));
            EditorGUILayout.Space();
            pattenData.RequiedData.RangeIn = EditorGUILayout.Slider(pattenData.RequiedData.RangeIn, 0f, 100f);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);


            for (int j = 0; j < pattenData.AttackDatas.Count; ++j)
            {
                var attackData = pattenData.AttackDatas[j];

                EditorGUILayout.Space();
                GUILayout.BeginVertical($"ATTACK [{j}]", "window");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space(4);
                EditorGUILayout.BeginVertical();

                // Create Header
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Hitbox +", GUILayout.MaxWidth(100)))
                {
                    attackData.tHitBoxDatas.Add(new tHitBoxData());
                    Debug.Log("[AnimallData] AttackData -> Add HitBox Data");
                }
                EditorGUILayout.Space();
                if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
                {
                    pattenData.AttackDatas.RemoveAt(j);
                    Debug.Log("[AnimallData] Delete Attack Data");
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(4);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Aniamation", GUILayout.MaxWidth(100));
                EditorGUILayout.Space();
                attackData.AnimationName = EditorGUILayout.TextField(attackData.AnimationName, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(4);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("SuperAmorDelay", GUILayout.MaxWidth(100));
                EditorGUILayout.Space();
                EditorGUILayout.MinMaxSlider(ref attackData.SuperArmorStartDelay, ref attackData.SuperArmorExitDelay, 0, 1f, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("NextAttackDelay", GUILayout.MaxWidth(100));
                EditorGUILayout.Space();
                EditorGUILayout.MinMaxSlider(ref attackData.NextStartDelay, ref attackData.NextExitDelay, 0, 1f, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Exit Delay", GUILayout.MaxWidth(100));
                EditorGUILayout.Space();
                attackData.ExitDelay = EditorGUILayout.Slider(attackData.ExitDelay, 0, 1f, GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();

                /// ---------------------------------------
                for (int k = 0; k < attackData.tHitBoxDatas.Count; ++k)
                {
                    var hitboxData = attackData.tHitBoxDatas[k];

                    if (k == 0)
                    {
                        EditorGUILayout.Space();
                    }

                    Color originalColor = GUI.color;
                    GUI.color = new Color(0.7f, 0.7f, 0.7f);
                    GUILayout.BeginVertical($"HITBOX [{k}]", "window");
                    GUI.color = originalColor;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.Space(4);
                    EditorGUILayout.BeginVertical();

                    // Create Header
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("X", GUILayout.MaxWidth(20)))
                    {
                        attackData.tHitBoxDatas.RemoveAt(k);
                        Debug.Log("[AnimallData] Delete HitBox Data");
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(4);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Is Canceled", GUILayout.MaxWidth(100));
                    EditorGUILayout.Space();
                    hitboxData.ParryedType = (PARRYED_TYPE)EditorGUILayout.EnumPopup(hitboxData.ParryedType, GUILayout.ExpandWidth(true));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(4);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Force Direction", GUILayout.MaxWidth(100));
                    EditorGUILayout.Space();
                    hitboxData.ForceType = (HIT_FORCE_TYPE)EditorGUILayout.EnumPopup(hitboxData.ForceType, GUILayout.ExpandWidth(true));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(4);

                
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("HitBox Scale", GUILayout.MaxWidth(100));
                    EditorGUILayout.Space();
                    hitboxData.HitboxScale = EditorGUILayout.Vector3Field("", hitboxData.HitboxScale, GUILayout.ExpandWidth(true));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Spawn Delay", GUILayout.MaxWidth(100));
                    EditorGUILayout.Space();
                    originalColor = GUI.color;
                    GUI.color = Color.red;
                    EditorGUILayout.MinMaxSlider(ref hitboxData.StartDelay, ref hitboxData.ExitDelay, 0, 1f, GUILayout.ExpandWidth(true));
                    EditorGUILayout.EndHorizontal();
                    GUI.color = originalColor;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Parry Delay", GUILayout.MaxWidth(100));
                    EditorGUILayout.Space();
                    hitboxData.ParryExitDelay = EditorGUILayout.Slider(hitboxData.ParryExitDelay, 0, 1f, GUILayout.ExpandWidth(true));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Force Strength", GUILayout.MaxWidth(100));
                    EditorGUILayout.Space();
                    hitboxData.ForceStrength = EditorGUILayout.Slider(hitboxData.ForceStrength, 0, 10, GUILayout.ExpandWidth(true));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(4);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(4);
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(4);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(4);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            if (GUI.changed || EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
                Debug.Log("[AniamllData] Save ScriptableObject");
            }
        }

        GUILayout.EndVertical();
    }


    /// ------------------------------------
    /// <summary>
    /// Editor Func
    /// </summary>
  
}
