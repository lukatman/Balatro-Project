using UnityEngine;
using UnityEngine.UI;

public class ButtonSoundPlayer : MonoBehaviour
{
    // 背景音乐的 AudioSource
    public AudioSource backgroundMusic;

    // 游戏音效的 AudioSource
    public AudioSource soundEffects;

    void Start()
    {
        // 直接播放背景音乐（每次都会播放，适合用于不跨场景的音乐）
        if (backgroundMusic != null)
        {
            backgroundMusic.playOnAwake = false;
            backgroundMusic.loop = true;
            backgroundMusic.priority = 256;
            backgroundMusic.Play();
            Debug.Log("Background music started.");
        }
        else
        {
            Debug.LogWarning("Background music AudioSource is missing!");
        }

        // 设置游戏音效
        if (soundEffects != null)
        {
            soundEffects.playOnAwake = false;
            soundEffects.volume = 0.8f;
            soundEffects.priority = 128;
        }
    }

    // 用于触发其他游戏音效的方法
    public void PlaySoundEffect(AudioClip sound)
    {
        if (soundEffects != null && sound != null)
        {
            soundEffects.PlayOneShot(sound);
        }
    }
}