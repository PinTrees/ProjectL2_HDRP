using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class HitBox_Npc : PoolObject<HitBox_Npc>
{
    object owner;
    Weapon wepone;

    [SerializeField] HITBOX_DMG_TYPE damageType;
    [SerializeField] List<HIT_TARGET> target = new List<HIT_TARGET>();

    private tHitBoxData _hitboxData;
    private bool _isHited;

    List<int> hitIdList = new List<int>();

    private float _parryDelay = 0f;
    private float _exitDelay = 0f;

    private float _curExitDelay = 0f;
    private float _curParryTime;
    private bool _parry;

    public void SetOwner(object _owner)
    {
        target.Clear();
        owner = _owner;

        if (owner is Player)
        {
            target.Add(HIT_TARGET.NPC_1);
        }
    }

    public void Enter(tHitBoxData hitboxData
        , object owner
        , float parryDelay = 0
        , float exitDelay = 0)
    {
        this.owner = owner;

        _curExitDelay = 0f;
        _curParryTime = 0f;

        _exitDelay = exitDelay;

        _parry = true;
        _isHited = false;
        _parryDelay = parryDelay;
        _hitboxData = hitboxData;

        hitIdList.Clear();

        transform.DOKill();
        transform.localScale = hitboxData.HitboxScale;

        gameObject.SetActive(true);
    }

    public void Enter(tHitBoxData hitboxData, object owner)
    {
        _curParryTime = 0f;

        this.owner = owner;

        hitIdList.Clear();
        _isHited = false;
        _parry = true;
        gameObject.SetActive(true);

        transform.DOKill();
        transform.DOScale(transform.localScale, hitboxData.ExitDelay).OnComplete(() =>
        {
            Exit();
        });
    }


    public void Exit()
    {
        Debug.Log("[HitBox] Exit()");

        _isHited = false;
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        Hit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        Hit(other);
    }

    private void OnTriggerExit(Collider other)
    {
    }

    public void Hit(Collider other)
    {
        for (int i = 0; i < target.Count; i++)
        {
            if (hitIdList.Contains(other.GetHashCode()))
            {
                return;
            }

            if (target[i] == HIT_TARGET.PLAYER && other.CompareTag("Player"))
            {
                var target = other.GetComponent<Player>();
                target.OnHit(owner
                           , hit_force_type: _hitboxData.ForceType
                           , hit_strength: _hitboxData.ForceStrength
                           , hitbox_tr: transform
                           , isParryOk: _parry);

                if (damageType == HITBOX_DMG_TYPE.SINGLE)
                {
                    Exit();
                }

                hitIdList.Add(other.GetHashCode());
            }
        }
    }

    public void Update()
    {
        _curParryTime += Time.deltaTime;

        if (_parryDelay != 0 && _curParryTime >= _parryDelay)
        {
            _parry = false;
        }
        if (_parryDelay == 0 && _parry)
        {
            _parry = false;
        }

        if (_curParryTime >= _exitDelay)
        {
            Exit();
        }
    }
}
