using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class uQuestInteractUI : UIObject
{
    private const int QuestTitleMaxCount = 10;

    [SerializeField] uQuestSlotUI questSlotPrefab;

    List<uQuestSlotUI> questSlots = new();
    List<QuestData> quests = new();

    void Start()
    {
        for(int i = 0; i < QuestTitleMaxCount; ++i)
        {
            var spawn = GameObject.Instantiate(questSlotPrefab);
            spawn.transform.SetParent(questSlotPrefab.transform.parent, true);

            questSlots.Add(spawn.GetComponent<uQuestSlotUI>());
        }

        Destroy(questSlotPrefab);
        questSlots.ForEach(e => e.Close());
        
        Close();
    }

    void Update()
    {
    }

    // ���ѵ� ����Ʈ ����� ���޵Ǿ�� �մϴ�.
    public void Show(List<QuestData> data)
    {
        base.Show();

        questSlots.ForEach(e => e.Close());

        for (int i= 0; i < data.Count; ++i)
        {
            questSlots[i].Show(data[i]);
            questSlots[i].OnClickEvent(() =>
            {
                QuestManager.Instance.StartQuest(data[i]);
            });
        }
    }
}
