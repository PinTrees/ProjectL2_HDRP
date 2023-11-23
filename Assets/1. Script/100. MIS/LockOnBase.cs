#if INVECTOR_BASIC
using Invector;
using Invector.vCharacterController;
#endif
using System.Collections;
using UnityEngine;

using com.mobilin.games;
using Invector.vCamera;




[vClassHeader("LockOn", iconName = "misIconRed")]
public class LockOnBase : vMonoBehaviour
{
#if MIS_LOCKON && INVECTOR_BASIC
    // ----------------------------------------------------------------------------------------------------
    // 
    [vEditorToolbar("Settings", order = 0)]
    [Tooltip("LockOn the current target. It also changes LockOn target that meets a LockOn condition.")]
    public GenericInput lockOnInput = new GenericInput("Mouse2", "", "");
    [Range(1, 5)] public int findTargetRate = 2;

    [Header("Player Character Type")]
    [Tooltip("MIS-LockOn can be used for non-Invector player characters")]
    public bool isPlayer = true;
    public Vector3 nonPlayerCharacterOffset = Vector3.up;

    [Header("Use Default Input System")]
    public bool isUseThirdPersonInput = true;

    // ----------------------------------------------------------------------------------------------------
    [vEditorToolbar("Debug", order = 100)]
    [mvReadOnly][SerializeField] protected bool isAvailable;

    // ----------------------------------------------------------------------------------------------------
    protected mvThirdPersonInput baseInputSystem;
    protected vThirdPersonCamera tpCamera;
    protected Transform tr;

    private mvHealthController healthController;
    private Animator animator;
    private bool _isLockOn = false;


    #region Property
    // ----------------------------------------------------------------------------------------------------
    // if true, it means this action is not blocked and can be used
    public virtual bool IsAvailable
    {
        get => isAvailable;
        set => isAvailable = value;
    }

    // ----------------------------------------------------------------------------------------------------
    bool useIndicator = true;
    public bool UseIndicator
    {
        get => useIndicator;
        set
        {
            if (useIndicator != value)
            {
                useIndicator = value;
                mvTargetManager.Instance.UseIndicator = value;
            }
        }
    }

    public bool IsLockOnTarget { get => _isLockOn; set => _isLockOn = value; }
    public bool IsLostTarget = false;
    #endregion


    // ----------------------------------------------------------------------------------------------------
    protected virtual IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();

        tr = transform;
        animator = transform.GetComponent<Animator>();
        
        if (isPlayer)
        {
            FindCamera();

            if (TryGetComponent(out baseInputSystem))
            {
                mvTargetManager.Instance.SetPlayer(
                    transform,
                    transform.InverseTransformPoint(baseInputSystem.animator.GetBoneTransform(HumanBodyBones.Head).position));
            }
            else
            {
                mvTargetManager.Instance.SetPlayer(
                 transform,
                 transform.InverseTransformPoint(animator.GetBoneTransform(HumanBodyBones.Head).position));
            }
        }
        else
        {
            mvTargetManager.Instance.SetPlayer(
                transform,
                transform.InverseTransformPoint(nonPlayerCharacterOffset));
        }

        mvTargetManager.Instance.SetIndicatorVisible(true);

        healthController = GetComponentInParent<mvHealthController>();

        if (healthController)
            healthController.onDead.AddListener(ResetPlayer);

        IsAvailable = true;
    }

    public virtual void FindCamera()
    {
        var tpCameras = FindObjectsOfType<vThirdPersonCamera>();

        if (tpCameras.Length > 1)
        {
            tpCamera = System.Array.Find(tpCameras, tp => !tp.isInit);

            if (tpCamera == null)
            {
                tpCamera = tpCameras[0];
            }

            if (tpCamera != null)
            {
                for (int i = 0; i < tpCameras.Length; i++)
                {
                    if (tpCamera != tpCameras[i])
                    {
                        Destroy(tpCameras[i].gameObject);
                    }
                }
            }
        }
        else if (tpCameras.Length == 1)
        {
            tpCamera = tpCameras[0];
        }

        if (tpCamera && tpCamera.mainTarget != transform)
        {
            tpCamera.SetMainTarget(this.transform);
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    protected virtual void Update()
    {
        if (!IsAvailable)
        {
            if (mvTargetManager.Instance.IsOnAction)
            {
                mvTargetManager.Instance.IsOnAction = false;

                mvTargetManager.Instance.ClearAllTargetList();
                mvTargetManager.Instance.SetIndicatorVisible(false);
            }
            return;
        }
        else
        {
            if (!mvTargetManager.Instance.IsOnAction)
            {
                mvTargetManager.Instance.IsOnAction = true;
                mvTargetManager.Instance.SetIndicatorVisible(true);
            }
        }

        if ((Time.frameCount % findTargetRate) == 0)
            mvTargetManager.Instance.FindTarget();

        mvTargetManager.Instance.UpdateIndicators();
    }

    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    public bool HasTarget()
    {
        return mvTargetManager.Instance.CurrentTarget != null;
    }

    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    public virtual bool HasLockOnTarget()
    {
        return mvTargetManager.Instance.CurrentTarget != null && mvTargetManager.Instance.CurrentTarget.isLocked;
    }

    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    public virtual void ResetPlayer(GameObject character = null)
    {
        IsAvailable = false;

        mvTargetManager.Instance.SetPlayer(null, Vector3.zero);
        mvTargetManager.Instance.ClearAllTargetList();
        mvTargetManager.Instance.SetIndicatorVisible(false);

        Destroy(this);
    }
#endif
}