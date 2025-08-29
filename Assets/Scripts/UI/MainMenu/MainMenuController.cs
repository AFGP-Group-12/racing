using System;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private Button createLobbyButton;
    private Button joinLobbyButton;
    private Button optionsButton;
    private Button exitGameButton;

    private VisualElement optionsOverlay;
    private Button optionsCloseButton;

    public event Action OnCreateLobbyPressed;
    public event Action OnJoinLobbyPressed;
    public event Action OnOptionsPressed;
    public event Action OnExitGamePressed;



    void Awake()
    {
        var root = uiDocument.rootVisualElement;
        createLobbyButton = root.Q<Button>("createLobbyButton");
        joinLobbyButton = root.Q<Button>("joinLobbyButton");
        optionsButton = root.Q<Button>("optionsButton");
        exitGameButton = root.Q<Button>("exitGameButton");

        optionsOverlay = root.Q<VisualElement>("optionsOverlay");
        optionsCloseButton = root.Q<Button>("optionsCloseButton");

        createLobbyButton.clicked += () => OnCreateLobbyPressed?.Invoke();
        joinLobbyButton.clicked += () => OnJoinLobbyPressed?.Invoke();
        optionsButton.clicked += () => OnOptionsPressed?.Invoke();
        exitGameButton.clicked += () => OnExitGamePressed?.Invoke();

        // Show/Hide the modal
        OnOptionsPressed += ShowOptions;
        optionsCloseButton.clicked += HideOptions;

        // Click backdrop to close
        optionsOverlay.RegisterCallback<ClickEvent>(evt =>
        {
            // If click landed directly on the overlay (not the panel), close
            if (evt.target == optionsOverlay) HideOptions();
        });

        // Esc to close (works when overlay has focus or you’re not inside Play)
        root.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.Escape && optionsOverlay.style.display != DisplayStyle.None)
                HideOptions();
        });

        TestSubscribeEvents();
    }

    private void ShowOptions()
    {
        optionsOverlay.RemoveFromClassList("hidden");
        // Optionally block scroll/tab on background or set focus to a control inside:
        optionsCloseButton.Focus();
    }

    private void HideOptions()
    {
        optionsOverlay.AddToClassList("hidden");
    }

    public void TestSubscribeEvents()
    {
        OnCreateLobbyPressed += () => Debug.Log("Create Lobby button pressed (test)");
        OnJoinLobbyPressed += () => Debug.Log("Join Lobby button pressed (test)");
        OnOptionsPressed += () => Debug.Log("Options button pressed (test)");
        OnExitGamePressed += () => Debug.Log("Exit Game button pressed (test)");
    }
}
