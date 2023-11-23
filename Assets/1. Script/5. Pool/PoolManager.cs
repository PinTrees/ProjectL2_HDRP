using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;



#if UNITY_EDITOR
using UnityEditor;
#endif


public class PoolManager : Singleton<PoolManager> 
{
    Dictionary<string, Pool> pools = new();

    public override void Awake()
    {
        BroadcastMessage("InitPoolObject", SendMessageOptions.DontRequireReceiver);
    }

    public void AddPoolObject<T>(PoolObject<T> poolObject) 
    {
        var pool = new Pool(poolObject.Type, poolObject.gameObject, poolObject.PoolMaxCount);

        if (poolObject.StartDisable)
            pool.SetCreateEvent(e => e.SetActive(false));

        pool.Create();

        pools[poolObject.Type] = pool;
    }

    public GameObject GetObject<T>() 
    {
        var type = typeof(T).ToString();
        if (pools.ContainsKey(type))
        {
            return pools[type].Get();
        }
        return null;
    }

    public T GetObjectComponent<T>() where T : MonoBehaviour
    {
        var type = typeof(T).ToString();
        if (pools.ContainsKey(type))
        {
            return pools[type].GetComponent<T>();
        }
        return null;
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(PoolManager))]
public class PoolMgrEditor : Editor
{
    private void OnEnable()
    {
        
    }
}
#endif