using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public enum MAP_MARK_TYPE
{
    PLAYER,
    MONSTER,
    NPC,
}


public class uMapMark : PoolObject<uMapMark>
{
    public MAP_MARK_TYPE markType;
    const float MARK_Y_DISTANCE = 4999;
    Vector3 MARK_ANGLE_OFFSET = new Vector3 (90, 0, 0);

    Transform target;

    public void Init(Transform owner)
    {
        this.target = owner;
        transform.eulerAngles = MARK_ANGLE_OFFSET;
    }

    public void Enter()
    {
        gameObject.SetActive(true);
    }

    public void Exit()
    {
        gameObject.SetActive(false);    
    }

    void Update()
    {
        var pos = target.position;
        pos.y = MARK_Y_DISTANCE;
        transform.position = pos;

        transform.eulerAngles = new Vector3(90, 0, target.transform.eulerAngles.y);
    }
}
