// 게임 내 메뉴 (일시정지 · 설정 · 인트로 재생 여부 저장)

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System;

public class GameMenu : MonoBehaviour
{
    public static GameMenu Instance;

    public enum GameState { Initializing, Running, Paused, GameOver }

    [Header("Game State")]
    public GameState currentState = GameState.Initializing;
    public bool isIntroPlayed = false;
    [Header("UI References")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;
    public GameObject startMenuCanvas;

    [Header("Settings UI Elements")]
    public Slider masterVolumeSlider;
    public TMP_InputField volumeInputField;
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown screenModeDropdown;
    public Toggle vSyncToggle;
    public TMP_Dropdown fpsDropdown;

    private int lastWidth, lastHeight;
    private CanvasGroup pauseMenuCanvasGroup;
    private Coroutine fadeCoroutine;
    private List<Resolution> resolutions = new List<Resolution>();
    private float tempVolume;

    private void Awake()
    {
        // 원본 방식: 나 자신만 보호해서 씬 넘어가기
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // GameMenu만 다음 씬으로 가져감

        SetupPauseMenu();
        InitResolutionList();
        LoadSettings();
        SetGameState(GameState.Running);
        
        lastWidth = Screen.width;
        lastHeight = Screen.height;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (this == Instance) 
        {
            StartCoroutine(DeferredSetup());

            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            
            SetGameState(GameState.Running);
        }
    }

    private IEnumerator DeferredSetup()
    {
        yield return new WaitForEndOfFrame();
        SetupPauseMenu();
    }

    // 🌟 1. 기존 페이드 + 일시정지 메뉴 로직 완벽 복구
    public void SetupPauseMenu()
    {
        if (pauseMenuPanel != null && pauseMenuPanel.gameObject != null) return;

        GameObject found = GameObject.Find("PauseMenu");
        if (found != null)
        {
            pauseMenuPanel = found;
        }
        else
        {
            Transform t = transform.Find("PauseMenu");
            if (t != null) pauseMenuPanel = t.gameObject;
        }

        if (pauseMenuPanel != null)
        {
            pauseMenuCanvasGroup = pauseMenuPanel.GetComponent<CanvasGroup>();
            if (pauseMenuCanvasGroup == null)
                pauseMenuCanvasGroup = pauseMenuPanel.AddComponent<CanvasGroup>();
        }
    }

    private void Update()
    {
        // ESC 눌렀을 때 작동하는 기존 로직
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Running)
                SetGameState(GameState.Paused);
            else if (currentState == GameState.Paused)
            {
                if (settingsPanel != null && settingsPanel.activeSelf)
                    CancelAndCloseSettings();
                else
                    ResumeGameWithFade();
            }
        }
        
