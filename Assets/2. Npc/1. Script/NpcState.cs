using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.AI;

using DG.Tweening;


namespace NPC.State
{
    public class Idle : StateBase<NPC_STATE_TYPE>
    {
        Npc owner;
        NpcData data;

        private float attackTimeDelay = 0.5f;
        private float attackProbability = 0.3f;

        private float nearMoveTimer = 0f;
        private float nearMoveDelay = 0.5f;
        private float nearMovePercent = 0.3f;

        public Idle() : base(NPC_STATE_TYPE.IDLE) { }

        public override void Enter()
        {
            nearMoveTimer = 0f;

            owner ??= AI.OwnerBase as Npc;
            data ??= owner.Data;

            AI.animator.SetBool("isIdle", true);
        }

        public override void Exit()
        {
            AI.animator.SetBool("isIdle", false);
        }

        public override void Update()
        {
            nearMoveTimer += Time.deltaTime;    
            AI.UpdateTimer();

            if(owner.Target != null)
            {
                TransformExtentions.LookAtTarget_Humanoid(AI.transform, owner.Target, 5 * owner.Data.WalkSpeed * 2.5f);
            }

            if(AI.animator.GetBool("isBattle"))
            {
                if(AI.CurTimer > attackTimeDelay)
                {
                    AI.InitTimer();

                    if (AI.RandomPercent(attackProbability))
                    {
                        if (Vector3.Distance(owner.transform.position, owner.Target.position) < 2f)
                        {
                            owner.CurrentAttackData = AI.weaponController.CurrentEquipWeapon_Right.WeaponData.AtkNormalDatas.First();
                            AI.ChangeState(NPC_STATE_TYPE.Attack_Normal);
                            return;
                        }
                    }
                }

                if(nearMoveTimer > nearMoveDelay)
                {
                    if (AI.RandomPercent(nearMovePercent))
                    {
                        AI.ChangeState(NPC_STATE_TYPE.WALK);
                        return;
                    }
                }
            }
        }
    }


    #region EQUIP
    public class OnEquip : StateBase<NPC_STATE_TYPE>
    {
        Weapon wepone;
        Npc owner;

        public OnEquip() : base(NPC_STATE_TYPE.ON_EQUIP) { }

        public override void Enter()
        {
            owner ??= AI.OwnerBase as Npc;
            wepone = AI.weaponController.SelectRightWeapon;

            AI.animator.SetBool("isOnEquip", false);
            AI.animator.SetBool("isBattle", true);
            
        }

        public override void Exit()
        {
            AI.animator.SetBool("isOnEquip", true);
        }

        public override void Update()
        {
            if (AI.animator.GetCurrentAnimatorStateInfo(0).IsTag("Equip") &&
                AI.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.75f)
            {
                if(!wepone.IsEquiped)
                {
                    AI.animator.SetFloat("idxWepone", (int)wepone.WeaponData.WeaponIDX);
                    wepone?.OnEquip();
                }

                AI.ChangeState(NPC_STATE_TYPE.IDLE);
            }

            if (AI.animator.GetCurrentAnimatorStateInfo(0).IsTag("Equip")
               && AI.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= wepone.WeaponData.OnDeffDelay
               && !wepone.IsEquiped)
            {
                AI.animator.SetFloat("idxWepone", (int)wepone.WeaponData.WeaponIDX);
                wepone?.OnEquip();
            }
        }
    }
    public class OffEquip : StateBase<NPC_STATE_TYPE>
    {
        Weapon wepone;
        Npc owner;

        public OffEquip() : base(NPC_STATE_TYPE.OFF_EQUIP) { }

        public override void Enter()
        {
            owner ??= AI.OwnerBase as Npc;
            wepone = AI.weaponController.CurrentEquipWeapon_Right;

            AI.animator.SetBool("isOffEquip", false);
            AI.animator.SetBool("isBattle", false);
        }

        public override void Exit()
        {
        }

