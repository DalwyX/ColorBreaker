using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{

    [SerializeField] Toggle startToggle;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider SFXSlider;
    [SerializeField] AudioSource music;

    private void Start()
    {
        startToggle.isOn = (PlayerPrefs.GetInt(GameConstants.StartWaveBool, 0) == 1);
        musicSlider.value = PlayerPrefs.GetFloat(GameConstants.MusicVolume, 1);
        SFXSlider.value = PlayerPrefs.GetFloat(GameConstants.SFXVolume, 0.8f);
    }

    private void Update()
    {
        PlayerPrefs.SetInt(GameConstants.StartWaveBool, startToggle.isOn ? 1 : 0);
        PlayerPrefs.SetFloat(GameConstants.MusicVolume, musicSlider.value);
        PlayerPrefs.SetFloat(GameConstants.SFXVolume, SFXSlider.value);
        music.volume = musicSlider.value;
    }

    public void Default()
    {
        startToggle.isOn = false;
        musicSlider.value = 1;
        SFXSlider.value = 0.8f;
    }

}
