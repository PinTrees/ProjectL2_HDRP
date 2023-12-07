using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class uLoadingView : MonoBehaviour
{
    [SerializeField] GameObject UI;
    [SerializeField] Image bar;

    public void SetAmount(float amount)
    {
        bar.fillAmount = amount;

        if(!UI.activeSelf)
            UI.SetActive(true); 
    }

    public void Close()
    {
        if(UI.activeSelf)
            UI.SetActive(false);
    }
}