        public override void Update()
        {
            if (AI.animator.GetCurrentAnimatorStateInfo(0).IsTag("Equip") &&
                AI.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.75f)
            {
                if (wepone.IsEquiped)
                {
                    AI.animator.SetFloat("idxWepone", 0);
                    wepone?.OffEquip();
                }

                AI.animator.SetBool("isOffEquip", true);
                AI.ChangeState(NPC_STATE_TYPE.IDLE);
            }

            if (AI.animator.GetCurrentAnimatorStateInfo(0).IsTag("Equip")
                && AI.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= wepone.WeaponData.OnDeffDelay
                && wepone.IsEquiped)
            {
                AI.animator.SetFloat("idxWepone", 0);
                wepone?.OffEquip();
            }
        }

    }
    public class ChangeEquip : StateBase<NPC_STATE_TYPE>
    {
        Weapon current_equip_weapon;
        Weapon next_equip_weapon;

        bool IsEquipOffAnim;
        bool IsEquipOnAnim;

        Npc owner;

        public ChangeEquip() : base(NPC_STATE_TYPE.CHANGE_EQUIP) { }

        public override void Enter()
        {
            /// 초기화
            IsEquipOnAnim = false;
            IsEquipOffAnim = false;
            owner ??= AI.OwnerBase as Npc;

            /// 오른손 무기 착용 확인
            if (owner.ChangeWeapon_Right)
            {
                current_equip_weapon = AI.weaponController.CurrentEquipWeapon_Right;
                next_equip_weapon = AI.weaponController.SelectRightWeapon;
            }
            /// 왼손 무기착용 확인
            else if (owner.ChangeWeapon_Left)
            {
                current_equip_weapon = AI.weaponController.CurrentEquipWeapon_Left;
                next_equip_weapon = AI.weaponController.SelectLeftWeapon;
            }

            /// 착용중인 무기, 착용 예정인 무기모두 미 존재
            if(current_equip_weapon == null && next_equip_weapon == null)
            {
                AI.ChangeState(NPC_STATE_TYPE.IDLE);
                return;
            }

            /// 착용중인 무기가 있는 경우
            if (current_equip_weapon)
            {
                /// 무기 해제
                _EquipOff();
            }
            /// 착용 예정인 무기가 있는 경우
            else if (next_equip_weapon)
            {
                /// 무기 착용
                _EquipOn();
            }
        }

        public override void Exit()
        {
            owner.ChangeWeapon_Left = false;
            owner.ChangeWeapon_Right = false;

            AI.animator.SetBool("isEquip", false);
        }

        public override void Update()
        {
            AI.UpdateAnimationState();

            /// 무기 해제로직 확인
            if (IsEquipOffAnim && AI.IsAnimationTag("EquipOff"))
            {
                /// 정상 종료일 경우
                if (AI.CurrentAnimationState.normalizedTime >= 0.9f)
                {
                    /// 무기 해제
                    IsEquipOffAnim = false;
                    current_equip_weapon.OffEquip();

                    /// 현재 착용중인 무기 해제
                    if (owner.ChangeWeapon_Left) AI.weaponController.CurrentEquipWeapon_Left = null;
                    if (owner.ChangeWeapon_Right) AI.weaponController.CurrentEquipWeapon_Right = null;

                    /// 현재 무기 -> 대기 상태로 변경
                    if (owner.ChangeWeapon_Left) AI.weaponController.SelectRightWeapon = current_equip_weapon;
                    if (owner.ChangeWeapon_Right) AI.weaponController.SelectLeftWeapon = current_equip_weapon;

                    /// 다음 착용 예정 무기 확인
                    if (next_equip_weapon)
                    {
                        /// 무기 착용 로직 시작
                        IsEquipOnAnim = true;
                        _EquipOn();
                    }
                }
                /// 무기 위치 변경 프레임 확인
                if (AI.CurrentAnimationState.normalizedTime >= current_equip_weapon.WeaponData.OffEquipDelay)
                {
                    current_equip_weapon.OffEquip();
                }
            }
            /// 무기 착용 로직 확인
            else if (IsEquipOnAnim && AI.IsAnimationTag("EquipOn"))
            {
                /// 정상 종료 확인
                if (AI.CurrentAnimationState.normalizedTime >= 0.9f)
                {
                    /// 무기 착용
                    IsEquipOnAnim = false;
                    next_equip_weapon.OnEquip();

                    /// 현재 착용중인 무기 변경
                    if (owner.ChangeWeapon_Left) AI.weaponController.CurrentEquipWeapon_Left = next_equip_weapon;
                    if (owner.ChangeWeapon_Right) AI.weaponController.CurrentEquipWeapon_Right = next_equip_weapon; 

                    AI.ChangeState(NPC_STATE_TYPE.IDLE);
                    return;
                }
                /// 무기 위치 변경 프레임 확인
                if (AI.CurrentAnimationState.normalizedTime >= next_equip_weapon.WeaponData.OnEquipDelay)
                {
                    /// 무기 위치 변경
                    next_equip_weapon.OnEquip();
                }
            }
        }

