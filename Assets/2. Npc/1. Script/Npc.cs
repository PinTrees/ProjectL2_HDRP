using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

using NPC.State;
using DG.Tweening;
using System.Linq;
using UnityEngine.Timeline;

#if UNITY_EDITOR
using UnityEditor;
#endif


public enum NPC_STATE_TYPE
{
    IDLE,

    ON_EQUIP,
    OFF_EQUIP,
    CHANGE_EQUIP,

    WALK,
    RUN,

    IDLE_BATTLE,

    DEF,

    HIT,
    HIT_KILL,
    HIT_BACK_KILL,

    Attack_Normal,

    DEATH,
}

public class Npc : MonoBehaviour
{
    [SerializeField]
    [HideInInspector]
    private string uid = "";
    public string UID { get => uid; }
    public void _Editor_SetUID(string uid) { this.uid = uid; }

    [Header("Is Static Data")]
    [SerializeField] NpcData data;
    [SerializeField] NpcCombatData combatData;
    [SerializeField] NpcMovementData movementData;

    public FSM<NPC_STATE_TYPE> AI = new FSM<NPC_STATE_TYPE>();

    private NpcGroup group_parent;
    private uMapMark marker;

    #region Property
    [HideInInspector] public bool IsBattle;
    [HideInInspector] public bool IsGodMode;
    [HideInInspector] public bool IsGotoHome;
    [HideInInspector] public bool ChangeWeapon_Left;
    [HideInInspector] public bool ChangeWeapon_Right;
    [HideInInspector] public WeaponAttackData CurrentAttackData;
    [HideInInspector] public Transform  Target;
    public bool UseNaveMashAgent = false;
    public NpcData      Data { get => data; }
    #endregion


    #region Init Func
    void Start()
    {
        // FSM 초기화
        AI.OwnerBase = this;
        AI.SetOwner(gameObject);

        AI.Init();

        AI.AddState(new Idle());

        AI.AddState(new OnEquip());
        AI.AddState(new OffEquip());
        AI.AddState(new ChangeEquip());

        AI.AddState(new Walk());
        AI.AddState(new Run());
      
        AI.AddState(new Attack_Normal());
         
        AI.AddState(new Hit());
        AI.AddState(new Hit_Kill());
        AI.AddState(new Hit_Back_Kill());

        AI.AddState(new Death());

        AI.ChangeState(NPC_STATE_TYPE.IDLE);

        Init();

        if (UseNaveMashAgent) AI.nav.enabled = true;
    }

    public void Init()
    {
        IsBattle = false;
        group_parent = GetComponentInParent<NpcGroup>();

        marker = PoolManager.Instance.GetObjectComponent<uMapMark>();
        marker.Init(transform);
        marker.Enter();

        AI.animator.SetBool("isBattle", false);
        AI.animator.SetFloat("idxWepone", -1);

        AI.healthController.maxHealth = Data.Hp;

        NpcManager.Instance.AddNpcForObject(this);
    }
    #endregion


