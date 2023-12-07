using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class uStatusView : MonoBehaviour
{
    [SerializeField] Transform ui;

    [SerializeField] Image hpBar;
    [SerializeField] Image spBar;
    [SerializeField] Image mpBar;

    [Space]
    [SerializeField] Transform StateRoot;
    [SerializeField] Image stateIcon;
    [SerializeField] Image parryIcon;

    private Player owner;
    private vHealthController healthController;


    void Start()
    {
        owner = PlayerManager.Instance.Player;
        healthController = owner.GetComponent<vHealthController>();
    }

    void Update()
    {
        if(healthController)
        {
            hpBar.fillAmount = healthController.currentHealth / healthController.maxHealth;
        }


        if(PlayerStaticStatus.IsGodMode)
        {
            if(!stateIcon.gameObject.activeSelf)
                stateIcon.gameObject.SetActive(true);
        }
        else if(!PlayerStaticStatus.IsGodMode)
        {
            if(stateIcon.gameObject.activeSelf)
                stateIcon.gameObject.SetActive(false);
        }


        if (PlayerStaticStatus.IsParring)
        {
            if (!parryIcon.gameObject.activeSelf)
                parryIcon.gameObject.SetActive(true);
        }
        else if (!PlayerStaticStatus.IsParring)
        {
            if (parryIcon.gameObject.activeSelf)
                parryIcon.gameObject.SetActive(false);
        }


        Vector3 uiPostion = Camera.main.WorldToScreenPoint(owner.transform.position + Vector3.up * 1.5f);
        uiPostion.z = 0f;
        StateRoot.transform.position = uiPostion;
    }
}
