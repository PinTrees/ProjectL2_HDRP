using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;




public class Inventory : MonoBehaviour
{
    public object Owner;

    [HideInInspector] public WeaponController weaponController;

    [SerializeField]
    public List<Item> items = new();

    void Start()
    {
        weaponController = GetComponent<WeaponController>();
    }

    public void AddItem(Item item)
    {
        items.Add(item);   
    }

    /*public List<Item> GetWeaponItems()
    {
        return items.Where(e => e.data.Type == ITEM_TYPE.RIGHT_WEAPON).ToList();
    }*/

    public void GetSelectWeapon(Item select_target)
    {
        weaponController.ChangeSelectWeapon_Right();
    }

    public List<Item> GetItemWithType(ITEM_TYPE type)
    {
        return items.Where(e => e.data.Type == type).ToList();
    }

    public List<Item> GetWeaponWithType(WEAPON_TYPE type)
    {
        var list = items.Where(e => e.data is WeaponData).ToList();
        return list.Where(e =>
        {
            var weaponData = e.data as WeaponData;
            return weaponData.WeaponType == type;
        }).ToList();
    }

    public List<Item> GetArmorWithType(ARMOR_TYPE type)
    {
        var list = items.Where(e => e.data is ArmorData).ToList();
        return list.Where(e =>
        {
            var weaponData = e.data as ArmorData;
            return weaponData.ArmorType == type;
        }).ToList();
    }
}