        /// --------------------------------------
        private void _EquipOff()
        {
            IsEquipOffAnim = true;
            AI.animator.SetFloat("idxWeapon", (float)current_equip_weapon.WeaponData.WeaponIDX);
            AI.animator.SetBool("isEquip", true);
            AI.animator.SetBool("isOffEquip", true);
            AI.animator.SetBool("isBattle", false);
            AI.animator.SetTrigger("StateOn");
        }

        private void _EquipOn()
        {
            IsEquipOnAnim = true;
            AI.animator.SetFloat("idxWeapon", (float)next_equip_weapon.WeaponData.WeaponIDX);
            AI.animator.SetBool("isEquip", true);
            AI.animator.SetBool("isOnEquip", true);
            AI.animator.SetBool("isBattle", true);
            AI.animator.SetTrigger("StateOn");
        }
    }
    #endregion


    #region MOVEMENT
    public class Walk : StateBase<NPC_STATE_TYPE>
    {
        Npc owner;
        public Walk() : base(NPC_STATE_TYPE.WALK) { }

        public override void Enter() 
        {
            AI.InitTimer();

            owner ??= AI.OwnerBase as Npc;
            AI.animator.SetBool("isWalk", true);

            AI.animator.speed = 0.65f;
        }

        public override void Exit()
        {
            AI.animator.speed = 1f;
            AI.animator.SetBool("isWalk", false);
        }

        public override void Update()
        {
            AI.UpdateTimer();
            AI.UpdateAnimationState();

            AI.animator.SetFloat("xDir", AI.transform.forward.x);
            AI.animator.SetFloat("yDir", AI.transform.forward.z);

            TransformExtentions.LookAtTarget_Humanoid_Dir(AI.transform, AI.vMoveSmoothDir, owner.Data.WalkSpeed);
        }
    }
    public class Run : StateBase<NPC_STATE_TYPE>
    {
        Npc owner;

        public Run() : base(NPC_STATE_TYPE.RUN) { }

        public override void Enter()
        {
            AI.InitTimer();

            owner ??= AI.OwnerBase as Npc;

            if (owner.UseNaveMashAgent)
            {
                AI.nav.speed = 2f;
                AI.nav.isStopped = false;
                AI.nav.angularSpeed = 180;
                AI.nav.updateRotation = true;
            }

            AI.animator.applyRootMotion = false;
            AI.animator.SetBool("isRun", true);
        }

        public override void Exit()
        {
            if (owner.UseNaveMashAgent)
            {
                AI.nav.isStopped = true;
            }

            AI.animator.applyRootMotion = true;
            AI.animator.SetBool("isRun", false);
        }

        public override void Update()
        {
            if(owner.Target != null)
            {
                if (AI.animator.GetBool("isBattle"))
                    AI.animator.SetBool("isBattle", true);

                AI.vMoveDir = Vector3.forward;
                AI.transform.Translate(AI.vMoveDir * owner.Data.RunSpeed * Time.deltaTime);

                AI.animator.SetFloat("xDir", AI.vMoveDir.x);
                AI.animator.SetFloat("yDir", AI.vMoveDir.z);

                TransformExtentions.LookAtTarget_Humanoid(AI.transform, owner.Target, 5 * owner.Data.RunSpeed * 2.5f);
            }
        }
    }
    #endregion

    
    public class Def : StateBase<NPC_STATE_TYPE>
    {
        float block_delay;

