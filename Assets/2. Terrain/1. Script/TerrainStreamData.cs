using System.Collections;
using System.Collections.Generic;
using UnityEngine;




[System.Serializable]
//[CreateAssetMenu(fileName = "TerrainStreams Data", menuName = "Scriptable Object/TerrainStreams Data", order = int.MaxValue)]
public class TerrainStreamsData
{
    [SerializeField]
    public List<TerrainObjectsData> streams = new();
}


[System.Serializable]
public class TerrainObjectData
{
    [SerializeField]
    public Vector3 position;

    [SerializeField]
    public Vector3 rotation;

    [SerializeField]
    public Vector3 scale;
}


[System.Serializable] 
public class TerrainObjectsData
{
    [SerializeField]
    public string ID;

    [SerializeField]
    public List<TerrainObjectData> childs = new();
}