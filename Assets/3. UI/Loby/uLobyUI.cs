using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class uLobyUI : UIObject
{
    [SerializeField] Button startButton;

    void Start()
    {
        startButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("Main");
        });
    }

    void Update()
    {
        
    }
}
