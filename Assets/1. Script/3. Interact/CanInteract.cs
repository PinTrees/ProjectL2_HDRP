using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;


public class CanInteract : MonoBehaviour
{
    // Interact Object Base Name
    public string Name;

    [Range(0.5f, 3)]
    public float InteractionRange;
     
    [Range(0, 5)]
    private float CurrentInteractionDuration;

    [Space(20)]
    [SerializeField]
    public List<InteractData> Datas;

    object triggerUser;

    #region Property
    [HideInInspector] public bool IsInteractioning = false;
    [HideInInspector] public bool IsDisable;
    [HideInInspector] public InteractData CurrentInteractData;
    #endregion


    #region Init Func
    void Start() 
    {
        IsDisable = false;
        IsInteractioning = false;

        GameObject childObject = new GameObject("TriggerLayer");
        SphereCollider collider = childObject.AddComponent<SphereCollider>();

        // 레이어를 "Triggers"로 변경
        collider.gameObject.layer = LayerMask.NameToLayer("Triggers");
        collider.isTrigger = true;
        collider.radius = InteractionRange;

        childObject.transform.parent = transform;
        childObject.transform.localEulerAngles = Vector3.zero;
        childObject.transform.localPosition = Vector3.zero;
        childObject.transform.localScale = Vector3.one;
    }
    #endregion


    #region Update Func
    void Update()
    {
        if(IsInteractioning && CurrentInteractData != null)
        {
            CurrentInteractionDuration += Time.deltaTime;
            if (CurrentInteractionDuration >= CurrentInteractData.InteractTime && CurrentInteractData.InteractTime > 0)
            {
                CurrentInteractionDuration = 0;
                OnComplete();
            }
        }
    }
    #endregion


    // 상호참조 구조변경 필요
    public InteractData FindInteraction()
    {
        foreach(var e in Datas)
        {
            if (Input.GetKeyDown(e.Key))
            {
                return e;
            }
        }

        return null;
    }


    #region Point Func
    /// <summary>
    /// 이 함수는 상호작용 시작시점에 호출됩니다.
    /// </summary>
    public virtual void EnterInteract(object trigger_user, InteractData interact)
    {
        IsDisable = true;
        IsInteractioning = true;
        CurrentInteractionDuration = 0;

        triggerUser = trigger_user;
        CurrentInteractData = interact;

        if(CurrentInteractData.InteractStartFunc != "")
        {
            gameObject.SendMessage(CurrentInteractData.InteractStartFunc);
        }

        if (CurrentInteractData.InteractTime <= 0)
        {
            OnComplete();
        }
    }

    public virtual void OnSelect()
    {
        InteractManager.Instance.CurrentInteract = this;
    }

    /// <summary>
    /// 이 함수는 상호작용 종료 시점, 또는 정상 완료 시점에 프레임워크에서 자동으로 호출됩니다.
    /// </summary>
    protected virtual void OnComplete()
    {
        InteractManager.Instance.CurrentInteract = null;
        IsInteractioning = false;
        IsDisable = false;

        if (CurrentInteractData.InteractExitFunc != "")
        {
            gameObject.SendMessage(CurrentInteractData.InteractExitFunc);
        }

        triggerUser = null;
        CurrentInteractData = null;

        return;
    }
    #endregion


    private void OnDrawGizmos()
    {
    }
}