        public Def() : base(NPC_STATE_TYPE.DEF) {}

        public override void Enter()
        {
            block_delay = Random.Range(0.5f, 2f);

            AI.animator.SetBool("isDef", true);
            AI.transform.DOScale(Vector3.one, block_delay).OnComplete(() =>
            {
                AI.ChangeState(NPC_STATE_TYPE.IDLE_BATTLE);
            });
        }

        public override void Exit()
        {
            AI.animator.SetBool("isDef", false);
        }

        public override void Update()
        {
        }
    }


    #region ATTACK
    public class Attack_Normal : StateBase<NPC_STATE_TYPE>
    {
        Npc owner;
        Weapon weapon;

        string animationName;
        int atkIndex;
        int hitboxIndex;
        WeaponAttackData tAttackData;

        public Attack_Normal() : base(NPC_STATE_TYPE.Attack_Normal) {}

        public override void Enter()
        {
            AI.InitTimer();

            hitboxIndex = 0;
            owner ??= AI.OwnerBase as Npc;
            weapon = AI.weaponController.CurrentEquipWeapon_Right;
            tAttackData = owner.CurrentAttackData;
            atkIndex = weapon.WeaponData.AtkNormalDatas.IndexOf(tAttackData);

            if(Vector3.Distance(AI.transform.position, owner.Target.position) < tAttackData.DistanceToTarget)
            {
                var dir = AI.transform.position - owner.Target.position;
                AI.transform.DOMove(owner.Target.position + dir * tAttackData.DistanceToTarget, 0.4f);
            }

            animationName = "isAtk_N_" + (atkIndex + 1).ToString();
            AI.animator.SetBool(animationName, true);
        }

        public override void Exit()
        {
            AI.transform.DOKill();
            AI.animator.SetBool(animationName, false);
        }

        public override void Update()
        {
            TransformExtentions.LookAtTarget_Humanoid(AI.transform, owner.Target, 5 * owner.Data.RunSpeed * 2.5f);

            AI.UpdateAnimationState();

            // 다음 공격이 실행되지 않았을 경우 해당 공격을 정상 종료
            if (AI.IsAnimationTag(animationName) && AI.CurrentAnimationState.normalizedTime >= 0.9)
            {
                owner.CurrentAttackData = null;
                AI.ChangeState(NPC_STATE_TYPE.IDLE);
                return;
            }

            // 공격 히트박스 생성 프레임 확인
            if (AI.IsAnimationTag(animationName)
             && tAttackData.tHitBoxDatas.Count > hitboxIndex
             && AI.CurrentAnimationState.normalizedTime >= tAttackData.tHitBoxDatas[hitboxIndex].StartDelay)
            {
                if (tAttackData.IsProjectile)
                {
                    weapon.OnLaunch();
                }
                else
                {
                    var hitBox = PoolManager.Instance.GetObject<HitBox_Player>().GetComponent<HitBox_Player>();
                    hitBox.transform.position = AI.transform.position;
                    hitBox.transform.rotation = AI.transform.rotation;
                    hitBox.Enter(tAttackData.tHitBoxDatas[hitboxIndex]
                               , owner
                               , parryDelay: tAttackData.tHitBoxDatas[hitboxIndex].GetParryDelay(AI.CurrentAnimationState.speed)
                               , exitDelay: tAttackData.tHitBoxDatas[hitboxIndex].GetExitDelay(AI.CurrentAnimationState.speed));
                }

                hitboxIndex++;
            }

            // 다음 공격 존재 확인
            if (weapon.WeaponData.AtkNormalDatas.Count <= (atkIndex + 1)) return;

            // 다음 공격 프레임 확인
            if (AI.IsAnimationTag(animationName)
             && AI.IsAnimationInRange(tAttackData.NextInputStartDelay + Random.Range(0, tAttackData.NextInputExitDelay - tAttackData.NextInputStartDelay), tAttackData.NextInputExitDelay))
            {
                owner.CurrentAttackData = weapon.WeaponData.AtkNormalDatas[atkIndex + 1];
                AI.ChangeState(NPC_STATE_TYPE.Attack_Normal);
                return;
            }
        }
    }
    #endregion


