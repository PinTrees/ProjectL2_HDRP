using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class WeaponController : MonoBehaviour
{
    [Space(20)]
    [HideInInspector] public List<WeaponData> Righthand = new(5);       /// 오른손 착용 퀵슬롯
    [HideInInspector] public List<WeaponData> Lefthand = new(5);        /// 왼손 착용 퀵슬롯

    #region Property
    [HideInInspector] public Weapon     SelectLeftWeapon;           /// 왼손 다음 착용예정 무기
    [HideInInspector] public Weapon     SelectRightWeapon;          /// 오른손 다음 착용예정 무기
    [HideInInspector] public Weapon     CurrentEquipWeapon_Right;   /// 현재 착용중인 오른손 무기
    [HideInInspector] public Weapon     CurrentEquipWeapon_Left;    /// 현재 착용중인 왼손 무기
    #endregion

    Dictionary<EQUIP_TRANSFORM_TYPE, EquipPosition> equipTransformMap = new();
    public object owner;
    int     current_right_index;
    int     current_left_index;

    #region Property Func
    public Transform GetEquipTransform(EQUIP_TRANSFORM_TYPE type) { return equipTransformMap[type].transform; }
  
    public int GetEquipWeaponIdx_Right() 
    {
        if (CurrentEquipWeapon_Right) return (int)CurrentEquipWeapon_Right.WeaponData.WeaponIDX;
        return 0;
    }
    public int GetEquipWeaponIdx_Left()
    {
        if (CurrentEquipWeapon_Left) return (int)CurrentEquipWeapon_Left.WeaponData.WeaponIDX;
        return 0;
    }
    public int GetSelectWeaponIdx_Right()
    {
        if (SelectRightWeapon) return (int)SelectRightWeapon.WeaponData.WeaponIDX;
        return 0;
    }
    public int GetSelectWeaponIdx_Left()
    {
        if (SelectLeftWeapon) return (int)SelectLeftWeapon.WeaponData.WeaponIDX;
        return 0;
    }

    /// <summary>
    /// 이 함수는 무기 보유정보를 반환합니다.
    /// </summary>
    /// <returns></returns>
    public bool HasWeapon()
    {
        foreach (var w in Righthand)
            if (w != null) return true;

        foreach (var w in Lefthand)
            if (w != null) return true;

        return false;
    }

    /// <summary>
    /// 이 함수는 현재 무기착용 여부를 반환합니다.
    /// </summary>
    /// <returns></returns>
    public bool IsEquipWeapon()
    {
        if (CurrentEquipWeapon_Right) return true;
        if (CurrentEquipWeapon_Left) return true;

        return false;
    }
    #endregion


    #region Init Func
    public void Init(object owner)
    {
        this.owner = owner;

        equipTransformMap.Clear();
        var equipPositions = GetComponentsInChildren<EquipPosition>().ToList();
        equipPositions.ForEach(e => equipTransformMap[e.EquipTransformType] = e);

        current_left_index = 0;
        current_right_index = 0;

        if (owner is Player)
        {
            LoadData();
        }

        if (owner is Npc)
        {
            SelectRightWeapon = SelectWeapon(Righthand[current_right_index]);
            SelectLeftWeapon = SelectWeapon(Lefthand[current_left_index]);
        }
    }
    public void LoadData()
    {
        Righthand.Clear();
        Lefthand.Clear();

        for(int i = 0; i < 5; ++i)
        {
            Righthand.Add(null);
            Lefthand.Add(null);
        }

        SelectLeftWeapon = null;
        SelectRightWeapon = null;

        CurrentEquipWeapon_Left = null;
        CurrentEquipWeapon_Right = null;
    }
    #endregion



    #region Point Func
    /// <summary>
    /// 이 함수는 오른손 무기 대기열의 다음 무기를 선택합니다.
    /// </summary>
    public void ChangeSelectWeapon_Right()
    {
        current_right_index++;
        if (current_right_index >= 5)
            current_right_index = 0;

        if(Righthand[current_right_index] != null)
        {
            SelectRightWeapon = SelectWeapon(Righthand[current_right_index]);
        }
        else
        {
            SelectRightWeapon = null;
        }
    }
    
    /// <summary>
    /// 이 함수는 왼손 무기 대기열의 다음 무기를 선택합니다.
    /// </summary>
    public void ChangeSelectWeapon_Left()
    {
        current_left_index++;
        if (current_left_index >= 5)
            current_left_index = 0;

        if (Lefthand[current_left_index] != null)
        {
            SelectLeftWeapon = SelectWeapon(Lefthand[current_left_index]);
        }
        else
        {
            SelectLeftWeapon = null;
        }
    }

    /// <summary>
    /// 이 함수는 현재 선택된 무기오브젝트를 소환하고 정보를 반환합니다.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    Weapon SelectWeapon(WeaponData weaponData)
    {
        if(weaponData == null) return null;

        var weapon = Instantiate(weaponData.EquipPrefab).GetComponent<Weapon>();
        weapon.controller = this;
        weapon.OffEquip();

        if (owner is Player)
        {
            (owner as Player).AI.animator.SetFloat("idxWeapon", GetEquipWeaponIdx_Right());
        }

        return weapon;
    }

   
    public void ChangeWeaponQuickSlot(WEAPON_TYPE type, int index, Item item)
    {
        if(type == WEAPON_TYPE.RIGHT_HAND)
        {
            Righthand[index] = item.data as WeaponData;

            if (SelectRightWeapon == null)
            {
                current_right_index = index;
                SelectRightWeapon = SelectWeapon(Righthand[index]);
            }
        }
        else if(type == WEAPON_TYPE.LEFT_HAND)
        {
            Lefthand[index] = item.data as WeaponData;

            if (SelectLeftWeapon == null)
            {
                current_left_index = index;
                SelectLeftWeapon = SelectWeapon(Lefthand[index]);
            }
        }
    }
    #endregion

#if UNITY_EDITOR
    public bool _Editor_IsNotInit()
    {
        if (Righthand.Count < 5) return true;
        if (Lefthand.Count < 5) return true;

        return false;
    }
    public void _Editor_Init()
    {
        equipTransformMap.Clear();
        var equipPositions = GetComponentsInChildren<EquipPosition>().ToList();
        equipPositions.ForEach(e => equipTransformMap[e.EquipTransformType] = e);

        Righthand.Clear();
        Lefthand.Clear();
   
        for (int i = 0; i < 5; ++i)
        {
            Righthand.Add(null);
            Lefthand.Add(null);
        }

        SelectLeftWeapon = null;
        SelectRightWeapon = null;
        CurrentEquipWeapon_Left = null;
        CurrentEquipWeapon_Right = null;
    }
#endif
}


#if UNITY_EDITOR
[CustomEditor(typeof(WeaponController)), CanEditMultipleObjects]
public class WeaponControllerEditor : Editor
{
    WeaponController owner;

    private void OnEnable()
    {
        owner = (WeaponController)target;
        if (owner._Editor_IsNotInit()) owner._Editor_Init();
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();

        GUILayout.BeginVertical("RightHand Weapons", "window");
        for(int i = 0; i < owner.Righthand.Count; ++i)
        {
            owner.Righthand[i] = (WeaponData)EditorGUILayout.ObjectField(owner.Righthand[i], typeof(WeaponData));
        }
        GUILayout.EndVertical();

        GUILayout.Space(10);
        GUILayout.BeginVertical("RightHand Weapons", "window");
        for(int i = 0; i < owner.Lefthand.Count; ++i)
        {
            owner.Lefthand[i] = (WeaponData)EditorGUILayout.ObjectField(owner.Lefthand[i], typeof(WeaponData));
        }
        GUILayout.EndVertical();

        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed || EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
            Debug.Log("[Weapon Controller] Save Mono");
        }
    }
}
#endif