using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class uItemInfoView : MonoBehaviour
{
    [SerializeField] GameObject UI;

    [Header("UI Value")]
    [SerializeField] TextMeshProUGUI item_name;
    [SerializeField] TextMeshProUGUI item_type;
    [SerializeField] TextMeshProUGUI item_effect;
    [SerializeField] TextMeshProUGUI item_stat;

    #region property
    public Item CurrentItem;
    #endregion

    private void Start()
    {
        UI.SetActive(false);
    }

    private void Update()
    {
        if(CurrentItem != null)
        {
            if(Input.GetKeyDown(KeyCode.LeftControl))
            {

            }
        }
    }

    public void Show(Item item)
    {
        if(!UI.activeSelf)
            UI.SetActive(true);

        item_name.text = item.data.Name;
        item_type.text = "¹«±â";

    }

    public void Close()
    {
        if (UI.activeSelf)
            UI.SetActive(false);
    }
}
