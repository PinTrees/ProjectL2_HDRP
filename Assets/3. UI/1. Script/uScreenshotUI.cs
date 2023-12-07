using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class uScreenshotUI : UIObject
{
    [Header("Controll Button")]
    [SerializeField] Button dayButton;
    [SerializeField] Button dawnButton;
    [SerializeField] Button nightButton;
    [SerializeField] Button dustButton;

    void Start()
    {
        base.Close();

        dayButton.onClick.AddListener(() =>
        {
            WorldManager.Instance.SetDayType(DAY_TYPE.DAY);
        });
        dawnButton.onClick.AddListener(() =>
        {
            WorldManager.Instance.SetDayType(DAY_TYPE.DAWN);
        });
        nightButton.onClick.AddListener(() =>
        {
            WorldManager.Instance.SetDayType(DAY_TYPE.NIGHT);
        });
        dustButton.onClick.AddListener(() =>
        {
            WorldManager.Instance.SetDayType(DAY_TYPE.DUST);
        });
    }

    public override void Show()
    {
        base.Show();
        UIManager.Instance.Navigation.Close();
    }
}
