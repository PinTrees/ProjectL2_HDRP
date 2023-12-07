using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class uTargetIndicator : MonoBehaviour
{
    public Image TargetIndicatorImage;
    public Image OffScreenTargetIndicator;

    public float OutOfSightOffset = 20f;
    private float outOfSightOffest { get => OutOfSightOffset; }

    private GameObject target;
    private Camera mainCamera;
    private RectTransform canvasRect;

    private RectTransform rectTransform;

    [Header("Hp Component")]
    public bool use_hp_bar;
    public Image TargetHpBar;
    private vHealthController hpController;

    private bool isActive = false;
    private bool isOutOffScreen = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void Enter(GameObject target, Camera mainCamera, Canvas canvas)
    {
        isActive = true;

        this.target = target;
        this.mainCamera = mainCamera;
        canvasRect = canvas.GetComponent<RectTransform>();

        if (use_hp_bar) hpController = target.GetComponent<vHealthController>();

        UIManager.Instance.IndicatorUI.AddTargetIndicator(this);
    }
    public void Exit()
    {
        isActive = false;
        transform.SetParent(target.transform, true);

        OffScreenTargetIndicator.gameObject.SetActive(false);
        TargetIndicatorImage.enabled = false;
    }

    

    public void UpdateTargetIndicator()
    {
        if(isActive)
        {
            SetIndicatorPosition();
            SetIndicatorHpbar();
        }
    }

    protected void SetIndicatorHpbar()
    {
        if (!use_hp_bar) return;

        if(isOutOffScreen)
        {
            if (TargetHpBar.transform.parent.gameObject.activeSelf)
                TargetHpBar.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            TargetHpBar.fillAmount = hpController.GetFillAmount();

            if (!TargetHpBar.transform.parent.gameObject.activeSelf)
                TargetHpBar.transform.parent.gameObject.SetActive(true);
        }
    }

    protected void SetIndicatorPosition()
    {
        // screenSpace를 기준으로 대상의 위치를 ​​가져옵니다.
        Vector3 indicatorPosition = mainCamera.WorldToScreenPoint(target.transform.position);

        // 대상이 카메라 앞과 경계 내에 있는 경우
        if (indicatorPosition.z >= 0f & indicatorPosition.x <= canvasRect.rect.width * canvasRect.localScale.x
         & indicatorPosition.y <= canvasRect.rect.height * canvasRect.localScale.x & indicatorPosition.x >= 0f & indicatorPosition.y >= 0f)
        {
            isOutOffScreen = false;
            indicatorPosition.z = 0f;
            targetOutOfSight(indicatorPosition);
        }

        // 대상이 캔버스 앞에 있으나 시야에서 벗어난 경우
        else if (indicatorPosition.z >= 0f)
        {
            isOutOffScreen = true;
            indicatorPosition = OutOfRangeindicatorPositionB(indicatorPosition);
            targetOutOfSight(indicatorPosition);
        }
        else
        {
            isOutOffScreen = true;
            // 인디케이터 위치 반전. 그렇지 않으면 대상이 카메라 뒷면에 있으면 인디케이터의 위치가 반전됩니다!
            indicatorPosition *= -1f;

            // 인디케이터 위치를 설정하고 targetIndicator를 outOfSight 으로 설정합니다.
            indicatorPosition = OutOfRangeindicatorPositionB(indicatorPosition);
            targetOutOfSight(indicatorPosition);
        }

        rectTransform.position = indicatorPosition;
    }

    private Vector3 OutOfRangeindicatorPositionB(Vector3 indicatorPosition)
    {
        //indicatorPosition.z를 0f로 설정합니다. 
        indicatorPosition.z = 0f;

        // 캔버스 중심을 계산하고 표시기 위치에서 빼서 왼쪽 하단 대신 캔버스 중심의 표시기 좌표를 갖습니다.
        Vector3 canvasCenter = new Vector3(canvasRect.rect.width / 2f, canvasRect.rect.height / 2f, 0f) * canvasRect.localScale.x;
        indicatorPosition -= canvasCenter;

        // 대상 벡터가 캔버스 직사각형의 y 테두리와 (첫 번째) 교차하는지 또는 벡터가 x 테두리와 (첫 번째) 교차하는지 계산합니다.
        // 최대값으로 설정해야 하는 경계선과 표시기를 이동해야 하는 경계선(위 및 아래 또는 왼쪽 및 오른쪽)을 확인하는 데 필요합니다.
        float divX = (canvasRect.rect.width / 2f - outOfSightOffest) / Mathf.Abs(indicatorPosition.x);
        float divY = (canvasRect.rect.height / 2f - outOfSightOffest) / Mathf.Abs(indicatorPosition.y);

        // x 경계선과 먼저 교차하는 경우 x-원을 경계선에 놓고 그에 따라 y-원을 조정합니다(삼각법).
        if (divX < divY)
        {
            float angle = Vector3.SignedAngle(Vector3.right, indicatorPosition, Vector3.forward);
            indicatorPosition.x = Mathf.Sign(indicatorPosition.x) * (canvasRect.rect.width * 0.5f - outOfSightOffest) * canvasRect.localScale.x;
            indicatorPosition.y = Mathf.Tan(Mathf.Deg2Rad * angle) * indicatorPosition.x;
        }

        // 경계선과 먼저 교차하는 경우 y값을 경계선에 놓고 x값을 그에 맞게 조정합니다(삼각법)
        else
        {
            float angle = Vector3.SignedAngle(Vector3.up, indicatorPosition, Vector3.forward);

            indicatorPosition.y = Mathf.Sign(indicatorPosition.y) * (canvasRect.rect.height / 2f - outOfSightOffest) * canvasRect.localScale.y;
            indicatorPosition.x = -Mathf.Tan(Mathf.Deg2Rad * angle) * indicatorPosition.y;
        }

        // 위치를 실제 rectTransform 좌표계로 다시 변경하고 위치를 반환합니다.
        indicatorPosition += canvasCenter;
        return indicatorPosition;
    }



    private void targetOutOfSight(Vector3 indicatorPosition)
    {
        // OutOfSight인 경우
        if (isOutOffScreen)
        {
            // 일부 항목을 활성화 및 비활성화합니다.
            if (OffScreenTargetIndicator.gameObject.activeSelf == false) OffScreenTargetIndicator.gameObject.SetActive(true);
            if (TargetIndicatorImage.isActiveAndEnabled == true) TargetIndicatorImage.enabled = false;

            // OutOfSight 방향 표시기의 회전 설정
            OffScreenTargetIndicator.rectTransform.rotation = Quaternion.Euler(rotationOutOfSightTargetindicator(indicatorPosition));

            //outOfSightArrow.rectTransform.rotation  = Quaternion.LookRotation(indicatorPosition- new Vector3(canvasRect.rect.width/2f,canvasRect.rect.height/2f,0f)) ;
            /*outOfSightArrow.rectTransform.rotation = Quaternion.LookRotation(indicatorPosition);
            viewVector = indicatorPosition- new Vector3(canvasRect.rect.width/2f,canvasRect.rect.height/2f,0f);
            
            //Debug.Log("CanvasRectCenter: " + canvasRect.rect.center);
            outOfSightArrow.rectTransform.rotation *= Quaternion.Euler(0f,90f,0f);*/
        }
        // 인디케이터가 InSight인 경우 inSight 활성화, OutOfSight 비활성화
        else
        {
            if (OffScreenTargetIndicator.gameObject.activeSelf == true) OffScreenTargetIndicator.gameObject.SetActive(false);
            if (TargetIndicatorImage.isActiveAndEnabled == false) TargetIndicatorImage.enabled = true;
        }
    }


    private Vector3 rotationOutOfSightTargetindicator(Vector3 indicatorPosition)
    {
        // canvasCenter 계산 
        // 인디케이터 위치와 위쪽 방향 사이의 signedAngle을 계산합니다.
        // 각도를 회전 벡터로 반환
        Vector3 canvasCenter = new Vector3(canvasRect.rect.width / 2f, canvasRect.rect.height / 2f, 0f) * canvasRect.localScale.x;
        float angle = Vector3.SignedAngle(Vector3.up, indicatorPosition - canvasCenter, Vector3.forward);
        return new Vector3(0f, 0f, angle);
    }
}
