using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorExtentions
{
    public static bool IsAxisUsedZ(Vector3 axis)
    {
        return Mathf.Approximately(axis.x, 0) && Mathf.Approximately(axis.y, 0) && !Mathf.Approximately(axis.z, 0);
    }
    public static bool IsAxisUsedXY(Vector3 axis)
    {
        return !Mathf.Approximately(axis.x, 0) && !Mathf.Approximately(axis.y, 0) && Mathf.Approximately(axis.z, 0);
    }
    public static bool IsAxisUsedAll(Vector3 axis)
    {
        return !Mathf.Approximately(axis.x, 0) && !Mathf.Approximately(axis.y, 0) && !Mathf.Approximately(axis.z, 0);
    }
}
