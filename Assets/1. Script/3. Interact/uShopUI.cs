using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class uShopUI : UIObject
{
    private const int shopMaxItemCount = 50;

    [Header("Purchase UI")]
    [SerializeField] GameObject PurchaseUI;
    [SerializeField] TextMeshProUGUI purchaseCountText;
    [SerializeField] TextMeshProUGUI purchaseUnitPriceText;
    [SerializeField] TextMeshProUGUI purchaseTotalPriceText;
    [SerializeField] Button purchaseConfirmButton;
    [SerializeField] Button purchaseCancelButton;
    Item selectItem;
    int currentPurchaseCount = 0;
    int purchaseMaxCount = 999;

    [Header("Shop UI")]
    [SerializeField] Image selectItemIcon;
    [SerializeField] Image selectItemBackground;
    [SerializeField] TextMeshProUGUI selectItemNameText;
    [SerializeField] TextMeshProUGUI selectItemInfoText;
    [SerializeField] TextMeshProUGUI selectItemPriceText;
    [SerializeField] TextMeshProUGUI selectItemEffectText;
    [SerializeField] uShopItemSlot slotPrefab;

    List<uShopItemSlot> slots = new();
    List<ShopItemData> shopItemList = new();

    void Start()
    {
        for(int i = 0; i < shopMaxItemCount; ++i)
        {
            var slot = GameObject.Instantiate(slotPrefab.gameObject);
            slot.transform.SetParent(slotPrefab.transform.parent, true);

            slots.Add(slot.GetComponent<uShopItemSlot>());
        }

        Destroy(slotPrefab.gameObject);
        slots.ForEach(e => { e.Close(); });

        Close();
    }

    public bool IsShowPurchase() { return PurchaseUI.activeSelf; }
    public void ShowPurchase() { if (!IsShowPurchase()) PurchaseUI.SetActive(true); }
    public void ClosePurchase() { if (IsShowPurchase()) PurchaseUI.SetActive(false); }

    public override void Close()
    {
        base.Close();
        ClosePurchase();
    }
    public void Show(List<ShopItemData> items)
    {
        base.Show();
        UIManager.Instance.Navigation.Close();

        shopItemList = items;
        slots.ForEach(e => { e.Close(); });

        for (int i = 0; i < items.Count; ++i)
        {
            slots[i].OnClickEvent(() => { ShowPurchase(); });
            slots[i].Show();
        }
    }

    void Update()
    {
        if(IsShowPurchase())
        {

        }
    }
}
