using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using FIMSpace.Basics;
using UnityEngine.AI;
using System.Linq;
using UnityEditor.Overlays;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace MonsterState.Base
{
    public class Idle : StateBase<MONSTER_STATE>
    {
        Monster owner;

        public Idle() : base(MONSTER_STATE.IDLE) { }

        public override void Enter()
        {
            owner ??= (AI.OwnerBase as Monster);
            AI.animator.SetBool("isIdle", true);
        }
        public override void Exit()
        {
            AI.animator.SetBool("isIdle", false);
        }
        public override void Update()
        {
            if(owner.Target)
            {
                if (Vector3.Distance(owner.Target.position, owner.transform.position) < owner.Data.TraceRange)
                {
                    AI.ChangeState(MONSTER_STATE.WALK);
                    return;
                }
            }
        }
    }

    public class Walk : StateBase<MONSTER_STATE>
    {
        const float MOVE_DELAY = 0.75f;
        const float MOVE_PERCENT = 0.3f;

        const float ATK_DELAY = 0.5f;

        Monster owner;

        Vector3 nextDir;
        Vector3 moveDirection;

        public Walk() : base(MONSTER_STATE.WALK) { }

        public override void Enter()
        {
            owner ??= (AI.OwnerBase as Monster);

            moveDirection = Vector3.left;

            AI.animator.SetBool("isWalk", true);

            AI.OnActionRepeat(MOVE_DELAY, () =>
            {
                if (AI.RandomPercent(MOVE_PERCENT))
                {
                    nextDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(0f, 1f));
                    nextDir.Normalize();

                    AI.transform.DOScale(AI.transform.localScale, 0.5f).OnStepComplete(() =>
                    {
                        moveDirection = nextDir;
                    });
                }
            });

            AI.OnActionRepeat(ATK_DELAY, () =>
            {
                for (int i = 0; i < owner.Data.AttackPatternDatas.Count; ++i)
                {
                    if (Vector3.Distance(owner.Target.position, owner.transform.position) < 3f
                        && AI.RandomPercent(owner.Data.AttackPatternDatas[i].ActivePercent))
                    {
                        owner.CurrentPattern = owner.Data.AttackPatternDatas[i];
                        owner.CurrentAttack = owner.CurrentPattern.AttackDatas.First();

                        AI.ChangeState(MONSTER_STATE.ATK_N);
                        break;
                    }
                }
            });
        }
        public override void Exit()
        {
            AI.StopActionRepeat();
            AI.transform.DOKill();
            AI.animator.SetBool("isWalk", false);
        }

        public override void Update()
        {
            if (Vector3.Distance(owner.Target.position, AI.transform.position) <= owner.Data.RunStartRange
                && Vector3.Distance(owner.Target.position, AI.transform.position) >= owner.Data.RunExitRange)
            {
                AI.ChangeState(MONSTER_STATE.RUN);
                return;
            }

            moveDirection = Vector3.Lerp(moveDirection, nextDir, 0.5f);
           
            AI.transform.DOLookAt(owner.Target.position, 0.3f, AxisConstraint.Y);
            AI.transform.Translate(moveDirection * owner.Data.WalkSpeed * Time.deltaTime);
        }
    }
    public class Run : StateBase<MONSTER_STATE>
    {
        Monster owner;

        public Run() : base(MONSTER_STATE.RUN) { }

        public override void Enter()
        {
            owner ??= (AI.OwnerBase as Monster);
           
            AI.nav.isStopped = false;
            AI.nav.speed = owner.Data.TraceSpeed;
            AI.nav.stoppingDistance = 1.5f;
            AI.animator.SetBool("isRun", true);
        }
        public override void Exit()
        {
            AI.nav.isStopped = true;
            AI.animator.SetBool("isRun", false);
        }

        public override void Update()
        {
            AI.nav.destination = owner.Target.position;

            if (Vector3.Distance(owner.Target.position, AI.transform.position) < owner.Data.RunExitRange)
            {
                AI.ChangeState(MONSTER_STATE.WALK);
                return;
            }
        }
    }

    public class Hit : StateBase<MONSTER_STATE>
    {
        const float HIT_DELAY = 1f;
        Monster monster;

        public Hit() : base(MONSTER_STATE.HIT) { }

        public override void Enter()
        {
            base.Enter();

            monster ??= AI.OwnerBase as Monster;
            AI.animator.SetBool("isHit", true);

            //AI.animator.SetFloat("xDir", 1);
            //AI.animator.SetFloat("yDir", 0);

            var dir = (AI.transform.position - monster.Target.position).normalized;

            AI.rb.AddForce(dir * 100f);
            AI.transform.DOKill();
            AI.transform.DOScale(AI.transform.localScale, HIT_DELAY).OnComplete(() => 
            { 
                AI.ChangeState(MONSTER_STATE.IDLE);
            });
        }
        public override void Exit()
        {
            AI.animator.SetBool("isHit", false);
        }
        public override void Update()
        {
            if(!AI.IsAnimatorInit)
            {
                InitAnimator();
            }
        }

        public void InitAnimator()
        {
            AnimatorStateInfo stateInfo = AI.animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsTag("Hit"))
            {
                float clipLength = stateInfo.length;

                float newNormalizedTime = Mathf.Clamp01(0);
                float newTimeValue = newNormalizedTime * clipLength;
                AI.animator.Play(stateInfo.fullPathHash, -1, newTimeValue / clipLength);

                AI.IsAnimatorInit = true;
            }
        }
    }

    public class Death : StateBase<MONSTER_STATE>
    {
        public Death() : base(MONSTER_STATE.DEATH) { }

        public override void Enter()
        {
            AI.nav.speed = 0f;
            AI.nav.isStopped = true;
            AI.animator.SetBool("isDeath", true);
        }
        public override void Exit()
        {
            AI.animator.SetBool("isDeath", false);
        }
        public override void Update() { }
    }

    public class BattleIdle : StateBase<MONSTER_STATE>
    {
        public enum STATE_MORE
        {
            IDLE,
            WALK,
        }

        STATE_MORE state;

        Monster owner;
        Transform owner_root;

        Transform target;

        float cur_trace_delay = 0.5f;
        float trace_delay = 0.5f;

        float cur_move_dir_delay = 0f;
        float move_dir_delay = 0.3f;
        float move_dir = 1f;

        Vector3 prev_dir = Vector3.zero;
        Vector3 dir = Vector3.zero;

        public BattleIdle() : base(MONSTER_STATE.BATTLE) { }

        public override void Enter()
        {
            owner ??= AI.OwnerBase as Monster;

            owner.Target = PlayerManager.Instance.GetPlayerTransform();

            target = owner.Target;
            owner_root = owner.transform.parent;

            cur_trace_delay = 0;
            AI.animator.SetBool("isIdle", true);

            state = STATE_MORE.IDLE;
        }

        public override void Exit()
        {
            AI.animator.SetBool("isIdle", true);
            AI.animator.SetBool("isWalk", false);
        }

        public override void Update()
        {
            cur_trace_delay += Time.deltaTime;
            cur_move_dir_delay += Time.deltaTime;

            if (cur_move_dir_delay > move_dir_delay)
            {
                cur_move_dir_delay = 0f;
                if (Random.Range(0f, 100f) < 5f)
                    move_dir *= -1f;

                if (Random.Range(0, 100) < 20)
                {
                    if (Vector3.Distance(target.position, owner.transform.position) < 3f)
                    {
                        AI.ChangeState(MONSTER_STATE.ATK_N);
                        return;
                    }
                }
            }

            if (state == STATE_MORE.IDLE)
            {
                AI.animator.SetBool("isWalk", false);

                if (cur_trace_delay > trace_delay)
                {
                    cur_trace_delay = 0f;
                    state = STATE_MORE.WALK;
                }
            }
            else if (state == STATE_MORE.WALK)
            {
                AI.animator.SetBool("isWalk", true);

                prev_dir = dir;

                var dist = Vector3.Distance(owner.transform.position, target.position);

                // 타겟을 바라보는 방향 벡터 계산
                Vector3 lookDirection = target.position - owner.transform.position;
                lookDirection.y = 0; // 수평 방향 벡터로 만듦
                Quaternion rotation = Quaternion.LookRotation(lookDirection);

                // 오너(Transform)을 회전시킴
                owner.transform.rotation = Quaternion.Slerp(owner.transform.rotation, rotation, 100f * Time.deltaTime);
                owner.transform.Translate(Vector3.right * move_dir
                    + (dist > 5f ? Vector3.forward : dist < 2.5f ? Vector3.back : Vector3.zero) * Time.deltaTime * 0.3f);

                //AI.animator.SetFloat("xDir", tmp_dir.x);
                //AI.animator.SetFloat("yDir", tmp_dir.z);
            }
        }
    }

    public class Attack : StateBase<MONSTER_STATE>
    {
        Monster owner;

        int attackIndex = 0;
        int hitboxIndex = 0;

        Player target;

        public Attack() : base(MONSTER_STATE.ATK_N) { }

        public override void Enter()
        {
            owner ??= AI.OwnerBase as Monster;
            target = owner.Target.GetComponent<Player>();

            attackIndex = owner.CurrentPattern.AttackDatas.IndexOf(owner.CurrentAttack);
            hitboxIndex = 0;

            AI.nav.stoppingDistance = 0.5f;
            AI.nav.destination = owner.Target.position;
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
                AI.ChangeState(MONSTER_STATE.WALK);
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
                    AI.ChangeState(MONSTER_STATE.ATK_N);
                    return;
                }
            }
        }
    }
}