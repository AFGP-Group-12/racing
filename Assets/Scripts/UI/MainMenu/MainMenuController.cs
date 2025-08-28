using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private Button createLobbyButton;
    private Button joinLobbyButton;
    private Button optionsButton;
    private Button exitGameButton;

    public event System.Action OnCreateLobbyPressed;
    public event System.Action OnJoinLobbyPressed;
    public event System.Action OnOptionsPressed;
    public event System.Action OnExitGamePressed;

    void Awake()
    {
        var root = uiDocument.rootVisualElement;
        createLobbyButton = root.Q<Button>("createLobbyButton");
        joinLobbyButton = root.Q<Button>("joinLobbyButton");
        optionsButton = root.Q<Button>("optionsButton");
        exitGameButton = root.Q<Button>("exitGameButton");

        if (createLobbyButton == null || joinLobbyButton == null || optionsButton == null || exitGameButton == null)
        {
            Debug.LogError("One or more buttons are missing");
            return;
        }

        createLobbyButton.clicked += () => OnCreateLobbyPressed?.Invoke();
        joinLobbyButton.clicked += () => OnJoinLobbyPressed?.Invoke();
        optionsButton.clicked += () => OnOptionsPressed?.Invoke();
        exitGameButton.clicked += () => OnExitGamePressed?.Invoke();

        TestSubscribeEvents();
    }

    public void TestSubscribeEvents()
    {
        OnCreateLobbyPressed += () => Debug.Log("Create Lobby button pressed (test)");
        OnJoinLobbyPressed += () => Debug.Log("Join Lobby button pressed (test)");
        OnOptionsPressed += () => Debug.Log("Options button pressed (test)");
        OnExitGamePressed += () => Debug.Log("Exit Game button pressed (test)");
    }
}
