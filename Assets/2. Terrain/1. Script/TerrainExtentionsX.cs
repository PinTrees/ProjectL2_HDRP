using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TerrainExtentionsX
{
    public static TerrainCollider GetNearTerrainCollider(Transform target)
    {
        var terrain_colliders = GameObject.FindObjectsOfType<TerrainCollider>();

        foreach (var t in terrain_colliders)
        {
            if (target.position.x > t.transform.position.x && target.position.z > t.transform.position.z
                && target.position.x < t.transform.position.x + 4000 && target.position.z < t.transform.position.z + 4000)
            {
                return t;
            }
        }

        return null;
    }
    public static TerrainCollider GetNearTerrainCollider(Vector3 position)
    {
        var terrain_colliders = GameObject.FindObjectsOfType<TerrainCollider>();

        foreach (var t in terrain_colliders)
        {
            if (position.x > t.transform.position.x && position.z > t.transform.position.z
                && position.x < t.transform.position.x + 4000 && position.z < t.transform.position.z + 4000)
            {
                return t;
            }
        }

        return null;
    }
}