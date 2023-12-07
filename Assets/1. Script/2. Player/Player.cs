using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using DG.Tweening;
using KWS;
using FIMSpace.FLook;
using FIMSpace.FProceduralAnimation;

using PlayerState;




/**
 * Invector 시스템 사용 금지
 * MIS 미들웨어 사용 금지
 * 
 * 브릿지 직접 구현
 * 호환성 및 확장성 문제로 인해 기존의 프로그램을 사용하지 않는 것으로 확정되었습니다.
 * 
 * LegAnimator에서 iStep 으로 변경
 * 
 * 
 */





public enum PLAYER_STATE
{
    IDLE,
    WALK,
    RUN,
    JUMP,
    DODGE,

    ON_EQUIP,
    OFF_EQUIP,
    CHANGE_EQUIP,

    SWIMMING, 
    DIVE,

    GATHER,
    FISHING,

    
    HORSE_RIDE,
    HORSE_UNRIDE,
    HORSE_IDLE,
    HORSE_WALK,
    HORSE_RUN,
    HORSE_BOOST,


    VEHICLE_RIDE,
    VEHICLE_UNRIDE,
    VEHICLE_IDLE,
    VEHICLE_MOVE,


    ATTACK_NOMARL,
    ATTACK_STRONG,

    ATTACK_KILL,
    ATTACK_BACK_KILL,

    ATTACK_D_1,

    HIT,
    HIT_KILL,

    DEATH,
    GUARD,
    GUARD_HIT,
    PARRY,
}


public class KillData
{
    public Npc Target;
    public tAttackKillData AttackData;
}


/// 모든 정보를 관리되는 비헤이비어 내로 이동
public static class PlayerStaticStatus
{
    static public bool IsParring = false;
    static public bool isSwimming = false;
    static public bool isSurfaceWater = false;
    static public bool isGrounded = false;

    static public bool IsAttackCancelInput = true;
    static public bool IsGodMode = false;

    static public bool IsLockOnTarget = false;
    
    static public bool ChangeWeaponRight = false;
    static public bool ChangeWeaponLeft = false;

    static public NatureObject currentGatherObject;
    static public Horse currentHorse;
    static public Vehicle CurrentVehicle;

