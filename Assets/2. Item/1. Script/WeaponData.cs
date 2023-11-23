using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using FIMSpace.Generating;
using Palmmedia.ReportGenerator.Core;
using UnityEditorInternal;
using System.Drawing.Drawing2D;
using Unity.VisualScripting;
#endif


[System.Serializable]
public enum WEPONE_IDX
{
    GREAT_SWORD,
    KATANA,
    SOWRD,
    LONGSWORD,

    LEFTWEQPONSTART = 100,
    BOW,
    SHIELD,

    END,
}

[System.Serializable]
public enum WEAPON_TYPE
{
    RIGHT_HAND,
    LEFT_HAND,
    ALL_HAND,
}


[System.Serializable]
[CreateAssetMenu(fileName = "Weapon Data", menuName = "Scriptable Object/Weapon Data", order = int.MaxValue)]
public class WeaponData : ItemData
{
    [Header("Weapon Data")]
    [SerializeField] public WEAPON_TYPE WeaponType;
    [SerializeField] public WEPONE_IDX WeaponIDX;
    [SerializeField] public GameObject EquipPrefab;

    public int Atk;

    [HideInInspector][SerializeField] public List<WeaponAttackData> AtkNormalDatas = new();
    [HideInInspector][SerializeField] public List<WeaponAttackData> AtkStrongDatas = new();

    [HideInInspector][SerializeField] public List<WeaponAttackData> AtkNormalDatas_Both = new();
    [HideInInspector][SerializeField] public List<WeaponAttackData> AtkNormalDatas_Once = new();
    [HideInInspector][SerializeField] public List<WeaponAttackData> AtkStrongDatas_Both = new();
    [HideInInspector][SerializeField] public List<WeaponAttackData> AtkStrongDatas_Once = new();

    /// <summary>
    /// CR - Rate
    /// CR - Stat
    /// </summary>

    [HideInInspector][SerializeField] public List<tAttackKillData> AtkKillDatas = new();
    [HideInInspector][SerializeField] public List<tAttackKillData> AtkBackKillDatas = new();

    [HideInInspector][SerializeField] public float OnEquipDelay;
    [HideInInspector][SerializeField] public float OffEquipDelay;
    [HideInInspector][SerializeField] public float OnDeffDelay;

    /// Parry Property
    [HideInInspector][SerializeField] public float StartParryDelay;
    [HideInInspector][SerializeField] public float ExitParryDelay;

    public bool IsRigthHand() { return WeaponType == WEAPON_TYPE.RIGHT_HAND; }
    public bool IsLeftHand() { return WeaponType == WEAPON_TYPE.LEFT_HAND; }
    public bool HasBothAttack() { return AtkNormalDatas_Both.Count > 0; }
    public bool HasOnceAttack() { return AtkNormalDatas_Once.Count > 0; }
}


#if UNITY_EDITOR
[CustomEditor(typeof(WeaponData))]
public class WeaponDataEditor : Editor
{
    private ReorderableList atkStrongList;
    private ReorderableList atkKillList;
    private ReorderableList atkKillBackList;

    private WeaponData owner;

    private void OnEnable()
    {
        owner = (WeaponData)target;

        // --------------------------------------
        atkKillList = new ReorderableList(serializedObject, serializedObject.FindProperty("AtkKillDatas"), true, true, true, true);
        DrawList_AttackKillData(atkKillList, owner.AtkKillDatas, "Attack Kill Datas");

        atkKillBackList = new ReorderableList(serializedObject, serializedObject.FindProperty("AtkBackKillDatas"), true, true, true, true);
        DrawList_AttackKillData(atkKillBackList, owner.AtkBackKillDatas, "Attack Back Kill Datas");
    }

    public void DrawList_AttackData(ReorderableList list, List<tAttackData> dataList, string title)
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

            elementHeight += 4;
            elementHeight += EditorGUIUtility.singleLineHeight;
            elementHeight += 4;
            elementHeight += EditorGUIUtility.singleLineHeight;
            elementHeight += 4;
            elementHeight += EditorGUIUtility.singleLineHeight;

            for (int j = 0; j < atkData.tHitBoxDatas.Count; ++j)
            {
                elementHeight += 4;
                elementHeight += EditorGUIUtility.singleLineHeight;
                elementHeight += EditorGUIUtility.singleLineHeight;
            }

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

            rect.y += 4;
            rect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(rect, "Is Projectile");
            var projectileRect = rect;
            projectileRect.x += 60;
            projectileRect.width = 200;
            atkData.IsProjectile = EditorGUI.Toggle(projectileRect, GUIContent.none, atkData.IsProjectile);

            if (GUI.changed || EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
                Debug.Log("[WeaponData] Save ScriptableObject");
            }


            EditorGUI.BeginChangeCheck();

            rect.y += 4;
            rect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(rect, "Cancel");
            var cancelRect = rect;
            cancelRect.x += 60;
            cancelRect.width -= 60;
            atkData.CancelStartDelay = EditorGUI.Slider(cancelRect, GUIContent.none, atkData.CancelStartDelay, 0f, 1f);

            // 슬라이더를 그립니다.
            rect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(rect, "N A Input");
            var nextInputRect = rect;
            nextInputRect.x += 60;
            nextInputRect.width -= 60;
            EditorGUI.MinMaxSlider(nextInputRect, GUIContent.none, ref atkData.NextInputStartDelay, ref atkData.NextInputExitDelay, 0f, 1f);

            if (GUI.changed || EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
                Debug.Log("[WeaponData] Save ScriptableObject");
            }

