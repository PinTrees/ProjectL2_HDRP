using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class uEquipmentView : UIObject
{
    protected const int WEAPON_SLOT_COUNT_MAX = 5;
    protected const int AMOR_SLOT_COUNT_MAX = 5;
    protected const int ITEM_SLOT_COUNT_MAX = 18;

    public uEquipSelectView SelectView;

    [Header("Prefab")]
    [SerializeField] uEquipSlot slotPrefab;

    [Header("Slots parent")]
    [SerializeField] Transform righthandSlotContianer;
    [SerializeField] Transform lefthandSlotContainer;
    [SerializeField] Transform amorSlotContainer;
    [SerializeField] Transform itemSlotContainer;

    List<uEquipSlot> righthandSlots = new();
    List<uEquipSlot> lefthandSlots = new();
    List<uEquipSlot> amorSlots = new();
    List<uEquipSlot> itemSlots = new();

   
    Item[] userItem_amor = new Item[5];
    WeaponController weaponController;

    void Start()
    {
        GameObjectExtensions.InstantiateList(slotPrefab.gameObject, WEAPON_SLOT_COUNT_MAX, righthandSlotContianer, (spawn) => { righthandSlots.Add(spawn.GetComponent<uEquipSlot>()); });
        GameObjectExtensions.InstantiateList(slotPrefab.gameObject, WEAPON_SLOT_COUNT_MAX, lefthandSlotContainer, (spawn) => { lefthandSlots.Add(spawn.GetComponent<uEquipSlot>()); });
        GameObjectExtensions.InstantiateList(slotPrefab.gameObject, AMOR_SLOT_COUNT_MAX, amorSlotContainer, (spawn) => { amorSlots.Add(spawn.GetComponent<uEquipSlot>()); });
        GameObjectExtensions.InstantiateList(slotPrefab.gameObject, ITEM_SLOT_COUNT_MAX, itemSlotContainer, (spawn) => { itemSlots.Add(spawn.GetComponent<uEquipSlot>()); });

        Destroy(slotPrefab.gameObject);

        Close();
    }

    public override void Show()
    {
        base.Show();
        SelectView?.Close();
        weaponController = PlayerManager.Instance.Player.AI.weaponController;

        Build_RightHand();
        Build_LeftHand();
        Build_Armor();
    }

    public override void Close()
    {
        base.Close();
        SelectView?.Close();
    }

    private void Build_RightHand()
    {
        for (int i = 0; i < weaponController.Righthand.Count; ++i)
        {
            int current_index = i;

            if (weaponController.Righthand[i] != null)
            {
                righthandSlots[i].Show(weaponController.Righthand[i]);
            }
            else
            {
                righthandSlots[i].Close();
            }

            righthandSlots[i].OnClickEvent(() =>
            {
                SelectView.Show(WEAPON_TYPE.RIGHT_HAND, current_index);
            });
        }
    }
    private void Build_LeftHand()
    {
        for (int i = 0; i < weaponController.Lefthand.Count; ++i)
        {
            int current_index = i;

            if (weaponController.Lefthand[i] != null)
            {
                lefthandSlots[i].Show(weaponController.Lefthand[i]);
            }
            else
            {
                lefthandSlots[i].Close();
            }

            lefthandSlots[i].OnClickEvent(() =>
            {
                SelectView.Show(WEAPON_TYPE.LEFT_HAND, current_index);
            });
        }
    }

    private void Build_Armor()
    {
        for (int i = 0; i < userItem_amor.Length; ++i)
        {
            int current_index = i;

            if (userItem_amor[i] != null)
            {
                amorSlots[i].Show(userItem_amor[i].data);
            }
            else
            {
                amorSlots[i].Close();
            }

            amorSlots[i].OnClickEvent(() =>
            {
                SelectView.Show((ARMOR_TYPE)i, current_index);
            });
        }
    }

    public void SelectEquipItemSlot_Weapon(WEAPON_TYPE type, int index, Item item)
    {
        if(type == WEAPON_TYPE.RIGHT_HAND)
        {
            weaponController.ChangeWeaponQuickSlot(type, index, item);
        }
        if (type == WEAPON_TYPE.LEFT_HAND)
        {
            weaponController.ChangeWeaponQuickSlot(type, index, item);
        }

        Show();
    }
    public void SelectEquipItemSlot_Armor(int index, Item item)
    {

        Show();
    }
}
