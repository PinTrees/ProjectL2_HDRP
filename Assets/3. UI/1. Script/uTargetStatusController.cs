using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class uTargetStatusController : MonoBehaviour
{
    [SerializeField] Transform UI;

    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] Image hpBar;
    [SerializeField] Image mpBar;
    [SerializeField] Image quardBar;
    [SerializeField] Image spBar;

    public void Init()
    {
        Exit();
    }

    public void SetStatus(string name="", float hp = 0, float curHp=0, float mp =0, float curMp=0, float sp =0, float curSp=0)
    {
        if(!UI.gameObject.activeSelf)
            UI.gameObject.SetActive(true);

        nameText.text = name;
        hpBar.fillAmount = curHp / hp;
        spBar.fillAmount = curSp / sp;
    }

    public void Exit()
    {
        UI.gameObject.SetActive(false);    
    }

    void Update()
    {
        
    }
}
