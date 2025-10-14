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
    private Button tutorialButton;

    private VisualElement connectionIcon;
    private Button connectButton;
    [SerializeField]
    private Texture2D connectedIcon;
    [SerializeField]
    private Texture2D disconnectedIcon;

    private VisualElement optionsOverlay;
    private Button optionsCloseButton;

    private VisualElement loadingOverlay;
    private VisualElement popupOverlay;
    private Label popupOverlayLabel;

    private VisualElement mainScreen;
    private VisualElement lobbyScreen;

    private OptionsMenuController optionsController;

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
        tutorialButton = root.Q<Button>("tutorialButton");

        optionsOverlay = root.Q<VisualElement>("optionsOverlay");
        optionsCloseButton = root.Q<Button>("optionsCloseButton");

        loadingOverlay = root.Q<VisualElement>("loadingOverlay");
        popupOverlay = root.Q<VisualElement>("popupOverlay");
        popupOverlayLabel = root.Q<Label>("popupOverlayLabel");

        mainScreen = root.Q<VisualElement>("mainScreen");
        lobbyScreen = root.Q<VisualElement>("lobbyScreen");

        connectionIcon = root.Q<VisualElement>("connectionIcon");
        connectButton = root.Q<Button>("connectButton");

        optionsController = GetComponent<OptionsMenuController>();

        // createLobbyButton.clicked += () => OnCreateLobbyPressed?.Invoke();
        optionsButton.clicked += () => OnOptionsPressed?.Invoke();
        exitGameButton.clicked += () => OnExitGamePressed?.Invoke();

        OnOptionsPressed += ShowOptions;
        optionsCloseButton.clicked += HideOptions;

        connectButton.clicked += () => LobbyClient.instance.ConnectToServer();

        optionsOverlay.RegisterCallback<ClickEvent>(evt =>
        {
            if (evt.target == optionsOverlay) HideOptions();
        });

        root.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.Escape && !optionsOverlay.ClassListContains("hidden"))
                HideOptions();
        });

        TestSubscribeEvents();
    }

    public void Start()
    {
        // Link networking code to buttons.
        // This is done in Start instead of Awake since we need the 'instance' variable to be populated.
        LobbyClient.instance.OnLobbyJoined += () =>
        {
            mainScreen.AddToClassList("hidden");
            lobbyScreen.RemoveFromClassList("hidden");
            loadingOverlay.AddToClassList("hidden");
        };

        LobbyClient.instance.OnLobbyExited += () =>
        {
            mainScreen.RemoveFromClassList("hidden");
            lobbyScreen.AddToClassList("hidden");
            loadingOverlay.AddToClassList("hidden");
        };

        LobbyClient.instance.SetPopup += (string s) =>
        {
            if (s == null || s.Length == 0)
            {
                popupOverlay.AddToClassList("hidden");
            } else
            {
                popupOverlay.RemoveFromClassList("hidden");
                popupOverlayLabel.text = s;
            }
        };
        createLobbyButton.clicked += () =>
        {
            if (LobbyClient.instance.client.IsConnectedTcp())
            {
                LobbyClient.instance.CreateLobby();
                loadingOverlay.RemoveFromClassList("hidden");
            }
        };

        tutorialButton.clicked += () =>
        {
            StartCoroutine(GameplayClient.instance.LoadSceneSinglePlayer("BasicMovementTestScene"));
            Debug.Log("Tutorial Button Pressed");
        };
    }

    void Update()
    {
        if (LobbyClient.instance.client.IsConnectedTcp())
        {
            connectionIcon.style.backgroundImage = new StyleBackground(connectedIcon);
            connectButton.AddToClassList("hidden");
        }
        else
        {
            connectionIcon.style.backgroundImage = new StyleBackground(disconnectedIcon);
            connectButton.RemoveFromClassList("hidden");
        }
    }

    private void ShowOptions()
    {
        optionsController.Show();
    }

    private void HideOptions()
    {
        optionsController.Hide();
    }

    public void TestSubscribeEvents()
    {
        OnCreateLobbyPressed += () => Debug.Log("Create Lobby button pressed (test)");
        OnJoinLobbyPressed += () => Debug.Log("Join Lobby button pressed (test)");
        OnOptionsPressed += () => Debug.Log("Options button pressed (test)");
        OnExitGamePressed += () => Debug.Log("Exit Game button pressed (test)");
    }
}
