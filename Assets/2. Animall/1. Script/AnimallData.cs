using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public enum AnimallTripType
{

}


// Main -------------------------------
[System.Serializable]
[CreateAssetMenu(fileName = "Animall Data", menuName = "Scriptable Object/Animall Data", order = int.MaxValue)]
public class AnimallData : ScriptableObject
{
    [SerializeField] public int ID;
    [SerializeField] public string Name;
    [SerializeField] public AnimallTripType Type;

    [SerializeField] public float WalkSpeed;
    [SerializeField] public float RunSpeed;

    [Range(0, 100)][SerializeField] public int AggressionFacter;

    [SerializeField] public float RotateSpeed;
    [SerializeField] public float Hp;

    [Range(0, 180)][SerializeField] public float WalkAngle;
    [SerializeField] public float ViewingAngle;

    [HideInInspector][SerializeField] public List<tMonPatternData> AttackPatternDatas = new();
}
