using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.VisualScripting;
using System;

public class MenuController : MonoBehaviour
{
    [Header("Volume Settings")]
    [SerializeField] private TMP_Text volumeTextValue = null;
    [SerializeField] private Slider volumeSlider = null;
    [SerializeField] private float defaultVolume = 1f;

    [SerializeField] private GameObject confirmationPromt = null;

    [Header("Gameplay Settings")]
    [SerializeField] private TMP_Text MouseSenTextValue = null;
    [SerializeField] private Slider MouseSenSlider = null;
    [SerializeField] private int defaultMouseSen = 1;
    public int mainSens = 4;

    [Header("Gameplay Settings")]
    [SerializeField] private Toggle InvertYToggle = null;

    [Header("Graphics Settings")]
    [SerializeField] private Slider brightnessSlider = null;
    [SerializeField] private TMP_Text brightnessTextValue = null;
    [SerializeField] private float defaultBrightness = 1;

    [Space(10)]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Resolution Dropdowns")]
    public TMP_Dropdown resolutionDropdown;
    private Resolution[] resolutions;

    private int _qualityLevel;
    private bool _isFullScreen;
    private float _brightnessLevel;

    public void Start()
    {
        // Přidání listeners pro slidery
        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(SetVolume);
        
        if (MouseSenSlider != null)
            MouseSenSlider.onValueChanged.AddListener(SetSens);
        
        if (brightnessSlider != null)
            brightnessSlider.onValueChanged.AddListener(SetBrightness);
        
        // Načíst uloženou hlasitost z PlayerPrefs (nebo použít default)
        // Pokud je 0 nebo nebylo nikdy uloženo, použij defaultVolume
        float savedVolume = PlayerPrefs.GetFloat("masterVolume", defaultVolume);
        if (savedVolume <= 0f) savedVolume = defaultVolume;
        AudioListener.volume = savedVolume;
        if (volumeSlider != null) volumeSlider.value = savedVolume;
        if (volumeTextValue != null) volumeTextValue.text = savedVolume.ToString("0.0");
        
        Debug.Log($"MenuController: Hlasitost načtena = {savedVolume}");

        if (resolutionDropdown != null)
        {
            resolutions = Screen.resolutions;
            resolutionDropdown.ClearOptions();

            List<string> options = new List<string>();
            int currentResolutionIndex = 0;

            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + " x " + resolutions[i].height;
                options.Add(option);

                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = i;
                }
            }

            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = currentResolutionIndex;
            resolutionDropdown.RefreshShownValue();
        }
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        if (volumeTextValue != null)
            volumeTextValue.text = volume.ToString("0.0");
    }

    public void VolumeApply()
    {
        PlayerPrefs.SetFloat("masterVolume", AudioListener.volume);
        StartCoroutine(ConfirmationBox());
    }

    public void SetSens(float sensitivity)
    {
        mainSens = (int)sensitivity;
        MouseSenTextValue.text = sensitivity.ToString("0");
    }

    public void GameplayApply()
    {
        if(InvertYToggle.isOn)
        {
            PlayerPrefs.SetInt("masterInvertY", 1);
            //invert Y
        }
        else
        {
            PlayerPrefs.SetInt("masterInvertY", 0);
            //not invert Y
        }
        PlayerPrefs.SetInt("mainSens", mainSens);
        StartCoroutine(ConfirmationBox());
    }

    public void SetBrightness(float brighrness)
    {
        _brightnessLevel = brighrness;
        brightnessTextValue.text = brighrness.ToString("0.0");
    }

    public void SetFullscreen(bool isFullscreen)
    {
        _isFullScreen = isFullscreen;
    }

    public void SetQuality(int qualityIndex)
    {
        _qualityLevel = qualityIndex;
    }

    public void GraphicsApply()
    {
        PlayerPrefs.SetFloat("masterBrightness", _brightnessLevel);
        
        PlayerPrefs.SetInt("masterQuality", _qualityLevel);
        QualitySettings.SetQualityLevel(_qualityLevel);
        
        PlayerPrefs.SetInt("masterFullscreen", _isFullScreen ? 1 : 0);
        Screen.fullScreen = _isFullScreen;
        
        StartCoroutine(ConfirmationBox());
    }

    public void ResetButton(string MenuType)
    {
        if(MenuType == "Graphics")
        {
            //Reset Brightness to default
            _brightnessLevel = defaultBrightness;
            brightnessSlider.value = defaultBrightness;
            brightnessTextValue.text = defaultBrightness.ToString("0.0");
            
            qualityDropdown.value = 0;
            QualitySettings.SetQualityLevel(0);

            fullscreenToggle.isOn = false;
            Screen.fullScreen = false;

            Resolution currentResolution = Screen.currentResolution;
            Screen.SetResolution(currentResolution.width, currentResolution.height, Screen.fullScreen);
            resolutionDropdown.value = resolutionDropdown.options.FindIndex(option => option.text == currentResolution.width + " x " + currentResolution.height);

            GraphicsApply();
        }

        if(MenuType == "Audio")
        {
            AudioListener.volume = defaultVolume;
            volumeSlider.value = defaultVolume;
            volumeTextValue.text = defaultVolume.ToString("0.0");
            VolumeApply();
        }

        if (MenuType == "Gameplay")
        {
            mainSens = defaultMouseSen;
            MouseSenSlider.value = defaultMouseSen;
            MouseSenTextValue.text = defaultMouseSen.ToString("0");
            InvertYToggle.isOn = false;
            GameplayApply();
        }
    }

    public IEnumerator ConfirmationBox()
    {
        confirmationPromt.SetActive(true);
        yield return new WaitForSeconds(2);
        confirmationPromt.SetActive(false);
    }

    [Header ("Start Game")]
    public string _startGameScene;
    
    [Header("Statistics Panel")]
    [SerializeField] private GameObject statisticsPanel = null;
    
    public void StartGame()
    {
        // Ujistit se, že vše je resetováno
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (string.IsNullOrEmpty(_startGameScene))
        {
            Debug.LogError("MenuController: _startGameScene není nastaven! Nastav název scény v Inspektoru.");
            return;
        }
        
        Debug.Log($"MenuController: Spuštění hry - načítám scénu: {_startGameScene}");
        SceneManager.LoadScene(_startGameScene);
    }

    public void ExitButton()
    {
        Application.Quit();
    }
    
    /// <summary>
    /// Otevřít panel se statistikami
    /// </summary>
    public void OpenStatistics()
    {
        if (statisticsPanel != null)
        {
            statisticsPanel.SetActive(true);
            
            // Aktualizovat statistiky při otevření
            MenuStatisticsPanel panel = statisticsPanel.GetComponent<MenuStatisticsPanel>();
            if (panel != null)
            {
                panel.RefreshStatistics();
            }
        }
    }
    
    /// <summary>
    /// Zavřít panel se statistikami
    /// </summary>
    public void CloseStatistics()
    {
        if (statisticsPanel != null)
        {
            statisticsPanel.SetActive(false);
        }
    }
}

internal class HAttribute : Attribute
{
}