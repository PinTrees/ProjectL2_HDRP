using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class uQuestSlotUI : UIObject
{
    [SerializeField] Button questButton;
    [SerializeField] TextMeshProUGUI titleText;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Show(QuestData quest)
    {
        base.Show();

        titleText.text = quest.Title;
    }

    public void OnClickEvent(Action action)
    {
        questButton.onClick.RemoveAllListeners();
        questButton.onClick.AddListener(() => { action(); });
    }
}
