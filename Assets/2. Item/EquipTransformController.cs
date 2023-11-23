using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;



#if UNITY_EDITOR
using UnityEditor;
#endif


public class EquipTransformController : MonoBehaviour
{
    List<EquipPosition> equipPos = new();

    [SerializeField] Transform Spine01;
    [SerializeField] Transform Spine02;
    [SerializeField] Transform Spine03;
    [SerializeField] Transform RightHand;
    [SerializeField] Transform LeftHand;
    [SerializeField] Transform LeftArm;

    [HideInInspector]
    public List<EquipPosition> equipPosExport = new();

    void Start()
    {
        
    }

    void Update()
    {

    }

#if UNITY_EDITOR
    public void _Editor_InitSpine()
    {
        Spine01 = null;
        Spine02 = null;
        Spine03 = null;
        RightHand = null;
        LeftHand = null;
        LeftArm = null;

        var owner = transform.parent;
        var transforms = owner.GetComponentsInChildren<Transform>().ToList();

        Spine01 = transforms.Where(e => e.name.ToLower().Replace("_", "").Replace("0", "").Replace(".", "").Contains("spine1")).First();
        Spine02 = transforms.Where(e => e.name.ToLower().Replace("_", "").Replace("0", "").Replace(".", "").Contains("spine2")).First();
        Spine03 = transforms.Where(e => e.name.ToLower().Replace("_", "").Replace("0", "").Replace(".", "").Contains("spine3")).First();
        LeftHand = transforms.Where(e => e.name.ToLower().Replace("_", "").Replace("0", "").Replace(".", "").Contains("middle1l")).First();
        RightHand = transforms.Where(e => e.name.ToLower().Replace("_", "").Replace("0", "").Replace(".", "").Contains("middle1r")).First();
        LeftArm = transforms.Where(e => e.name.ToLower().Replace("_", "").Replace("0", "").Replace(".", "").Contains("twist1l")).First();
    }

    public Transform _Editor_FindSpine(EQUIP_TRANSFORM_PARENT_TYPE type)
    {
        if (type == EQUIP_TRANSFORM_PARENT_TYPE.SPINE_01) return Spine01;
        if (type == EQUIP_TRANSFORM_PARENT_TYPE.SPINE_02) return Spine02;
        if (type == EQUIP_TRANSFORM_PARENT_TYPE.SPINE_03) return Spine03;
        if (type == EQUIP_TRANSFORM_PARENT_TYPE.LEFT_HAND) return LeftHand;
        if (type == EQUIP_TRANSFORM_PARENT_TYPE.RIGHT_HAND) return RightHand;
        if (type == EQUIP_TRANSFORM_PARENT_TYPE.LEFT_ARM) return LeftArm;
        return null;
    }

    public void _Editor_SpinSetup(string animationName)
    {
        Animator animator = GetComponentInParent<Animator>();
        animator.SetBool("isIdle", false);
        animator.SetBool("isWalk", false);
        animator.SetBool("isRun", false);

        animator.SetBool(animationName, true);
        animator.SetBool("isBattle", false);
        animator.SetTrigger("StateOn");

        animator.Update(Time.deltaTime);
    }

    public void _Editor_ExportRemove()
    {
        equipPosExport.ForEach(e =>
        {
            DestroyImmediate(e.gameObject);
        });
        equipPosExport.Clear();
    }

    public void _Editor_Export()
    {
        _Editor_ExportRemove();

        equipPos.Clear();
        equipPos = transform.parent.GetComponentsInChildren<EquipPosition>().ToList();

        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;

        equipPos.ForEach(e =>
        {
            var worldPosition = e.transform.position;
            var worldEulerAngle = e.transform.eulerAngles;

            var spawn = GameObject.Instantiate(e.gameObject);
            spawn.transform.SetParent(transform, true);
            spawn.transform.position = worldPosition;
            spawn.transform.eulerAngles = worldEulerAngle;
             
            equipPosExport.Add(spawn.GetComponent<EquipPosition>());
        });
    }

    public void _Editor_Setting()
    {
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;

        var owner = transform.parent;

        _Editor_SpinSetup("isIdle");
        _Editor_SpinSetup("isIdle");
        _Editor_SpinSetup("isIdle");
        var animator = GetComponentInParent<Animator>();
        animator.Play(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, -1, 0.9f);

        equipPosExport.ForEach(e =>
        {
            var parent = _Editor_FindSpine(e.ParentTransformType);
            e.transform.SetParent(parent, true);
        });
    }
#endif
}


#if UNITY_EDITOR
[CustomEditor(typeof(EquipTransformController))]
public class EquipTransformControllerEditor : Editor 
{
    EquipTransformController owner;
    public void OnEnable()
    {
        owner = (EquipTransformController)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();

        GUILayout.Space(10);
        GUILayout.Label("Export");
        GUILayout.Label($"Export Equip Pos Count: {owner.equipPosExport.Count}");
        if (GUILayout.Button("Export Equip Position"))
        {
            owner._Editor_Export();
        }
        if (GUILayout.Button("Export Remove"))
        {
            owner._Editor_ExportRemove();
        }

        GUILayout.Space(10);
        GUILayout.Label("Setting");
        GUILayout.Label("Spine Pos Setup");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Idle"))
        {
            owner._Editor_SpinSetup("isIdle");
        }
        if (GUILayout.Button("Walk"))
        {
            owner._Editor_SpinSetup("isWalk");
        }
        if (GUILayout.Button("Run"))
        {
            owner._Editor_SpinSetup("isRun");
        }
        GUILayout.EndHorizontal();
        
        if (GUILayout.Button("Init SpineBone"))
        {
            owner._Editor_InitSpine();
        }
        if (GUILayout.Button("Set Equip Position"))
        {
            owner._Editor_Setting();
        }

        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();
        if (GUI.changed || EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
            Debug.Log("[EquipTransformController] Save Mono");
        }
    }
}
#endif