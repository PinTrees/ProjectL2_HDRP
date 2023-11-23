using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 상호작용 애니메이션 순서 정보 입니다.
/// 순서 변경은 불가능하며 하위로 추가하세요.
/// </summary>
public enum INTERACTION_ANIM_TYPE
{
    AXE,            /// 도끼
    PICKE_AXE,      /// 곡괭이
    GROUND_HAND,    /// 앉음 -> 맨손채집

    END
}


/// SO 키로 변경
public enum NatureObjectType
{
    Rock,
    Tree,
    Fern,
    Water,
}



/// <summary>
/// 채집 가능한 모든 오브젝트는 해당 클래스를 소유해야 합니다.
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
    /// 이 함수는 해당 자연오브젝트의 채집이 시작되었을 경우 호출되어야 합니다.
    /// </summary>
    public void EnterGather()
    {
        // 상호작용 대상자를 플레이어로 고정합니다.
        var player = PlayerManager.Instance.Player;

        PlayerStaticStatus.currentGatherObject = this;
        player.AI.ChangeState(PLAYER_STATE.GATHER);
    }


    /// <summary>
    /// 이 함수는 해당 자연오브젝트의 채집이 완료되었을 경우 호출되어야 합니다.
    /// </summary>
    public void ExitGather()
    {
        // 상호작용 대상자를 플레이어로 고정합니다.
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
    /// 이 함수는 파괴가능한 자연오브젝트가 파괴될 경우 호출됩니다. (파괴된 시점에 호출)
    /// </summary>
    public void _OnDestroy()
    {
        // 상호작용 대상자를 플레이어로 고정합니다.
    }
    #endregion
}
