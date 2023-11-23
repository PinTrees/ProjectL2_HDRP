using DG.Tweening;
using Invector;
using KWS;
using NPC.Animalls.State.HorseBase;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;



namespace PlayerState
{
    public class Idle : StateBase<PLAYER_STATE>
    {
        public Idle() : base(PLAYER_STATE.IDLE) {}

        public override void Enter()
        {
            AI.legsAnimator.enabled = true;
            AI.animator.SetTrigger("StateOn");
            AI.animator.SetBool("isIdle", true);
        }

        public override void Exit()
        {
            AI.animator.SetBool("isIdle", false);
        }

        public override void Update()
        {
            if(Input.GetKeyDown(KeyCode.Q))
            {
                if (AI.animator.GetBool("isBattle")) AI.ChangeState(PLAYER_STATE.GUARD);
                return;
            }
        }
    }
    public class Death : StateBase<PLAYER_STATE>
    {
        public Death() : base(PLAYER_STATE.DEATH) { }

        public override void Enter()
        {
            AI.legsAnimator.enabled = true;
            AI.animator.SetTrigger("StateOn");
            AI.animator.SetBool("isDeath", true);
        }

        public override void Exit()
        {
            AI.animator.SetBool("isDeath", false);
        }

        public override void Update()
        {
        }
    }

    #region Equip
    public class OnEquip : StateBase<PLAYER_STATE>
    {
        Weapon weapon;
        int weaponIdx = 0;

        public OnEquip() : base(PLAYER_STATE.ON_EQUIP) { }

        public override void Enter()
        {
            if (PlayerStaticStatus.ChangeWeaponLeft)
            {
                weapon = AI.weaponController.SelectLeftWeapon;
                weaponIdx = AI.weaponController.GetSelectWeaponIdx_Left();
            }
            else if (PlayerStaticStatus.ChangeWeaponRight)
            {
                weapon = AI.weaponController.SelectRightWeapon;
                weaponIdx = AI.weaponController.GetSelectWeaponIdx_Right();
            }

            if (weapon == null)
            {
                AI.ChangeState(PLAYER_STATE.IDLE);
                return;
            }

            AI.animator.applyRootMotion = false;
            AI.lookAnimator.enabled = false;

            AI.animator.SetFloat("idxWeapon", weaponIdx);
            AI.animator.SetBool("isEquip", true);
            AI.animator.SetBool("isOnEquip", true);
            AI.animator.SetTrigger("StateOn");
        }

        public override void Exit()
        {
            PlayerStaticStatus.ChangeWeaponLeft = false;
            PlayerStaticStatus.ChangeWeaponRight = false;

            AI.lookAnimator.enabled = true;
            AI.animator.applyRootMotion = true;

            AI.animator.SetBool("isBattle", true);
            AI.animator.SetBool("isEquip", false);
            AI.animator.SetBool("isOnEquip", false);
        }

        public override void Update()
        {
            AI.UpdateAnimationState();

            if (AI.IsAnimationTag("EquipOn") && AI.CurrentAnimationState.normalizedTime >= 0.85f)
            {
                if (!weapon.IsEquiped) weapon.OnEquip();
                AI.ChangeState(PLAYER_STATE.IDLE);
                return;
            }

            if(AI.IsAnimationTag("EquipOn") && AI.CurrentAnimationState.normalizedTime >= weapon.WeaponData.OnDeffDelay && !weapon.IsEquiped)
            {
                weapon.OnEquip();
            }

            if(!AI.IsStopped)
            {
                AI.transform.Translate(AI.vMoveDir * 1.5f * Time.deltaTime);

                Vector3 cameraForward = Camera.main.transform.forward.normalized;
                cameraForward.y = 0;

                AI.transform.DOKill();
                AI.transform.DORotate(Quaternion.LookRotation(cameraForward).eulerAngles, 0.3f);
            }
        }
    }
    public class OffEquip : StateBase<PLAYER_STATE>
    {
        Weapon weapon;
        int weaponIdx = 0;

        public OffEquip() : base(PLAYER_STATE.OFF_EQUIP) { }

        public override void Enter()
        {
            if (PlayerStaticStatus.ChangeWeaponLeft)
            {
                weapon = AI.weaponController.CurrentEquipWeapon_Left;
                weaponIdx = AI.weaponController.GetEquipWeaponIdx_Left();
            }
            else if (PlayerStaticStatus.ChangeWeaponRight)
            {
                weapon = AI.weaponController.CurrentEquipWeapon_Right;
                weaponIdx = AI.weaponController.GetEquipWeaponIdx_Right();
            }

            if (weapon == null)
            {
                AI.ChangeState(PLAYER_STATE.IDLE);
                return;
            }

            AI.lookAnimator.enabled = false;
            AI.animator.applyRootMotion = false;

            AI.animator.SetFloat("idxWeapon", weaponIdx);

            AI.animator.SetBool("isEquip", true);
            AI.animator.SetBool("isOffEquip", true);
            AI.animator.SetTrigger("StateOn");
        }

        public override void Exit()
        {
            PlayerStaticStatus.ChangeWeaponLeft = false;
            PlayerStaticStatus.ChangeWeaponRight = false;

            AI.lookAnimator.enabled = true;
            AI.animator.applyRootMotion = true;

            AI.animator.SetBool("isBattle", false);
            AI.animator.SetBool("isEquip", false);
            AI.animator.SetBool("isOffEquip", false);
        }

        public override void Update()
        {
            AI.UpdateAnimationState();

            if (AI.IsAnimationTag("EquipOff") && AI.CurrentAnimationState.normalizedTime >= 0.85f)
            {
                if(weapon.IsEquiped) weapon.OffEquip();
                AI.ChangeState(PLAYER_STATE.IDLE);
                return;
            }

            if (AI.IsAnimationTag("EquipOff") && AI.CurrentAnimationState.normalizedTime >= weapon.WeaponData.OffEquipDelay && weapon.IsEquiped)
            {
                weapon.OffEquip();
            }

            if (!AI.IsStopped)
            {
                Vector3 cameraForward = Camera.main.transform.TransformDirection(AI.vMoveDir).normalized;
                cameraForward.y = 0;
                Quaternion rotation = Quaternion.LookRotation(cameraForward);
                AI.transform.rotation = Quaternion.Slerp(AI.transform.rotation, rotation, 5 * Time.deltaTime);

                AI.transform.Translate(Vector3.forward * 1.5f * Time.deltaTime);
            }
        }
    }
    #endregion


    #region Movenent
    public class Walk : StateBase<PLAYER_STATE>
    {
        public Walk() : base(PLAYER_STATE.WALK) {}

        public override void Enter()
        {
            AI.legsAnimator.enabled = false;
            AI.animator.SetTrigger("StateOn");
            AI.animator.SetBool("isWalk", true);
        }

        public override void Exit()
        {
            AI.legsAnimator.enabled = true;
            AI.animator.SetBool("isWalk", false);
        }

