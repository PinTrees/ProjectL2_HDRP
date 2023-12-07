using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class uNavigation : UIObject
{
    [Header("Menues")]
    [SerializeField] Button InventoryButton;
    [SerializeField] Button EquipmentButton;
    [SerializeField] Button ScreenshotButton;

    void Start()
    {
        base.Close();

        InventoryButton.onClick.AddListener(() =>
        {
            UIManager.Instance.InventoryView.Show();
        });

        EquipmentButton.onClick.AddListener(() =>
        {
            UIManager.Instance.EquipmentView.Show();
        });

        ScreenshotButton.onClick.AddListener(() =>
        {
            UIManager.Instance.ScreenshotUI.Show();
        });
    }

    void Update()
    {
    }
}
