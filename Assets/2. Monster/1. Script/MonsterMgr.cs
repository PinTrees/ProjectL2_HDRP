using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterMgr : Singleton<MonsterMgr>
{
    private List<Monster> _monster_list = new List<Monster>();

    void Start()
    {
    }

    public void AddMonster(Monster monster)
    {
        _monster_list.Add(monster);
    }

    public Monster GetNearMonster()
    {
        return _monster_list.First();
    }

    void Update()
    {
        
    }
}
