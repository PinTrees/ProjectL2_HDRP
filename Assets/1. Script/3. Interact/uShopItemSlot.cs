using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class uShopItemSlot : UIObject
{
    [SerializeField] Image selectFrame;
    [SerializeField] Image background;
    [SerializeField] Image itemIcon;
    [SerializeField] TextMeshProUGUI itemNameText;
    [SerializeField] TextMeshProUGUI itemPriceText;
    [SerializeField] Button itemButton; 

    protected override void Start()
    {
    }

    protected override void Update()
    {
    }

    public void OnClickEvent(Action action)
    {
        itemButton.onClick.RemoveAllListeners();
        itemButton.onClick.AddListener(() => { action(); });
    }
}
