using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Audio;

public class OptionsMenuController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private AudioMixer masterMixer;   // expose param: "MasterVolume"

    // UI refs
    private VisualElement optionsOverlay;
    private Slider volumeSlider;
    private DropdownField resolutionDropdown;
    private DropdownField displayModeDropdown;
    private Button applyButton;
    private Button closeButton;

    // Data
    private List<Resolution> _resolutions = new();
    private readonly (string label, FullScreenMode mode)[] _modes = new[]
    {
        ("Windowed",            FullScreenMode.Windowed),
        ("Borderless",          FullScreenMode.FullScreenWindow),
        ("Exclusive Fullscreen",FullScreenMode.ExclusiveFullScreen),
    };

    // PlayerPrefs keys
    const string PP_VOL = "opt_masterVol";
    const string PP_RES = "opt_resIndex";
    const string PP_MODE = "opt_modeIndex";

    // Snapshot of saved values when the modal opens (used to revert on Cancel)
    private float _snapVol;
    private int _snapResIndex;
    private int _snapModeIndex;

    void Awake()
    {
        if (!uiDocument) uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        optionsOverlay = root.Q<VisualElement>("optionsOverlay");
        volumeSlider = root.Q<Slider>("volumeSlider");
        resolutionDropdown = root.Q<DropdownField>("resolutionDropdown");
        displayModeDropdown = root.Q<DropdownField>("displayModeDropdown");
        applyButton = root.Q<Button>("applyButton");
        closeButton = root.Q<Button>("optionsCloseButton");

        // Populate choices
        PopulateResolutions();
        PopulateModes();

        // Load saved -> UI and apply to engine once at boot
        LoadSettingsIntoUI();
        ApplyCurrentSettings(applyToEngine: true, save: false);

        // Wire events
        applyButton.clicked += OnApplyClicked;         // apply & close
        closeButton.clicked += CancelAndClose;         // revert preview & close

        // Live preview for volume only (no saving)
        volumeSlider.RegisterValueChangedCallback(_ =>
            ApplyCurrentSettings(applyToEngine: true, save: false)
        );
    }

    // Called by MainMenuController when opening
    public void Show()
    {
        // Take a snapshot of SAVED settings (from PlayerPrefs) so Cancel can revert preview.
        _snapVol = PlayerPrefs.GetFloat(PP_VOL, 0.8f);
        _snapResIndex = PlayerPrefs.GetInt(PP_RES, FindCurrentResolutionIndex());
        _snapModeIndex = PlayerPrefs.GetInt(PP_MODE, displayModeDropdown.index);

        optionsOverlay.RemoveFromClassList("hidden");

        // Reset UI to saved values (do NOT trigger callbacks)
        LoadSettingsIntoUI();

        applyButton.Focus();
    }

    // Hide without reverting (rarely used now; prefer CancelAndClose for backdrop/Esc).
    public void Hide() => optionsOverlay.AddToClassList("hidden");

    // Close button / Cancel path: revert live preview to the saved snapshot, then hide.
    public void CancelAndClose()
    {
        // Revert engine preview (only volume is live-previewed)
        SetMasterVolume(_snapVol);

        // Reset UI to saved values so reopening shows the saved state
        LoadSettingsIntoUI();

        Hide();
    }

    private void OnApplyClicked()
    {
        // Apply & save, then close
        ApplyCurrentSettings(applyToEngine: true, save: true);
        Hide();
    }

    private void PopulateResolutions()
    {
        _resolutions.Clear();
        _resolutions.AddRange(Screen.resolutions);

        var labels = new List<string>(_resolutions.Count);
        for (int i = 0; i < _resolutions.Count; i++)
        {
#if UNITY_2022_2_OR_NEWER
            labels.Add($"{_resolutions[i].width} x {_resolutions[i].height} @ {Mathf.RoundToInt((float)_resolutions[i].refreshRateRatio.value)}Hz");
#else
            labels.Add($"{_resolutions[i].width} x {_resolutions[i].height} @ {_resolutions[i].refreshRate}Hz");
#endif
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

    private void LoadSettingsIntoUI()
    {
        float vol = PlayerPrefs.GetFloat(PP_VOL, 0.8f);
        volumeSlider.SetValueWithoutNotify(vol * 100f);

        int resIdx = Mathf.Clamp(PlayerPrefs.GetInt(PP_RES, FindCurrentResolutionIndex()), 0, _resolutions.Count - 1);
        resolutionDropdown.index = resIdx;

        int modeIdx = Mathf.Clamp(PlayerPrefs.GetInt(PP_MODE, displayModeDropdown.index), 0, _modes.Length - 1);
        displayModeDropdown.index = modeIdx;
    }

    private void ApplyCurrentSettings(bool applyToEngine, bool save)
    {
        float normVol = Mathf.Clamp01(volumeSlider.value / 100f);
        SetMasterVolume(normVol);

        var r = _resolutions[Mathf.Clamp(resolutionDropdown.index, 0, _resolutions.Count - 1)];
        var mode = _modes[Mathf.Clamp(displayModeDropdown.index, 0, _modes.Length - 1)].mode;

        if (applyToEngine) Screen.SetResolution(r.width, r.height, mode);

        if (save)
        {
            PlayerPrefs.SetFloat(PP_VOL, normVol);
            PlayerPrefs.SetInt(PP_RES, resolutionDropdown.index);
            PlayerPrefs.SetInt(PP_MODE, displayModeDropdown.index);
            PlayerPrefs.Save();

            // Update the snapshot so an immediate Close doesn't "revert" the thing we just saved
            _snapVol = normVol;
            _snapResIndex = resolutionDropdown.index;
            _snapModeIndex = displayModeDropdown.index;
        }
    }

    private void SetMasterVolume(float normalized01)
    {
        if (masterMixer)
        {
            float db = Mathf.Log10(Mathf.Clamp(normalized01, 0.0001f, 1f)) * 20f;
            masterMixer.SetFloat("MasterVolume", db);
        }
        else
        {
            AudioListener.volume = normalized01;
        }
    }
}