    #region HIT
    public class Hit : StateBase<NPC_STATE_TYPE>
    {
        Npc owner;

        public Hit() : base(NPC_STATE_TYPE.HIT) { }

        public override void Enter()
        {
            AI.InitTimer();

            owner ??= AI.GetOwner().GetComponent<Npc>();

            AI.animator.SetBool("isHit", true);
            AI.animator.SetTrigger("StateOn");
        }

        public override void Exit()
        {
            AI.animator.SetBool("isHit", false);
        }

        public override void Update()
        {
            AI.UpdateTimer();
            AI.UpdateAnimationState();

            if(AI.IsAnimationTag("Hit") && AI.CurrentAnimationState.normalizedTime >= 0.9f)
            {
                AI.ChangeState(NPC_STATE_TYPE.IDLE);
                return;
            }

            if(AI.CurTimer > owner.Data.HitDelayNormal)
            {
                if (AI.weaponController.HasWeapon())
                {
                    owner.Target = PlayerManager.Instance.Player.transform;
                    if (!AI.animator.GetBool("isBattle")) AI.ChangeState(NPC_STATE_TYPE.ON_EQUIP);
                    else AI.ChangeState(NPC_STATE_TYPE.IDLE);
                }
                else AI.ChangeState(NPC_STATE_TYPE.IDLE);
            }
        }
    }
    public class Hit_Kill : StateBase<NPC_STATE_TYPE>
    {
        Npc owner;

        public Hit_Kill() : base(NPC_STATE_TYPE.HIT_KILL) { }

        public override void Enter()
        {
            owner ??= AI.GetOwner().GetComponent<Npc>();

            AI.animator.SetBool("isIdle", false);
            AI.animator.SetBool("isHit_Kill", true);
            AI.animator.SetTrigger("StateOn");
        }

        public override void Exit()
        {
            AI.animator.SetBool("isHit_Kill", false);
        }

        public override void Update()
        {
            if (AI.animator.GetCurrentAnimatorStateInfo(0).IsTag("HitKill") &&
            AI.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.85f)
            {
                AI.ChangeState(NPC_STATE_TYPE.DEATH);
            }
        }
    }
    public class Hit_Back_Kill : StateBase<NPC_STATE_TYPE>
    {
        Npc owner;

        public Hit_Back_Kill() : base(NPC_STATE_TYPE.HIT_BACK_KILL) { }

        public override void Enter()
        {
            owner ??= AI.GetOwner().GetComponent<Npc>();

            AI.rb.isKinematic = true;
            AI.collider.enabled = false;
            AI.animator.SetBool("isIdle", false);
            AI.animator.SetBool("isHit_Back", true);
            AI.animator.SetTrigger("StateOn");
        }

        public override void Exit()
        {
            AI.collider.enabled = true;
            AI.animator.SetBool("isHit_Back", false);
        }

        public override void Update()
        {
            if (AI.animator.GetCurrentAnimatorStateInfo(0).IsTag("HitBack") &&
            AI.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.85f)
            {
                AI.ChangeState(NPC_STATE_TYPE.DEATH);
            }
        }
    }
    #endregion


    public class Death : StateBase<NPC_STATE_TYPE>
    {
        Weapon weapon;
        Npc owner;

        public Death() : base(NPC_STATE_TYPE.DEATH) { }

        public override void Enter()
        {
            AI.transform.DOKill();

            owner ??= AI.OwnerBase as Npc;
            weapon = AI.weaponController.CurrentEquipWeapon_Right;

            AI.collider.isTrigger = true;
            AI.legsAnimator.enabled = false;
            AI.animator.SetBool("isDeath", true);
            AI.animator.SetTrigger("StateOn");
        }

        public override void Exit()
        {
        }

        public override void Update()
        {
        }
    }
}

