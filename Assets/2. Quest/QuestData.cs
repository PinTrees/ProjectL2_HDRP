using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


[System.Serializable]
public enum QUEST_EXIT_TYPE
{
    NOW,

}


[System.Serializable]
public class QuestEnemyKillCount
{
    public MonsterData monstor;
    public int killCount = 0;
}


[System.Serializable]
public class QuestLineData
{
    [TextArea(1, 2)]
    public string Description;

    public NpcData owner;
    public bool IsCamCloseup;
    public bool IsAutoSkip;
}


[System.Serializable]
public class CutSceneData
{
    public string uid;
}


[System.Serializable]
public class QuestEventData
{
    public CutSceneData cutScene;
    [SerializeField]
    public List<QuestLineData> lines = new();
}


// 선형, 비선형 구조
[System.Serializable]
[CreateAssetMenu(fileName = "Quest Data", menuName = "Scriptable Object/Quest Data", order = int.MaxValue)]
public class QuestData : ScriptableObject
{
    public string Uid;
    public string Title;
    public string Description;

    [HideInInspector] public object Owner;

    [Header("Requeid")]
    public List<QuestData> RequeidQuests = new();
    public int ownerIntimacy = 0; 

    [Header("Quest")]
    public List<Item> questItems = new();
    public List<QuestEnemyKillCount> questMonsterKill = new();

    [Header("Resurlt")]
    public List<Item> resurltItems = new();

    [Header("Enter Event")]
    public List<QuestEventData> lines = new();

    [Header("Exit")]
    public QUEST_EXIT_TYPE ExitType;
}


#if UNITY_EDITOR
[CustomEditor(typeof(QuestData)), CanEditMultipleObjects]
public class QuestDataEditor : Editor
{
    QuestData owner;
    private void OnEnable()
    {
        owner = (QuestData)target;
    }

    private void OnInspectorDraw()
    {
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();

        if(GUI.changed)
        {
        }
    }
}
#endif