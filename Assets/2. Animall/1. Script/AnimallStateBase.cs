using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace NPC.Animalls.State.Base
{
    public class Idle : StateBase<AnimalState>
    {
        AnimallBase owner;

        public Idle() : base(AnimalState.Idle) { }

        public override void Enter()
        {
            owner ??= (AI.OwnerBase as AnimallBase);
            AI.animator.SetBool("isIdle", true);

            AI.OnActionRepeat(1f, () =>
            {
                if (AI.RandomPercent(0.25f))
                {
                    AI.ChangeState(AnimalState.Walk);
                }
            });
        }
        public override void Exit()
        {
            AI.StopActionRepeat();
            AI.animator.SetBool("isIdle", false);
        }
        public override void Update()
        {
            if(owner.Target)
            {
                AI.ChangeState(AnimalState.Run);
                return;
            }
        }
    }


    public class Walk : StateBase<AnimalState>
    {
        float targetAngleY;

        AnimallBase owner;

        public Walk() : base(AnimalState.Walk) { }

        public override void Enter()
        {
            owner ??= (AI.OwnerBase as AnimallBase);
            AI.animator.SetBool("isWalk", true);

            AI.OnActionRepeat(1f, () =>
            {
                if (owner.Target) return;

                if (AI.RandomPercent(0.1f))
                {
                    AI.ChangeState(AnimalState.Idle);
                    return;
                }

                if(AI.RandomPercent(0.2f))
                {
                    targetAngleY = Random.Range(-owner.Data.WalkAngle, owner.Data.WalkAngle);
                }
            });
        }
        public override void Exit()
        {
            AI.StopActionRepeat();
            AI.animator.SetBool("isWalk", false);
        }
        public override void Update()
        {
            if (owner.Target)
            {
                for(int i = 0; i < owner.Data.AttackPatternDatas.Count; ++i)
                {
                    var patternData = owner.Data.AttackPatternDatas[i];
                    //if (AI.RandomPercent(owner.Data.AttackPatternDatas.First().ActivePercent))
                    if (Vector3.Distance(AI.transform.position, owner.Target.position) < patternData.RequiedData.RangeIn)
                    {
                        owner.CurrentPattern = patternData;
                        owner.CurrentAttack = patternData.AttackDatas.First();
                        AI.ChangeState(AnimalState.Attack);
                        return;
                    }
                }

                AI.transform.Translate(Vector3.forward * owner.Data.WalkSpeed * Time.deltaTime);
                TransformExtentions.LookAtTarget_Humanoid(AI.transform, owner.Target, owner.Data.RotateSpeed * 5f);
            }
            else
            {
                AI.transform.Translate(Vector3.forward * owner.Data.WalkSpeed * Time.deltaTime);

                var rotation = Quaternion.Euler(new Vector3(AI.transform.eulerAngles.x, targetAngleY, AI.transform.eulerAngles.z));
                AI.transform.rotation = Quaternion.Slerp(AI.transform.rotation, rotation, owner.Data.RotateSpeed * Time.deltaTime);
            }
        }
    }
    public class Run : StateBase<AnimalState>
    {
        float targetAngleY;

        AnimallBase owner;

        public Run() : base(AnimalState.Run) { }

        public override void Enter()
        {
            owner ??= (AI.OwnerBase as AnimallBase);
            AI.animator.SetBool("isRun", true);

            AI.OnActionRepeat(1f, () =>
            {
                if (owner.Target) return;

                if (AI.RandomPercent(0.1f))
                {
                    AI.ChangeState(AnimalState.Idle);
                    return;
                }

                if (AI.RandomPercent(0.2f))
                {
                    targetAngleY = Random.Range(-owner.Data.WalkAngle, owner.Data.WalkAngle);
                }
            });

            Debug.Log("[Animall] Run State Enter");
        }
        public override void Exit()
        {
            AI.StopActionRepeat();
            AI.animator.SetBool("isRun", false);
        }
        public override void Update()
        {
            if (owner.Target)
            {
                for (int i = 0; i < owner.Data.AttackPatternDatas.Count; ++i)
                {
                    var patternData = owner.Data.AttackPatternDatas[i];
                    //if (AI.RandomPercent(owner.Data.AttackPatternDatas.First().ActivePercent))
                    if (Vector3.Distance(AI.transform.position, owner.Target.position) < patternData.RequiedData.RangeIn)
                    {
                        owner.CurrentPattern = patternData;
                        owner.CurrentAttack = patternData.AttackDatas.First();
                        AI.ChangeState(AnimalState.Attack);
                        return;
                    }
                }

                AI.transform.Translate(Vector3.forward * owner.Data.RunSpeed * Time.deltaTime);
                TransformExtentions.LookAtTarget_Humanoid(AI.transform, owner.Target, owner.Data.RotateSpeed * owner.Data.RunSpeed * 2.5f);
            }
            else
            {
                AI.transform.Translate(Vector3.forward * owner.Data.RunSpeed * Time.deltaTime);
                var rotation = Quaternion.Euler(new Vector3(AI.transform.eulerAngles.x, targetAngleY, AI.transform.eulerAngles.z));
                AI.transform.rotation = Quaternion.Slerp(AI.transform.rotation, rotation, owner.Data.RotateSpeed * 5f * Time.deltaTime);
            }
        }
    }



    public class Attack : StateBase<AnimalState>
    {
        AnimallBase owner;

        int attackIndex = 0;
        int hitboxIndex = 0;

        Transform target;

        public Attack() : base(AnimalState.Attack) { }

        public override void Enter()
        {
            Debug.Log("[Animall] Attack State Enter");

            owner ??= AI.OwnerBase as AnimallBase;
            target = owner.Target;

            attackIndex = owner.CurrentPattern.AttackDatas.IndexOf(owner.CurrentAttack);
            hitboxIndex = 0;

            //AI.nav.stoppingDistance = 0.5f;
            //AI.nav.destination = owner.Target.position;
            AI.animator.SetBool(owner.CurrentAttack.AnimationName, true);

            // Option
            AI.transform.DOLookAt(owner.Target.position, 0.3f, AxisConstraint.Y);
        }

        public override void Exit()
        {
            owner.IsSuperAmor = false;

            AI.transform.DOKill();
            AI.animator.SetBool(owner.CurrentPattern.AttackDatas[attackIndex].AnimationName, false);
        }

        bool IsWithinAngleRange()
        {
            if (target != null)
            {
                Vector3 directionToTarget = target.transform.position - AI.transform.position;
                float angle = Vector3.Angle(AI.transform.forward, directionToTarget);
                return angle <= 100;
            }

            return false;
        }

        public override void Update()
        {
            AI.UpdateAnimationState();

            // 해당 공격의 정상 종료
            if (AI.IsAnimationTag(owner.CurrentAttack.AnimationName)
             && AI.CurrentAnimationState.normalizedTime >= owner.CurrentAttack.ExitDelay)
            {
                owner.CurrentAttack = null;
                AI.ChangeState(AnimalState.Run);
                return;
            }

            // 해당 공격의 슈퍼아머 - 무적, 등 상태 확인 및 기록
            if (AI.IsAnimationTag(owner.CurrentAttack.AnimationName)
             && owner.CurrentAttack.SuperArmorExitDelay != 0f
             && AI.IsAnimationInRange(owner.CurrentAttack.SuperArmorStartDelay, owner.CurrentAttack.SuperArmorExitDelay)
             && !owner.IsSuperAmor)
            {
                owner.IsSuperAmor = true;
            }

            if (AI.IsAnimationTag(owner.CurrentAttack.AnimationName)
             && AI.CurrentAnimationState.normalizedTime > owner.CurrentAttack.SuperArmorExitDelay
             && owner.IsSuperAmor)
            {
                owner.IsSuperAmor = false;
            }

            // 공격의 히트박스 생성 프레임 확인
            if (AI.IsAnimationTag(owner.CurrentAttack.AnimationName)
             && owner.CurrentAttack.tHitBoxDatas.Count > hitboxIndex
             && AI.CurrentAnimationState.normalizedTime >= owner.CurrentAttack.tHitBoxDatas[hitboxIndex].StartDelay)
            {
                var hitBox = PoolManager.Instance.GetObject<HitBox_Player>().GetComponent<HitBox_Player>();
                hitBox.transform.position = AI.transform.position;
                hitBox.transform.rotation = AI.transform.rotation;
                hitBox.Enter(owner.CurrentAttack.tHitBoxDatas[hitboxIndex]
                           , owner
                           , parryDelay: owner.CurrentAttack.tHitBoxDatas[hitboxIndex].GetParryDelay(AI.CurrentAnimationState.speed)
                           , exitDelay: owner.CurrentAttack.tHitBoxDatas[hitboxIndex].GetExitDelay(AI.CurrentAnimationState.speed));

                hitboxIndex++;
            }

            // 공걱의 다음 공격정보 확인 및 스테이트 변경
            if (attackIndex + 1 < owner.CurrentPattern.AttackDatas.Count)
            {
                if (AI.IsAnimationTag(owner.CurrentAttack.AnimationName)
                 && AI.CurrentAnimationState.normalizedTime >= owner.CurrentAttack.NextStartDelay)
                {
                    owner.CurrentAttack = owner.CurrentPattern.AttackDatas[attackIndex + 1];
                    AI.ChangeState(AnimalState.Attack);
                    return;
                }
            }
        }
    }

    public class Hit : StateBase<AnimalState>
    {
        const float HIT_DELAY = 1f;
        AnimallBase owner;

        public Hit() : base(AnimalState.Hit) { }

        public override void Enter()
        {
            base.Enter();

            owner ??= AI.OwnerBase as AnimallBase;
            AI.animator.SetBool("isHit", true);

            if (owner.Target == null) { owner.Target = PlayerManager.Instance.Player.transform; }

            AI.transform.DOKill();
            AI.transform.DOScale(AI.transform.localScale, HIT_DELAY).OnComplete(() =>
            {
                AI.ChangeState(AnimalState.Idle);
            });
        }
        public override void Exit()
        {
            AI.animator.SetBool("isHit", false);
        }
        public override void Update()
        {
            AI.UpdateAnimationState();
         
            if (!AI.IsAnimatorInit)
            {
                InitAnimator();
            }
        }

        public void InitAnimator()
        {
            if (AI.CurrentAnimationState.IsTag("Hit"))
            {
                float clipLength = AI.CurrentAnimationState.length;

                float newNormalizedTime = Mathf.Clamp01(0);
                float newTimeValue = newNormalizedTime * clipLength;
                AI.animator.Play(AI.CurrentAnimationState.fullPathHash, -1, newTimeValue / clipLength);

                AI.IsAnimatorInit = true;
            }
        }
    }


    public class Death : StateBase<AnimalState>
    {
        AnimallBase owner;

        public Death() : base(AnimalState.Death) { }

        public override void Enter()
        {
            owner ??= (AI.OwnerBase as AnimallBase);
            AI.animator.SetBool("isDeath", true);

            Debug.Log("[Animall] Death State Enter");
        }
        public override void Exit()
        {
            AI.StopActionRepeat();
            AI.animator.SetBool("isDeath", false);
        }
        public override void Update()
        {
        }
    }
}