    static public KillData CurrentKillData;
}


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(LegsAnimator))]
[RequireComponent(typeof(FLookAnimator))]
[RequireComponent(typeof(vHealthController))]
[RequireComponent(typeof(vSpController))]
[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    public FSM<PLAYER_STATE> AI = new FSM<PLAYER_STATE>();

    public Transform    LookAtTransform;
    [HideInInspector]   public Stat Stat;
    private Transform   _rootTransform;

    public Transform target;


    #region Main Component
    private KW_Buoyancy _buoyancy = null;
    private uMapMark marker;
    #endregion


    #region Property
    public Inventory Inven;
    public bool UseBuoyancy { get => _buoyancy.enabled; set => _buoyancy.enabled = value; }
    public Transform GetCharactorTransform() { return transform; }
    public Transform GetCharactorRootTransform() { return _rootTransform; }

    [HideInInspector] public WeaponAttackData CurrentAttack;
    [HideInInspector] public CanInteract CurrentInteract;

    public bool IsInWater = false;
    public bool IsBattle = false;
    #endregion


    #region Init Func
    private void Start()
    {
        //_buoyancy = GetComponent<KW_Buoyancy>();
        _rootTransform = transform.parent;

        // FSM 초기화
        AI.OwnerBase = this;
        AI.SetOwner(this.gameObject);
        AI.Init();
        Init();

        AI.AddState(new OnEquip());
        AI.AddState(new OffEquip());

        AI.AddState(new Idle());
        AI.AddState(new Death());
        AI.AddState(new Walk());
        AI.AddState(new Run());
        AI.AddState(new Jump());
        AI.AddState(new Dodge());
        AI.AddState(new Hit());
        AI.AddState(new Parry());
        AI.AddState(new Gather());
        AI.AddState(new Fishing());
        AI.AddState(new Swimming());
        AI.AddState(new Dive());

        AI.AddState(new Guard());
        AI.AddState(new Guard_Hit());

        AI.AddState(new HorseRide());
        AI.AddState(new HorseUnRide());
        AI.AddState(new HorseIdle());
        AI.AddState(new HorseWalk());
        AI.AddState(new HorseRun());
        AI.AddState(new HorseBoost());

        AI.AddState(new VehicleRide());
        AI.AddState(new VehicleUnRide());
        AI.AddState(new VehicleIdle());
        AI.AddState(new VehicleMove());

        AI.AddState(new Atk_Nomarl());
        AI.AddState(new Atk_Strong());

        AI.AddState(new Atk_Kill());
        AI.AddState(new Atk_Back_Kill());

        AI.ChangeState(PLAYER_STATE.IDLE);
    }
    public void Init()
    {
        Inven = GetComponent<Inventory>();

        marker = PoolManager.Instance.GetObjectComponent<uMapMark>();
        marker.Init(transform);
        marker.Enter();

        AI.healthController.maxHealth = 10000;

        PlayerStaticStatus.isGrounded = true;
    }
    #endregion


    #region Update Func
    private void Update()   
    {
        TransformExtentions.FreezeRotationXZ(transform);

        AI.Update();
     
        UpdateDeath();
        UpdateBuoyancy();
        UpdateKillTarget();
    }

    private void UpdateKillTarget()
    {
        if (!IsBattle) return;

        Weapon current_weapon = AI.weaponController.CurrentEquipWeapon_Right; 

        if (current_weapon)
        {
            bool flag = false;

            // 앞잡
            current_weapon.WeaponData.AtkKillDatas.ForEach(e =>
            {
                if (flag) return;

                List<Npc> killNpcList = NpcManager.Instance.GetNearKillNpc(transform.position, e.KillFindRange, current_weapon.WeaponData.Atk * 3);

                if (killNpcList.Count > 0)
                {
                    if (Input.GetKey(KeyCode.F) && AI.ContainsState(new List<PLAYER_STATE> { PLAYER_STATE.IDLE, PLAYER_STATE.RUN, PLAYER_STATE.WALK }))
                    {
                        var target = killNpcList.First();

                        target.AI.ChangeState(NPC_STATE_TYPE.HIT_KILL);
                        target.transform.LookAt(transform.position);

                        PlayerStaticStatus.CurrentKillData = new KillData();
                        PlayerStaticStatus.CurrentKillData.Target = target;
                        PlayerStaticStatus.CurrentKillData.AttackData = e;

                        AI.ChangeState(PLAYER_STATE.ATTACK_KILL);

                        flag = true;
                    }
                }
            });

            if (flag) return;

            // 뒤잡
            current_weapon.WeaponData.AtkBackKillDatas.ForEach(e =>
            {
                if (flag) return;

                List<Npc> killNpcList = NpcManager.Instance.GetNearBackKillNpc(transform, e.KillFindRange, 999);

                if (killNpcList.Count > 0)
                {
                    if (Input.GetKey(KeyCode.F) && AI.ContainsState(new List<PLAYER_STATE> { PLAYER_STATE.IDLE, PLAYER_STATE.RUN, PLAYER_STATE.WALK }))
                    {
                        var target = killNpcList.First();

                        PlayerStaticStatus.CurrentKillData = new KillData();
                        PlayerStaticStatus.CurrentKillData.Target = target;
                        PlayerStaticStatus.CurrentKillData.AttackData = e;

                        target.AI.ChangeState(NPC_STATE_TYPE.HIT_BACK_KILL);
                        target.transform.LookAt(2 * target.transform.position - transform.position);

                        AI.ChangeState(PLAYER_STATE.ATTACK_BACK_KILL);

                        flag = true;
                    }
                }
            });


            // 넉다운 모션 추가
        }
    }


    private void UpdateDeath()
    {
        if(AI.healthController.currentHealth <= 0)
        {
            AI.ChangeState(PLAYER_STATE.DEATH);
        }
    }

    /// <summary>
    /// 이 함수는 해당 오브젝트의 부력을 계산합니다.
    /// </summary>
    private void UpdateBuoyancy()
    {
        if (!IsInWater) return;
    }
    #endregion


    #region Point Func
    public void OnRide(object ride_object)
    {
        if(ride_object is Horse)
        {
            var horse = ride_object as Horse;

            PlayerStaticStatus.currentHorse = horse;
            AI.ChangeState(PLAYER_STATE.HORSE_RIDE);
        }
    }

    /// <summary>
    /// 이 함수는 피격처리 함수 입니다.
    /// </summary>
    /// <param name="attack_owner"></param>
    /// <param name="hit_force_type"></param>
    /// <param name="hit_strength"></param>
    /// <param name="hitbox_tr"></param>
    /// <param name="isParryOk"></param>
    public void OnHit(object attack_owner
                    , HIT_FORCE_TYPE? hit_force_type = null
                    , float hit_strength = 0f
                    , Transform hitbox_tr = null
                    , bool isParryOk = false)
    {
        if (PlayerStaticStatus.IsGodMode) return;

        if (isParryOk && PlayerStaticStatus.IsParring)
        {
            var parryEffect = PoolManager.Instance.GetObject<uMapMark>();
            parryEffect.SetActive(false);
            parryEffect.transform.localScale = Vector3.one * hit_strength * 2.5f;
            parryEffect.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            parryEffect.transform.position = AI.weaponController.CurrentEquipWeapon_Right.effectTransform.position;
            parryEffect.SetActive(true);

            if (attack_owner is Npc)
            {
                var npc = attack_owner as Npc;
                npc.OnHit(transform.position);
            }

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

            if (hitbox_tr)
            {
                var dir = Vector3.zero;
                if (hit_force_type == HIT_FORCE_TYPE.RIGHT) dir = Vector3.right;
                if (hit_force_type == HIT_FORCE_TYPE.LEFT) dir = Vector3.left;
                if (hit_force_type == HIT_FORCE_TYPE.FORWORD) dir = Vector3.back;
                if (hit_force_type == HIT_FORCE_TYPE.BACK) dir = Vector3.forward;

                dir = hitbox_tr.TransformDirection(dir).normalized;

                transform.DOKill();
                transform.DOMove(transform.position + dir * 0.8f * hit_strength, 0.8f);

                dir = transform.TransformDirection(dir).normalized;
                AI.animator.SetFloat("xDir", dir.x);
                AI.animator.SetFloat("yDir", dir.z);
            }
        }

        // 가드 이펙트
        if (AI.CurrentState.IsEquals(PLAYER_STATE.GUARD))
        {
            AI.ChangeState(PLAYER_STATE.GUARD_HIT);

            var hitEffect = PoolManager.Instance.GetObjectComponent<EffectBlood>();
            hitEffect.transform.SetPositionAndRotation(AI.collider.bounds.center, Quaternion.Euler(0, Random.Range(0, 360), 0));
            hitEffect.transform.localScale = Vector3.one * 1.5f;
            hitEffect.Enter();
        }
        else
        {
            AI.healthController?.TakeDamage(new vDamage(10));
            AI.ChangeState(PLAYER_STATE.HIT);

            var hitEffect = PoolManager.Instance.GetObjectComponent<EffectBlood>();
            hitEffect.transform.SetPositionAndRotation(AI.collider.bounds.center, Quaternion.Euler(0, Random.Range(0, 360), 0));
            hitEffect.transform.localScale = Vector3.one * 1.5f;
            hitEffect.Enter();
        }

        Debug.Log("[Hit] Player");
    }

    public void AcquireItem(List<Item> itemList)
    {
        itemList.ForEach(e =>
        {
            Inven.AddItem(e);
            UIManager.Instance.GetAcquireUI().Show(e);
        });
    }
    #endregion


    #region Editor Func
    private void OnDrawGizmos()
    {
        AI.OnDrawGizmos();
    }
    #endregion
}


