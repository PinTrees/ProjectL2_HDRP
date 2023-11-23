using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


[System.Serializable]
public class ShopItemData
{
    public ItemData itemData;
    public float priceMultifly;
}

public class Shop : MonoBehaviour
{
    [SerializeField]
    List<ShopItemData> shopItems = new();

    void Start()
    {
    }

    void Update()
    {
    }

    public bool CanUse()
    {
        return true;
    }

    public void OpenShop()
    {
        UIManager.Instance.ShopUI.Show(shopItems);
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(Shop)), CanEditMultipleObjects]
public class ShopEditor : Editor
{
    Shop owner;

    private void OnEnable()
    {
        owner = (Shop)target;
    }
}
#endif