        public override void Update()
        {
            if (AI.IsStopped)
            {
                AI.ChangeState(PLAYER_STATE.IDLE);
                return;
            }

            if (PlayerStaticStatus.isSwimming)
            {
                AI.ChangeState(PLAYER_STATE.SWIMMING);
                return;
            }
            if (Input.GetKey(KeyCode.LeftShift))
            {
                AI.ChangeState(PLAYER_STATE.RUN);
                return;
            }
            if (AI.animator.GetBool("isBattle"))
            {
                if(Input.GetKey(KeyCode.Q))
                {
                    AI.ChangeState(PLAYER_STATE.GUARD);
                    return;
                }

                Vector3 cameraForward = Camera.main.transform.forward.normalized;
                cameraForward.y = 0;

                AI.transform.DOKill();
                AI.transform.DORotate(Quaternion.LookRotation(cameraForward).eulerAngles, 0.3f);
            }
            else
            {
                if (AI.vMoveDir == Vector3.zero) return;

                Vector3 cameraForward = Camera.main.transform.TransformDirection(AI.vMoveDir).normalized;
                cameraForward.y = 0; 

                Quaternion rotation = Quaternion.LookRotation(cameraForward); 
                AI.transform.rotation = Quaternion.Slerp(AI.transform.rotation, rotation, 5 * Time.deltaTime); 
            }
        }
    }
    public class Run : StateBase<PLAYER_STATE>
    {
        public Run() : base(PLAYER_STATE.RUN) {}

        public override void Enter()
        {
            AI.legsAnimator.enabled = false;
            AI.animator.SetTrigger("StateOn");
            AI.animator.SetBool("isRun", true);
        }

        public override void Exit()
        {
            AI.legsAnimator.enabled = true;
            AI.animator.SetBool("isRun", false);
        }

        public override void Update()
        {
            if (AI.IsStopped)
            {
                AI.ChangeState(PLAYER_STATE.IDLE);
                return;
            }

            if (PlayerStaticStatus.isSwimming)
            {
                AI.ChangeState(PLAYER_STATE.SWIMMING);
                return;
            }
            if (AI.animator.GetBool("isBattle"))
            {
                if (Input.GetKey(KeyCode.Q))
                {
                    AI.ChangeState(PLAYER_STATE.GUARD);
                    return;
                }

                Vector3 cameraForward = Camera.main.transform.forward.normalized;
                cameraForward.y = 0;

                AI.transform.DOKill();
                AI.transform.DORotate(Quaternion.LookRotation(cameraForward).eulerAngles, 0.3f);
            }
            else
            {
                if (AI.vMoveDir == Vector3.zero) return;

                Vector3 cameraForward = Camera.main.transform.TransformDirection(AI.vMoveDir).normalized;
                cameraForward.y = 0;

                Quaternion rotation = Quaternion.LookRotation(cameraForward);                    // 회전 방향 계산
                AI.transform.rotation = Quaternion.Slerp(AI.transform.rotation, rotation, 5 * Time.deltaTime);    // 회전 보간
            }
        }
    }
    public class Jump : StateBase<PLAYER_STATE>
    {
        const float GROUND_RAY_LENGHT = 0.5f + 0.1f;
        const float GROUND_RAY_SIZE = 0.2f;

        const float JUMP_HEIGHT = 1f;
        const float JUMP_MOVE_SPEED = 1.8f;

        bool IsEnd = false;

        public Jump() : base(PLAYER_STATE.JUMP) {}

        public override void Enter()
        {
            IsEnd = false;
            AI.IsAnimatorInit = false;
            AI.transform.DOKill();

            PlayerStaticStatus.isGrounded = false;

            AI.legsAnimator.enabled = false;
            AI.animator.applyRootMotion = false;
            AI.animator.SetBool("isJump", true);
            AI.animator.SetTrigger("StateOn");
        }

        public override void Exit()
        {
            PlayerStaticStatus.isGrounded = true;

            AI.legsAnimator.enabled = true;
            AI.animator.applyRootMotion = true;
            AI.animator.SetBool("isJump", false);
        }

        public override void Update()
        {
            AI.UpdateAnimationState();

            if (PlayerStaticStatus.isSwimming)
            {
                AI.ChangeState(PLAYER_STATE.SWIMMING);
                return;
            }

            if (!AI.IsAnimatorInit && AI.IsAnimationTag("JumpStart"))
            {
                AI.IsAnimatorInit = true;
                //AI.transform.DOMoveY(AI.transform.position.y + JUMP_HEIGHT, 0.8f);
                AI.rb.AddForce(Vector3.up * 150, ForceMode.Impulse);            
            }

            if(!IsEnd && AI.IsAnimationTag("JumpLoop"))
            {
                PlayerStaticStatus.isGrounded = false;

                int layerMask = ~(1 << LayerMask.NameToLayer("Water"));
                PlayerStaticStatus.isGrounded = Physics.BoxCast(AI.transform.position + Vector3.up * 0.5f, Vector3.one * GROUND_RAY_SIZE, Vector3.down, Quaternion.identity, GROUND_RAY_LENGHT, layerMask);

                if (PlayerStaticStatus.isGrounded)
                {
                    AI.animator.SetTrigger("isJumpExit");
                    IsEnd = true;
                }
            }
            else if(IsEnd && AI.IsAnimationTag("JumpEnd") && AI.CurrentAnimationState.normalizedTime > 0.7f)
            {
                AI.ChangeState(PLAYER_STATE.IDLE);
                return;
            }

            Vector3 cameraForward = Camera.main.transform.forward.normalized;    
            cameraForward.y = 0;           

            var camRotation = Camera.main.transform.rotation;
            camRotation.x = camRotation.z = 0;

            Quaternion rotation = Quaternion.LookRotation(cameraForward);
            AI.transform.position = AI.transform.position + AI.vMoveDir * JUMP_MOVE_SPEED * Time.deltaTime;
        }