    #region Point Func
    public void OnHit(object attack_owner
                    , HIT_FORCE_TYPE? hit_force_type = null
                    , float hit_strength = 0f
                    , Transform hitbox_tr = null
                    , bool isParryOk = false)
    {
        if (IsGodMode)
        {
            return;
        }

        if (hit_force_type != null)
        {
            if (hit_force_type == HIT_FORCE_TYPE.RIGHT) AI.animator.SetInteger("idxHitForce", 1);
            if (hit_force_type == HIT_FORCE_TYPE.LEFT) AI.animator.SetInteger("idxHitForce", 2);
            if (hit_force_type == HIT_FORCE_TYPE.FORWORD) AI.animator.SetInteger("idxHitForce", 3);
            if (hit_force_type == HIT_FORCE_TYPE.BACK) AI.animator.SetInteger("idxHitForce", 4);
            if (hit_force_type == HIT_FORCE_TYPE.DOWN) AI.animator.SetInteger("idxHitForce", 5);

            AI.animator.SetFloat("fHitStrength", hit_strength);

            // ---------------
            if (hitbox_tr)
            {
                var dir = Vector3.zero;
                if (hit_force_type == HIT_FORCE_TYPE.RIGHT) dir = Vector3.right;
                if (hit_force_type == HIT_FORCE_TYPE.LEFT) dir = Vector3.left;
                if (hit_force_type == HIT_FORCE_TYPE.FORWORD) dir = Vector3.back;
                if (hit_force_type == HIT_FORCE_TYPE.BACK) dir = Vector3.forward;
                if (hit_force_type == HIT_FORCE_TYPE.DOWN) dir = Vector3.forward;

                dir += Vector3.forward * 2;
                dir = hitbox_tr.TransformDirection(dir).normalized;

                transform.DOKill();
                transform.DOMove(transform.position + dir * 0.7f * hit_strength, 1f);

                dir = transform.TransformDirection(dir).normalized;
                AI.animator.SetFloat("xDir", dir.x);
                AI.animator.SetFloat("yDir", dir.z);
            }
        }

        AI.healthController?.TakeDamage(new vDamage(10));
        AI.ChangeState(NPC_STATE_TYPE.HIT);

        var hitEffect = PoolManager.Instance.GetObjectComponent<EffectBlood>();
        hitEffect.transform.SetPositionAndRotation(AI.collider.bounds.center, Quaternion.Euler(0, Random.Range(0, 360), 0));
        hitEffect.transform.localScale = Vector3.one * 1.5f;
        hitEffect.Enter();

        Debug.Log("[Hit] Npc");
    }

    public void OnHitOnly()
    {
        if(AI.CurrentState.IsNotEquals(NPC_STATE_TYPE.HIT_KILL) && AI.CurrentState.IsNotEquals(NPC_STATE_TYPE.HIT_BACK_KILL))
        {
            AI.healthController.TakeDamage(new vDamage(10));
        }

        /// 이펙트를 소환합니다.
        var hitEffect = HitEffectMgr.Instance.GetHitEffect(HIT_EFFECT_TYPE.BLOOD);
        hitEffect.SetActive(false);
        hitEffect.transform.localScale = Vector3.one * 1f;
        hitEffect.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        hitEffect.transform.position = AI.transform.position;
        hitEffect.SetActive(true);
    }
    #endregion


    #region Update Func
    void Update()
    {
        AI.Update();
        UpdateDeath();

        UpdateAttack();
        UpdateMovement();
    }
    void FixedUpdate()
    {
        UpdateFirstStrikeTarget();
    }

    /// <summary>
    /// NPC 이동 업데이터 입니다.
    /// 모든 NPC 이동실행은 해당 업데이터에서 호출되어야 합니다.
    /// </summary>
    private void UpdateMovement()
    {
        /// 이동 보간
        AI.vMoveSmoothDir = Vector3.Lerp(AI.vMoveDir, AI.vMoveSmoothDir, 0.75f);
         
        /// 공격 상태 확인
        if (!IsBattle)
        {
            if(!IsGotoHome && !movementData.FixeblePosition && movementData.IsChangeDirection())
            {
                movementData.ChangeMoveDirection_RootMotion(AI);
            }

            if(!AI.IsStopped)
            {
                if (!IsGotoHome && Vector3.Distance(transform.position, group_parent.transform.position) > group_parent.MoveAbleRange)
                {
                    IsGotoHome = true;

                    var newDir = group_parent.transform.position - transform.position;
                    newDir.y = 0;

                    AI.vMoveDir = newDir.normalized;
                }

                AI.ChangeState(NPC_STATE_TYPE.WALK);
            }
            else
            {
                AI.ChangeState(NPC_STATE_TYPE.IDLE);
            }

            if(IsGotoHome && Vector3.Distance(transform.position, group_parent.transform.position) < group_parent.MoveAbleRange * 0.75f)
            {
                IsGotoHome = false;
            }
        }
        else
        {
        }
    }

