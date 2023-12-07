using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


// 전역 UI 상태 변수
public static class UIStaticState
{
    public static bool isCameraLock = false;
} 


public class UIManager : Singleton<UIManager>
{
    // 상수 정의

    // 초기 멤버 변수 정의
    [Header("Legercy UI")]
    [SerializeField] InteractUI mInterationInfoUI;
    [SerializeField] HotEquipUI mHotEquipUI;
    [SerializeField] GraphicSettingUI mGraphicSettingUI;
    [SerializeField] CombatTextUI mCombatTextUI;
    [SerializeField] uTargetStatusController uTargetStatusController;

    [Header("Main UI")]
    [HideInInspector] public uNavigation Navigation;
    [HideInInspector] public uShopUI ShopUI;
    [HideInInspector] public uEquipmentView EquipmentView;
    [HideInInspector] public uQuestInteractUI QuestInteractUI;
    [HideInInspector] public uIndicatorUI IndicatorUI;
    [HideInInspector] public uScreenshotUI ScreenshotUI;

    #region UI Controller value
    [Header("Navigation View UI")]
    public uInventoryView InventoryView;
    public uItemInfoView ItemInfoView;
    public uStatusView StatusView; 
    #endregion

    private List<AcquireUI> mAcquireUIs = new List<AcquireUI>();

    // 멤버 변수 정의
    private int mAcquireIndex = 0;
    private Monster monsterTarget;

    // 프로퍼티 정의
    public GraphicSettingUI GraphicSettingUI { get => mGraphicSettingUI; }
    public uLoadingView LoadingView;

    public override void Awake()
    {
        ShopUI = GameObject.FindObjectOfType<uShopUI>();
        Navigation = GameObject.FindObjectOfType<uNavigation>();
        EquipmentView = GameObject.FindObjectOfType<uEquipmentView>();
        QuestInteractUI = GameObject.FindObjectOfType<uQuestInteractUI>();
        IndicatorUI = GameObject.FindObjectOfType<uIndicatorUI>();
        ScreenshotUI = GameObject.FindObjectOfType<uScreenshotUI>();
    }
     
    // 멤버 함수 정의
    private void Start()
    {
        Cursor.visible = false;

        Navigation = GameObject.FindObjectOfType<uNavigation>();

        InventoryView?.Init();
        mHotEquipUI?.Init();
        mGraphicSettingUI?.Init();
        mCombatTextUI?.Init();
        mAcquireUIs?.ForEach(e => e.Init());

        uTargetStatusController?.Init();
    }

    public InteractUI GetGatherInfoUI() { return mInterationInfoUI; }
    public HotEquipUI GetHotEquipUI() { return mHotEquipUI; }
    public AcquireUI GetAcquireUI()
    {
        mAcquireIndex++;
        if (mAcquireUIs.Count <= mAcquireIndex) mAcquireIndex = 0;

        return mAcquireUIs[mAcquireIndex];
    }
    public CombatTextUI GetCombatTextUI() { return mCombatTextUI; }


    public void SetTarget(Monster monster) { monsterTarget = monster; }
    public void RemoveTarget() { monsterTarget = null; }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(ScreenshotUI.IsShow())
            {
                ScreenshotUI.Close();
            }
            else if(EquipmentView.IsShow())
            {
                if (EquipmentView.SelectView.IsShow()) EquipmentView.SelectView.Close();
                else EquipmentView.Close();
            }
            else if(QuestInteractUI.IsShow())
            {
                QuestInteractUI.Close();
            }
            else if(ShopUI.IsShow())
            {
                if (ShopUI.IsShowPurchase()) ShopUI.ClosePurchase();
                else ShopUI.Close();
            }
            else if (Navigation.IsShow())
            {
                if (InventoryView && InventoryView.IsShow()) InventoryView.Close();
                if (EquipmentView && EquipmentView.IsShow()) EquipmentView.Close();

                Navigation.Close();
            }
            else
            {
                Navigation.Show();
            }
        }

        BuildTargetStatusUI();
    }

    public void BuildTargetStatusUI()
    {
        if (uTargetStatusController && monsterTarget)
        {
            uTargetStatusController.SetStatus(
                  name: monsterTarget.Data.Name
                , hp: monsterTarget.AI.healthController.maxHealth
                , curHp: monsterTarget.AI.healthController.currentHealth
                , sp: 1
                , curSp: 1
            );
        }
    }
}
