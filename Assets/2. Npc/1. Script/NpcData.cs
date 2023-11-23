using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum NPC_TYPE
{
    DEFAULT,
    END,
}



// 제거 예정인 코드
// NPC 컴포넌트에 작성
[CreateAssetMenu(fileName = "Npc Data", menuName = "Scriptable Object/Npc Data", order = int.MaxValue)]
public class NpcData : ScriptableObject
{
    public NPC_TYPE Type;
    [SerializeField] public string Name;
    [SerializeField] public WeaponMastery WeaponMastery;
    
    public bool IsRootAnimation = false;

    public int Hp;
    public int Def;
    public float WalkSpeed;
    public float RunSpeed;

    public float HitDelayNormal;
    public float HitDelayStrong;

    public float RunStartRange;

    public float TraceRange;
    public float AgroRange;

    public float AroundRange;

    public float Belligerence;   // 호전성
}
