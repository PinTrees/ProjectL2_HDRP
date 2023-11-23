using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class PlayerManager : Singleton<PlayerManager>
{
    Player player;
    public bool IsCanInteract = true;

    public Transform Target { get => player.target; set => player.target = value; }
    public Player Player { get => player; }

    public Vector3 GetPlayerPosition() { return player.transform.position; }
    public Transform GetPlayerTransform() { return player.transform; }
    public Transform GetPlayerModelTransform() { return player.transform; }
    public Transform GetPlayerRootTransform() { return player.GetCharactorRootTransform(); }

    public FSM<PLAYER_STATE> GetAI() { return player.AI; }

    public override void Awake()
    {
        player = GameObject.FindObjectOfType<Player>();
    }
}
