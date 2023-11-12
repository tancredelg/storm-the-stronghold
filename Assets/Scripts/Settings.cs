using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Settings : MonoBehaviour
{   
    private Slider _musicVolumeSlider, _SFXVolumeSlider;
    private TMP_Dropdown _graphicsDropdown;

    private void Awake()
    {
        _musicVolumeSlider = GameObject.Find("MusicVolume").GetComponentInChildren<Slider>();
        _SFXVolumeSlider = GameObject.Find("SoundsVolume").GetComponentInChildren<Slider>();
        _graphicsDropdown = GameObject.Find("GraphicsPreset").GetComponentInChildren<TMP_Dropdown>();

        _musicVolumeSlider.value = SettingsData.Instance.MusicVolume;
        _SFXVolumeSlider.value = SettingsData.Instance.SFXVolume;
        _graphicsDropdown.value = SettingsData.Instance.QualityLevel;
    }

    public void UpdateSettings()
    {
        QualitySettings.SetQualityLevel(_graphicsDropdown.value);
        AudioManager.Instance.SetVolumeLevels(_musicVolumeSlider.value, _SFXVolumeSlider.value);
        SettingsData.Instance.QualityLevel = QualitySettings.GetQualityLevel();
        SerializationManager.SaveSettings();
    }
}
