#if INVECTOR_BASIC
using Invector;
#endif
#if INVECTOR_MELEE
using Invector.vMelee;
#endif
using System.Collections;
using UnityEngine;

using com.mobilin.games;



public class LockOnPlayer : LockOnBase
{
#if MIS && MIS_LOCKON && INVECTOR_MELEE
    // ----------------------------------------------------------------------------------------------------
    [vEditorToolbar("Settings", order = 0)]
    [Space(10)]
    [vHelpBox("If true, rotates towards the selected target.")]
    [SerializeField] protected bool rotateTowardsTarget = true;
    [SerializeField] protected float lockCameraSpeed = 5f;


    // ----------------------------------------------------------------------------------------------------
    vMeleeManager meleeManager;


    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    protected override IEnumerator Start()
    {
        yield return StartCoroutine(base.Start());

        if (IsAvailable)
        {
            TryGetComponent(out meleeManager);
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    protected override void Update()
    {
        if (!IsAvailable)
            return;

        base.Update();

        if (rotateTowardsTarget)
        {
            if(IsLockOnTarget)
            {
                if (HasTarget())
                {
                    tpCamera.SetLockTarget(mvTargetManager.Instance.CurrentTarget.tr.parent, 0f, lockCameraSpeed);

#if MIS_MOTORCYCLE
                        if (!meleeCombatInput.cc.IsRiderOnAction)
#endif
                    {
/*                        Vector3 dir = mvTargetManager.Instance.CurrentTarget.tr.parent.position - tr.position;
                        meleeCombatInput.cc.RotateToDirection(dir, true);*/
                    }
                }
            }
            else
            {
                 //tpCamera.RemoveLockTarget();
            }
        }
    }
#endif
}