        public override void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawCube((AI.transform.position + Vector3.up * 0.5f) + Vector3.down * GROUND_RAY_LENGHT, Vector3.one * GROUND_RAY_SIZE);
        }
    }
    public class Dodge : StateBase<PLAYER_STATE>
    {
        const float END_DELAY_PER = 0.75f;

        const float START_DELAY = 0.05f;
        const float END_IK_DELAY = 0.5f;

        const float DODGE_RANGE = 5f;

        private bool isDodge = false;

        public Dodge() : base(PLAYER_STATE.DODGE) { }

        public override void Enter()
        {
            isDodge = false;
            AI.IsAnimatorInit = false;

            if (!AI.animator.GetBool("isBattle"))
            {
                AI.animator.SetFloat("xDir", Vector3.forward.x);
                AI.animator.SetFloat("yDir", Vector3.forward.z);
            }
            else
            {
                AI.animator.SetFloat("xDir", AI.vMoveDir.x);
                AI.animator.SetFloat("yDir", AI.vMoveDir.z);
            }

            AI.legsAnimator.enabled = false;
            AI.animator.applyRootMotion = false;

            AI.animator.SetBool("isDodge", true);
            AI.animator.SetTrigger("StateOn");
        }

        public override void Exit()
        {
            PlayerStaticStatus.IsGodMode = false;

            AI.transform.DOScale(Vector3.one, END_IK_DELAY).OnComplete(() =>
            {
                AI.legsAnimator.enabled = true;
            });

            AI.transform.DOKill();
            AI.animator.applyRootMotion = true;
            AI.animator.SetBool("isDodge", false);
        }

        public override void Update()
        {
            AI.UpdateAnimationState();

            if(!AI.IsAnimatorInit && AI.IsAnimationTag("Dodge"))
            {
                AI.IsAnimatorInit = true;
                isDodge = true;
            }

            if (AI.IsAnimationTag("Dodge")
             && AI.CurrentAnimationState.normalizedTime >= END_DELAY_PER)
            {
                if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0) AI.ChangeState(PLAYER_STATE.IDLE);
                else if (Input.GetKey(KeyCode.LeftShift)) AI.ChangeState(PLAYER_STATE.RUN);
                else AI.ChangeState(PLAYER_STATE.WALK);
                return;
            }

            // 플레이어 구르기 무적 프레임 확인
            if (AI.IsAnimationTag("Dodge")
             && AI.IsAnimationInRange(0.05f, 0.6f)
             && !PlayerStaticStatus.IsGodMode)
            {
                PlayerStaticStatus.IsGodMode = true;
            }

            if (AI.IsAnimationTag("Dodge")
             && PlayerStaticStatus.IsGodMode
             && AI.CurrentAnimationState.normalizedTime > 0.6f)
            {
                PlayerStaticStatus.IsGodMode = false;
            }

            AI.transform.Translate(Vector3.down * 1f * Time.deltaTime);

            if (isDodge)
            {
                if (!AI.animator.GetBool("isBattle"))
                {
                    AI.transform.Translate(Vector3.forward * DODGE_RANGE * Time.deltaTime);
                }
                else
                {
                    AI.transform.Translate(AI.vMoveDir * DODGE_RANGE * Time.deltaTime);
                }
            }
        }
    }
    #endregion


    #region Water Interaction
    public class Swimming : StateBase<PLAYER_STATE>
    {
        const float GROUND_RAY_LENGHT = 2f;
        const float GROUND_START_HEIGHT = 2.5f;
        const float GROUND_RAY_SCALE = 1f;

        Transform transform;
        KW_Buoyancy buoyancy;

        public Swimming() : base(PLAYER_STATE.SWIMMING) {}

        public override void Enter()
        {
            transform ??= PlayerManager.Instance.GetPlayerTransform();
            buoyancy ??= AI.transform.GetComponent<KW_Buoyancy>();

            buoyancy.enabled = true;
            AI.legsAnimator.enabled = false;
            AI.animator.SetBool("isSwimming", true);
            AI.animator.SetBool("isJump", false);
        }

        public override void Exit()
        {
            AI.legsAnimator.enabled = true;
            AI.animator.SetBool("isSwimming", false);
            AI.animator.SetBool("isRun", false);
            AI.animator.SetBool("isIdle", false);
        }

        public override void Update()
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            int layerMask = ~(1 << LayerMask.NameToLayer("Water") | 1 << LayerMask.NameToLayer("Player"));
            PlayerStaticStatus.isGrounded = Physics.BoxCast(transform.position + Vector3.up * GROUND_START_HEIGHT, Vector3.one * GROUND_RAY_SCALE, Vector3.down, Quaternion.identity, GROUND_RAY_LENGHT, layerMask);

            if (!PlayerStaticStatus.isSwimming)
            {
                if (horizontalInput == 0 && verticalInput == 0)
                {
                    AI.ChangeState(PLAYER_STATE.IDLE);
                }
                else
                {
                    if (Input.GetKey(KeyCode.LeftShift)) AI.ChangeState(PLAYER_STATE.RUN);
                    else AI.ChangeState(PLAYER_STATE.WALK);
                }
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                AI.ChangeState(PLAYER_STATE.DIVE);
                return;
            }

            if (horizontalInput == 0 && verticalInput == 0)
            {
                AI.animator.SetBool("isRun", false);
                AI.animator.SetBool("isIdle", true);
            }
            else 
            {
                AI.animator.SetBool("isRun", true);
                AI.animator.SetBool("isIdle", false);

                Vector3 moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;
                Vector3 cameraForward = Camera.main.transform.TransformDirection(moveDirection).normalized;
                cameraForward.y = 0; 
                
                Quaternion rotation = Quaternion.LookRotation(cameraForward); // 회전 방향 계산
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 5 * Time.deltaTime); // 회전 보간
                transform.Translate(Vector3.forward * 3 * Time.deltaTime);
            }
        }

        public override void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawCube((transform.position + Vector3.up * GROUND_START_HEIGHT) + Vector3.down * GROUND_RAY_LENGHT, Vector3.one * GROUND_RAY_SCALE);
        }
    }
    public class Dive : StateBase<PLAYER_STATE>
    {
        const float SURFACE_WATER_CHECK_LENGTH = 1.15f;
        const float SURFACE_START_HEIGHT = 1.15f;

        Transform transform;

        public Dive() : base(PLAYER_STATE.DIVE) {}

        public override void Enter()
        {
            transform ??= PlayerManager.Instance.GetPlayerTransform();
            
            PlayerManager.Instance.Player.UseBuoyancy = false;
            AI.rb.useGravity = false;

            AI.animator.SetBool("isDive", true);
            transform.DOMoveY(transform.position.y - 5f, 0.5f).OnComplete(() =>
            {
            });
        }

        public override void Exit()
        {
            PlayerManager.Instance.Player.UseBuoyancy = true;
            AI.rb.useGravity = true;
            AI.animator.SetBool("isDive", false);
        }

        public override void Update()
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            int waterLayerMask = 1 << LayerMask.NameToLayer("Water");
            PlayerStaticStatus.isSurfaceWater = Physics.BoxCast(transform.position + Vector3.up * SURFACE_START_HEIGHT, Vector3.one * 0.5f, Vector3.down, Quaternion.identity, SURFACE_WATER_CHECK_LENGTH, waterLayerMask);
            if(PlayerStaticStatus.isSurfaceWater)
            {
                AI.ChangeState(PLAYER_STATE.SWIMMING);
                return;
            }

            if (horizontalInput == 0 && verticalInput == 0)
            {
                AI.animator.SetBool("isRun", false);
                AI.animator.SetBool("isIdle", true);
            }
            else
            {
                AI.animator.SetBool("isRun", true);

                Vector3 moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;
                Vector3 cameraForward = Camera.main.transform.TransformDirection(moveDirection).normalized;

                Quaternion rotation = Quaternion.LookRotation(cameraForward); // 회전 방향 계산
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 5 * Time.deltaTime); // 회전 보간
                transform.Translate(Vector3.forward * 3 * Time.deltaTime);
            }
        }

        public override void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube((transform.position + Vector3.up * SURFACE_START_HEIGHT) + Vector3.down * SURFACE_WATER_CHECK_LENGTH, Vector3.one * 0.5f);
        }
    }
    #endregion


    #region Hit
    public class Hit : StateBase<PLAYER_STATE>
    {
        public Hit() : base(PLAYER_STATE.HIT) {}

        public override void Enter()
        {
            AI.IsAnimatorInit = false;
            AI.animator.applyRootMotion = false;

            AI.animator.SetBool("isHit", true);
            AI.animator.SetTrigger("StateOn");
        }

        public override void Exit()
        {
            AI.animator.applyRootMotion = true;

            AI.animator.SetInteger("idxHitForce", 0);
            AI.animator.SetFloat("fHitStrength", 0);

            AI.transform.DOKill();
            AI.animator.SetBool("isHit", false);
        }

        public override void Update()
        {
            AI.UpdateAnimationState();

            if (!AI.IsAnimatorInit && AI.PriveStateType == PLAYER_STATE.HIT)
            {
                InitAnimator();
                return;
            }
            else if(AI.IsAnimationTag("Hit") && AI.CurrentAnimationState.normalizedTime > 0.9f)
            {
                AI.ChangeState(PLAYER_STATE.IDLE);
                return;
            }
        }

        public void InitAnimator()
        {
            if (AI.IsAnimationTag("Hit"))
            {
                float clipLength = AI.CurrentAnimationState.length;

                float newNormalizedTime = Mathf.Clamp01(0);
                float newTimeValue = newNormalizedTime * clipLength;
                AI.animator.Play(AI.CurrentAnimationState.fullPathHash, -1, newTimeValue / clipLength);

                AI.IsAnimatorInit = true;
            }
        }
    }
    public class Hit_Kill : StateBase<PLAYER_STATE>
    {
        const float END_DELAY = 3f;

        public Hit_Kill() : base(PLAYER_STATE.HIT_KILL) { }

        public override void Enter()
        {
            AI.animator.SetBool("isHit_Kill", true);
            AI.transform.DOScale(Vector3.one, END_DELAY).OnComplete(() =>
            {
                AI.ChangeState(PLAYER_STATE.IDLE);
            });
        }

        public override void Exit()
        {
            AI.animator.SetBool("isHit_Kill", false);
        }

        public override void Update()
        {
        }
    }
    #endregion


    #region Horse Interaction
    public class HorseRide : StateBase<PLAYER_STATE>
    {
        const float RIDE_DELAY = 1.3f;
        Horse horse;

        public HorseRide() : base(PLAYER_STATE.HORSE_RIDE) {}

        public override void Enter()
        {
            PlayerManager.Instance.IsCanInteract = false;
            AI.InitTimer();

            AI.lookAnimator.enabled = false;
            AI.legsAnimator.enabled = false;
            AI.rb.useGravity = false;
            AI.collider.isTrigger = true;

            horse = PlayerStaticStatus.currentHorse;
            AI.transform.SetParent(horse.RideOnTransform);
            AI.transform.DOMove(horse.RideOnTransform.position, RIDE_DELAY);
            AI.transform.DOLocalRotate(Vector3.zero, RIDE_DELAY);

            AI.animator.SetTrigger("StateOn");
            AI.animator.SetBool("isHorseRide", true);
            AI.animator.SetBool("isVehicle", true);
        }

        public override void Exit()
        {
            AI.transform.DOKill();
        }

        public override void Update()
        {
            AI.UpdateTimer();

            if(AI.CurTimer > RIDE_DELAY)
            {
                AI.ChangeState(PLAYER_STATE.HORSE_IDLE);
                horse.AI.ChangeState(HORSE_STATE.IDLE);
                return;
            }
        }
    }
    public class HorseUnRide : StateBase<PLAYER_STATE>
    {
        const float EXIT_DELAY = 1.3f;

        Horse horse;
        public HorseUnRide() : base(PLAYER_STATE.HORSE_UNRIDE) {}

        public override void Enter()
        {
            AI.rb.useGravity = false;
            AI.collider.isTrigger = true;
            AI.legsAnimator.enabled = false;

            horse = PlayerStaticStatus.currentHorse;

            AI.animator.SetTrigger("StateOn");
            AI.animator.SetBool("isHorseRide", false);
            AI.animator.SetBool("isVehicle", true);
            AI.transform.SetParent(PlayerManager.Instance.GetPlayerRootTransform(), true);

            AI.transform.DOMove(horse.RideOffTransform.position, EXIT_DELAY);
            AI.transform.DOScale(AI.transform.localScale, EXIT_DELAY).OnComplete(() =>
            {
                AI.ChangeState(PLAYER_STATE.IDLE);
                horse.AI.ChangeState(HORSE_STATE.IDLE);
            });
        }

        public override void Exit()
        {
            AI.rb.useGravity = true;
            AI.collider.isTrigger = false;
            AI.legsAnimator.enabled = true;
            AI.animator.SetBool("isVehicle", false);

            PlayerStaticStatus.currentHorse = null;
            PlayerManager.Instance.IsCanInteract = true;
        }

        public override void Update()
        {
            TransformExtentions.LookAtTarget_Humanoid(AI.transform, PlayerStaticStatus.currentHorse.RideOnTransform, 3f);
        }
    }
    public class HorseIdle : StateBase<PLAYER_STATE>
    {
        Horse horse;

        public HorseIdle() : base(PLAYER_STATE.HORSE_IDLE) {}
         
        public override void Enter()
        {
            horse = PlayerStaticStatus.currentHorse;

            AI.legsAnimator.enabled = false;
            AI.animator.SetBool("isIdle", true);

            horse.AI.ChangeState(HORSE_STATE.IDLE);
        }

        public override void Exit()
        {
            AI.animator.SetBool("isIdle", false);
        }

        public override void Update()
        {
            AI.transform.localEulerAngles = Vector3.zero;
            AI.transform.localPosition = Vector3.zero;
        }
    }
    public class HorseWalk : StateBase<PLAYER_STATE>
    {
        Horse horse;

        public HorseWalk() : base(PLAYER_STATE.HORSE_WALK) {}

        public override void Enter()
        {
            horse ??= PlayerStaticStatus.currentHorse;

            AI.legsAnimator.enabled = false;
            AI.animator.SetBool("isWalk", true);

            horse.AI.ChangeState(HORSE_STATE.WALK);
        }

        public override void Exit()
        {
            AI.animator.SetBool("isWalk", false);
        }

        public override void Update()
        {
            AI.transform.localEulerAngles = Vector3.zero;
            AI.transform.localPosition = Vector3.zero;

            if (AI.IsStopped)
            {
                AI.ChangeState(PLAYER_STATE.HORSE_IDLE);
                horse.AI.ChangeState(HORSE_STATE.IDLE);
                return;
            }
            else
            {
                Vector3 cameraForward = Camera.main.transform.TransformDirection(AI.vMoveDir).normalized;
                cameraForward.y = 0;

                TransformExtentions.LookAtTarget_Humanoid_Dir(PlayerStaticStatus.currentHorse.transform, cameraForward, 1f);
            }
        }
    }
    public class HorseRun : StateBase<PLAYER_STATE>
    {
        Horse horse;

        public HorseRun() : base(PLAYER_STATE.HORSE_RUN) {}

        public override void Enter()
        {
            horse ??= PlayerStaticStatus.currentHorse;

            AI.legsAnimator.enabled = false;
            AI.animator.SetBool("isRun", true);

            horse.AI.ChangeState(HORSE_STATE.RUN);
        }

        public override void Exit()
        {
            AI.animator.SetBool("isRun", false);
        }

        public override void Update()
        {
            AI.transform.localEulerAngles = Vector3.zero;
            AI.transform.position = PlayerStaticStatus.currentHorse.RideOnTransform.position;

            if (AI.IsStopped)
            {
                AI.ChangeState(PLAYER_STATE.HORSE_IDLE);
                horse.AI.ChangeState(HORSE_STATE.IDLE);
                return;
            }
            else
            {
                Vector3 cameraForward = Camera.main.transform.TransformDirection(AI.vMoveDir).normalized;
                cameraForward.y = 0;

                TransformExtentions.LookAtTarget_Humanoid_Dir(PlayerStaticStatus.currentHorse.transform, cameraForward, 2f);
            }
        }
    }
    public class HorseBoost : StateBase<PLAYER_STATE>
    {
        const float BOOST_DELAY = 3f;

        Horse horse;

        public HorseBoost() : base(PLAYER_STATE.HORSE_BOOST) { }

        public override void Enter()
        {
            AI.InitTimer();

            horse ??= PlayerStaticStatus.currentHorse;

            AI.legsAnimator.enabled = false;
            AI.animator.SetBool("isBoost", true);

            horse.AI.ChangeState(HORSE_STATE.BOOST);
        }

        public override void Exit()
        {
            AI.animator.SetBool("isBoost", false);
        }

        public override void Update()
        {
            AI.transform.localEulerAngles = Vector3.zero;
            AI.transform.position = PlayerStaticStatus.currentHorse.RideOnTransform.position;

            AI.UpdateTimer();
            AI.UpdateAnimationState();

            if(AI.CurTimer > BOOST_DELAY)
            {
                AI.ChangeState(PLAYER_STATE.HORSE_RUN);
                horse.AI.ChangeState(HORSE_STATE.RUN);
                return;
            }
            else
            {
                Vector3 cameraForward = Camera.main.transform.TransformDirection(AI.vMoveDir).normalized;
                cameraForward.y = 0;

                TransformExtentions.LookAtTarget_Humanoid_Dir(PlayerStaticStatus.currentHorse.transform, cameraForward, 2f);
            }
        }
    }
    #endregion


    #region Vehicle Interaction
    public class VehicleRide : StateBase<PLAYER_STATE>
    {
        KW_Buoyancy buoyancy;

        public VehicleRide() : base(PLAYER_STATE.VEHICLE_RIDE) { }

        public override void Enter()
        {
            buoyancy ??= AI.transform.GetComponent<KW_Buoyancy>();

            buoyancy.enabled = false;
            AI.legsAnimator.enabled = false;

            if (PlayerStaticStatus.CurrentVehicle.Type == 0)
            {
                AI.animator.SetBool("isSmallBoatRide", true);

                AI.transform.SetParent(PlayerStaticStatus.CurrentVehicle.RideTransform);
                AI.transform.DOLocalRotate(Vector3.zero, 0.5f).OnComplete(() =>
                {
                    AI.ChangeState(PLAYER_STATE.VEHICLE_IDLE);
                });
            }
        }

        public override void Exit()
        {
            AI.animator.SetBool("isSmallBoatRide", false);
        }

        public override void Update()
        {
            AI.transform.localPosition = new Vector3(0, 0, 0);
        }
    }
    public class VehicleUnRide : StateBase<PLAYER_STATE>
    {
        KW_Buoyancy buoyancy;

        public VehicleUnRide() : base(PLAYER_STATE.VEHICLE_UNRIDE) { }

        public override void Enter()
        {
            buoyancy ??= AI.transform.GetComponent<KW_Buoyancy>();

            buoyancy.enabled = false;
            AI.legsAnimator.enabled = false;

            if (PlayerStaticStatus.CurrentVehicle.Type == VEHICLE_TYPE.BOAT_SMALL)
            {
                AI.animator.SetBool("isSmallBoatUnRide", true);

                AI.transform.SetParent(PlayerManager.Instance.GetPlayerRootTransform());
                AI.transform.DOScale(Vector3.one, 0.5f).OnComplete(() =>
                {
                    AI.ChangeState(PLAYER_STATE.IDLE);
                });
            }
        }

        public override void Exit()
        {
            AI.animator.SetBool("isSmallBoatUnRide", false);
        }

        public override void Update()
        {
        }
    }
    public class VehicleIdle : StateBase<PLAYER_STATE>
    {
        public VehicleIdle() : base(PLAYER_STATE.VEHICLE_IDLE) { }

        public override void Enter()
        {
            AI.legsAnimator.enabled = false;
            AI.animator.SetBool("isIdle", true);
        }

        public override void Exit()
        {
            AI.animator.SetBool("isIdle", false);
        }

        public override void Update()
        {
            AI.transform.localEulerAngles = Vector3.zero;
            AI.transform.localPosition = Vector3.zero;

            if (Input.GetKeyDown(KeyCode.R))
            {
                AI.ChangeState(PLAYER_STATE.VEHICLE_UNRIDE);
                return;
            }
            else if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
            {
                AI.ChangeState(PLAYER_STATE.VEHICLE_MOVE);
                return;
            }
        }
    }
    public class VehicleMove : StateBase<PLAYER_STATE>
    {
        const float MOVE_SPEED = 3f;
        const float ROTATE_SPEED = 5f;

        public VehicleMove() : base(PLAYER_STATE.VEHICLE_MOVE) { }

        public override void Enter()
        {
            AI.legsAnimator.enabled = false;
            AI.animator.SetBool("isRun", true);
        }

        public override void Exit()
        {
            AI.animator.SetBool("isRun", false);
        }

        public override void Update()
        {
            AI.transform.localEulerAngles = Vector3.zero;
            AI.transform.localPosition = Vector3.zero;

            if (Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0)
            {
                AI.ChangeState(PLAYER_STATE.VEHICLE_IDLE);
                return;
            }
            else
            {
                float horizontalInput = Input.GetAxis("Horizontal");
                float verticalInput = Input.GetAxis("Vertical");

                Vector3 moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;
                Vector3 cameraForward = Camera.main.transform.TransformDirection(moveDirection).normalized;
                cameraForward.y = 0;

                Quaternion rotation = Quaternion.LookRotation(cameraForward) * Quaternion.Euler(PlayerStaticStatus.CurrentVehicle.ForwardAngleOffset);
                PlayerStaticStatus.CurrentVehicle.transform.rotation = Quaternion.Slerp(
                    PlayerStaticStatus.CurrentVehicle.transform.rotation, rotation, ROTATE_SPEED * Time.deltaTime);
                PlayerStaticStatus.CurrentVehicle.transform.Translate(
                    PlayerStaticStatus.CurrentVehicle.Forward * MOVE_SPEED * Time.deltaTime);
            }
        }
    }
    #endregion


    #region Attack
    public class Atk_Nomarl : StateBase<PLAYER_STATE>
    {
        WeaponAttackData tAttackData;
        Weapon weapon;

        Player owner;

        string animationName = "";
        int atkIndex = 0;
        int hitboxIndex = 0;
        bool isCancelInput = false;

        public Atk_Nomarl() : base(PLAYER_STATE.ATTACK_NOMARL) {}

        public override void Enter()
        {
            isCancelInput = false;
            PlayerStaticStatus.IsAttackCancelInput = false;

            owner ??= AI.OwnerBase as Player;
            weapon = AI.weaponController.CurrentEquipWeapon_Right;
            tAttackData = owner.CurrentAttack;
            atkIndex = weapon.WeaponData.AtkNormalDatas.IndexOf(owner.CurrentAttack);
            
            hitboxIndex = 0;

            animationName = "isAtk_N_" + (atkIndex + 1).ToString();
            AI.animator.SetBool(animationName, true);
            AI.animator.SetBool("isAttackNormal", true);
            AI.animator.SetTrigger("StateOn");
    
            Vector3 cameraForward = Camera.main.transform.forward.normalized;
            cameraForward.y = 0;

            AI.transform.DORotate(Quaternion.LookRotation(cameraForward).eulerAngles, 0.3f);
        }

        public override void Exit()
        {
            PlayerStaticStatus.IsAttackCancelInput = false;
            
            AI.transform.DOKill();
            AI.animator.SetBool(animationName, false);
            AI.animator.SetBool("isAttackNormal", false);

            // 마지막 공격일 경우 현재 공격정보 초기화
            if(tAttackData == weapon.WeaponData.AtkNormalDatas.Last())
            {
                owner.CurrentAttack = null;
            }
        }

        public override void Update()
        {
            AI.UpdateAnimationState();

            // 다음 공격이 실행되지 않았을 경우 해당 공격을 정상 종료
            if (AI.CurrentAnimationState.IsTag(animationName) && AI.CurrentAnimationState.normalizedTime >= 0.9)
            {
                owner.CurrentAttack = null;
                AI.ChangeState(PLAYER_STATE.IDLE);
                return;
            }

            // 공격 취소 가능 프레임 확인
            if(AI.CurrentAnimationState.IsTag(animationName)
            && AI.CurrentAnimationState.normalizedTime >= tAttackData.CancelStartDelay
            && !PlayerStaticStatus.IsAttackCancelInput
            && tAttackData.CancelStartDelay != 0f)
            {
                isCancelInput = true;
                PlayerStaticStatus.IsAttackCancelInput = true;
            }

            // 공격 히트박스 생성 프레임 확인
            if (AI.CurrentAnimationState.IsTag(animationName)
             && tAttackData.tHitBoxDatas.Count > hitboxIndex
             && AI.CurrentAnimationState.normalizedTime >= tAttackData.tHitBoxDatas[hitboxIndex].StartDelay)
            {
                if(tAttackData.IsProjectile)
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
                               , parryDelay: 0f
                               , exitDelay: tAttackData.tHitBoxDatas[hitboxIndex].GetExitDelay(AI.CurrentAnimationState.speed));
                }

                hitboxIndex++;
            }

            // 다음 공격 존재 확인
            if (weapon.WeaponData.AtkNormalDatas.Count <= (atkIndex + 1)) return;

            // 다음 공격 프레임 확인
            if (AI.CurrentAnimationState.IsTag(animationName)
             && (AI.CurrentAnimationState.normalizedTime >= tAttackData.NextInputStartDelay 
              && AI.CurrentAnimationState.normalizedTime <= tAttackData.NextInputExitDelay)
             && Input.GetKey(KeyCode.Mouse0)
             && (isCancelInput == PlayerStaticStatus.IsAttackCancelInput)) 
            {
                owner.CurrentAttack = weapon.WeaponData.AtkNormalDatas[atkIndex + 1];
                AI.ChangeState(PLAYER_STATE.ATTACK_NOMARL);
                return;
            }
        }
    }
    public class Atk_Strong : StateBase<PLAYER_STATE>
    {
        WeaponAttackData tAttackData;
        Weapon weapon;

        Player owner;

        string animationName = "";
        int attackIndex = 0;
        int hitboxIndex = 0;

        public Atk_Strong() : base(PLAYER_STATE.ATTACK_STRONG) { }

        public override void Enter()
        {
            owner ??= AI.OwnerBase as Player;
            weapon = AI.weaponController.CurrentEquipWeapon_Right;
            tAttackData = owner.CurrentAttack;
            attackIndex = weapon.WeaponData.AtkStrongDatas.IndexOf(tAttackData);
            hitboxIndex = 0;

            animationName = "isAtk_S_" + (attackIndex + 1).ToString();
            AI.animator.SetBool(animationName, true);
            AI.animator.SetBool("isAttackStrong", true);
            AI.animator.SetTrigger("StateOn");

            Vector3 cameraForward = Camera.main.transform.forward.normalized;
            cameraForward.y = 0;

            AI.transform.DORotate(Quaternion.LookRotation(cameraForward).eulerAngles, 0.3f);
        }

        public override void Exit()
        {
            AI.rb.velocity = Vector3.zero;
        
            AI.transform.DOKill();
            AI.animator.SetBool(animationName, false);
            AI.animator.SetBool("isAttackStrong", false);
            AI.animator.SetTrigger("StateOn");

            if(tAttackData == weapon.WeaponData.AtkStrongDatas.Last())
            {
                owner.CurrentAttack = null;
            }
        }

        public override void Update()
        {
            var currentAnimation = AI.animator.GetCurrentAnimatorStateInfo(0);

            if (currentAnimation.IsTag(animationName) && currentAnimation.normalizedTime >= 0.9)
            {
                owner.CurrentAttack = null;
                AI.ChangeState(PLAYER_STATE.IDLE);
                return;
            }

            if (currentAnimation.IsTag(animationName) && tAttackData.tHitBoxDatas.Count > hitboxIndex
               && currentAnimation.normalizedTime >= tAttackData.tHitBoxDatas[hitboxIndex].StartDelay)
            {
                var hitBox = PoolManager.Instance.GetObject<HitBox_Player>().GetComponent<HitBox_Player>();
                hitBox.transform.position = AI.transform.position;
                hitBox.transform.rotation = AI.transform.rotation;
                hitBox.Enter(tAttackData.tHitBoxDatas[hitboxIndex]
                           , owner
                           , parryDelay: 0f
                           , exitDelay: tAttackData.tHitBoxDatas[hitboxIndex].GetExitDelay(currentAnimation.speed));

                hitboxIndex++;
            }

            if (weapon.WeaponData.AtkStrongDatas.Count <= attackIndex + 1) return;

            if (currentAnimation.IsTag(animationName)
                && (currentAnimation.normalizedTime >= tAttackData.NextInputStartDelay && currentAnimation.normalizedTime <= tAttackData.NextInputExitDelay)
                && Input.GetKey(KeyCode.Mouse0) && Input.GetKey(KeyCode.LeftShift))
            {
                owner.CurrentAttack = weapon.WeaponData.AtkStrongDatas[attackIndex + 1];
                AI.ChangeState(PLAYER_STATE.ATTACK_STRONG);
                return;
            }

            if (!IsGrounded())
            {
                AI.rb.velocity = Vector3.down * 1.5f;
            }
            else
            {
                AI.rb.velocity = Vector3.zero;
            }
        }

        private bool IsGrounded()
        {
            int layerMask = ~(1 << LayerMask.NameToLayer("Water"));
            layerMask |= 1 << LayerMask.NameToLayer("Player");

            const float GROUND_RAY_LENGHT = 0.5f + 0.3f;
            const float GROUND_RAY_SIZE = 0.4f;

            return Physics.BoxCast(AI.transform.position + Vector3.up * 0.5f, Vector3.one * GROUND_RAY_SIZE, Vector3.down, Quaternion.identity, GROUND_RAY_LENGHT, layerMask);
        }
    }
    public class Atk_Kill : StateBase<PLAYER_STATE>
    {
        Weapon weapon;

        int tickDamageIndex = 0;
        tAttackKillData AtttackData;

        Npc target;

        public Atk_Kill() : base(PLAYER_STATE.ATTACK_KILL) { }

        public override void Enter()
        {
            AI.InitTimer();
            AI.transform.DOKill();

            tickDamageIndex = 0;

            weapon = AI.weaponController.CurrentEquipWeapon_Right;
            target = PlayerStaticStatus.CurrentKillData.Target;
            AtttackData = PlayerStaticStatus.CurrentKillData.AttackData;

            Vector3 vDir = (AI.transform.position - target.transform.position).normalized;
            vDir.y = 0;

            AI.rb.useGravity = false;
            AI.collider.enabled = false;
            AI.transform.LookAt(target.transform.position);
            AI.transform.DOMove(target.transform.position + vDir * AtttackData.KillDistance, 0.3f);

            AI.animator.SetBool("isAtk_K_1", true);
            target.AI.animator.SetFloat("idxHitWeaponIdx", (float)weapon.WeaponData.WeaponIDX);

            Debug.Log("[Player] State Attack Kill");
        }

        public override void Exit()
        {
            target.AI.healthController.TakeDamage(new vDamage(999999));

            AI.rb.useGravity = true;
            AI.collider.enabled = true;
            AI.transform.DOKill();
            AI.animator.SetBool("isAtk_K_1", false);
        }

        public override void Update()
        {
            AI.UpdateTimer();

            if (AI.animator.GetCurrentAnimatorStateInfo(0).IsTag("ATK_K_1")
             && AI.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= AtttackData.ExitDelay)
            {
                AI.ChangeState(PLAYER_STATE.IDLE);
                return;
            }

            if (AI.animator.GetCurrentAnimatorStateInfo(0).IsTag("ATK_K_1")
                && AtttackData.TickDamageDelay.Count > tickDamageIndex
                && AI.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= AtttackData.TickDamageDelay[tickDamageIndex])
            {
                tickDamageIndex++;
                PlayerStaticStatus.CurrentKillData.Target.OnHitOnly();
            }
        }
    }
    public class Atk_Back_Kill : StateBase<PLAYER_STATE>
    {
        Weapon weapon;

        int tickDamageIndex = 0;
        tAttackKillData AtttackData;

        Npc target;

        public Atk_Back_Kill() : base(PLAYER_STATE.ATTACK_BACK_KILL) { }

        public override void Enter()
        {
            AI.transform.DOKill();

            tickDamageIndex = 0;
            target = PlayerStaticStatus.CurrentKillData.Target;
            AtttackData = PlayerStaticStatus.CurrentKillData.AttackData;
            weapon = AI.weaponController.CurrentEquipWeapon_Right;

            Vector3 vDir = (AI.transform.position - target.transform.position).normalized;
            vDir.y = 0;

            AI.rb.isKinematic = true;
            AI.collider.enabled = false;
            AI.transform.LookAt(target.transform.position);
            AI.transform.DOMove(target.transform.position + vDir * AtttackData.KillDistance, 0.3f);

            AI.animator.SetBool("isAtk_B_1", true);
            target.AI.animator.SetFloat("idxHitWeaponIdx", (float)weapon.WeaponData.WeaponIDX);
        }

        public override void Exit()
        {
            target.AI.healthController.TakeDamage(new vDamage(999999));

            AI.rb.isKinematic = false;
            AI.collider.enabled = true;
            AI.transform.DOKill();
            AI.animator.SetBool("isAtk_B_1", false);
        }

        public override void Update()
        {
            if (AI.animator.GetCurrentAnimatorStateInfo(0).IsTag("ATK_B_1")
                && AI.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= AtttackData.ExitDelay)
            {
                AI.ChangeState(PLAYER_STATE.IDLE);
                return;
            }

            if (AI.animator.GetCurrentAnimatorStateInfo(0).IsTag("ATK_B_1")
                && AtttackData.TickDamageDelay.Count > tickDamageIndex
                && AI.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= AtttackData.TickDamageDelay[tickDamageIndex])
            {
                tickDamageIndex++;
                PlayerStaticStatus.CurrentKillData.Target.OnHitOnly();
            }
        }
    }
    public class Atk_D_1 : StateBase<PLAYER_STATE>
    {
        Weapon weapon;

        float curDelay = 0f;

        public Atk_D_1() : base(PLAYER_STATE.ATTACK_D_1) { }

        public override void Enter()
        {
            weapon = AI.weaponController.CurrentEquipWeapon_Right;

            AI.animator.SetBool("isAtk_D_1", true);

            Vector3 cameraForward = Camera.main.transform.forward.normalized;
            cameraForward.y = 0;

            AI.transform.DORotate(Quaternion.LookRotation(cameraForward).eulerAngles, 0.3f);

            curDelay = 0f;
        }

        public override void Exit()
        {
            AI.transform.DOKill();
            AI.animator.SetBool("isAtk_D_1", false);
        }

        public override void Update()
        {
            curDelay += Time.deltaTime;

            if (curDelay > weapon.WeaponData.AtkNormalDatas[3].ExitDelay)
            {
                AI.ChangeState(PLAYER_STATE.IDLE);
                return;
            }

            if (weapon.WeaponData.AtkNormalDatas.Count <= 4) return;
        }
    }
    #endregion


    #region Guard, Parry
    public class Guard : StateBase<PLAYER_STATE>
    {
        const float MOVE_SPEED = 1.5f;
        Weapon wepone;

        public Guard() : base(PLAYER_STATE.GUARD) {}

        public override void Enter()
        {
            wepone = AI.weaponController.CurrentEquipWeapon_Right;

            AI.animator.SetBool("isDef", true);

            AI.transform.DOScale(Vector3.one, 0.2f).OnComplete(() =>
            {
                wepone?.OnGuard();
            });
        }

        public override void Exit()
        {
            AI.animator.SetBool("isWalk", false);
            if (AI.CurrentState.IsNotEquals(PLAYER_STATE.GUARD_HIT))
            {
                AI.animator.SetBool("isDef", false);
                AI.transform.DOScale(Vector3.one, 0.35f).OnComplete(() =>
                {
                    wepone?.OnEquip();
                });
            }
        }

        public override void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                AI.ChangeState(PLAYER_STATE.IDLE);
                return;
            }

            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            if (horizontalInput != 0 || verticalInput != 0)
            {
                AI.animator.SetBool("isWalk", true);
                AI.animator.SetFloat("xDir", horizontalInput);
                AI.animator.SetFloat("yDir", verticalInput);

                Vector3 cameraForward = Camera.main.transform.forward.normalized;
                cameraForward.y = 0;

                AI.transform.DOKill();
                AI.transform.DORotate(Quaternion.LookRotation(cameraForward).eulerAngles, 0.3f);
                if(wepone?.WeaponData.WeaponIDX == WEPONE_IDX.GREAT_SWORD)
                {
                    AI.transform.Translate(new Vector3(horizontalInput, 0, verticalInput).normalized * MOVE_SPEED * Time.deltaTime);
                }
            }
            else
            {
                AI.animator.SetBool("isWalk", false);
            }
        }
    }
    public class Guard_Hit : StateBase<PLAYER_STATE>
    {
        Weapon wepone;
        float exitHitDelay = 0.8f;

        public Guard_Hit() : base(PLAYER_STATE.GUARD_HIT) {}

        public override void Enter()
        {
            AI.transform.DOKill();
            AI.InitTimer();

            wepone = AI.weaponController.CurrentEquipWeapon_Right;

            AI.animator.SetBool("isDef", true);
            AI.animator.SetBool("isHit", true);
        }

        public override void Exit()
        {
            AI.animator.SetBool("isDef", true);
            AI.animator.SetBool("isHit", false);
        }

        public override void Update()
        {
            AI.UpdateTimer();

            if(AI.CurTimer > exitHitDelay)
            {
                AI.ChangeState(PLAYER_STATE.GUARD);
                return;
            }
        }
    }
    public class Parry : StateBase<PLAYER_STATE>
    {
        const string ANIMATION_TAG = "Parry";
        const float EXIT_DELAY = 0.95f;

        Weapon weapon;

        public Parry() : base(PLAYER_STATE.PARRY) {}

        public override void Enter()
        {
            AI.InitTimer();
            AI.animator.SetBool("isParry", true);

            weapon = AI.weaponController.CurrentEquipWeapon_Right;

            // Init
            PlayerStaticStatus.IsParring = false;
        }

        public override void Exit()
        {
            AI.animator.SetBool("isParry", false);

            // Exit
            PlayerStaticStatus.IsParring = false;
        }

        public override void Update()
        {
            var currentAnimationState = AI.animator.GetCurrentAnimatorStateInfo(0);
            AI.UpdateTimer();
            
            if(currentAnimationState.IsTag(ANIMATION_TAG)
            && currentAnimationState.normalizedTime >= EXIT_DELAY)
            {
                AI.ChangeState(PLAYER_STATE.IDLE);
                return;
            }

            if(currentAnimationState.IsTag(ANIMATION_TAG) 
            && currentAnimationState.normalizedTime >= weapon.WeaponData.StartParryDelay 
            && currentAnimationState.normalizedTime < weapon.WeaponData.ExitParryDelay
            && !PlayerStaticStatus.IsParring)
            {
                PlayerStaticStatus.IsParring = true;
                Debug.Log("[Player] StartParryFrame");
            }
            else if(currentAnimationState.IsTag(ANIMATION_TAG)
                 && currentAnimationState.normalizedTime >= weapon.WeaponData.ExitParryDelay
                 && PlayerStaticStatus.IsParring)
            {
                PlayerStaticStatus.IsParring = false;
                Debug.Log("[Player] ExitParryFrame");
            }
        }
    }
    #endregion


    public class Gather : StateBase<PLAYER_STATE>
    {
        Player owner;

        public Gather() : base(PLAYER_STATE.GATHER) {}

        public override void Enter()
        {
            owner ??= AI.OwnerBase as Player;

            AI.animator.SetBool("isInteract", true);
            AI.animator.SetFloat("idxInteract", PlayerStaticStatus.currentGatherObject.gatherAnimationID);
            AI.animator.SetTrigger("StateOn");

            TransformExtentions.LookAtTarget_Humanoid_FixedTime(AI.transform, PlayerStaticStatus.currentGatherObject.transform, 0.5f);
        }

        public override void Exit()
        {
            AI.animator.SetBool("isInteract", false);
        }

        public override void Update()
        {
        }
    }
    public class Fishing : StateBase<PLAYER_STATE>
    {
        Transform modelTranform;
        Transform rootTransform;

        float curDelay = 0.0f;
        float delay = 5f;

        public Fishing() : base(PLAYER_STATE.FISHING) {}

        public override void Enter()
        {
            rootTransform = PlayerManager.Instance.GetPlayerRootTransform();
            modelTranform = PlayerManager.Instance.GetPlayerModelTransform();

            curDelay = 0.0f;
            AI.animator.SetBool("isFishing", true);
            //animator.SetFloat("idxGather", PlayerStaticStatus.currentNatureObject.GatherAnim);
        }

        public override void Exit()
        {
            AI.animator.SetBool("isFishing", false);
        }

        public override void Update()
        {
            curDelay += Time.deltaTime;
            if (curDelay > delay)
            {
                AI.ChangeState(PLAYER_STATE.IDLE);
                return;
            }
        }
    }
}

