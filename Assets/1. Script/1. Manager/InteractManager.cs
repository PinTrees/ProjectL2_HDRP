using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractManager : Singleton<InteractManager>
{
    const int CHUNK_SIZE = 8;
    (int, int)[] DIR = {
              (0, 0)
            , (0, 1)
            , (1, 0)
            , (0, -1)
            , (-1, 0)
            , (1, 1) 
            , (-1, -1)
            , (-1, 1)
            , (1, -1)
    };

    private Dictionary<(int, int), List<CanInteract>> _objects = new Dictionary<(int, int), List<CanInteract>>();

    [HideInInspector] 
    public CanInteract CurrentInteract;

    // ��� �޼ҵ� ����
    private void Start()
    {
    }

    public void AddInteraction(CanInteract interaction)
    {
        int x = (int)(interaction.transform.position.x / CHUNK_SIZE);
        int z = (int)(interaction.transform.position.z / CHUNK_SIZE);

        if (!_objects.ContainsKey((x, z)))
        {
            _objects[(x, z)] = new List<CanInteract>();
        }

        _objects[(x, z)].Add(interaction);
    }

    private void Update()
    {
        // UpdateNearInteractionObject();
    }



    // �� �Լ��� ���� ����� ä�� ���� ������Ʈ�� Ȯ���ϰ� ��ȯ�մϴ�. ���� ��� NUll ��ȯ
    public CanInteract FindNearObject(Transform target)
    {
        int x = (int)(target.position.x / CHUNK_SIZE);
        int z = (int)(target.position.z / CHUNK_SIZE);

        CanInteract result = null;

        float lenght = 999;

        for (int i = 0; i < DIR.Length; i++)
        {
            if (!_objects.ContainsKey((x + DIR[i].Item1, z + DIR[i].Item2))) continue;

            _objects[(x + DIR[i].Item1, z + DIR[i].Item2)].ForEach(obj =>
            {
                var distance = Vector3.Distance(obj.transform.position, target.position);
                if (distance <= obj.InteractionRange && distance < lenght)
                {
                    lenght = distance;
                    result = obj;
                }
            });
        }

        return result;
    }
}
