using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : MonoBehaviour
{
    [SerializeField] Slider sfxSlider;
    [SerializeField] Slider bgmSlider;
    [SerializeField] Button gameExitButton;
    [SerializeField] Button infoButton;
    [SerializeField] Button allMuteButton;

    private void OnEnable()
    {
        UpdateBGMSlider(bgmSlider.value);
        UpdateSFXSlider(sfxSlider.value);

        sfxSlider.onValueChanged.AddListener(UpdateSFXSlider);
        bgmSlider.onValueChanged.AddListener(UpdateBGMSlider);

        gameExitButton.onClick.AddListener(GameExit);
        //infoButton.onClick.AddListener(Info);
        allMuteButton.onClick.AddListener(AllMute);
    }

    private void OnDisable()
    {
        sfxSlider.onValueChanged.RemoveListener(UpdateSFXSlider);
        bgmSlider.onValueChanged.RemoveListener(UpdateBGMSlider);

        gameExitButton.onClick.RemoveListener(GameExit);
        //infoButton.onClick.RemoveListener(Info);
        allMuteButton.onClick.RemoveListener(AllMute);
    }

    public void UpdateSFXSlider(float sliderValue)
    {
        //Mathf.Log10(sliderValue) * 20; // 오디오 연결
    }

    public void UpdateBGMSlider(float sliderValue)
    {
        //Mathf.Log10(sliderValue) * 20; // 오디오 연결
    }

    public void Close() => gameObject.SetActive(false);

    private void GameExit()
    {
        // 게임 나가기 로직
    }

    private void Info()
    {

    }

    private void AllMute()
    {
        // AudioManager AllMute 로직
    }
}