            for (int i = 0; i < atkData.tHitBoxDatas.Count; i++)
            {
                var hitbox = atkData.tHitBoxDatas[i];

                rect.y += 4;
                rect.y += EditorGUIUtility.singleLineHeight;

                EditorGUI.LabelField(rect, "HitBox " + i);

                var hitboxLabelRect = rect;
                hitboxLabelRect.x += 60;
                hitboxLabelRect.width = 100;
                EditorGUI.LabelField(hitboxLabelRect, "Scale");

                var hitboxScaleRect = rect;
                hitboxScaleRect.x += hitboxLabelRect.width + 4;
                hitboxScaleRect.width = 200;
                hitbox.HitboxScale = EditorGUI.Vector3Field(hitboxScaleRect, GUIContent.none, hitbox.HitboxScale);

                rect.y += 4;
                rect.y += EditorGUIUtility.singleLineHeight;

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
        };
    }
    public void DrawList_AttackKillData(ReorderableList list, List<tAttackKillData> dataList, string title)
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
            float elementHeight = EditorGUIUtility.singleLineHeight;
            elementHeight += EditorGUIUtility.singleLineHeight;
            elementHeight += EditorGUIUtility.singleLineHeight;
            elementHeight += atkData.TickDamageDelay.Count * EditorGUIUtility.singleLineHeight;
            elementHeight += 10;
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
            EditorGUI.LabelField(rect, "Kill Data " + index);

            var buttonRect = rect;
            buttonRect.x += 80;
            buttonRect.width = 120;
            if (GUI.Button(buttonRect, "Tick Damage +"))
            {
                atkData.TickDamageDelay.Add(0f);
            }

            rect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(rect, "Atk Distance");
            var distanceRect = rect;
            distanceRect.x += 80;
            distanceRect.width -= 80;
            EditorGUI.MinMaxSlider(distanceRect, GUIContent.none, ref atkData.KillDistance, ref atkData.KillFindRange, 0f, 7f);

            rect.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(rect, "Exit Delay");
            var exitRect = rect;
            exitRect.x += 80;
            exitRect.width -= (80 + 30);
            atkData.ExitDelay = EditorGUI.Slider(exitRect, GUIContent.none, atkData.ExitDelay, 0f, 1f);

            var exitResetButtonRect = exitRect;
            exitResetButtonRect.x = exitRect.right + 10;
            exitResetButtonRect.width = 20;
            if (GUI.Button(exitResetButtonRect, "R"))
            {
                atkData.ExitDelay = 0.9f;
            }

            for (int i = 0; i < atkData.TickDamageDelay.Count; i++)
            {
                var damageTime = atkData.TickDamageDelay[i];

                rect.y += EditorGUIUtility.singleLineHeight;

                EditorGUI.LabelField(rect, "Damage " + i);

                var hitboxRect = rect;
                hitboxRect.x += 80;
                hitboxRect.width -= (80 + 30);

                EditorGUI.BeginChangeCheck();
                var ret = EditorGUI.Slider(hitboxRect, GUIContent.none, damageTime, 0f, 1f);

                if (GUI.changed)
                {
                    atkData.TickDamageDelay[i] = ret;
                }

                var hitboxRemoveButtonRect = hitboxRect;
                hitboxRemoveButtonRect.x = hitboxRect.right + 10;
                hitboxRemoveButtonRect.width = 20;
                if (GUI.Button(hitboxRemoveButtonRect, "X"))
                {
                    atkData.TickDamageDelay.RemoveAt(i);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
            }
        };
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

        EditorGUILayout.BeginVertical();

        Divider();
        BuildParryDelayInspector();
        Divider();

        BuildAttackList(owner.AtkNormalDatas, "Normal Attack List");

        Divider();

        BuildAttackList(owner.AtkStrongDatas, "Strong Attack List");

        Divider();

        BuildAttackList(owner.AtkNormalDatas_Both, "Both Attacks - Normal");

        Divider();

        // 앞잡 인스펙터
        atkKillList.DoLayoutList();

        Divider();

        // 뒤잡 인스펙터
        atkKillBackList.DoLayoutList();

        EditorGUILayout.EndVertical();

        // save --------------------------------------
        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();
        if (GUI.changed || EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
            Debug.Log("[WeaponData] Save ScriptableObject");
        }
    }

    public void Divider()
    {
        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }

    public void BuildParryDelayInspector()
    {
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Parry Delay", GUILayout.MaxWidth(70));
        EditorGUILayout.Space();
        EditorGUILayout.MinMaxSlider(ref owner.StartParryDelay, ref owner.ExitParryDelay, 0f, 1f);

        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
            Debug.Log("[WeaponData] Save ScriptableObject - Parry Delay");
        }
    }

    public void BuildAttackList(List<WeaponAttackData> list, string title)
    {
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

        EditorGUIHub.BeginVerticalWindow("Attack Datas Editor", padding_hor: 4);

        EditorGUILayout.Space(4);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Attack Data +"))
        {
            list.Add(null);
        }
        GUILayout.EndHorizontal();

        for (int i = 0; i < list.Count; ++i)
        {
            GUILayout.BeginHorizontal();
            list[i] = (WeaponAttackData)EditorGUILayout.ObjectField(list[i], typeof(WeaponAttackData));
            if (GUILayout.Button("X", GUILayout.Width(40)))
            {
                list.RemoveAt(i);
            }
            GUILayout.EndHorizontal();
        }

        EditorGUIHub.EndVerticalWindow(padding_hor: 4);
    }
}
#endif