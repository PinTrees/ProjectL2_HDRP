using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Drawing2D;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

#if UNITY_EDITOR
[CustomEditor(typeof(MonsterData))]
public class MonsterDataEditor : Editor
{
    private ReorderableList atkNomarlList;
    private ReorderableList atkPatternList;

    MonsterData value;

    private void OnEnable()
    {
        value = (MonsterData)target;

        atkNomarlList = new ReorderableList(serializedObject, serializedObject.FindProperty("AttackNomarlDatas"), true, true, true, true);
        DrawList_AttackData(atkNomarlList, value.AttackNomarlDatas, "AttackNomarlDatas");

        atkPatternList = new ReorderableList(serializedObject, serializedObject.FindProperty("AttackPatternDatas"), true, true, true, true);
        DrawList_Pattern(atkPatternList, value.AttackPatternDatas, "Pattern Datas");
    }

    public void DrawList_AttackData(ReorderableList list, List<tMonAttackData> dataList, string title)
    {
        list.drawHeaderCallback = rect => { EditorGUI.LabelField(rect, title); };

        list.onAddCallback = list =>
        {
            ReorderableList.defaultBehaviours.DoAddButton(list);
            int index = list.serializedProperty.arraySize - 1;
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
        };

        list.elementHeightCallback = (int index) =>
        {
            var atkData = dataList[index];
            float elementHeight = 68;

            elementHeight += atkData.tHitBoxDatas.Count * EditorGUIUtility.singleLineHeight;
            return elementHeight;
        };

        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            EditorGUI.BeginChangeCheck();

            var atkData = dataList[index];

            // 수직 정렬 시작
            rect.y += 2;
            rect.height = EditorGUIUtility.singleLineHeight;

            // 엘리먼트의 제목을 그립니다.
            EditorGUI.LabelField(rect, "Atk Data " + index);

            var buttonRect = rect;
            buttonRect.x += 80;
            buttonRect.width = 80;
            if (GUI.Button(buttonRect, "HitBox +"))
            {
                atkData.tHitBoxDatas.Add(new tHitBoxData());
            }

            // 슬라이더를 그립니다.
            rect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(rect, "Next State");
            var nextInputRect = rect;
            nextInputRect.x += 60;
            nextInputRect.width -= 60;
            EditorGUI.MinMaxSlider(nextInputRect, GUIContent.none, ref atkData.NextStartDelay, ref atkData.NextExitDelay, 0f, 1f);

            for (int i = 0; i < atkData.tHitBoxDatas.Count; i++)
            {
                var hitbox = atkData.tHitBoxDatas[i];

                rect.y += EditorGUIUtility.singleLineHeight;

                EditorGUI.LabelField(rect, "HitBox " + i);

                Color originalColor = GUI.color;
                GUI.color = Color.red;

                var hitboxRect = rect;
                hitboxRect.x += 60;
                hitboxRect.width -= (60 + 30);
                EditorGUI.MinMaxSlider(hitboxRect, GUIContent.none, ref hitbox.StartDelay, ref hitbox.ExitDelay, 0f, 1f);

                GUI.color = originalColor;

                var hitboxRemoveButtonRect = hitboxRect;
                hitboxRemoveButtonRect.x = hitboxRect.right + 10;
                hitboxRemoveButtonRect.width = 20;
                if (GUI.Button(hitboxRemoveButtonRect, "X"))
                {
                    atkData.tHitBoxDatas.Remove(hitbox);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
            }
        };
    }
    public void DrawList_Pattern(ReorderableList list, List<tMonPatternData> dataList, string title)
    {
        list.elementHeightCallback = (int index) =>
        {
            var patterData = dataList[index];
            float elementHeight = EditorGUIUtility.singleLineHeight + 6;

            for (int i = 0; i < patterData.AttackDatas.Count; ++i)
            {
                elementHeight += 4;

                elementHeight += EditorGUIUtility.singleLineHeight * 3 + 4;
                elementHeight += EditorGUIUtility.singleLineHeight;
                elementHeight += EditorGUIUtility.singleLineHeight;
                elementHeight += 4;
                elementHeight += EditorGUIUtility.singleLineHeight;

                for (int j = 0; j < patterData.AttackDatas[i].tHitBoxDatas.Count; ++j)
                {
                    elementHeight += 4;
                    elementHeight += EditorGUIUtility.singleLineHeight;
                    elementHeight += 4;
                    elementHeight += EditorGUIUtility.singleLineHeight;
                    elementHeight += 4;
                    elementHeight += EditorGUIUtility.singleLineHeight;
                    elementHeight += 4;
                    elementHeight += EditorGUIUtility.singleLineHeight;
                }
            }

            elementHeight += 20;

            return elementHeight;
        };

        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => 
        {
            EditorGUI.BeginChangeCheck();

            rect.y += 6;
            rect.height = EditorGUIUtility.singleLineHeight;

            var patternData = value.AttackPatternDatas[index];
            EditorGUI.LabelField(rect, "Patterm [" + index + "]");

            var buttonRect = rect;
            buttonRect.x += 80;
            buttonRect.width = 92;
            buttonRect.height = 20;
            if (GUI.Button(buttonRect, "Attack +"))
            {
                patternData.AttackDatas.Add(new tMonAttackData());
            }

            var activeLabelRect = rect;
            activeLabelRect.x = buttonRect.x + 90 + 10;
            activeLabelRect.width = 40;
            EditorGUI.LabelField(activeLabelRect, "Active");

            var acriveSliderRect = rect;
            acriveSliderRect.x = activeLabelRect.x + 40 + 10;
            acriveSliderRect.width = 150;

            patternData.ActivePercent = EditorGUI.Slider(acriveSliderRect, GUIContent.none, patternData.ActivePercent, 0f, 1f);


            for (int i = 0; i < patternData.AttackDatas.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                rect.y += 4;
                rect.y += EditorGUIUtility.singleLineHeight;

                var attackData = patternData.AttackDatas[i];
                EditorGUI.LabelField(rect, "AttacK [" + i + "]");

                var removeBtnRect = rect;
                removeBtnRect.x += 80;
                removeBtnRect.width = 20;
                removeBtnRect.height = 20;
                if (GUI.Button(removeBtnRect, "X"))
                {
                    patternData.AttackDatas.RemoveAt(i);
                }

                var addHitboxBtnRect = removeBtnRect;
                addHitboxBtnRect.x += 20 + 2;
                addHitboxBtnRect.width = 70;
                addHitboxBtnRect.height = 20;
                if (GUI.Button(addHitboxBtnRect, "HitBox +"))
                {
                    attackData.tHitBoxDatas.Add(new tHitBoxData());
                }

                rect.y += 4;

                rect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(rect, "Animation");
                var animationRect = rect;
                animationRect.x += 80;
                animationRect.width = 92;
                attackData.AnimationName = EditorGUI.TextField(animationRect, attackData.AnimationName);

                var exitLabelRect = rect;
                exitLabelRect.x = animationRect.x + 90 + 10;
                exitLabelRect.width = 40;
                EditorGUI.LabelField(exitLabelRect, "Exit");
                
                var exitSliderRect = rect;
                exitSliderRect.x = exitLabelRect.x + 40 + 10;
                exitSliderRect.width = 150;
                attackData.ExitDelay = EditorGUI.Slider(exitSliderRect, attackData.ExitDelay, 0f, 1f);


                // Super Amor GUI --------------------------------
                rect.y += 4;
                rect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(rect, "SuperAmor");
                var superAmorRect = rect;
                superAmorRect.x += 80;
                superAmorRect.width -= 80;
                EditorGUI.MinMaxSlider(superAmorRect, GUIContent.none, ref attackData.SuperArmorStartDelay, ref attackData.SuperArmorExitDelay, 0f, 1f);


                // Next Attack Delay GUI --------------------------------
                rect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(rect, "Next Attack");
                var nextRect = rect;
                nextRect.x += 80;
                nextRect.width -= 80;
                EditorGUI.MinMaxSlider(nextRect, GUIContent.none, ref attackData.NextStartDelay, ref attackData.NextExitDelay, 0f, 1f);

                rect.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(rect, "Required");

                var requiredLabelRect = rect;
                requiredLabelRect.x += 80;
                requiredLabelRect.width = 40;
                EditorGUI.LabelField(requiredLabelRect, "Range");

                var requiredRect = rect;
                requiredRect.x = requiredLabelRect.x + 40 + 10;
                requiredRect.width = 150;

                attackData.CheckData.AttackRange = EditorGUI.Slider(requiredRect, GUIContent.none, attackData.CheckData.AttackRange, 0f, 20f);

                for (int h = 0; h < attackData.tHitBoxDatas.Count; ++h)
                {
                    var hitBoxData = attackData.tHitBoxDatas[h];

                    rect.y += 4;
                    rect.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.LabelField(rect, "HitBox [" + h + "]");

                    var hitboxScaleRect = rect;
                    hitboxScaleRect.x += 80;
                    hitboxScaleRect.width = 350;
                    hitBoxData.HitboxScale = EditorGUI.Vector3Field(hitboxScaleRect, "Scale", hitBoxData.HitboxScale);

                    rect.y += 2;
                    rect.y += EditorGUIUtility.singleLineHeight;
                    var removeHitboxBtnRect = rect;
                    removeHitboxBtnRect.x = rect.right - 20;
                    removeHitboxBtnRect.width = 20;
                    removeHitboxBtnRect.height = 20;
                    if (GUI.Button(removeHitboxBtnRect, "X"))
                    {
                        attackData.tHitBoxDatas.RemoveAt(h);
                    }

                    Color originalColor = GUI.color;
                    GUI.color = Color.red;

                    var hitboxRect = rect;
                    hitboxRect.x += 80;
                    hitboxRect.width -= 80 + 20 + 5;
                    EditorGUI.MinMaxSlider(hitboxRect, GUIContent.none, ref hitBoxData.StartDelay, ref hitBoxData.ExitDelay, 0f, 1f);
                    GUI.color = originalColor;


                    rect.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.LabelField(rect, "Parry");

                    originalColor = GUI.color;
                    GUI.color = Color.green;
                    
                    var parryRect = rect;
                    parryRect.x += 80;
                    parryRect.width = 200;
                    hitBoxData.ParryExitDelay = EditorGUI.Slider(parryRect, hitBoxData.ParryExitDelay, 0f, 1f);
                    
                    GUI.color = originalColor;

                    var parryedTypeRect = parryRect;
                    parryedTypeRect.x += 200 + 4;
                    parryedTypeRect.width = 100;
                    hitBoxData.ParryedType = (PARRYED_TYPE)EditorGUI.EnumPopup(parryedTypeRect, hitBoxData.ParryedType);

                    rect.y += 4;
                    rect.y += EditorGUIUtility.singleLineHeight;
                    EditorGUI.LabelField(rect, "Force");

                    var forceStrRect = rect;
                    forceStrRect.x += 80;
                    forceStrRect.width = 200;
                    hitBoxData.ForceStrength = EditorGUI.Slider(forceStrRect, hitBoxData.ForceStrength, 0f, 10f);

                    var forceTypeRect = forceStrRect;
                    forceTypeRect.x += 200 + 4;
                    forceTypeRect.width = 100;
                    hitBoxData.ForceType = (HIT_FORCE_TYPE)EditorGUI.EnumPopup(forceTypeRect, hitBoxData.ForceType);
                }

                rect.y += 10;

                if (GUI.changed || EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(target);
                    Debug.Log("[MonsterData] Save ScriptableObject");
                }
            }
        };

        list.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, title);
        };
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();

        serializedObject.Update();

        EditorGUILayout.BeginVertical(); 
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Run Range 0 ~ 100");
        EditorGUILayout.MinMaxSlider(ref value.RunExitRange, ref value.RunStartRange, 0, 100);

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        atkNomarlList.DoLayoutList();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        atkPatternList.DoLayoutList();

        EditorGUILayout.Space();
        EditorGUILayout.EndVertical();

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
#endif