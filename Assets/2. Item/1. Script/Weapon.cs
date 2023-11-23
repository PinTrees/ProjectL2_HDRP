using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class Weapon : MonoBehaviour
{
    [HideInInspector]
    public WeaponController controller;

    public Transform LaunchTransform;
    public string ProjectileObjectName;

    [Header("Data")]
    public WeaponData WeaponData;
    public EQUIP_TRANSFORM_TYPE EquipOnTransform;
    public EQUIP_TRANSFORM_TYPE EquipOffTransform;
    public EQUIP_TRANSFORM_TYPE GuardOnTransform;
    public EQUIP_TRANSFORM_TYPE ParryOnTransform;

    public Transform effectTransform;

    private Collider collider;
    private XWeaponTrailController trailEffect;
    Dictionary<WEAPON_POSITION_TYPE, WeaponPosition> weaponPositions = new();

    #region property
    [HideInInspector] public bool IsEquiped = false;
    #endregion

    bool __init = false;

    public void Init()
    {
        __init = true;
        IsEquiped = false;

        collider = GetComponent<Collider>();
        trailEffect = GetComponentInChildren<XWeaponTrailController>();

        weaponPositions.Clear();
        weaponPositions[WEAPON_POSITION_TYPE.EQUIP] = null;
        weaponPositions[WEAPON_POSITION_TYPE.UNEQUIP] = null;
        weaponPositions[WEAPON_POSITION_TYPE.GUARD] = null;
        weaponPositions[WEAPON_POSITION_TYPE.PARRY] = null;

        var weaponPosTmp = transform.GetComponentsInChildren<WeaponPosition>().ToList();
        weaponPosTmp.ForEach(e => weaponPositions[e.PosType] = e);

        foreach(var weaponPos in weaponPositions.Values) { weaponPos?.Init(); }
    }
    
    public void OnLaunch()
    {
        //var projectile = ObjectPoolMgr.GetI.GetObject(ProjectileObjectName).GetComponent<ProjectileObject>();
        //projectile.Enter(LaunchTransform);
    }

    public virtual void OnEquip() 
    {
        if (!__init) Init();

        IsEquiped = true;
        weaponPositions[WEAPON_POSITION_TYPE.EQUIP]?.SetPosition();

        if (WeaponData.IsLeftHand())
            controller.CurrentEquipWeapon_Left = this;
        else if(WeaponData.IsRigthHand())
            controller.CurrentEquipWeapon_Right = this;
    }
    public virtual void OffEquip()
    {
        if (!__init) Init();

        IsEquiped = false;
        weaponPositions[WEAPON_POSITION_TYPE.UNEQUIP]?.SetPosition();

        if (WeaponData.IsLeftHand())
            controller.CurrentEquipWeapon_Left = null;
        else if (WeaponData.IsRigthHand())
            controller.CurrentEquipWeapon_Right = null;
    }

    public virtual void OnGuard()
    {
        if (!__init) Init();
        weaponPositions[WEAPON_POSITION_TYPE.GUARD]?.SetPosition();
    }

    public void OnDrop()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
    }

    public void OnTrailEffect()
    {
        trailEffect.Enter();
    }
    public void OffTrailEffect()
    {
        trailEffect.Exit();
    }

    public void OnHit()
    {
    }

    void Update()
    {
    }

#if UNITY_EDITOR
    private void _Editor_Init() 
    {
        controller = GetComponentInParent<WeaponController>();
        controller._Editor_Init();

        weaponPositions.Clear();
        weaponPositions[WEAPON_POSITION_TYPE.EQUIP] = null;
        weaponPositions[WEAPON_POSITION_TYPE.UNEQUIP] = null;
        weaponPositions[WEAPON_POSITION_TYPE.GUARD] = null;
        weaponPositions[WEAPON_POSITION_TYPE.PARRY] = null;

        var weaponPosTmp = transform.GetComponentsInChildren<WeaponPosition>().ToList();
        weaponPosTmp.ForEach(e => weaponPositions[e.PosType] = e);
    }
    public void _Editor_OnEquip()
    {
        _Editor_Init();
        weaponPositions[WEAPON_POSITION_TYPE.EQUIP]?._Editor_SetPosition();
    }
    public void _Editor_OffEquip()
    {
        _Editor_Init();
        weaponPositions[WEAPON_POSITION_TYPE.UNEQUIP]?._Editor_SetPosition();
    }
    public void _Editor_Guard()
    {
        _Editor_Init();

    }
    public void _Editor_Parry()
    {
        _Editor_Init();

    }
#endif
}


#if UNITY_EDITOR
[CustomEditor(typeof(Weapon)), CanEditMultipleObjects]
public class WeaponEditor : Editor
{
    private Weapon owner;

    private void OnEnable()
    {
        owner = (Weapon)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        DrawDefaultInspector();
        
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Equip"))
        {
            owner._Editor_OnEquip();
        }
        if(GUILayout.Button("UnEquip"))
        {
            owner._Editor_OffEquip();
        }
        if(GUILayout.Button("Guard"))
        {
            owner._Editor_Guard();
        }
        if(GUILayout.Button("Parry"))
        {
            owner._Editor_Parry();
        }
        GUILayout.EndHorizontal();

        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed || EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
            Debug.Log("[Weapon] Save Mono");
        }
    }
}
#endif