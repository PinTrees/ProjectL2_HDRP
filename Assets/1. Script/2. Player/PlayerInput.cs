using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.Rendering.HighDefinition;
using System.Linq;
using NPC.Animalls.State.HorseBase;


public class PlayerInput : MonoBehaviour
{
    protected Camera _cameraMain;
    protected bool withoutMainCamera;

    [Header("Camera Settings")]
    public bool lockCameraInput;
    public bool invertCameraInputVertical;
    public bool invertCameraInputHorizontal;

    [Range(0f, 1f)]
    [SerializeField]
    private float moveAnimationSmooth = 0.75f;

    private Player owner;

    public virtual Camera cameraMain
    {
        get
        {
            if (!_cameraMain && !withoutMainCamera)
            {
                if (!Camera.main)
                {
                    Debug.Log("Missing a Camera with the tag MainCamera, please add one.");
                    withoutMainCamera = true;
                }
                else
                {
                    _cameraMain = Camera.main;
                }
            }
            return _cameraMain;
        }
        set
        {
            _cameraMain = value;
        }
    }

    #region Init Func
    void Start()
    {
        owner = GetComponent<Player>();
    }
    #endregion


    #region Update Func
    void Update()
    {
        owner.AI.vMoveDir = (new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"))).normalized;
        owner.AI.vMoveSmoothDir = Vector3.Lerp(owner.AI.vMoveSmoothDir, owner.AI.vMoveDir, moveAnimationSmooth);
        owner.AI.animator.SetFloat("xDir", owner.AI.vMoveSmoothDir.x);
        owner.AI.animator.SetFloat("yDir", owner.AI.vMoveSmoothDir.z);

        if (EquipInput()) return;
        if (AttackInput()) return;
        if (MoveInput()) return;
        if (VehicleInput()) return;
        if (ChangeWeaponInput()) return;

        InteractInput();
        DebugInput();
    }
    protected virtual void LateUpdate()
    {
        LockOnInput();
    }
    protected virtual void FixedUpdate()
    {
        Raycast_Interaction();
    }
    #endregion




    #region Input Func
    public virtual void LockOnInput()
    {
        /*if (lockOnSystem.IsLockOnTarget)
        {
            if (Input.GetKeyDown(KeyCode.Mouse2))
            {
                mvTargetManager.Instance.ChangeLockOnTarget();
                tpCamera.SetLockTarget(mvTargetManager.Instance.CurrentTarget.tr);
                return;
            }
        }

        if (!lockOnSystem.HasTarget())
        {
            if (lockOnSystem.IsLockOnTarget)
                lockOnSystem.IsLostTarget = true;

            LostTarget();

            return;
        }
        else
        {
            if(lockOnSystem.IsLostTarget)
            {
                lockOnSystem.IsLostTarget = false;
                LockOnTarget();
                return;
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                if (!lockOnSystem.IsLockOnTarget)
                {
                    LockOnTarget();
                }
                else
                {
                    LostTarget();
                }
            }
        }*/
    }
    public virtual bool EquipInput()
    {
        /// 무기 양잡 입력 확인
        if(Input.GetKey(KeyCode.E))
        {
            /// 왼잡
            if(Input.GetKeyDown(KeyCode.Mouse0))
            {
                if(owner.AI.weaponController.CurrentEquipWeapon_Left)
                {
                    PlayerStaticStatus.ChangeWeaponLeft = true;
                    owner.AI.ChangeState(PLAYER_STATE.OFF_EQUIP);
                }
                else if(owner.AI.weaponController.SelectLeftWeapon)
                {
                    if (owner.AI.weaponController.SelectLeftWeapon.WeaponData.HasBothAttack())
                    {
                        if(owner.AI.weaponController.CurrentEquipWeapon_Right)
                        {
                            PlayerStaticStatus.ChangeWeaponRight = true;
                            owner.AI.ChangeState(PLAYER_STATE.OFF_EQUIP).OnExitEvent(() =>
                            {
                                PlayerStaticStatus.ChangeWeaponLeft = true;
                                owner.AI.ChangeState(PLAYER_STATE.ON_EQUIP);
                            });
                        }
                        else
                        {
                            PlayerStaticStatus.ChangeWeaponLeft = true;
                            owner.AI.ChangeState(PLAYER_STATE.ON_EQUIP);
                        }
                    }
                }
                return true;
            }
            /// 오른잡
            else if (Input.GetKeyDown(KeyCode.Mouse1))
            {

                return true;
            }
        }

        /// 무기 변경 키 입력 확인
        if(!Input.GetKey(KeyCode.Tab) || !Input.GetKeyDown(KeyCode.Tab))
        {
            return false;
        }
        /// 무기 착용 및 해제가능 상태 확인
        if(!owner.AI.ContainsState(new List<PLAYER_STATE> { PLAYER_STATE.WALK, PLAYER_STATE.IDLE, PLAYER_STATE.RUN }))
        {
            return false;
        }

        // 착용 해제를 확인
        if (owner.AI.weaponController.CurrentEquipWeapon_Left || owner.AI.weaponController.CurrentEquipWeapon_Right)
        {
            if (owner.AI.weaponController.CurrentEquipWeapon_Left && owner.AI.weaponController.CurrentEquipWeapon_Right)
            {
                PlayerStaticStatus.ChangeWeaponLeft = true;
                PlayerStaticStatus.ChangeWeaponRight = true;
                owner.AI.ChangeState(PLAYER_STATE.OFF_EQUIP);
            }
            else if (owner.AI.weaponController.CurrentEquipWeapon_Left)
            {
                PlayerStaticStatus.ChangeWeaponLeft = true;
                owner.AI.ChangeState(PLAYER_STATE.OFF_EQUIP);
            }
            else if (owner.AI.weaponController.CurrentEquipWeapon_Right)
            {
                PlayerStaticStatus.ChangeWeaponRight = true;
                owner.AI.ChangeState(PLAYER_STATE.OFF_EQUIP);
            }
            return true;
        }

        // 착용을 확인
        if (owner.AI.weaponController.SelectRightWeapon)
        {
            PlayerStaticStatus.ChangeWeaponRight = true;
            owner.AI.ChangeState(PLAYER_STATE.ON_EQUIP);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Player Attack Input Functions
    /// </summary>
    public virtual bool AttackInput()
    {
        /// 공격 미 해당 조건을 확인
        if (Input.GetKey(KeyCode.Mouse0) && EventSystem.current.IsPointerOverGameObject())
        {
            return false;
        }
        /// 공격키 미 입력을 확인
        if (!Input.GetKey(KeyCode.Mouse0))
        {
            return false;
        }

        /// None Equip Weapone
        if (!owner.AI.weaponController.IsEquipWeapon())
        {
            if(!owner.AI.animator.GetBool("isBattle"))
            {
                if (owner.AI.ContainsState(new List<PLAYER_STATE> {
                    PLAYER_STATE.IDLE, PLAYER_STATE.RUN, PLAYER_STATE.WALK
                }) && Input.GetKey(KeyCode.Mouse0))
                {
                    if (owner.AI.weaponController.SelectRightWeapon == null) return false;

                    owner.AI.ChangeState(PLAYER_STATE.ON_EQUIP);
                    return true;
                }
            }
        }

        /// Idle - Attack State
        if(owner.AI.animator.GetBool("isBattle"))
        {
            /// Strong Attack
            if (owner.AI.weaponController.CurrentEquipWeapon_Right.WeaponData.AtkStrongDatas.Count > 0)
            {
                if (owner.AI.ContainsState(new List<PLAYER_STATE> { PLAYER_STATE.IDLE, PLAYER_STATE.RUN, PLAYER_STATE.WALK })
                    && Input.GetKey(KeyCode.LeftShift)
                    && Input.GetKey(KeyCode.Mouse0))
                {
                    if (EventSystem.current.IsPointerOverGameObject())
                    {
                        return false;
                    }

                    owner.CurrentAttack = owner.AI.weaponController.CurrentEquipWeapon_Right.WeaponData.AtkStrongDatas.First();
                    owner.AI.ChangeState(PLAYER_STATE.ATTACK_STRONG);
                    return true;
                }
            }

            /// Normal Attack
            if (owner.AI.ContainsState(new List<PLAYER_STATE> {  PLAYER_STATE.IDLE, PLAYER_STATE.RUN, PLAYER_STATE.WALK }) 
             && Input.GetKey(KeyCode.Mouse0))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return false;
                }

                owner.CurrentAttack = owner.AI.weaponController.CurrentEquipWeapon_Right.WeaponData.AtkNormalDatas.First();
                owner.AI.ChangeState(PLAYER_STATE.ATTACK_NOMARL);
                return true;
            }

            /// Parring
            if(owner.AI.ContainsState(new List<PLAYER_STATE> { PLAYER_STATE.IDLE, PLAYER_STATE.WALK, PLAYER_STATE.RUN })
            && Input.GetKey(KeyCode.Mouse1))
            {
                owner.AI.ChangeState(PLAYER_STATE.PARRY);
                return true;
            }
        }

        return false;
    }

    public virtual bool ChangeWeaponInput()
    {
        /// 무기 변경 키 입력 확인
        if(!Input.GetKeyDown(KeyCode.RightArrow) && !Input.GetKeyDown(KeyCode.LeftArrow))
        {
            return false;
        }
        /// 무기 변경 가능 상태 확인
        if(!owner.AI.ContainsState(new List<PLAYER_STATE> { PLAYER_STATE.IDLE, PLAYER_STATE.WALK, PLAYER_STATE.RUN }))
        {
            return false;
        }

        /// 오른손 무기 변경 확인
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            /// 오른손 무기 변경
            PlayerStaticStatus.ChangeWeaponRight = true;
            owner.AI.weaponController.ChangeSelectWeapon_Right();
            owner.AI.ChangeState(PLAYER_STATE.CHANGE_EQUIP);
            return true;
        }
        /// 왼손 무기 변경 확인
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            /// 왼손 무기 변경
            PlayerStaticStatus.ChangeWeaponLeft = true;
            owner.AI.weaponController.ChangeSelectWeapon_Left();
            owner.AI.ChangeState(PLAYER_STATE.CHANGE_EQUIP);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 이 함수는 플레이어의 이동 입력 처리함수 입니다.
    /// </summary>
    public virtual bool MoveInput()
    {
        if(owner.AI.ContainsState(new List<PLAYER_STATE> { PLAYER_STATE.HORSE_IDLE, PLAYER_STATE.HORSE_WALK, 
            PLAYER_STATE.HORSE_RIDE, PLAYER_STATE.HORSE_UNRIDE, PLAYER_STATE.HORSE_RUN}))
        {
            return false;
        }

        if(Input.GetKey(KeyCode.Space))
        {
            if(owner.AI.ContainsState(new List<PLAYER_STATE> {  PLAYER_STATE.IDLE, PLAYER_STATE.RUN, PLAYER_STATE.WALK }))
            {
                owner.AI.ChangeState(PLAYER_STATE.JUMP);
                return true;
            }
        }

        /// Attack (Normal, Strong) -> Walk Input
        if (owner.AI.ContainsState(new List<PLAYER_STATE> { PLAYER_STATE.ATTACK_NOMARL, PLAYER_STATE.ATTACK_STRONG })
         && !owner.AI.IsStopped
         && PlayerStaticStatus.IsAttackCancelInput
         && !Input.GetKey(KeyCode.Mouse0))
        {
            owner.CurrentAttack = null;
            PlayerStaticStatus.IsAttackCancelInput = false;
            owner.AI.ChangeState(PLAYER_STATE.WALK);
            return true;
        }

        /// Dodge Input - Requied DirectionVector
        if (owner.AI.ContainsState(new List<PLAYER_STATE> { PLAYER_STATE.IDLE, PLAYER_STATE.WALK, PLAYER_STATE.RUN })
         && !owner.AI.IsStopped
         && Input.GetKey(KeyCode.C))
        {
            owner.AI.ChangeState(PLAYER_STATE.DODGE);
            return true;
        }

        /// Base Run Input
        if (owner.AI.ContainsState(new List<PLAYER_STATE> { PLAYER_STATE.IDLE, PLAYER_STATE.WALK })
         && !owner.AI.IsStopped
         && Input.GetKey(KeyCode.LeftShift))
        {
            owner.AI.ChangeState(PLAYER_STATE.RUN);
            return true;
        }
        /// Base Walk Input
        if (owner.AI.ContainsState(new List<PLAYER_STATE> { PLAYER_STATE.IDLE })
         && !owner.AI.IsStopped)
        {
            owner.AI.ChangeState(PLAYER_STATE.WALK);
            return true;
        }

        return false;
    }

    ///
    public bool VehicleInput()
    {
        /// 말 이동 및 입력 불가 상태 확인   탑승, 탑승해제 상태
        if (owner.AI.ContainsState(new List<PLAYER_STATE> { PLAYER_STATE.HORSE_RIDE, PLAYER_STATE.HORSE_UNRIDE }))
        {
            return false;
        }

        /// 말 이동 입력 확인    대기, 걷기 -> 달리기
        if (owner.AI.ContainsState(new List<PLAYER_STATE> { PLAYER_STATE.HORSE_IDLE, PLAYER_STATE.HORSE_WALK })
         && !owner.AI.IsStopped
         && Input.GetKey(KeyCode.LeftShift))
        {
            owner.AI.ChangeState(PLAYER_STATE.HORSE_RUN);
            PlayerStaticStatus.currentHorse.AI.ChangeState(HORSE_STATE.RUN);
            return true;
        }

        /// 말 이동 입력 확인   대기 -> 이동
        if (owner.AI.ContainsState(new List<PLAYER_STATE> { PLAYER_STATE.HORSE_IDLE, })
         && !owner.AI.IsStopped)
        {
            owner.AI.ChangeState(PLAYER_STATE.HORSE_WALK);
            PlayerStaticStatus.currentHorse.AI.ChangeState(HORSE_STATE.WALK);
            return true;
        }

        /// 말 탑승 해제 입력 확인
        if (owner.AI.ContainsState(new List<PLAYER_STATE> { PLAYER_STATE.HORSE_IDLE, PLAYER_STATE.HORSE_WALK })
         && Input.GetKey(KeyCode.R))
        {
            owner.AI.ChangeState(PLAYER_STATE.HORSE_UNRIDE);
            PlayerStaticStatus.currentHorse.OffRide();
            return true;
        }


        /// 말 순간가속 입력 확인
        if(owner.AI.ContainsState(new List<PLAYER_STATE> { PLAYER_STATE.HORSE_RUN })
        && Input.GetKey(KeyCode.F))
        {
            owner.AI.ChangeState(PLAYER_STATE.HORSE_BOOST);
            PlayerStaticStatus.currentHorse.AI.ChangeState(HORSE_STATE.BOOST);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 이 함수는 상호작용 입력 처리 함수입니다.
    /// </summary>
    /// <returns></returns>
    public virtual bool InteractInput()
    {
        if(InteractManager.Instance.CurrentInteract)
        {
            var interact = InteractManager.Instance.CurrentInteract.FindInteraction();

            if (interact != null
            && !InteractManager.Instance.CurrentInteract.IsInteractioning
            && !InteractManager.Instance.CurrentInteract.IsDisable)
            {
                InteractManager.Instance.CurrentInteract.EnterInteract(owner, interact);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Debug Input
    /// </summary>
    public void DebugInput()
    {
        if(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.L))
        {
            PlayerStaticStatus.IsParring = !PlayerStaticStatus.IsParring;
        }
    }
    #endregion


    public void LockOnTarget()
    {
    }

    // 이 함수는 인터렉션 레이캐스트 처리 함수 입니다. 
    // 레이캐스트 처리 프레임 시점에 호출시켜야 합니다.
    public void Raycast_Interaction()
    {
        LayerMask triggerLayer = LayerMask.GetMask("Triggers");
        var hits = Physics.SphereCastAll(transform.position, 0.5f, Vector3.up, 0.01f, triggerLayer);

        if (hits.Count() > 0)
        {
            var best = hits[0];

            float distance = 999;
            Transform current_hit = null;

            foreach (var hit in hits)
            {
                var current_distance = Vector3.Distance(owner.AI.transform.position, hit.transform.position);
                if (current_distance < distance)
                {
                    current_hit = hit.transform;
                }
            }

            var current_interact = current_hit.GetComponentInParent<CanInteract>();
            if (!current_interact.IsDisable)
            {
                current_interact.OnSelect();
            }
            else
            {
                InteractManager.Instance.CurrentInteract = null;
            }
        }
        else
        {
            InteractManager.Instance.CurrentInteract = null;
        }
    }


    #region EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
    }
    #endregion
}
