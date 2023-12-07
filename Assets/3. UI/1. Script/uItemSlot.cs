using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class uItemSlot : UIObject
{
    [SerializeField] Button button;
    [SerializeField] Image frame;
    [SerializeField] Image item_background;
    [SerializeField] Image item_icon;
    [SerializeField] GameObject item_selct_effect;
    [SerializeField] TextMeshProUGUI item_count;

    public void Init()
    {
        Close();
    }

    public void OnClickEvent(Action action)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            action();
        });
    }

    public void RemoveAction()
    {
        button.onClick.RemoveAllListeners();
    }

    public void Show(Item item)
    {
        if (item.data == null)
        {
            Close();
            return;
        }

        base.Show();

        item_count.text = item.count.ToString();
        item_icon.sprite = item.data.Icon;

        item_icon.gameObject.SetActive(true);
        item_count.gameObject.SetActive(true);
        item_background.gameObject.SetActive(true);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            UIManager.Instance.ItemInfoView.Show(item);
        });
    }

    public override void Close()
    {
        button.onClick.RemoveAllListeners();
        item_icon.gameObject.SetActive(false);
        item_count.gameObject.SetActive(false);
        item_background.gameObject.SetActive(false);
    }
}
