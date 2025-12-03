using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerUIController : MonoBehaviour
{
    [SerializeField] private PlayerAbilityManager abilityManager;
    [SerializeField] private UIDocument uiDocument;

    private VisualElement root;
    private VisualElement abilitySlot1;
    private VisualElement abilitySlot2;
    private VisualElement abilitySlot3;
    private Image abilityIcon1;
    private Image abilityIcon2;
    private Image abilityIcon3;

    // Cooldown overlay elements
    private VisualElement cooldownOverlay1;
    private VisualElement cooldownOverlay2;
    private VisualElement cooldownOverlay3;

    // Store currently equipped abilities
    private readonly Ability[] equippedAbilities = new Ability[3];

    // Cooldown coroutines
    private Coroutine cooldownCoroutine1;
    private Coroutine cooldownCoroutine2;
    private Coroutine cooldownCoroutine3;

    private bool uiInitialized;
    private bool abilityEventsHooked;

    void Awake()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (abilityManager == null)
        {
            abilityManager = GetComponentInParent<PlayerAbilityManager>();
        }
    }

    void OnEnable()
    {
        RegisterPanelCallbacks();
        TryInitializeUI();
    }

    void Start()
    {
        if (abilityManager == null)
            abilityManager = GetComponentInParent<PlayerAbilityManager>();

        if (abilityManager == null)
        {
            Debug.LogError("PlayerUIController could not locate PlayerAbilityManager.", this);
            return;
        }

        SubscribeToAbilityEvents();
    }

    void OnDisable()
    {
        ResetCachedUI();
        UnregisterPanelCallbacks();
    }

    void OnDestroy()
    {
        UnsubscribeFromAbilityEvents();
        ResetCachedUI();
        UnregisterPanelCallbacks();
    }

    private void RegisterPanelCallbacks()
    {
        if (uiDocument == null || uiDocument.rootVisualElement == null) return;

        var rootElement = uiDocument.rootVisualElement;
        rootElement.RegisterCallback<AttachToPanelEvent>(OnPanelAttached);
        rootElement.RegisterCallback<DetachFromPanelEvent>(OnPanelDetached);
    }

    private void UnregisterPanelCallbacks()
    {
        if (uiDocument == null || uiDocument.rootVisualElement == null) return;

        var rootElement = uiDocument.rootVisualElement;
        rootElement.UnregisterCallback<AttachToPanelEvent>(OnPanelAttached);
        rootElement.UnregisterCallback<DetachFromPanelEvent>(OnPanelDetached);
    }

    private void OnPanelAttached(AttachToPanelEvent evt)
    {
        TryInitializeUI();
    }

    private void OnPanelDetached(DetachFromPanelEvent evt)
    {
        ResetCachedUI();
    }

    private void TryInitializeUI()
    {
        if (uiInitialized || uiDocument == null) return;

        root = uiDocument.rootVisualElement;
        if (root == null || root.panel == null) return;

        abilitySlot1 = root.Q<VisualElement>("AbilitySlot1");
        abilitySlot2 = root.Q<VisualElement>("AbilitySlot2");
        abilitySlot3 = root.Q<VisualElement>("AbilitySlot3");

        abilityIcon1 = root.Q<Image>("AbilityIcon1");
        abilityIcon2 = root.Q<Image>("AbilityIcon2");
        abilityIcon3 = root.Q<Image>("AbilityIcon3");

        CreateCooldownOverlays();
        uiInitialized = true;

        RefreshAbilityIcons();

        if (abilityManager != null)
            MoveIndicator(abilityManager.abilityIndex);
    }

    private void SubscribeToAbilityEvents()
    {
        if (abilityEventsHooked || abilityManager == null) return;

        abilityManager.OnAbilityChanged += UpdateAbilityIcon;
        abilityManager.OnAbilityCooldownStart += StartCooldownVisual;
        abilityManager.OnAbilityCooldownEnd += EndCooldownVisual;
        abilityManager.OnAbilityDurationStart += StartDurationVisual;
        abilityManager.OnAbilityDurationEnd += EndDurationVisual;
        abilityManager.OnIndexChange += MoveIndicator;
        abilityEventsHooked = true;
    }

    private void UnsubscribeFromAbilityEvents()
    {
        if (!abilityEventsHooked || abilityManager == null) return;

        abilityManager.OnAbilityChanged -= UpdateAbilityIcon;
        abilityManager.OnAbilityCooldownStart -= StartCooldownVisual;
        abilityManager.OnAbilityCooldownEnd -= EndCooldownVisual;
        abilityManager.OnAbilityDurationStart -= StartDurationVisual;
        abilityManager.OnAbilityDurationEnd -= EndDurationVisual;
        abilityManager.OnIndexChange -= MoveIndicator;
        abilityEventsHooked = false;
    }

    private void MoveIndicator(int abilityIndex)
    {
        if (!uiInitialized) return;

        Color yellow = new Color(1f, 1f, 0f);
        Color black = new Color(0f, 0f, 0f);

        switch (abilityIndex)
        {
            case 0:
                SetSlotBorderColor(abilitySlot1, yellow);
                SetSlotBorderColor(abilitySlot2, black);
                SetSlotBorderColor(abilitySlot3, black);
                break;
            case 1:
                SetSlotBorderColor(abilitySlot2, yellow);
                SetSlotBorderColor(abilitySlot1, black);
                SetSlotBorderColor(abilitySlot3, black);
                break;
            case 2:
                SetSlotBorderColor(abilitySlot3, yellow);
                SetSlotBorderColor(abilitySlot2, black);
                SetSlotBorderColor(abilitySlot1, black);
                break;
        }
    }

    private void UpdateAbilityIcon(int abilityIndex, Ability ability)
    {
        if (ability == null)
        {
            Debug.LogWarning($"Ability is null for index {abilityIndex}");
            return;
        }

        equippedAbilities[abilityIndex] = ability;
        ApplyAbilityIcon(abilityIndex, ability);
    }

    private void ApplyAbilityIcon(int abilityIndex, Ability ability)
    {
        if (!uiInitialized || ability == null) return;

        Sprite icon = ability.icon;

        switch (abilityIndex)
        {
            case 0:
                if (abilityIcon1 != null)
                    abilityIcon1.sprite = icon;
                break;
            case 1:
                if (abilityIcon2 != null)
                    abilityIcon2.sprite = icon;
                break;
            case 2:
                if (abilityIcon3 != null)
                    abilityIcon3.sprite = icon;
                break;
            default:
                Debug.LogWarning($"Invalid ability index: {abilityIndex}");
                break;
        }
    }

    private void RefreshAbilityIcons()
    {
        if (!uiInitialized) return;

        for (int i = 0; i < equippedAbilities.Length; i++)
        {
            if (equippedAbilities[i] != null)
                ApplyAbilityIcon(i, equippedAbilities[i]);
        }
    }

    private void StartCooldownVisual(int abilityIndex)
    {
        if (!uiInitialized)
        {
            return;
        }

        if (abilityIndex < 0 || abilityIndex >= equippedAbilities.Length || equippedAbilities[abilityIndex] == null)
        {
            Debug.LogWarning($"No ability equipped at index {abilityIndex}");
            return;
        }

        float cooldownDuration = equippedAbilities[abilityIndex].cooldown;

        switch (abilityIndex)
        {
            case 0:
                if (cooldownCoroutine1 != null) StopCoroutine(cooldownCoroutine1);
                cooldownCoroutine1 = StartCoroutine(AnimateCooldown(cooldownOverlay1, cooldownDuration));
                break;
            case 1:
                if (cooldownCoroutine2 != null) StopCoroutine(cooldownCoroutine2);
                cooldownCoroutine2 = StartCoroutine(AnimateCooldown(cooldownOverlay2, cooldownDuration));
                break;
            case 2:
                if (cooldownCoroutine3 != null) StopCoroutine(cooldownCoroutine3);
                cooldownCoroutine3 = StartCoroutine(AnimateCooldown(cooldownOverlay3, cooldownDuration));
                break;
        }
    }

    private void EndCooldownVisual(int abilityIndex)
    {
        if (!uiInitialized) return;

        switch (abilityIndex)
        {
            case 0:
                if (cooldownOverlay1 != null) cooldownOverlay1.style.display = DisplayStyle.None;
                break;
            case 1:
                if (cooldownOverlay2 != null) cooldownOverlay2.style.display = DisplayStyle.None;
                break;
            case 2:
                if (cooldownOverlay3 != null) cooldownOverlay3.style.display = DisplayStyle.None;
                break;
        }
    }

    private void StartDurationVisual(int abilityIndex)
    {
        if (!uiInitialized) return;

        Color yellow = new Color(1f, 1f, 0f);

        switch (abilityIndex)
        {
            case 0:
                SetSlotBorderColor(abilitySlot1, yellow);
                break;
            case 1:
                SetSlotBorderColor(abilitySlot2, yellow);
                break;
            case 2:
                SetSlotBorderColor(abilitySlot3, yellow);
                break;
        }
    }

    private void EndDurationVisual(int abilityIndex)
    {
        if (!uiInitialized) return;
        Color black = new Color(0f, 0f, 0f);

        switch (abilityIndex)
        {
            case 0:
                SetSlotBorderColor(abilitySlot1, black);
                break;
            case 1:
                SetSlotBorderColor(abilitySlot2, black);
                break;
            case 2:
                SetSlotBorderColor(abilitySlot3, black);
                break;
        }
    }

    private void CreateCooldownOverlays()
    {
        CreateOrResetOverlay(ref cooldownOverlay1, abilitySlot1);
        CreateOrResetOverlay(ref cooldownOverlay2, abilitySlot2);
        CreateOrResetOverlay(ref cooldownOverlay3, abilitySlot3);
    }

    private void CreateOrResetOverlay(ref VisualElement overlay, VisualElement slot)
    {
        if (slot == null) return;

        if (overlay == null)
        {
            overlay = new VisualElement();
            overlay.style.position = Position.Absolute;
            overlay.style.width = Length.Percent(100);
            overlay.style.backgroundColor = new Color(0, 0, 0, 0.7f);
        }

        overlay.style.height = Length.Percent(100);
        overlay.style.display = DisplayStyle.None;
        overlay.RemoveFromHierarchy();
        slot.Add(overlay);
    }

    private IEnumerator AnimateCooldown(VisualElement overlay, float duration)
    {
        if (overlay == null) yield break;

        overlay.style.display = DisplayStyle.Flex;
        overlay.style.height = Length.Percent(100);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            overlay.style.height = Length.Percent((1f - progress) * 100f);

            yield return null;
        }

        overlay.style.display = DisplayStyle.None;
    }

    private void ResetCachedUI()
    {
        uiInitialized = false;

        if (cooldownCoroutine1 != null)
        {
            StopCoroutine(cooldownCoroutine1);
            cooldownCoroutine1 = null;
        }
        if (cooldownCoroutine2 != null)
        {
            StopCoroutine(cooldownCoroutine2);
            cooldownCoroutine2 = null;
        }
        if (cooldownCoroutine3 != null)
        {
            StopCoroutine(cooldownCoroutine3);
            cooldownCoroutine3 = null;
        }

        root = null;
        abilitySlot1 = null;
        abilitySlot2 = null;
        abilitySlot3 = null;
        abilityIcon1 = null;
        abilityIcon2 = null;
        abilityIcon3 = null;
        cooldownOverlay1 = null;
        cooldownOverlay2 = null;
        cooldownOverlay3 = null;
    }

    private static void SetSlotBorderColor(VisualElement slot, Color color)
    {
        if (slot == null) return;

        slot.style.borderTopColor = color;
        slot.style.borderBottomColor = color;
        slot.style.borderLeftColor = color;
        slot.style.borderRightColor = color;
    }
}
