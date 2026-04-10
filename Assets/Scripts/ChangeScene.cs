using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    // 背景音乐的 AudioSource
    public AudioSource backgroundMusic;

    // 游戏音效的 AudioSource
    public AudioSource soundEffects;

    // 点击进入游戏按钮时调用此方法
    public void OnPlayButtonClicked()
    {
        if (backgroundMusic != null)
        {
            backgroundMusic.priority = 256;
            backgroundMusic.playOnAwake = false;
            backgroundMusic.loop = true;

            if (!backgroundMusic.isPlaying)
            {
                backgroundMusic.Play();
                Debug.Log("背景音乐播放中。");
            }
        }
        else
        {
            Debug.LogWarning("未设置背景音乐 AudioSource！");
        }

        // 切换到游戏主场景
        SceneManager.LoadScene(1);
    }
}
