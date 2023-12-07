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
        /// ���� ���� �Է� Ȯ��
        if(Input.GetKey(KeyCode.E))
        {
            /// ����
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
            /// ������
            else if (Input.GetKeyDown(KeyCode.Mouse1))
            {

                return true;
            }
        }

        /// ���� ���� Ű �Է� Ȯ��
        if(!Input.GetKey(KeyCode.Tab) || !Input.GetKeyDown(KeyCode.Tab))
        {
            return false;
        }
        /// ���� ���� �� �������� ���� Ȯ��
        if(!owner.AI.ContainsState(new List<PLAYER_STATE> { PLAYER_STATE.WALK, PLAYER_STATE.IDLE, PLAYER_STATE.RUN }))
        {
            return false;
        }

        // ���� ������ Ȯ��
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

        // ������ Ȯ��
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
        /// ���� �� �ش� ������ Ȯ��
        if (Input.GetKey(KeyCode.Mouse0) && EventSystem.current.IsPointerOverGameObject())
        {
            return false;
        }
        /// ����Ű �� �Է��� Ȯ��
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
        /// ���� ���� Ű �Է� Ȯ��
        if(!Input.GetKeyDown(KeyCode.RightArrow) && !Input.GetKeyDown(KeyCode.LeftArrow))
        {
            return false;
        }
        /// ���� ���� ���� ���� Ȯ��
        if(!owner.AI.ContainsState(new List<PLAYER_STATE> { PLAYER_STATE.IDLE, PLAYER_STATE.WALK, PLAYER_STATE.RUN }))
        {
            return false;
        }

        /// ������ ���� ���� Ȯ��
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            /// ������ ���� ����
            PlayerStaticStatus.ChangeWeaponRight = true;
            owner.AI.weaponController.ChangeSelectWeapon_Right();
            owner.AI.ChangeState(PLAYER_STATE.CHANGE_EQUIP);
            return true;
        }
        /// �޼� ���� ���� Ȯ��
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            /// �޼� ���� ����
            PlayerStaticStatus.ChangeWeaponLeft = true;
            owner.AI.weaponController.ChangeSelectWeapon_Left();
            owner.AI.ChangeState(PLAYER_STATE.CHANGE_EQUIP);
            return true;
        }

        return false;
    }

    /// <summary>
    /// �� �Լ��� �÷��̾��� �̵� �Է� ó���Լ� �Դϴ�.
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
        /// �� �̵� �� �Է� �Ұ� ���� Ȯ��   ž��, ž������ ����
        if (owner.AI.ContainsState(new List<PLAYER_STATE> { PLAYER_STATE.HORSE_RIDE, PLAYER_STATE.HORSE_UNRIDE }))
        {
            return false;
        }

        /// �� �̵� �Է� Ȯ��    ���, �ȱ� -> �޸���
        if (owner.AI.ContainsState(new List<PLAYER_STATE> { PLAYER_STATE.HORSE_IDLE, PLAYER_STATE.HORSE_WALK })
         && !owner.AI.IsStopped
         && Input.GetKey(KeyCode.LeftShift))
        {
            owner.AI.ChangeState(PLAYER_STATE.HORSE_RUN);
            PlayerStaticStatus.currentHorse.AI.ChangeState(HORSE_STATE.RUN);
            return true;
        }

        /// �� �̵� �Է� Ȯ��   ��� -> �̵�
        if (owner.AI.ContainsState(new List<PLAYER_STATE> { PLAYER_STATE.HORSE_IDLE, })
         && !owner.AI.IsStopped)
        {
            owner.AI.ChangeState(PLAYER_STATE.HORSE_WALK);
            PlayerStaticStatus.currentHorse.AI.ChangeState(HORSE_STATE.WALK);
            return true;
        }

        /// �� ž�� ���� �Է� Ȯ��
        if (owner.AI.ContainsState(new List<PLAYER_STATE> { PLAYER_STATE.HORSE_IDLE, PLAYER_STATE.HORSE_WALK })
         && Input.GetKey(KeyCode.R))
        {
            owner.AI.ChangeState(PLAYER_STATE.HORSE_UNRIDE);
            PlayerStaticStatus.currentHorse.OffRide();
            return true;
        }


        /// �� �������� �Է� Ȯ��
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
    /// �� �Լ��� ��ȣ�ۿ� �Է� ó�� �Լ��Դϴ�.
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

    // �� �Լ��� ���ͷ��� ����ĳ��Ʈ ó�� �Լ� �Դϴ�. 
    // ����ĳ��Ʈ ó�� ������ ������ ȣ����Ѿ� �մϴ�.
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
