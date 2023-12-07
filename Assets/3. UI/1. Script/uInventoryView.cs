using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class uInventoryView : UIObject
{
    [SerializeField] int inventoryCount = 0;
    [SerializeField] uItemSlot slotPrefab;
    [SerializeField] Transform slotParentTransform;
    
    List<uItemSlot> slots = new List<uItemSlot>();

    public void Init()
    {
        for(int i = 0; i < inventoryCount; ++i)
        {
            var go = GameObject.Instantiate(slotPrefab.gameObject);
            go.transform.parent = slotParentTransform;
            slots.Add(go.GetComponent<uItemSlot>());
        }

        Destroy(slotPrefab.gameObject);
        slots.ForEach(e => e.Init());
    }

    void Start()
    {
        Close();
    }

    public override void Show()
    {
        base.Show();

        slots.ForEach(e => e.Close());

        var itemList = PlayerManager.Instance.Player.Inven.items;

        int index = 0;
        itemList.ForEach((e) =>
        {
            slots[index++].Show(e);
        });
    }
    
    public override void Close()
    {
        base.Close();

        UIManager.Instance.ItemInfoView.Close();
    }
}
