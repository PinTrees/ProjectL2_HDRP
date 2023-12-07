using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class uEquipSelectView : UIObject
{
    protected const int ITEM_SLOT_MAX_COUNT = 27;

    [SerializeField] uEquipSlot slotPrefab;
    [SerializeField] Transform slotsContainer;

    List<uEquipSlot> slots = new();

    void Start()
    {
        GameObjectExtensions.InstantiateList(slotPrefab.gameObject, ITEM_SLOT_MAX_COUNT, slotsContainer, (spawn) => { slots.Add(spawn.GetComponent<uEquipSlot>()); });
        Destroy(slotPrefab.gameObject);

        Close();
    }

    // Show Override
    public void Show(WEAPON_TYPE type, int index)
    {
        base.Show();
        slots.ForEach(e => e.Close());

        var items = PlayerManager.Instance.Player.Inven.GetWeaponWithType(type);
        for(int i = 0; i < items.Count; ++i)
        {
            Item item = items[i];

            slots[i].Show(item.data);
            slots[i].OnClickEvent(() =>
            {
                UIManager.Instance.EquipmentView.SelectEquipItemSlot_Weapon(type, index, item);
                Close();
            });
        }
    }

    public void Show(ARMOR_TYPE type, int index)
    {
        base.Show();
        slots.ForEach(e => e.Close());

        var items = PlayerManager.Instance.Player.Inven.GetArmorWithType(type);
        for (int i = 0; i < items.Count; ++i)
        {
            Item item = items[i];

            slots[i].Show(item.data);
            slots[i].OnClickEvent(() =>
            {
                UIManager.Instance.EquipmentView.SelectEquipItemSlot_Armor(index, item);
                Close();
            });
        }
    }
}
