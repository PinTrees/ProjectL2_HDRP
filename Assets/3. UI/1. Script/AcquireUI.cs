using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AcquireUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI mTitleText;
    [SerializeField] RawImage mIcon;

    public void Init()
    {
        Close();
    }

    public void Show(Item item)
    {
        mIcon.texture = item.data.Icon.texture;
        mTitleText.text = item.data.Name;

        gameObject.SetActive(true);

        gameObject.transform.DOScale(Vector3.one, 2.0f).OnComplete(() =>
        {
            Close();
        });
    }

    public void Close()
    {
        gameObject.SetActive(false);    
    }
}
