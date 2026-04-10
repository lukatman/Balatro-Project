using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RuninfoCtrl : MonoBehaviour
{
    public Button runInfoButton;   // Run Info按钮
    public Button backButton;      // Back按钮
    public GameObject runInfoPanel;  // Run Info面板


    // Start is called before the first frame update
    void Start()
    {
        // 初始化按钮点击事件
        runInfoButton.onClick.AddListener(ShowRunInfoPanel);
        backButton.onClick.AddListener(HideRunInfoPanel);

        // 初始时隐藏RunInfo面板
        runInfoPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 显示RunInfo面板
    void ShowRunInfoPanel()
    {
        runInfoPanel.SetActive(true);  // 显示RunInfo面板
    }

    // 隐藏RunInfo面板
    void HideRunInfoPanel()
    {
        runInfoPanel.SetActive(false);  // 隐藏RunInfo面板
    }
}
