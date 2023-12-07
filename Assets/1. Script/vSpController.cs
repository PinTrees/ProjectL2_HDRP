using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vSpController : MonoBehaviour
{
    public int maxSp;
    public int currentSp;

    public void TakeSp(int amount)
    {
        currentSp -= amount;
        currentSp = Mathf.Max(currentSp, 0);
    }
}
