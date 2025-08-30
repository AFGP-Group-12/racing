using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Audio;

public class OptionsMenuController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private AudioMixer masterMixer;   // Expose these params in the mixer!

    // Mixer parameter names (must match exposed params on your AudioMixer)
    private const string MIXER_MASTER_PARAM   = "MasterVolume";
    private const string MIXER_MUSIC_PARAM    = "MusicVolume";
    private const string MIXER_SFX_PARAM      = "SFXVolume";
    private const string MIXER_AMBIENCE_PARAM = "AmbienceVolume";

    // PlayerPrefs keys
    private const string PP_VOL_MASTER   = "opt_masterVol";
    private const string PP_VOL_MUSIC    = "opt_musicVol";
    private const string PP_VOL_SFX      = "opt_sfxVol";
    private const string PP_VOL_AMBIENCE = "opt_ambienceVol";
    private const string PP_RES          = "opt_resIndex";
    private const string PP_MODE         = "opt_modeIndex";

    // UI refs
    private VisualElement optionsOverlay;
    private Slider masterSlider;
    private Slider musicSlider;
    private Slider sfxSlider;
    private Slider ambienceSlider;
    private DropdownField resolutionDropdown;
    private DropdownField displayModeDropdown;
    private Button applyButton;
    private Button closeButton;

    // Data
    private readonly List<Resolution> _resolutions = new();
    private readonly (string label, FullScreenMode mode)[] _modes = new[]
    {
        ("Windowed",             FullScreenMode.Windowed),
        ("Borderless",           FullScreenMode.FullScreenWindow),
        ("Exclusive Fullscreen", FullScreenMode.ExclusiveFullScreen),
    };

    void Awake()
    {
        if (!uiDocument) uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        optionsOverlay      = root.Q<VisualElement>("optionsOverlay");
        masterSlider        = root.Q<Slider>("masterSlider");
        musicSlider         = root.Q<Slider>("musicSlider");
        sfxSlider           = root.Q<Slider>("sfxSlider");
        ambienceSlider      = root.Q<Slider>("ambienceSlider");
        resolutionDropdown  = root.Q<DropdownField>("resolutionDropdown");
        displayModeDropdown = root.Q<DropdownField>("displayModeDropdown");
        applyButton         = root.Q<Button>("applyButton");
        closeButton         = root.Q<Button>("optionsCloseButton");

        PopulateResolutions();
        PopulateModes();

        // Load saved UI state
        LoadSavedIntoUI();

        // Apply saved engine state once at boot
        ApplySavedVolumes();
        ApplyDisplaySettings(applyToEngine: true, save: false);

        // Wire live volume preview + save
        BindVolumeSlider(masterSlider,   PP_VOL_MASTER,   MIXER_MASTER_PARAM);
        BindVolumeSlider(musicSlider,    PP_VOL_MUSIC,    MIXER_MUSIC_PARAM);
        BindVolumeSlider(sfxSlider,      PP_VOL_SFX,      MIXER_SFX_PARAM);
        BindVolumeSlider(ambienceSlider, PP_VOL_AMBIENCE, MIXER_AMBIENCE_PARAM);

        // Apply (display only) and close
        applyButton.clicked += OnApplyClicked;
        closeButton.clicked += () => optionsOverlay.AddToClassList("hidden");
    }

    // Called by MainMenuController when opening
    public void Show()
    {
        optionsOverlay.RemoveFromClassList("hidden");
        LoadSavedIntoUI();  // refresh UI to saved
        ApplySavedVolumes(); // ensure engine matches saved
        applyButton.Focus();
    }

    public void Hide() => optionsOverlay.AddToClassList("hidden");

    private void BindVolumeSlider(Slider slider, string prefKey, string mixerParam)
    {
        if (slider == null) return;

        slider.RegisterValueChangedCallback(evt =>
        {
            float normalized = Mathf.Clamp01(evt.newValue / 100f);
            SetVolume(mixerParam, normalized);      // live preview
            PlayerPrefs.SetFloat(prefKey, normalized); // save immediately
            PlayerPrefs.Save();
        });
    }

    private void OnApplyClicked()
    {
        // Only apply + save DISPLAY settings
        ApplyDisplaySettings(applyToEngine: true, save: true);
        Hide();
    }

    private void PopulateResolutions()
    {
        _resolutions.Clear();
        _resolutions.AddRange(Screen.resolutions);

        var labels = new List<string>(_resolutions.Count);
        for (int i = 0; i < _resolutions.Count; i++)
        {
            var r = _resolutions[i];
            int hz = Mathf.RoundToInt((float)r.refreshRateRatio.value);
            labels.Add($"{r.width} x {r.height} @ {hz}Hz");
        }

        resolutionDropdown.choices = labels;
        resolutionDropdown.index = FindCurrentResolutionIndex();
    }

    private void PopulateModes()
    {
        var labels = new List<string>(_modes.Length);
        foreach (var m in _modes) labels.Add(m.label);
        displayModeDropdown.choices = labels;

        int idx = Array.FindIndex(_modes, m => m.mode == Screen.fullScreenMode);
        displayModeDropdown.index = Mathf.Max(0, idx);
    }

    private int FindCurrentResolutionIndex()
    {
        var cur = Screen.currentResolution;
        int best = 0, bestScore = int.MaxValue;

        for (int i = 0; i < _resolutions.Count; i++)
        {
            var r = _resolutions[i];
            int score = Mathf.Abs(r.width - cur.width) + Mathf.Abs(r.height - cur.height);
            if (score < bestScore) { bestScore = score; best = i; }
        }
        return best;
    }

    private void LoadSavedIntoUI()
    {
        // Volume sliders
        masterSlider?.SetValueWithoutNotify(PlayerPrefs.GetFloat(PP_VOL_MASTER,   0.8f) * 100f);
        musicSlider?.SetValueWithoutNotify(PlayerPrefs.GetFloat(PP_VOL_MUSIC,     0.8f) * 100f);
        sfxSlider?.SetValueWithoutNotify(PlayerPrefs.GetFloat(PP_VOL_SFX,         0.8f) * 100f);
        ambienceSlider?.SetValueWithoutNotify(PlayerPrefs.GetFloat(PP_VOL_AMBIENCE,0.8f) * 100f);

        // Display dropdowns
        int resDefault = FindCurrentResolutionIndex();
        int resIdx = _resolutions.Count > 0
            ? Mathf.Clamp(PlayerPrefs.GetInt(PP_RES, resDefault), 0, _resolutions.Count - 1)
            : 0;
        resolutionDropdown.index = resIdx;

        int modeDefault = Mathf.Max(0, Array.FindIndex(_modes, m => m.mode == Screen.fullScreenMode));
        int modeIdx = Mathf.Clamp(PlayerPrefs.GetInt(PP_MODE, modeDefault), 0, _modes.Length - 1);
        displayModeDropdown.index = modeIdx;
    }

    private void ApplySavedVolumes()
    {
        SetVolume(MIXER_MASTER_PARAM,   PlayerPrefs.GetFloat(PP_VOL_MASTER,   0.8f));
        SetVolume(MIXER_MUSIC_PARAM,    PlayerPrefs.GetFloat(PP_VOL_MUSIC,    0.8f));
        SetVolume(MIXER_SFX_PARAM,      PlayerPrefs.GetFloat(PP_VOL_SFX,      0.8f));
        SetVolume(MIXER_AMBIENCE_PARAM, PlayerPrefs.GetFloat(PP_VOL_AMBIENCE, 0.8f));
    }

    private void ApplyDisplaySettings(bool applyToEngine, bool save)
    {
        if (_resolutions.Count == 0) return;

        var r    = _resolutions[Mathf.Clamp(resolutionDropdown.index, 0, _resolutions.Count - 1)];
        var mode = _modes[Mathf.Clamp(displayModeDropdown.index, 0, _modes.Length - 1)].mode;

        if (applyToEngine)
            Screen.SetResolution(r.width, r.height, mode);

        if (save)
        {
            PlayerPrefs.SetInt(PP_RES,  resolutionDropdown.index);
            PlayerPrefs.SetInt(PP_MODE, displayModeDropdown.index);
            PlayerPrefs.Save();
        }
    }

    private void SetVolume(string mixerParam, float normalized01)
    {
        normalized01 = Mathf.Clamp01(normalized01);

        if (masterMixer)
        {
            // Map 0..1 to ~-80dB..0dB (avoid -Infinity at 0)
            float db = normalized01 <= 0f ? -80f : Mathf.Log10(Mathf.Max(0.0001f, normalized01)) * 20f;
            masterMixer.SetFloat(mixerParam, db);
        }
        else
        {
            // Fallback: only Master affects AudioListener
            if (mixerParam == MIXER_MASTER_PARAM)
                AudioListener.volume = normalized01;
        }
    }
}
