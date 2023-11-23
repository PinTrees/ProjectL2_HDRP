using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateBase<T> 
{
    FSM<T> context;
    T type;

    public StateBase(T t) { type = t; }

    public T Type { get => type; }
    public FSM<T> AI { get => context; set => context = value; }

    public bool IsEquals(T t) { return type.Equals(t); }
    public bool IsNotEquals(T t) { return !type.Equals(t); }

    public virtual void Enter()
    {
        AI.IsAnimatorInit = false;
    }

    public virtual void Update() {}

    public virtual void FinalUpdate() {}

    public virtual void LateUpdate() {}

    public virtual void OnDrawGizmos() {}

    public virtual void Exit()
    {
        AI.IsAnimatorInit = false;
    }
}
