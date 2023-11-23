using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum MONSTER_POSITION_TYPE
{
    DYNAMIC,
    FIXED,
    END,
}


public enum MONSTER_TRIB_TYPE
{
    NORMAL,
    END,
}

[System.Serializable]
public enum MONSTER_TYPE
{
    NOMARL,
    ELITE,
    BOSS,
    END,
}


// Player - Npc Attack ----------------------------------
[System.Serializable]
public class tAttackData
{
    public float CancelStartDelay;

    public float ExitDelay;
    public float TickDelay;

    [SerializeField] 
    public List<tHitBoxData> tHitBoxDatas = new();

    public float SuperAmmorStartDelay;
    public float SuperAmmorExitDelay;

    public float NextInputStartDelay;
    public float NextInputExitDelay;

    /// - 
    public bool IsProjectile;

    /// NPC Only
    [SerializeField]
    public tMonAttackRequiedData RequiedData;
    public float DistanceToTarget;
}


// Player Kill Attack ----------------------------------
[System.Serializable]
public class tAttackKillData
{
    public string AnimationName;
    public float DeathAnimation;

    public float ExitDelay;

    public List<float> TickDamageDelay;

    public float SuperAmmorStartDelay;
    public float SuperAmmorExitDelay;

    public float KillFindRange;
    public float KillDistance;

}


// Monster Attack ----------------------------------
[System.Serializable]
public class tMonAttackData
{
    public string AnimationName;
    public string NextAnimaionName;
    public float ExitDelay;

    // Monster State Data
    public float SuperArmorStartDelay;
    public float SuperArmorExitDelay;

    [SerializeField] public tAttackChekData CheckData = new();
    [SerializeField] public List<tHitBoxData> tHitBoxDatas = new();

    public float NextStartDelay;
    public float NextExitDelay;
} 


// Animall Attack ----------------------------------
[System.Serializable]
public class tAttackChekData
{
    public float AttackRange;
}


[System.Serializable]
// 시전자 기준
public enum HIT_FORCE_TYPE
{
    RIGHT,
    LEFT,
    FORWORD,
    BACK,

    UP,
    DOWN,

    EXPLOSION,
}

[System.Serializable]
public enum PARRYED_TYPE
{
    CANCEL,
    GROGGY,
    SUPER_ARMOR,
}


// ----------------------------------
[System.Serializable]
public class tHitBoxData
{
    public float StartDelay;
    public float ExitDelay;

    [SerializeField]
    public PARRYED_TYPE ParryedType;

    public float ParryExitDelay;
    public Vector3 HitboxScale;
    public Vector3 HitboxOfset;

    [SerializeField] 
    public HIT_FORCE_TYPE ForceType;
    public float ForceStrength;

    public float GetExitDelay(float rank) { return (ExitDelay - StartDelay) * rank; }
    public float GetParryDelay(float speed) { return (ParryExitDelay) * speed;  }
}



// -------------------------------------
[System.Serializable]
public class tMonPatternData
{
    public float ActivePercent;

    public float CoolTime;
    public float CurrentCoolTime;

    [SerializeField] 
    public tMonAttackRequiedData RequiedData = new();

    [SerializeField] 
    public List<tMonAttackData> AttackDatas = new();
}


[System.Serializable]
public class tMonAttackRequiedData
{
    public float RangeIn;
}


// ---------------------------------------
[System.Serializable]
public class tPhaseData
{
    public float HealthAmount;
}


// Main -------------------------------
[System.Serializable]
[CreateAssetMenu(fileName = "Monster Data", menuName = "Scriptable Object/Monster Data", order = int.MaxValue)]
public class MonsterData : ScriptableObject
{
    [SerializeField] public string Name;
    [SerializeField] public MONSTER_TYPE Type;
    [SerializeField] public MONSTER_POSITION_TYPE PositionType;
    [SerializeField] public MONSTER_TRIB_TYPE Trib;

    [HideInInspector][SerializeField] public List<tMonAttackData> AttackNomarlDatas = new();
    [HideInInspector][SerializeField] public List<tMonPatternData> AttackPatternDatas = new();

    [SerializeField] public bool IsFirstStrike;
    [SerializeField] public tPhaseData Phase1Data;
    [SerializeField] public tPhaseData Phase2Data;
    [SerializeField] public tPhaseData Phase3Data;
    [SerializeField] public tPhaseData Phase4Data;
    [SerializeField] public tPhaseData Phase5Data;

    public int Hp;
    public int Def;
    public float WalkSpeed;
    public float TraceSpeed;

    public float HitDelayNormal;
    public float HitDelayStrong;

    [SerializeField][Range(0, 100)] public float InfoRange;
    [SerializeField][Range(0, 100)] public float TraceRange;
    [SerializeField][Range(0, 100)] public float AgroRange;

    [HideInInspector][SerializeField] public float RunStartRange;
    [HideInInspector][SerializeField] public float RunExitRange;

    public tMonAttackData CheckAtkTime(float distance)
    {
        tMonAttackData resurlt = null;

        AttackNomarlDatas.ForEach(e =>
        {
            resurlt = e;
        });

        return resurlt;
    }
}

