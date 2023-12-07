using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class GameObjectExtensions 
{
#if UNITY_EDITOR
    public static GameObject InstantiatePrefab(GameObject target)
    {
        return (GameObject)PrefabUtility.InstantiatePrefab(target);
    }

    public static GameObject InstantiatePrefab(GameObject target, Transform parent)
    {
        var spawn = (GameObject)PrefabUtility.InstantiatePrefab(target);
        spawn.transform.parent = parent;
        return spawn;
    }
#endif

    public static GameObject InstantiateLastLOD(GameObject target, Transform parent=null)
    {
        var lod = target.GetComponent<LODGroup>();

        if (lod == null) return null;

        try
        {
            var impostor = lod.GetLODs().Last().renderers.First().gameObject;
            var spawn = GameObject.Instantiate(impostor);
            spawn.transform.parent = parent;
            return spawn;
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public static List<GameObject> InstantiateList(GameObject target, int count, Transform parent, Action<GameObject> spawnAction)
    {
        List<GameObject> spawns = new();
        
        for(int i = 0; i < count; ++i)
        {
            var go = GameObject.Instantiate(target);
            go.transform.SetParent(parent, true);

            spawnAction(go);
            spawns.Add(go);
        }

        return spawns;
    }

    public static List<GameObject> GetChildrenObject(GameObject target)
    {
        List<GameObject> gameObjects = new();
        
        for(int i = 0; i < target.transform.childCount; ++i)
        {
            gameObjects.Add(target.transform.GetChild(i).gameObject);
        }

        return gameObjects;
    }
}
