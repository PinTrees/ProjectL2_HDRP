using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.AI;

using DG.Tweening;
using FIMSpace.FLook;
using FIMSpace.FProceduralAnimation;


/***
 * 인벡터 및 MIS 미들웨어 사용금지
 */


public class FSM<T>
{
    private StateBase<T> currentState;

    private GameObject owner;
    private Dictionary<T, StateBase<T>> stateMap = new Dictionary<T, StateBase<T>>();

    private System.Action ExitEvent;
    private bool __exit_event_block;

    #region Component
    public Animator animator;
    public Transform transform;
    public NavMeshAgent nav;
    public Rigidbody rb;
    public Collider collider;
    #endregion


    #region Controller
    public vHealthController healthController;
    public WeaponController weaponController;
    #endregion


    #region Animator
    public LegsAnimator legsAnimator;
    public FLookAnimator lookAnimator;
    #endregion


    // Base Timer ----------------------
    public float CurTimer;


    #region Property
    public Vector3 vMoveSmoothDir;
    public Vector3 vMoveDir;
    public object OwnerBase;

    public bool IsAnimatorInit;
    public bool IsStopped { get => (vMoveSmoothDir.x < 0.07f && vMoveSmoothDir.x > -0.07f) && ( vMoveSmoothDir.z > -0.07f && vMoveSmoothDir.z < 0.07f); }
  
    public T PriveStateType;
    public StateBase<T> CurrentState { get => currentState; }
    public AnimatorStateInfo CurrentAnimationState;

    public bool IsAnimationTag(string tag) { return CurrentAnimationState.IsTag(tag); }
    public bool IsAnimationInRange(float start, float exit) { return (CurrentAnimationState.normalizedTime >= start && CurrentAnimationState.normalizedTime <= exit); }

    public GameObject GetOwner() { return owner; }
    public void SetOwner(GameObject ow) { owner = ow; }
    public void AddState(StateBase<T> val) { stateMap[val.Type] = val; stateMap[val.Type].AI = this; }
    #endregion


    #region Init Func
    /// <summary>
    /// FSM 초기화 함수 입니다. 
    /// FSM 이 사용되기 전에 반드시 실행되어야 합니다.
    /// 오브젝트의 생성시점 또는 시작시점에 호출되어야 합니다.
    /// </summary>
    public void Init()
    {
        vMoveDir = Vector3.zero;
        vMoveSmoothDir = Vector3.zero;
        transform = owner?.transform;

        owner?.TryGetComponent<LegsAnimator>(out legsAnimator);
        owner?.TryGetComponent<NavMeshAgent>(out nav);
        owner?.TryGetComponent<Rigidbody>(out rb);
        owner?.TryGetComponent<Animator>(out animator);
        owner?.TryGetComponent<vHealthController>(out healthController);
        owner?.TryGetComponent<Collider>(out collider);
        owner?.TryGetComponent<FLookAnimator>(out lookAnimator);

        try
        {
            owner?.TryGetComponent<WeaponController>(out weaponController);
            weaponController?.Init(OwnerBase);
        } catch {}
    }
    public void InitTimer() { CurTimer = 0f; }
    #endregion


    #region Update Func
    public void Update() { currentState?.Update(); }
    public void UpdateTimer() { CurTimer += Time.deltaTime; }
    public void UpdateAnimationState() { CurrentAnimationState = animator.GetCurrentAnimatorStateInfo(0); }
    #endregion


    public bool RandomPercent(float range) { return Random.Range(0f, 1f) < range; }

    public bool ContainsState(List<T> list)
    {
        bool result = false;
        
        list.ForEach(e => { if(e.Equals(currentState.Type)) result = true; });

        return result;
    }
    public FSM<T> ChangeState(T type)
    {
        var target = stateMap.Where(item => item.Key.ToString() == type.ToString());

        if(target != null)
        {
            var next = target.First().Value;

            if(currentState != null)
            {
                currentState.Exit();
                PriveStateType = currentState.Type;
            }

            next.Enter();
            currentState = next;

            if (ExitEvent != null && __exit_event_block) 
            {
                __exit_event_block = false;

                ExitEvent(); 
                ExitEvent = null;
            }
        }

        return this;
    }
    public FSM<T> OnExitEvent(System.Action action)
    {
        __exit_event_block = true;
        ExitEvent = action;
        return this;
    }

    /// <summary>
    /// 이 함수는 FSM의 베이스 오브젝트에 대한 반복 호출 로직을 등록합니다.
    /// DOT Tween 베이스로 구현되었습니다.
    /// </summary>
    /// <param name="tick"></param>
    /// <param name="action"></param>
    public void OnActionRepeat(float tick, System.Action action)
    {
        transform.DOScale(transform.localScale, tick).SetLoops(-1, LoopType.Yoyo).OnStepComplete(() =>
        {
            action();
        });
    }
    /// <summary>
    /// 이 함수는 등록된 반복 호출로직을 모두 제거합니다.
    /// </summary>
    public void StopActionRepeat() { transform.DOKill(); }


    /// <summary>
    /// 이 함수는 FSM을 소유하고 있는 대상이 OnDrawGizmos 실행순서에 직접 호출시켜야 합니다.
    /// </summary>
    public void OnDrawGizmos() { currentState?.OnDrawGizmos(); }
}
