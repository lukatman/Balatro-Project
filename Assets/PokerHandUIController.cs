using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PokerHandUIController : MonoBehaviour
{
    public GameObject recordPrefab;  // 记录的Prefab（每条记录的UI）
    public Transform content;        // ScrollView的Content区域
    public int maxRecords = 4;       // 最多显示4条记录

    // 引用ScrollRect组件用于刷新布局
    public ScrollRect scrollRect;

    private List<PokerHandRecord> pokerHandRecords = new List<PokerHandRecord>();

    private void Awake()
    {
        // 如果未指定ScrollRect，尝试获取
        if (scrollRect == null)
        {
            scrollRect = GetComponentInParent<ScrollRect>();
        }
    }

    private void Start()
    {
        // 在初始化时可以显示一些数据
        DisplayPokerHands();
    }

    // 用于更新记录列表
    public void AddNewRecord(int score, float multiplier, string handName = "")
    {
        // 创建新的记录
        PokerHandRecord newRecord = new PokerHandRecord
        {
            baseScore = score,
            multiplier = multiplier,
            pokerHandName = handName
        };

        // 在列表的顶部插入新的记录
        pokerHandRecords.Insert(0, newRecord);

        // 如果记录数量超过最大值，移除最旧的记录
        if (pokerHandRecords.Count > maxRecords)
        {
            pokerHandRecords.RemoveAt(pokerHandRecords.Count - 1);
        }

        // 显示更新后的记录
        DisplayPokerHands();

        // 确保在下一帧刷新布局
        StartCoroutine(RefreshLayout());
    }

    // 协程用于确保在下一帧刷新布局
    private IEnumerator RefreshLayout()
    {
        yield return null; // 等待下一帧

        // 强制刷新布局
        if (content.GetComponent<LayoutGroup>())
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
        }

        // 刷新ScrollRect
        if (scrollRect != null)
        {
            scrollRect.normalizedPosition = new Vector2(0, 1); // 滚动到顶部
            Canvas.ForceUpdateCanvases();
        }
    }

    // 更新UI显示记录
    private void DisplayPokerHands()
    {
        // 清空现有的记录
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        Debug.Log($"显示 {pokerHandRecords.Count} 条记录");

        // 显示最新的记录
        foreach (PokerHandRecord record in pokerHandRecords)
        {
            GameObject recordObject = Instantiate(recordPrefab, content);

            // 确保记录对象处于激活状态
            recordObject.SetActive(true);

            // 获取所有的TextMeshPro组件
            TextMeshProUGUI[] texts = recordObject.GetComponentsInChildren<TextMeshProUGUI>(true);

            if (texts.Length < 2)
            {
                Debug.LogWarning("记录Prefab中没有足够的TextMeshPro组件");
                continue;
            }

            // 设置文本内容（分数和乘数）
            texts[0].text = record.baseScore.ToString();  // 显示分数
            texts[1].text = record.multiplier.ToString("F1");  // 显示乘数

            // 如果prefab中有第三个文本组件，并且有牌型名称，则显示
            if (texts.Length > 2 && !string.IsNullOrEmpty(record.pokerHandName))
            {
                texts[2].text = record.pokerHandName;
            }

            Debug.Log($"创建记录: 分数={record.baseScore}, 倍数={record.multiplier}, 牌型={record.pokerHandName}");
        }
    }
}

// 数据结构：记录每个手牌的数据
[System.Serializable]
public class PokerHandRecord
{
    public int baseScore;        // 分数
    public float multiplier;     // 乘数
    public string pokerHandName; // 牌型名称
}
