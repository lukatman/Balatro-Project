using UnityEngine;

public class CardInfoChecker : MonoBehaviour
{
    // 你可以在这里放置所有卡片对象的引用（例如，在 Inspector 中手动拖放）
    public GameObject[] cards; // 可以绑定卡片对象的数组

    void Start()
    {
        foreach (var card in cards)
        {
            // 检查每个卡片对象是否有 CardInfo 组件
            if (card.GetComponent<CardInfo>() == null)
            {
                Debug.LogWarning("CardInfo component missing on card: " + card.name);
                // 如果需要，你可以在这里自动添加 CardInfo 组件：
                // card.AddComponent<CardInfo>();
            }
        }
    }
}
