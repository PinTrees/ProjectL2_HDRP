using System.Collections;
using System.Collections.Generic;
using UnityEngine;




namespace NPC.Animalls.State.HorseBase
{
    public enum HORSE_STATE
    {
        IDLE,
        WALK,
        RUN,
        BOOST,
    }

    public class Idle : StateBase<HORSE_STATE>
    {
        Horse owner;

        public Idle() : base(HORSE_STATE.IDLE) { }

        public override void Enter()
        {
            owner ??= AI.OwnerBase as Horse;

            AI.animator.SetBool("isIdle", true);
            AI.animator.SetTrigger("StateOn");

            if (owner.wingAnimator)
            {
                owner.wingAnimator.SetBool("isIdle", true);
                owner.wingAnimator.SetTrigger("StateOn");
            }
        }
        public override void Exit()
        {
            AI.animator.SetBool("isIdle", false);
            if(owner.wingAnimator) owner.wingAnimator.SetBool("isIdle", false);
        }

        public override void Update() 
        { 

        }
    }

    public class Walk : StateBase<HORSE_STATE>
    {
        Horse owner;

        public Walk() : base(HORSE_STATE.WALK) { }

        public override void Enter()
        {
            owner ??= AI.OwnerBase as Horse;

            AI.animator.SetBool("isWalk", true);
            AI.animator.SetTrigger("StateOn");

            if(owner.wingAnimator)
            {
                owner.wingAnimator.SetBool("isWalk", true);
                owner.wingAnimator.SetTrigger("StateOn");
            }
        }
        public override void Exit()
        {
            AI.animator.SetBool("isWalk", false);
            if(owner.wingAnimator) owner.wingAnimator.SetBool("isWalk", false);
        }

        public override void Update()
        {
        }
    }

    public class Run : StateBase<HORSE_STATE>
    {
        Horse owner;
        public Run() : base(HORSE_STATE.RUN) { }

        public override void Enter()
        {
            owner ??= AI.OwnerBase as Horse;

            AI.animator.SetBool("isRun", true);
            AI.animator.SetTrigger("StateOn");

            if(owner.wingAnimator)
            {
                owner.wingAnimator.SetBool("isRun", true);
                owner.wingAnimator.SetTrigger("StateOn");
            }
        }
        public override void Exit()
        {
            AI.animator.SetBool("isRun", false);
            if (owner.wingAnimator) owner.wingAnimator.SetBool("isRun", false);
        }

        public override void Update()
        {
        }
    }

    public class Boost : StateBase<HORSE_STATE>
    {
        Horse owner;
        public Boost() : base(HORSE_STATE.BOOST) { }

        public override void Enter()
        {
            owner ??= AI.OwnerBase as Horse;
            AI.animator.SetBool("isBoost", true);
            AI.animator.SetTrigger("StateOn");

            if (owner.wingAnimator)
            {
                owner.wingAnimator.SetBool("isBoost", true);
                owner.wingAnimator.SetTrigger("StateOn");
            }
        }
        public override void Exit()
        {
            AI.animator.SetBool("isBoost", false);
            if (owner.wingAnimator) owner.wingAnimator.SetBool("isBoost", false);
        }

        public override void Update()
        {
        }
    }
}
