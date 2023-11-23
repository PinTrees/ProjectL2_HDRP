using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NpcManager : Singleton<NpcManager>
{
    // 제거예정
    List<Npc> NpcList = new();

    Dictionary<string, Npc> npcs = new();

    private void Start()
    {
        NpcList = GameObject.FindObjectsOfType<Npc>().ToList();  
    }

    public void AddNpcForObject(Npc target) { npcs[target.UID] = target; }
    public void RemoveNpcForObject(Npc target) { if(npcs.ContainsKey(target.UID)) npcs.Remove(target.UID); }

    public List<Npc> GetNearNpc(Vector3 pos, float range)
    {
        List<Npc> result_list = new List<Npc>();

        NpcList.ForEach(e =>
        {
            if(Vector3.Distance(pos, e.transform.position) < range)
                result_list.Add(e);
        });

        return result_list;
    }

    public List<Npc> GetNearKillNpc(Vector3 pos, float range, float damage)
    {
        List<Npc> result_list = new List<Npc>();

        NpcList.ForEach(e =>
        {
            if(e.AI.healthController.currentHealth < damage
            && e.AI.transform.gameObject.activeSelf
            && e.AI.CurrentState.IsNotEquals(NPC_STATE_TYPE.DEATH))
            {
                if (Vector3.Distance(pos, e.transform.position) < range)
                    result_list.Add(e);
            }
        });

        return result_list;
    }

    public List<Npc> GetNearBackKillNpc(Transform baseTransform, float range, float damage)
    {
        float detectionAngle = 100;

        List<Npc> result_list = new List<Npc>();

        NpcList.ForEach(e =>
        {
            if (e.AI.healthController.currentHealth < damage
            && e.AI.CurrentState.IsNotEquals(NPC_STATE_TYPE.DEATH))
            {
                if (Vector3.Distance(baseTransform.position, e.transform.position) < range)
                {
                    Vector3 toTarget = e.AI.transform.position - baseTransform.position;
                    toTarget.y = 0.0f;

                    Vector3 forward = baseTransform.forward;
                    forward.y = 0.0f;

                    float angle = Vector3.Angle(forward, toTarget);

                    if(angle <= detectionAngle * 0.5f)
                    {
                        result_list.Add(e);
                    }
                }
            }
        });

        return result_list;
    }
}
