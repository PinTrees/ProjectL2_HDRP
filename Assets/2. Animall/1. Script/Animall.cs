using com.mobilin.games;
using FIMSpace.FSpine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NPC.Animalls.State.Base;
using FIMSpace.FProceduralAnimation;
using DG.Tweening;
using Invector;
using static UnityEngine.UI.GridLayoutGroup;
using TMPro;

public enum AnimalState
{
    Idle,
    Walk,
    Run,

    Hit,

    Attack,
    Death,
}


[RequireComponent(typeof(LegsAnimator))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(mvHealthController))]
public class AnimallBase : MonoBehaviour
{
    public FSM<AnimalState> AI = new();
    public AnimallData Data;

    public Vector3 CameraLockOnRotateOffset;
    public Vector3 CameraLockOnMouseOffset;

    public Transform Target;
    public bool IsSuperAmor;
    public bool IsBattle;
    public bool IsGotoHome;

    [HideInInspector] public tMonPatternData CurrentPattern;
    [HideInInspector] public tMonAttackData CurrentAttack;

    private AnimallGroup parent;


    #region Init Func
    public void Start()
    {
        parent = GetComponentInParent<AnimallGroup>();

        AI.OwnerBase = this;
        AI.SetOwner(this.gameObject);
        AI.Init();

        AI.AddState(new Idle());
        AI.AddState(new Walk());
        AI.AddState(new Run());
        AI.AddState(new Hit());
        AI.AddState(new Attack());
        AI.AddState(new Death());

        AI.ChangeState(AnimalState.Idle);

        AI.healthController.maxHealth = (int)Data.Hp;

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }
    #endregion


    #region Update Func
    void Update()
    {
        AI.Update();
        UpdateDeathState();
    }
    public void UpdateDeathState()
    {
        if(AI.healthController)
        {
            if(AI.healthController.currentHealth <= 0 && AI.CurrentState.IsNotEquals(AnimalState.Death))
            {
                AI.ChangeState(AnimalState.Death);
                return;
            }
        }
    }
    #endregion


    #region Point Func
    public void OnHit(Vector3 targetPosition)
    {
        if (AI.CurrentState.IsEquals(AnimalState.Death))
            return;

        UIManager.Instance.GetCombatTextUI().ShowCombatText(100);
        AI.healthController.TakeDamage(new vDamage(10));

        if (AI.healthController.currentHealth <= 0)
        {
            AI.ChangeState(AnimalState.Death);
        }
        else
        {
            AI.ChangeState(AnimalState.Hit);
        }

        HitForce(targetPosition);

        // Effect Spawner
        var hitEffect = HitEffectMgr.Instance.GetHitEffect(HIT_EFFECT_TYPE.BLOOD);
        hitEffect.SetActive(false);
        hitEffect.transform.localScale = Vector3.one * 1.8f;
        hitEffect.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        hitEffect.transform.position = AI.collider.bounds.center; //transform.position + hitOffset;
        hitEffect.SetActive(true);

        Debug.Log("[Hit] Monster" + hitEffect.transform.position);
    }

    // ----------------
    public void HitForce(Vector3 hitPosition)
    {
        Vector3 reactVec = (transform.position - hitPosition).normalized + Vector3.up;
        AI.rb.AddForce(reactVec * 100, ForceMode.Impulse);

        Vector3 vHitDir = (transform.position - hitPosition).normalized;
        vHitDir.y = 0;
        transform.DOMove(transform.position + vHitDir * 1f, 0.5f);
    }
    #endregion
}