        if (Screen.fullScreenMode == FullScreenMode.Windowed)
        {
            if (Screen.width != lastWidth || Screen.height != lastHeight)
            {
                lastWidth = Screen.width;
                lastHeight = Screen.height;
                PlayerPrefs.SetInt("WindowWidth", lastWidth);
                PlayerPrefs.SetInt("WindowHeight", lastHeight);
            }
        }
    }

    public void SetGameState(GameState newState)
    {
        if (currentState == newState && newState != GameState.Initializing) return;
        currentState = newState;

        switch (newState)
        {
            case GameState.Running:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                if (pauseMenuPanel != null)
                {
                    pauseMenuPanel.SetActive(true);
                    if (pauseMenuCanvasGroup != null) pauseMenuCanvasGroup.alpha = 1f;
                }
                break;
            case GameState.GameOver:
                Time.timeScale = 0f;
                break;
        }
    }

    // 🌟 2. 설정창 열고 닫기 (시작화면 & 인게임 공용)
    public void OpenSettingsFromMainMenu()
    {
        tempVolume = AudioListener.volume;
        if (settingsPanel != null) settingsPanel.SetActive(true);
        // if (startMenuCanvas != null) startMenuCanvas.SetActive(false); 
    }

    public void OpenSettings()
    {
        tempVolume = AudioListener.volume;
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
    }

    public void SaveAndCloseSettings()
    {
        PlayerPrefs.SetFloat("Volume", AudioListener.volume);
        PlayerPrefs.SetInt("VSync", QualitySettings.vSyncCount);
        PlayerPrefs.SetInt("WindowWidth", Screen.width);
        PlayerPrefs.SetInt("WindowHeight", Screen.height);
        PlayerPrefs.Save();
        CloseSettingsPanel();
    }

    public void CancelAndCloseSettings()
    {
        AudioListener.volume = tempVolume;
        LoadSettings();
        CloseSettingsPanel();
    }

    private void CloseSettingsPanel()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);

        if (currentState == GameState.Paused)
        {
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        }
        else
        {
            if (startMenuCanvas != null) startMenuCanvas.SetActive(true);
        }
    }

    // 🌟 3. 설정 세부 세팅 (볼륨, 해상도 등 기존 코드)
    private void InitResolutionList()
    {
        if (resolutionDropdown == null) return;
        resolutions.Clear();
        Resolution[] allResolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        if (allResolutions.Length == 0)
        {
            List<string> fallback = new List<string> { Screen.currentResolution.width + " x " + Screen.currentResolution.height };
            resolutionDropdown.AddOptions(fallback);
            return;
        }

        List<string> options = new List<string>();
        int currentResIndex = 0;
        for (int i = 0; i < allResolutions.Length; i++)
        {
            string option = allResolutions[i].width + " x " + allResolutions[i].height;
            if (!options.Contains(option))
            {
                options.Add(option);
                resolutions.Add(allResolutions[i]);
            }
            if (allResolutions[i].width == Screen.currentResolution.width &&
                allResolutions[i].height == Screen.currentResolution.height)
                currentResIndex = i;
        }
        resolutionDropdown.AddOptions(options);
        
        int savedRes = PlayerPrefs.GetInt("ResIndex", currentResIndex);
        resolutionDropdown.value = Mathf.Clamp(savedRes, 0, options.Count - 1);
        resolutionDropdown.RefreshShownValue();
    }

    public void SetResolution(int index)
    {
        if (resolutions == null || index < 0 || index >= resolutions.Count) return;
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
        PlayerPrefs.SetInt("ResIndex", index);
    }

    public void SetScreenMode(int index)
    {
        if (index == 0) Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        else Screen.fullScreenMode = FullScreenMode.Windowed;
        PlayerPrefs.SetInt("ScreenMode", index);
    }

    public void SetVSync(bool isOn)
    {
        QualitySettings.vSyncCount = isOn ? 1 : 0;
        PlayerPrefs.SetInt("VSync", isOn ? 1 : 0);
    }

    public void SetFPSLimit(int index)
    {
        switch (index)
        {
            case 0: Application.targetFrameRate = 60; break;
            case 1: Application.targetFrameRate = 144; break;
            case 2: Application.targetFrameRate = -1; break;
        }
        PlayerPrefs.SetInt("FPSLimit", index);
    }

    public void OnVolumeSliderChanged(float value)
    {
        AudioListener.volume = value / 100f;
        if (volumeInputField != null)
        {
            string newText = Mathf.RoundToInt(value).ToString();
            if (volumeInputField.text != newText) volumeInputField.text = newText;
        }
    }

    public void OnVolumeInputChanged(string text)
    {
        if (float.TryParse(text, out float value))
        {
            float clampedValue = Mathf.Clamp(value, 0, 100);
            AudioListener.volume = clampedValue / 100f;
            if (masterVolumeSlider != null && !Mathf.Approximately(masterVolumeSlider.value, clampedValue))
                masterVolumeSlider.value = clampedValue;
        }
    }

    private void LoadSettings()
    {
        float vol = PlayerPrefs.GetFloat("Volume", 1f);
        AudioListener.volume = vol;
        float uiVolume = vol * 100f;

        if (masterVolumeSlider != null) masterVolumeSlider.value = uiVolume;
        if (volumeInputField != null) volumeInputField.text = Mathf.RoundToInt(uiVolume).ToString();

        int vsync = PlayerPrefs.GetInt("VSync", 0);
        QualitySettings.vSyncCount = vsync;
        if (vSyncToggle != null) vSyncToggle.isOn = (vsync == 1);

        int fpsIndex = PlayerPrefs.GetInt("FPSLimit", 0);
        SetFPSLimit(fpsIndex);
        if (fpsDropdown != null) fpsDropdown.value = fpsIndex;

        int screenMode = PlayerPrefs.GetInt("ScreenMode", 0);
        if (screenMode == 1) 
        {
            int savedW = PlayerPrefs.GetInt("WindowWidth", Screen.width);
            int savedH = PlayerPrefs.GetInt("WindowHeight", Screen.height);
            Screen.SetResolution(savedW, savedH, FullScreenMode.Windowed);
        }
        else SetScreenMode(screenMode);

        if (screenModeDropdown != null) screenModeDropdown.value = screenMode;
    }

    // 🌟 4. 버튼 전환 시 사용되는 기존 페이드 효과
    public void ResumeGameWithFade()
    {
        StartFadeRoutine(() => {
            SetGameState(GameState.Running);
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        });
    }

    public void GoToMainMenuWithFade()
    {
        StartFadeRoutine(() => {
            Time.timeScale = 1f;
            SceneManager.LoadScene(0);
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        });
    }

    public void QuitGameWithFade()
    {
        StartFadeRoutine(() => {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        });
    }

    private void StartFadeRoutine(Action onComplete)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOutAndExecute(onComplete));
    }

    private IEnumerator FadeOutAndExecute(Action onComplete)
    {
        if (pauseMenuCanvasGroup != null)
        {
            float duration = 0.3f;
            float startAlpha = pauseMenuCanvasGroup.alpha;
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
            {
                pauseMenuCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0, t / duration);
                yield return null;
            }
            pauseMenuCanvasGroup.alpha = 0;
        }
        onComplete?.Invoke();
        fadeCoroutine = null;
    }
}
