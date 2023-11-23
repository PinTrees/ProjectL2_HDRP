using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class QuestManager : Singleton<QuestManager>
{
    List<QuestData> currentQuests = new();

    public override void Awake()
    {
        LoadData();
    }

    public void LoadData() { }

    public void StartQuest(QuestData quest)
    {
        currentQuests.Add(quest); 
    }
}
