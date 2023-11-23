using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public class AniamtorStateData
{
    public string Tag;
    public string BoolName;
}


public class StateBehaviour : MonoBehaviour
{
    public string Name;
    public FsmBehaviour FSM;
    public List<AniamtorStateData> animationData;


    [Space]
    #region Menu Data
    public bool UseAnimatorState;
    #endregion


    [Header("Event Func")]
    #region Event Func
    [Space]
    public UnityEvent EnterEvent;
    [Space]
    public UnityEvent ExitEvent;
    #endregion


    void Start() {}

    public virtual void ManualUpdate()
    {
    }
}
