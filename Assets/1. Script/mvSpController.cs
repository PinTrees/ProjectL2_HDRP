using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mvSpController : MonoBehaviour
{
    [SerializeField] public int maxSp;

    public int currentSp;
    
    public void SetMaxSp(int amount) 
    {
        maxSp = amount;
        currentSp = maxSp;
    }

    public void TakeSp(int amount)
    {
        currentSp -= amount;
        currentSp = Mathf.Max(currentSp, 0);
    }
}
