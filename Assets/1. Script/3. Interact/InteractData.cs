using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
[CreateAssetMenu(fileName = "Interact Data", menuName = "Scriptable Object/Interact Data", order = int.MinValue)]
public class InteractData : ScriptableObject
{
    public KeyCode Key;
    public float InteractTime;
    public string InteractName;
    public string InteractRequiedFunc = "";
    public string InteractStartFunc = "";
    public string InteractExitFunc = "";
}
