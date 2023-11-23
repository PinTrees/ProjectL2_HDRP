using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Questor : MonoBehaviour
{
    [SerializeField]
    List<QuestData> quests = new();
    Npc owner;

    void Start()
    {
        TryGetComponent(out owner);
        quests.ForEach(e => e.Owner = owner);
    }

    public void OnQuest()
    {
        UIManager.Instance.QuestInteractUI.Show(quests);
    }
}
