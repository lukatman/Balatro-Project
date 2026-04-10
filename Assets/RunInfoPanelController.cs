using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RunInfoPanelController : MonoBehaviour
{
    public GameObject pokerHandsContent;
    public GameObject blindsContent;
    public GameObject vouchersContent;

    public Button pokerHandsButton;
    public Button blindsButton;
    public Button vouchersButton;
    public Button backButton;

    void Start()
    {
        // 默认显示Poker Hands
        ShowPokerHands();

        // 添加按钮事件
        pokerHandsButton.onClick.AddListener(ShowPokerHands);
        blindsButton.onClick.AddListener(ShowBlinds);
        vouchersButton.onClick.AddListener(ShowVouchers);
        backButton.onClick.AddListener(HidePanel);
    }

    void ShowPokerHands()
    {
        pokerHandsContent.SetActive(true);
        blindsContent.SetActive(false);
        vouchersContent.SetActive(false);

        // 更新按钮高亮状态
        UpdateButtonHighlight(pokerHandsButton);
    }

    void ShowBlinds()
    {
        pokerHandsContent.SetActive(false);
        blindsContent.SetActive(true);
        vouchersContent.SetActive(false);

        // 更新按钮高亮状态
        UpdateButtonHighlight(blindsButton);
    }

    void ShowVouchers()
    {
        pokerHandsContent.SetActive(false);
        blindsContent.SetActive(false);
        vouchersContent.SetActive(true);

        // 更新按钮高亮状态
        UpdateButtonHighlight(vouchersButton);
    }

    void UpdateButtonHighlight(Button activeButton)
    {
        // 重置所有按钮颜色
        ColorBlock cb = pokerHandsButton.colors;
        cb.normalColor = new Color(0.8f, 0.2f, 0.2f); // 普通红色
        pokerHandsButton.colors = cb;
        blindsButton.colors = cb;
        vouchersButton.colors = cb;

        // 高亮活动按钮
        cb = activeButton.colors;
        cb.normalColor = new Color(1f, 0.3f, 0.3f); // 亮红色
        activeButton.colors = cb;
    }

    void HidePanel()
    {
        gameObject.SetActive(false);
    }
}