    /// <summary>
    /// 선제공격 NPC의 타겟 탐색 업데이터 입니다.
    /// </summary>
    private void UpdateFirstStrikeTarget()
    {
        /// 미 공격 상태 확인
        /// 선제공격 확인
        if (IsBattle) return;
        if (combatData != null && !combatData.IsFirstStrike) return;

        if(combatData.HasFindTarget(transform, PlayerManager.Instance.Player.transform))
        {
            IsBattle = true;
            Target = PlayerManager.Instance.Player.transform;
            Debug.Log("[NPC] First Strike!");
        }
    }

    /// <summary>
    /// NPC 공격 업데이터 입니다.
    /// 모든 NPC 공격실행은 해당 업데이터에서 호출되어야 합니다.
    /// </summary>
    private void UpdateAttack()
    {
        /// 타겟이 존재하는지 확인
        /// 공격상태인지 확인
        if (Target == null) return;
        if (!IsBattle) return;

        /// 현재 무기를 착용중인지 확인
        if (AI.weaponController.CurrentEquipWeapon_Right == null && AI.CurrentState.IsNotEquals(NPC_STATE_TYPE.CHANGE_EQUIP))
        {
            /// 무기 착용
            ChangeWeapon_Right = true;
            AI.ChangeState(NPC_STATE_TYPE.CHANGE_EQUIP);
            Debug.Log("[NPC] Equip Weapon");
            return;
        }

        /// 현재 공격이 가능한 산태인지 확인
        if (AI.ContainsState(new List<NPC_STATE_TYPE> { NPC_STATE_TYPE.IDLE, NPC_STATE_TYPE.RUN, NPC_STATE_TYPE.WALK }))
        {
            /// 현재 공격정보 저장
            Debug.Log("[NPC] Attack Start");
            CurrentAttackData = AI.weaponController.CurrentEquipWeapon_Right.WeaponData.AtkNormalDatas.First();
            AI.ChangeState(NPC_STATE_TYPE.Attack_Normal);
        }
    }

    /// <summary>
    /// 이 함수는 NPC의 사망 상태 업데이터 입니다.
    /// </summary>
    private void UpdateDeath()
    {
        /// 현재 사망상태를 확인합니다.
        if(AI.healthController.currentHealth <= 0 && AI.CurrentState.Type != NPC_STATE_TYPE.DEATH)
        {
            AI.ChangeState(NPC_STATE_TYPE.DEATH);
            marker.Exit();
            return;
        }
    }
    #endregion


    #region EDITOR
    private void OnDrawGizmos()
    {
        if (combatData == null) return;

        GizmosExtensions.DrawWireArc(transform.position, transform.forward, combatData.TargetFindAngle, combatData.TargetFindRange);

        Gizmos.color = Color.red;
        GizmosExtensions.DrawWireArc(transform.position, -transform.forward, combatData.TargetBackAngle, combatData.TargetBackRange);
    }
    #endregion
}


#if UNITY_EDITOR
[CustomEditor(typeof(Npc), true), CanEditMultipleObjects]
public class NpcEditor : Editor
{
    Npc owner;

    private void OnEnable()
    {
        owner = target as Npc;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Label($"UID: {owner.UID}");

        if(owner.UID.Length < 5)
        {
            if(GUILayout.Button("Create UID"))
            {
                owner._Editor_SetUID(System.Guid.NewGuid().ToString()); 
                EditorUtility.SetDirty(target);
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        DrawDefaultInspector();

        GUILayout.BeginVertical();
        
        GUILayout.EndVertical();

        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed || EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(target);
            Debug.Log("[NPC] Save Data");
        }
    }

    private void OnSceneGUI()
    {
    }
}
#endif