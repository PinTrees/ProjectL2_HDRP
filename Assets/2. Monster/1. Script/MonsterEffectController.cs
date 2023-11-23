using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class EffectControl
{
    public string AniamtionTrigger;
    public GameObject SpawnEffect;
    public GameObject ExistEffect;

    public List<ParticleSystem> particles = new();

    [Range(0f, 1f)]
    public float ExitDelay;

    public bool isPlay = false;
    public bool isExit = false;
}

public class MonsterEffectController : MonoBehaviour
{
    Monster owner;

    [SerializeField]
    List<EffectControl> effects = new();

    void Start()
    {
        owner = GetComponent<Monster>();

        effects.ForEach(e =>
        {
            e.particles = e.ExistEffect.GetComponentsInChildren<ParticleSystem>().ToList();
        });
    }

    void Update()
    {
        var currentAnimation = owner.AI.animator.GetCurrentAnimatorStateInfo(0);

        effects.ForEach(e =>
        {
            if(currentAnimation.IsTag(e.AniamtionTrigger))
            {
                if(!e.isPlay)
                {
                    e.isPlay = true;
                    e.particles.ForEach(p => p.Play());
                }
                else if(currentAnimation.normalizedTime >= e.ExitDelay && !e.isExit)
                {
                    e.isExit = true;
                    e.particles.ForEach(p => p.Stop());
                }
            }
            else
            {
                e.isExit = false;
                e.isPlay = false;
            }
        });
    }
}
