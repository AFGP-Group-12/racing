using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ProfileBoxController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private Button profileButton;
    private VisualElement profileEditor;      // overlay
    private TextField profileNameField;
    private Button applyBtn, cancelBtn;
    private Label nameLabel;
    private VisualElement avatar;

    public event Action<string> OnProfileNameApplied;

    void Awake()
    {
        if (!uiDocument) uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        profileButton = root.Q<Button>("profileButton");
        profileEditor = root.Q<VisualElement>("profileOverlay");
        profileNameField = root.Q<TextField>("profileNameField");
        applyBtn = root.Q<Button>("profileApply");
        cancelBtn = root.Q<Button>("profileCancel");
        nameLabel = root.Q<Label>("profileName");
        avatar = root.Q<VisualElement>("profileAvatar");

        profileButton.clicked += ShowEditor;
        applyBtn.clicked += ApplyAndClose;
        cancelBtn.clicked += HideEditor;

        // Backdrop click -> close
        profileEditor.RegisterCallback<ClickEvent>(evt =>
        {
            if (evt.target == profileEditor) HideEditor();
        });

        root.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (profileEditor.ClassListContains("hidden")) return;

            var focused = root.focusController?.focusedElement as VisualElement;
            if (focused == null || !profileEditor.Contains(focused)) return;

            if (evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter)
            {
                ApplyAndClose();
                evt.StopImmediatePropagation();
            }
            else if (evt.keyCode == KeyCode.Escape)
            {
                HideEditor();
                evt.StopImmediatePropagation();
            }
        }, TrickleDown.TrickleDown);

        profileEditor.AddToClassList("hidden");
    }

    private void ShowEditor()
    {
        profileEditor.RemoveFromClassList("hidden");
        profileNameField.Focus();
        profileNameField.SelectAll();
    }

    private void HideEditor()
    {
        profileEditor.AddToClassList("hidden");
        profileButton.Focus();
    }

    private void ApplyAndClose()
    {
        string newName = (profileNameField.value ?? "").Trim();
        if (!string.IsNullOrEmpty(newName))
        {
            nameLabel.text = newName;
            OnProfileNameApplied?.Invoke(newName);
            Debug.Log($"Name Changed: {newName}");
        }
        HideEditor();
    }

    public void SetDisplayedName(string name)
    {
        nameLabel.text = name;
        profileNameField.SetValueWithoutNotify(name);
    }

    public void SetAvatar(Texture2D tex) => avatar.style.backgroundImage = new StyleBackground(tex);
    public void SetAvatar(Sprite spr) => avatar.style.backgroundImage = new StyleBackground(spr);
}
