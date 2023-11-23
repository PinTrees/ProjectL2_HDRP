using DG.Tweening;
using FIMSpace.Basics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace MonsterState.Lacercharias
{
    public class Idle : StateBase<MONSTER_STATE>
    {
        float traceRange = 10000f;
        Animator animator;

        public Idle() : base(MONSTER_STATE.IDLE) {}

        public override void Enter()
        {
            if(animator == null)
                animator = AI.GetOwner().GetComponent<Animator>();

            animator.SetBool("isIdle", true);
        }
        public override void Exit()
        {
            animator.SetBool("isIdle", false);
        }
        public override void Update() 
        {
            Vector3 target = PlayerManager.Instance.GetPlayerPosition();
            Vector3 pos = AI.GetOwner().transform.position;

            if(Vector3.Distance(target, pos) < traceRange)
            {
                AI.ChangeState(MONSTER_STATE.RUN);
            }
        }
    }

    public class Trace : StateBase<MONSTER_STATE>
    {
        Transform transform;
        NavMeshAgent nav;
        Animator animator;

        public Trace() : base(MONSTER_STATE.RUN) { }

        public override void Enter()
        {
            if(nav == null) nav = AI.GetOwner().GetComponent<NavMeshAgent>();
            if (animator == null) animator = AI.GetOwner().GetComponent<Animator>();
            if (transform == null) transform = AI.GetOwner().transform;

            nav.updateRotation = false;
            nav.speed = 1.5f;
            nav.stoppingDistance = 1.2f;
            animator.SetBool("isRun", true);
        }
        public override void Exit()
        {
            nav.speed = 0.01f;
            animator.SetBool("isRun", false);
        }

        public override void Update()
        {
            Vector3 target = PlayerManager.Instance.GetPlayerPosition();
            nav.destination = target;
        }
    }

    public class Hit : StateBase<MONSTER_STATE>
    {
        Transform transform;
        Animator animator;
        float hitDelay = 0.7f;

        public Hit() : base(MONSTER_STATE.HIT) { }

        public override void Enter()
        {
            if (animator == null)  animator = AI.GetOwner().GetComponent<Animator>();
            if (transform == null) transform = AI.GetOwner().transform;

            animator.SetBool("isHit", true);

            transform.DOKill();
            transform.DOScale(transform.localScale, hitDelay).OnComplete(() =>  { AI.ChangeState(MONSTER_STATE.IDLE); });
        }
        public override void Exit()
        {
            animator.SetBool("isHit", false);
        }
        public override void Update()
        {
        }
    }

    public class Dead : StateBase<MONSTER_STATE>
    {
        Animator animator;
        NavMeshAgent nav;

        public Dead() : base(MONSTER_STATE.DEATH) { }

        public override void Enter()
        {
            if (animator == null) animator = AI.GetOwner().GetComponent<Animator>();
            if (nav == null) nav = AI.GetOwner().GetComponent<NavMeshAgent>();

            nav.speed = 0f;
            nav.isStopped = true;
            animator.SetBool("isDead", true);
        }
        public override void Exit()
        {
            animator.SetBool("isDead", false);
        }
        public override void Update() { }
    }
}