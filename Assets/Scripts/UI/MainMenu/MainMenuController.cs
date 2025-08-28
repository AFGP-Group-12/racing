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
