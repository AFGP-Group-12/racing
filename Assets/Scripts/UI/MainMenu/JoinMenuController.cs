using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class JoinMenuController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private VisualElement joinOverlay, loadingOverlay;
    private TextField joinTextField;
    private Button matchmakeButton, joinButton, joinPrivateButton, cancelButton;

    public event Action<string> OnJoinPrivateButtonClicked;
    public event Action OnMatchmakeButtonClicked;

    void Awake()
    {
        if (!uiDocument) uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        joinOverlay = uiDocument.rootVisualElement.Q<VisualElement>("joinOverlay");
        joinTextField = uiDocument.rootVisualElement.Q<TextField>("joinTextField");
        matchmakeButton = uiDocument.rootVisualElement.Q<Button>("matchmakeButton");
        joinButton = uiDocument.rootVisualElement.Q<Button>("joinLobbyButton");
        joinPrivateButton = uiDocument.rootVisualElement.Q<Button>("joinPrivateButton");
        cancelButton = uiDocument.rootVisualElement.Q<Button>("cancelButton");
        loadingOverlay = uiDocument.rootVisualElement.Q<VisualElement>("loadingOverlay");


        joinButton.clicked += () => ShowJoinMenu();
        joinPrivateButton.clicked += () => OnJoinPrivateButtonClicked?.Invoke(joinTextField.value);
        matchmakeButton.clicked += () => OnMatchmakeButtonClicked?.Invoke();

        matchmakeButton.clicked += () =>
        {
            loadingOverlay.RemoveFromClassList("hidden");
            joinOverlay.AddToClassList("hidden");
        };

        joinOverlay.RegisterCallback<ClickEvent>(evt =>
        {
            if (evt.target == joinOverlay) HideJoinMenu();
        });

        root.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (joinOverlay.ClassListContains("hidden")) return;

            var focused = root.focusController?.focusedElement as VisualElement;
            if (focused == null || !joinOverlay.Contains(focused)) return;

            if ((evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter) && joinTextField.value.Length == 6)
            {
                OnJoinPrivateButtonClicked?.Invoke(joinTextField.value);
                evt.StopImmediatePropagation();
            }
            else if (evt.keyCode == KeyCode.Escape)
            {
                HideJoinMenu();
                evt.StopImmediatePropagation();
            }
        }, TrickleDown.TrickleDown);
    }

    void Start()
    {
        OnMatchmakeButtonClicked += LobbyClient.instance.JoinLobby;
    }

    private void HideJoinMenu()
    {
        joinOverlay.AddToClassList("hidden");
        joinTextField.value = string.Empty;
    }

    private void ShowJoinMenu()
    {
        joinOverlay.RemoveFromClassList("hidden");
        joinTextField.Focus();
    }
}