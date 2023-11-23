using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// ��ȣ�ۿ� �ִϸ��̼� ���� ���� �Դϴ�.
/// ���� ������ �Ұ����ϸ� ������ �߰��ϼ���.
/// </summary>
public enum INTERACTION_ANIM_TYPE
{
    AXE,            /// ����
    PICKE_AXE,      /// ���
    GROUND_HAND,    /// ���� -> �Ǽ�ä��

    END
}


/// SO Ű�� ����
public enum NatureObjectType
{
    Rock,
    Tree,
    Fern,
    Water,
}



/// <summary>
/// ä�� ������ ��� ������Ʈ�� �ش� Ŭ������ �����ؾ� �մϴ�.
/// </summary>
public class NatureObject : MonoBehaviour
{
    public NatureObjectType Type;
    public int gatherAnimationID;
    public Animation gatherAnimation;

    [SerializeField] List<ItemDrop> gatherItems = new List<ItemDrop>();
    
    void Start()
    {
    }


    #region Event Func
    /// <summary>
    /// �� �Լ��� �ش� �ڿ�������Ʈ�� ä���� ���۵Ǿ��� ��� ȣ��Ǿ�� �մϴ�.
    /// </summary>
    public void EnterGather()
    {
        // ��ȣ�ۿ� ����ڸ� �÷��̾�� �����մϴ�.
        var player = PlayerManager.Instance.Player;

        PlayerStaticStatus.currentGatherObject = this;
        player.AI.ChangeState(PLAYER_STATE.GATHER);
    }


    /// <summary>
    /// �� �Լ��� �ش� �ڿ�������Ʈ�� ä���� �Ϸ�Ǿ��� ��� ȣ��Ǿ�� �մϴ�.
    /// </summary>
    public void ExitGather()
    {
        // ��ȣ�ۿ� ����ڸ� �÷��̾�� �����մϴ�.
        var player = PlayerManager.Instance.Player;

        PlayerStaticStatus.currentGatherObject = null;
        player.AI.ChangeState(PLAYER_STATE.IDLE);

        List<Item> results = new List<Item>();

        gatherItems.ForEach(e =>
        {
            if (Random.Range(0, 1.0f) < e.rate)
            {
                Item item = new Item();
                item.data = e.data;
                item.count = 1;

                results.Add(item);
            }
        });
    }

    /// <summary>
    /// �� �Լ��� �ı������� �ڿ�������Ʈ�� �ı��� ��� ȣ��˴ϴ�. (�ı��� ������ ȣ��)
    /// </summary>
    public void _OnDestroy()
    {
        // ��ȣ�ۿ� ����ڸ� �÷��̾�� �����մϴ�.
    }
    #endregion
}
