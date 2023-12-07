using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class uEquipSlot : UIObject
{
    [SerializeField] Button button;
    [SerializeField] Image frame;
    [SerializeField] Image item_background;
    [SerializeField] Image item_icon;
    [SerializeField] GameObject item_selct_effect;

    [HideInInspector] public Weapon weapon;
    [HideInInspector] public ItemData itemdata;

    void Start()
    {
    }

    void Update()
    {
    }

    public void Show(ItemData item)
    {
        if (item == null)
        {
            Close();
            return;
        }

        base.Show();

        item_icon.sprite = item.Icon;

        item_icon.gameObject.SetActive(true);
        item_background.gameObject.SetActive(true);

        button.onClick.RemoveAllListeners();
    }

    public void Show(Weapon item)
    {
        if (item == null)
        {
            Close();
            return;
        }

        base.Show();

        item_icon.sprite = item.WeaponData.Icon;

        item_icon.gameObject.SetActive(true);
        item_background.gameObject.SetActive(true);

        button.onClick.RemoveAllListeners();
    }


    public override void Close()
    {
        button.onClick.RemoveAllListeners();
        item_icon.gameObject.SetActive(false);
        item_background.gameObject.SetActive(false);
    }

    public void OnClickEvent(Action action)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            action();
        });
    }
}
