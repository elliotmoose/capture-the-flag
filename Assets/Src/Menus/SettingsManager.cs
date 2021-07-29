using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;
    public string username = "";

    float minMouseSensitivity = 2;
    float maxMouseSensitivity = 30;
    public float mouseSensitivity = 6;
    public float mouseSensitivityPercentage => (mouseSensitivity - minMouseSensitivity)/(maxMouseSensitivity - minMouseSensitivity); 
    float minVolume = 2;
    float maxVolume = 12;
    public float musicVolume = 6;
    public float musicVolumePercentage => (musicVolume - minVolume)/(maxVolume - minVolume); 
    float minVFXV = 2;
    public float vfxVolume = 6;
    public float vfxVolumePercentage => (vfxVolume - minVolume)/(maxVolume - minVolume); 
    
    public GameObject musicVolumeSlider;
    public GameObject vfxVolumeSlider;
    public GameObject mouseSensSlider;  

    void Awake()
    {
        Instance = this;
        LoadSettings();
        UpdateSliderTextValues();
    }

    private void LoadSettings() {
        username = PlayerPrefs.GetString("username");
        mouseSensitivity = PlayerPrefs.GetFloat("mouse_sensitivity") == 0 ? mouseSensitivity : PlayerPrefs.GetFloat("mouse_sensitivity");
        musicVolume = PlayerPrefs.GetFloat("music_volume") == 0 ? musicVolume : PlayerPrefs.GetFloat("music_volume");
        vfxVolume = PlayerPrefs.GetFloat("vfx_volume") == 0 ? vfxVolume : PlayerPrefs.GetFloat("vfx_volume");
    }
    public void SetSetting(string setting, float percentage) {
        switch (setting)
        {
            case "music":
                musicVolume = Mathf.Lerp(minVolume, maxVolume, percentage);                
                PlayerPrefs.SetFloat("music_volume", musicVolume);
                break;
            case "vfx":
                vfxVolume = Mathf.Lerp(minVolume, maxVolume, percentage);                
                PlayerPrefs.SetFloat("vfx_volume", vfxVolume);
                break;
            case "mouse_sensitivity":
                mouseSensitivity = Mathf.Lerp(minMouseSensitivity, maxMouseSensitivity, percentage);                
                PlayerPrefs.SetFloat("mouse_sensitivity", mouseSensitivity);
                break;
            default:
                break;
        }
    }

    public void OnSliderChange(string setting) {
        switch (setting)
        {
            case "music":
                SettingsManager.Instance.SetSetting(setting, musicVolumeSlider.GetComponentInChildren<Slider>().value);
                musicVolumeSlider.GetComponentInChildren<TextMeshProUGUI>().text = $"Music Volume: {SettingsManager.Instance.musicVolume:0.0}";
                break;
            case "vfx":
                SettingsManager.Instance.SetSetting(setting, vfxVolumeSlider.GetComponentInChildren<Slider>().value);
                vfxVolumeSlider.GetComponentInChildren<TextMeshProUGUI>().text = $"VFX Volume: {SettingsManager.Instance.vfxVolume:0.0}";
                break;
            case "mouse_sensitivity":
                SettingsManager.Instance.SetSetting(setting, mouseSensSlider.GetComponentInChildren<Slider>().value);
                mouseSensSlider.GetComponentInChildren<TextMeshProUGUI>().text = $"Mouse Sensitivity: {SettingsManager.Instance.mouseSensitivity:0.0}";
                break;
            default:
                break;
        }
    }

    private void UpdateSliderTextValues() {
        musicVolumeSlider.GetComponentInChildren<TextMeshProUGUI>().text = $"Music Volume: {SettingsManager.Instance.musicVolume:0.0}";
        vfxVolumeSlider.GetComponentInChildren<TextMeshProUGUI>().text = $"VFX Volume: {SettingsManager.Instance.vfxVolume:0.0}";
        mouseSensSlider.GetComponentInChildren<TextMeshProUGUI>().text = $"Mouse Sensitivity: {SettingsManager.Instance.mouseSensitivity:0.0}";
        musicVolumeSlider.GetComponentInChildren<Slider>().value = SettingsManager.Instance.musicVolumePercentage;
        vfxVolumeSlider.GetComponentInChildren<Slider>().value = SettingsManager.Instance.vfxVolumePercentage;
        mouseSensSlider.GetComponentInChildren<Slider>().value = SettingsManager.Instance.mouseSensitivityPercentage;
    }
}
