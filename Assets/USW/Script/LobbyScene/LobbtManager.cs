using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    [Header("UI Panels")] [SerializeField] private GameObject settingPanel;

    
    [Header("BGM")]
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private Slider bgmSlider;
    
    [Header("SFX")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private Slider sfxSlider;
    
    [SerializeField] private SimpleFade simpleFade;
    //[Header("그래픽 품질")]
    //[SerializeField] private TMP_Dropdown qualityDropdown;
    
    
    private void Start()
    {
        if (settingPanel != null)
            settingPanel.SetActive(false);

        if (sfxAudioSource != null && sfxSlider != null)
        {
            sfxSlider.value = sfxAudioSource.volume;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        if (bgmAudioSource != null && bgmSlider != null)
        {
            bgmSlider.value = bgmAudioSource.volume;
            bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        }

        //if (qualityDropdown != null)
        //{
        //    qualityDropdown.value = QualitySettings.GetQualityLevel();
        //    qualityDropdown.onValueChanged.AddListener(SetQualityLevel);
        //}
    }

    /// <summary>
    /// UI 버튼 
    /// </summary>
    public void OpenSettingPanel()
    {
        if (settingPanel != null)
            settingPanel.SetActive(true);
    }

    public void CloseSettingPanel()
    {
        if (settingPanel != null)
            settingPanel.SetActive(false);
    }

    public void OnMatchGameClick()
    {
        simpleFade.FadeAndLoadScene("InGameScene");
    }

    public void OnGachaClick()
    {
        SceneManager.LoadScene("GachaScene");
    }

    /// <summary>
    /// 볼륨 및 품질 제어 
    /// </summary>
    /// <param name="level"></param>

    public void SetSFXVolume(float volume)
    {
        if (sfxAudioSource != null)
            sfxAudioSource.volume = volume;
    }

    public void SetBGMVolume(float volume)
    {
        if (bgmAudioSource != null)
            bgmAudioSource.volume = volume;
    }
    
    public void SetQualityLevel(int level)
    {
        QualitySettings.SetQualityLevel(level);
    }
}
