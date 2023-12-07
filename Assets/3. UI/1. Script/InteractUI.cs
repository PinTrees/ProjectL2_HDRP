using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


[System.Serializable]
public class InteractDataUI
{
    public GameObject parent;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI KeyCodeText;
}


public class InteractUI : UIObject
{
    [SerializeField] GameObject InteractInfoUI;
    List<InteractDataUI> interactInfos = new();

    void Start()
    {
        for(int i = 0; i < 5; ++i)
        {
            var spawn = Instantiate(InteractInfoUI);
            spawn.transform.SetParent(InteractInfoUI.transform.parent, true);
           
            var ui = new InteractDataUI();
            ui.parent = spawn;
            ui.NameText = spawn.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            ui.KeyCodeText = spawn.transform.GetChild(1).GetComponent<TextMeshProUGUI>();

            interactInfos.Add(ui);
        }
        
        Destroy(InteractInfoUI);
        base.Close();
    }

    void Update()
    {
        if (InteractManager.Instance.CurrentInteract
        && !InteractManager.Instance.CurrentInteract.IsInteractioning
        &&  PlayerManager.Instance.IsCanInteract)
        {
            var interactData = InteractManager.Instance.CurrentInteract;

            base.Show();

            for(int i = 0; i < interactInfos.Count; ++i)
            {
                if(i >= interactData.Datas.Count)
                {
                    if (interactInfos[i].parent.activeSelf)
                        interactInfos[i].parent.SetActive(false);
                
                    continue;
                }

                interactInfos[i].NameText.text = interactData.Datas[i].InteractName;
                interactInfos[i].KeyCodeText.text = interactData.Datas[i].Key.ToString();
               
                if (!interactInfos[i].parent.activeSelf) interactInfos[i].parent.SetActive(true);
                if (!interactInfos[i].NameText.gameObject.activeSelf) interactInfos[i].NameText.gameObject.SetActive(true);
                if (!interactInfos[i].KeyCodeText.gameObject.activeSelf) interactInfos[i].KeyCodeText.gameObject.SetActive(true);
            }
        }
        else
        {
            base.Close();
        }
    }
}
