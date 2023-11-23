using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class WeaponMastery
{
    public float AllMastery;

    public float KatanaMastery;
    public float GreateSowrdMastery;
    public float SowrdMastery;
    public float BowMastery;

    [Range(1, 2)] public float KatanaMastery_multiply;
    [Range(1, 2)] public float GreateSowrdMastery_multiply;
    [Range(1, 2)] public float SowrdMastery_multiply;
    [Range(1, 2)] public float BowMastery_multiply;
}
