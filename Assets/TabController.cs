using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    public Button pokerHandsButton; // Poker Hands按钮
    public Button blindsButton;     // Blinds按钮
    public Button vouchersButton;   // Vouchers按钮

    public GameObject pokerHandsPanel;  // Poker Hands面板
    public GameObject blindsPanel;      // Blinds面板
    public GameObject vouchersPanel;    // Vouchers面板

    public RectTransform indicator;    // 红色倒三角指示器（使用RectTransform）

    private Vector2 pokerHandsPosition;  // 默认位置（Poker Hands按钮位置）
    private Vector2 blindsPosition;      // Blinds按钮位置
    private Vector2 vouchersPosition;    // Vouchers按钮位置

    // Start is called before the first frame update
    void Start()
    {
        // 初始化按钮点击事件
        pokerHandsButton.onClick.AddListener(() => ShowPanel("PokerHands"));
        blindsButton.onClick.AddListener(() => ShowPanel("Blinds"));
        vouchersButton.onClick.AddListener(() => ShowPanel("Vouchers"));

        float buttonHeight = pokerHandsButton.GetComponent<RectTransform>().rect.height;
        float extraOffset = 10f; // 你可以调整这个额外的偏移量，增加指示器与按钮之间的距离

        // 获取倒三角指示器的初始位置
        pokerHandsPosition = pokerHandsButton.GetComponent<RectTransform>().anchoredPosition;
        blindsPosition = blindsButton.GetComponent<RectTransform>().anchoredPosition;
        vouchersPosition = vouchersButton.GetComponent<RectTransform>().anchoredPosition;

        pokerHandsPosition.y += buttonHeight / 2 + indicator.rect.height / 2 + extraOffset;  // 设置指示器在PokerHands按钮上方
        blindsPosition.y += buttonHeight / 2 + indicator.rect.height / 2 + extraOffset;      // 设置指示器在Blinds按钮上方
        vouchersPosition.y += buttonHeight / 2 + indicator.rect.height / 2 + extraOffset;    // 设置指示器在Vouchers按钮上方

        // 默认显示Poker Hands面板
        ShowPanel("PokerHands");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 切换面板
    private void ShowPanel(string panelName)
    {
        // 隐藏所有面板
        pokerHandsPanel.SetActive(false);
        blindsPanel.SetActive(false);
        vouchersPanel.SetActive(false);

        // 根据按钮显示对应面板
        if (panelName == "PokerHands")
        {
            pokerHandsPanel.SetActive(true);
            MoveIndicator(pokerHandsPosition);  // 移动指示器
        }
        else if (panelName == "Blinds")
        {
            blindsPanel.SetActive(true);
            MoveIndicator(blindsPosition);  // 移动指示器
        }
        else if (panelName == "Vouchers")
        {
            vouchersPanel.SetActive(true);
            MoveIndicator(vouchersPosition);  // 移动指示器
        }
    }

    // 移动倒三角指示器
    private void MoveIndicator(Vector2 targetPosition)
    {
        indicator.anchoredPosition = targetPosition;
    }
}
