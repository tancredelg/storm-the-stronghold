using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioMixer AudioMixer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Load();
    }

    public void SetVolumeLevels(float musicVolume, float sfxVolume)
    {
        AudioMixer.SetFloat("MusicVolume", 20 * Mathf.Log10(musicVolume));
        AudioMixer.SetFloat("SFXVolume", 20 * Mathf.Log10(sfxVolume));

        SettingsData.Instance.MusicVolume = musicVolume;
        SettingsData.Instance.SFXVolume = sfxVolume;
    }

    private void Load()
    {
        SerializationManager.LoadSettings();
        SetVolumeLevels(SettingsData.Instance.MusicVolume, SettingsData.Instance.SFXVolume);
    }
}
