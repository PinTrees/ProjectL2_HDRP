using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MonsterState.Base;
using TMPro;

using UnityEngine.AI;
using Invector;
using com.mobilin.games;
using FIMSpace.FProceduralAnimation;
using UnityEditor.ShaderGraph.Internal;


public enum MONSTER_STATE
{
    IDLE,
    RUN,
    WALK,

    HIT,
    ATK_N,

    BATTLE,

    DEATH,
}


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(LegsAnimator))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(mvHealthController))]
public class Monster : MonoBehaviour
{
    [SerializeField] Vector3 lookOffset;
    [SerializeField] Transform lookTransform;
    [SerializeField] public MonsterData Data;
    [SerializeField] Vector3 hitOffset = Vector3.zero;
    [SerializeField] float hitForceOffset = 150f;
    [SerializeField] float hitScaleFactor = 2.5f;

    public FSM<MONSTER_STATE> AI = new FSM<MONSTER_STATE>();

    private Transform target;
    public Transform Target { get => target; set => target = value; }

    public Transform weaponTransform;

    public Vector3 CameraLockOnRotateOffset = Vector3.zero;
    public Vector2 CameraLockOnMouseOffset = Vector2.zero;
    
    public float ParryEffectScaleFactor = 2.5f;
   
    [HideInInspector] public tMonPatternData CurrentPattern;
    [HideInInspector] public tMonAttackData CurrentAttack;

  
    public bool IsSuperAmor = false;

    void Awake()
    {
        AI.OwnerBase = this;
        AI.SetOwner(this.gameObject);
        AI.Init();

        AI.AddState(new Idle());
        AI.AddState(new Walk());
        AI.AddState(new Run());
       
        AI.AddState(new Hit());
        AI.AddState(new Death());
        AI.AddState(new BattleIdle());
        AI.AddState(new Attack());

        AI.ChangeState(MONSTER_STATE.IDLE);

        Init();
    }

    void Init()
    {
        AI.healthController.maxHealth = Data.Hp;
    }

    void Start()
    {
        MonsterMgr.Instance.AddMonster(this);
    }

    #region Update
    void Update()
    {
        AI.Update();
        UpdateFindTarget();
        
        UpdateDeathState();

        if (Target != null)
        {
            lookTransform.position = Target.position + lookOffset;   
        }
    }

    /// <summary>
    /// 이 함수는 해당몬스터의 죽음 상태 업데이터 입니다.
    /// </summary>
    void UpdateDeathState()
    {
        if (AI.healthController.currentHealth <= 0 && AI.CurrentState.Type != MONSTER_STATE.DEATH)
        {
            UIManager.Instance.RemoveTarget();
            AI.ChangeState(MONSTER_STATE.DEATH);
            return;
        }
    }

    /// <summary>
    /// 이 함수는 해당 몬스터의 목표탐색 업데이터 입니다.
    /// </summary>
    public void UpdateFindTarget()
    {
        /// 목표 대상 확인
        Vector3 targetPos = PlayerManager.Instance.GetPlayerPosition();

        /// 선제 공격 확인
        if (Data.IsFirstStrike)
        {
            if (Vector3.Distance(targetPos, transform.position) < Data.InfoRange)
            {
                UIManager.Instance.SetTarget(this);
            }
        }

        if (target == null)
        {
            if (Vector3.Distance(targetPos, transform.position) < Data.TraceRange)
            {
                target = PlayerManager.Instance.Player.transform;
            }
        }
    }
    #endregion


    #region Point Func 
    public void OnHit(Vector3 vTargetPos)
    {
        if (AI.CurrentState.Type == MONSTER_STATE.DEATH) return;

        UIManager.Instance.GetCombatTextUI().ShowCombatText(100);
        AI.healthController.TakeDamage(new vDamage(10));

        if (AI.healthController.currentHealth <= 0)
        {
            AI.ChangeState(MONSTER_STATE.DEATH);
        }
        else if (!IsSuperAmor)
        {
            AI.ChangeState(MONSTER_STATE.HIT);
        }


        if (Data.Trib == MONSTER_TRIB_TYPE.NORMAL)
        {
            if(Data.PositionType == MONSTER_POSITION_TYPE.DYNAMIC)
            {
                Vector3 reactVec = (transform.position - vTargetPos).normalized + Vector3.up;
                AI.rb.AddForce(reactVec * hitForceOffset, ForceMode.Impulse);

                Vector3 vHitDir = (transform.position - vTargetPos).normalized;
                vHitDir.y = 0;
                transform.DOMove(transform.position + vHitDir * 1f, 0.5f);
            }

            var hitEffect = HitEffectMgr.Instance.GetHitEffect(HIT_EFFECT_TYPE.BLOOD);
            hitEffect.SetActive(false);
            hitEffect.transform.localScale = Vector3.one * hitScaleFactor;
            hitEffect.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            hitEffect.transform.position = AI.collider.bounds.center; //transform.position + hitOffset;
            hitEffect.SetActive(true);

            Debug.Log("[Hit] Monster" + hitEffect.transform.position);
        }
    }
    public void OnHitOnlyEffect()
    {
        var hitEffect = HitEffectMgr.Instance.GetHitEffect(HIT_EFFECT_TYPE.BLOOD);
        hitEffect.SetActive(false);
        hitEffect.transform.localScale = Vector3.one * 2.5f;
        hitEffect.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        hitEffect.transform.position = transform.position + hitOffset;
        hitEffect.SetActive(true);

        Debug.Log("[Hit] Monster" + hitEffect.transform.position);
    }

    public void OnParry(Vector3 vTargetPos, PARRYED_TYPE? parry_type=null)
    {
        UIManager.Instance.GetCombatTextUI().ShowCombatText("PARRY");

        if (AI.CurrentState.IsEquals(MONSTER_STATE.DEATH))
            return;

        if (Data.Trib == MONSTER_TRIB_TYPE.NORMAL)
        {
            AI.healthController.TakeDamage(new vDamage(5));

            if (AI.healthController.currentHealth <= 0)
            {
                AI.ChangeState(MONSTER_STATE.DEATH);
            }
            
            if(parry_type !=null)
            {
                if (parry_type == PARRYED_TYPE.SUPER_ARMOR) { }
                else if (parry_type == PARRYED_TYPE.CANCEL) AI.ChangeState(MONSTER_STATE.WALK);

                if (Data.PositionType == MONSTER_POSITION_TYPE.DYNAMIC)
                {
                    Vector3 reactVec = (transform.position - vTargetPos).normalized + Vector3.up;
                    AI.rb.AddForce(reactVec * hitForceOffset, ForceMode.Impulse);
                }
            }
        }
    }
    #endregion
}
