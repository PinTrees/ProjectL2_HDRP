using com.mobilin.games;
using FIMSpace.FLook;
using FIMSpace.FProceduralAnimation;
using FIMSpace.FSpine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NPC.Animalls.State.HorseBase;


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(LegsAnimator))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(mvHealthController))]
public class Horse : MonoBehaviour
{
    [HideInInspector] public FSM<HORSE_STATE> AI = new FSM<HORSE_STATE>();

    public Animator wingAnimator;

    public Transform RideOnTransform;
    public Transform RideOffTransform;

    private void Start()
    {
        AI.OwnerBase = this;
        AI.SetOwner(this.gameObject);
        AI.Init();

        AI.AddState(new Idle());
        AI.AddState(new Walk());
        AI.AddState(new Run());
        AI.AddState(new Boost());

        AI.ChangeState(HORSE_STATE.IDLE);
    }

    public void OffRide()
    {
        AI.ChangeState(HORSE_STATE.IDLE);
    }

    public void OnRide()
    {
        Debug.Log("[Interaction] Horse");
        
        PlayerManager.Instance.Player.OnRide(this);
        AI.ChangeState(HORSE_STATE.IDLE);
    }

    private void Update()
    {
        AI.Update();
    }
}
