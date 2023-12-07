using System.Collections;
using System.Collections.Generic;

using UnityEngine;


// 락온 파인더 컨트롤러
// 기존에 MIS 미들웨어가 수행하던 mvTargetManager 역할을 대신 수행해야 합니다.
public class LockOnTargetFinder : MonoBehaviour
{
    // 타겟 대상 레이어 
    // 대상을 설정할 콜라이더를 보유하는 최상위 부모의 레이어만 해당 레이어로 설정
    [SerializeField] LayerMask targetLayers;
    [SerializeField] Transform enemyTarget_Locator;

    [Tooltip("StateDrivenMethod for Switching Cameras")]
    [SerializeField] Animator cinemachineAnimator;

    [Header("Settings")]
    [SerializeField] bool isZeroVertLook;
    [SerializeField] float finderRange = 50;
    [SerializeField] float lookAtSmoothing = 2;
    [Tooltip("Angle_Degree")]
    [SerializeField] float maxFindAngle = 60;
    [SerializeField] float crossHairScale = 0.1f;

    public bool IsTargetLocked;

    CameraFollow cameraFollow;

    Transform currentTarget;
    Transform followTarget;
    Transform cam;
    float currentYOffset;
    
    private void Awake()
    {
        followTarget = GameObject.FindObjectOfType<Player>().transform;
        cameraFollow = GameObject.FindObjectOfType<CameraFollow>();
    }

    private void Start()
    {
        cam = Camera.main.transform;
    }

    private void Update()
    {
        transform.position = followTarget.position;
        cameraFollow.IsLockTarget = IsTargetLocked;

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (currentTarget)
            {
                ResetTarget();
                return;
            }
            else
            {
                currentTarget = ScanNearBy();

                if (currentTarget) FoundTarget();
                else ResetTarget();
            }
        }

        if (IsTargetLocked)
        {
            if (!TargetOnRange()) ResetTarget();
            else LookAtTarget();
        }
    }

    void FoundTarget()
    {
        IsTargetLocked = true;
        cinemachineAnimator.Play("TargetCam");
    }

    void ResetTarget()
    {
        currentTarget = null;
        IsTargetLocked = false;
        cinemachineAnimator.Play("FollowCam");
    }

    // 타겟의 범위가 올바른지 확인
    bool TargetOnRange()
    {
        if (Vector3.Distance(transform.position, currentTarget.position) > finderRange) return false;
        else return true;
    }

    private void LookAtTarget()
    {
        if (currentTarget == null)
        {
            ResetTarget();
            return;
        }

        enemyTarget_Locator.position = currentTarget.position + new Vector3(0, currentYOffset, 0);
     
        Vector3 dir = currentTarget.position - transform.position;
        dir.y = 0;
        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, Time.deltaTime * lookAtSmoothing);
    }

    // 타겟 탐색 알고리즘 (각도, 거리 기반)
    // 타겟 탐색 방법을 변경하고 싶다면 해당 함수를 수정하거나 하위 계산함수를 이 곳에 추가해야 합니다.
    Transform ScanNearBy()
    {
        // 1. 가장 가까운 적 탐색일 경우 
        // 2. 화면의 각도상 가장 중앙에 위치한 적 탐색일 경우

        Collider[] nearbyTargets = Physics.OverlapSphere(transform.position, finderRange, targetLayers);
        float closestAngle = maxFindAngle;
        Transform closestTarget = null;
        if (nearbyTargets.Length <= 0) return null;

        for (int i = 0; i < nearbyTargets.Length; i++)
        {
            Vector3 dir = nearbyTargets[i].transform.position - cam.position;
            dir.y = 0;
            float _angle = Vector3.Angle(cam.forward, dir);

            if (_angle < closestAngle)
            {
                closestTarget = nearbyTargets[i].transform;
                closestAngle = _angle;
            }
        }

        if (!closestTarget) return null;
        float h1 = closestTarget.GetComponent<CapsuleCollider>().height;
        float h2 = closestTarget.localScale.y;
        float h = h1 * h2;
        float half_h = (h / 2) / 2;
        currentYOffset = h - half_h;
        if (isZeroVertLook && currentYOffset > 1.6f && currentYOffset < 1.6f * 3) currentYOffset = 1.6f;

        Vector3 tarPos = closestTarget.position + new Vector3(0, currentYOffset, 0);

        return closestTarget;
    }
}
