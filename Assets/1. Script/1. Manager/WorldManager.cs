using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DAY_TYPE
{
    NIGHT,
    DAWN,
    DAY,
    DUST,
}

public class WorldManager : Singleton<WorldManager>
{
    [Header("Post Processing")]
    [SerializeField] GameObject dayPostProcessing;
    [SerializeField] GameObject dawnPostProcessing; 
    [SerializeField] GameObject nightPostProcessing;
    [SerializeField] GameObject dustPostProcessing;

    [HideInInspector] public DAY_TYPE currentDayType;

    void Start()
    {
        SetDayType(DAY_TYPE.DAY);
    }

    public void SetDayType(DAY_TYPE type)
    {
        currentDayType = type;

        dayPostProcessing.SetActive(false);
        dawnPostProcessing.SetActive(false);
        nightPostProcessing.SetActive(false);
        dustPostProcessing.SetActive(false);

        if (type == DAY_TYPE.NIGHT) nightPostProcessing.SetActive(true);
        else if (type == DAY_TYPE.DAWN) dawnPostProcessing.SetActive(true);
        else if (type == DAY_TYPE.DAY) dayPostProcessing.SetActive(true);
        else if (type == DAY_TYPE.DUST) dustPostProcessing.SetActive(true);
    }
}
