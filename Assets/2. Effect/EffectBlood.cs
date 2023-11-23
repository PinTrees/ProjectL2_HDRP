using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectBlood : PoolObject<EffectBlood>
{
    public GameObject EffectObject;

    public void Enter()
    {
        EffectObject.SetActive(true);

        transform.DOScale(transform.localScale, 15f).OnComplete(() =>
        {
            Exit();
        });
    }

    public void Exit()
    {
        EffectObject.SetActive(false);
    }
}
