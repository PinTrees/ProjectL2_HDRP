using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum HIT_EFFECT_TYPE
{
    BLOOD,
    BLOOD_LEFT,
    BLOOD_RANDOM,
    EXF,
    END
}

[Serializable]
public struct HitEffect
{
    public HIT_EFFECT_TYPE type;
    public GameObject effect;
    public ParticleSystem particle;
}

public class HitEffectMgr : Singleton<HitEffectMgr>
{
    [SerializeField] Transform parnetTransform;
    [SerializeField] List<HitEffect> initList = new List<HitEffect>();

    Dictionary<HIT_EFFECT_TYPE, List<GameObject>> objMap = new Dictionary<HIT_EFFECT_TYPE, List<GameObject>>();
    Dictionary<HIT_EFFECT_TYPE, int> objIdx = new Dictionary<HIT_EFFECT_TYPE, int>();

    private void Start()
    {
        initList.ForEach(e =>
        {
            objIdx[e.type] = 0;
            objMap[e.type] = new List<GameObject>();

            for (int i = 0; i < 50; ++i)
            {
                var go = Instantiate(e.effect);
                go.SetActive(false);
                go.transform.SetParent(parnetTransform, false);

                objMap[e.type].Add(go);
            }
        });
    }

    public GameObject GetHitEffect(HIT_EFFECT_TYPE type)
    {
        objIdx[type]++;
        if (objIdx[type] >= 50) objIdx[type] = 0;

        var result = objMap[type][objIdx[type]];
        return result;
    }
}
