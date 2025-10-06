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
    private Ability[] equippedAbilities = new Ability[3];

    // Cooldown coroutines
    private Coroutine cooldownCoroutine1;
    private Coroutine cooldownCoroutine2;
    private Coroutine cooldownCoroutine3;

    void Start()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        // Get the root visual element
        root = uiDocument.rootVisualElement;

        // Get references to the ability slots and icons
        abilitySlot1 = root.Q<VisualElement>("AbilitySlot1");
        abilitySlot2 = root.Q<VisualElement>("AbilitySlot2");
        abilitySlot3 = root.Q<VisualElement>("AbilitySlot3");

        abilityIcon1 = root.Q<Image>("AbilityIcon1");
        abilityIcon2 = root.Q<Image>("AbilityIcon2");
        abilityIcon3 = root.Q<Image>("AbilityIcon3");

        // Create cooldown overlays
        CreateCooldownOverlays();

        // Subscribe to ability manager events
        abilityManager.OnAbilityChanged += UpdateAbilityIcon;
        abilityManager.OnAbilityCooldownStart += StartCooldownVisual;
        abilityManager.OnAbilityCooldownEnd += EndCooldownVisual;
        abilityManager.OnAbilityDurationStart += StartDurationVisual;
        abilityManager.OnAbilityDurationEnd += EndDurationVisual;

    }

    void OnDestroy()
    {
        // Unsubscribe from events when destroyed
        if (abilityManager != null)
        {
            abilityManager.OnAbilityChanged -= UpdateAbilityIcon;
            abilityManager.OnAbilityCooldownStart -= StartCooldownVisual;
            abilityManager.OnAbilityCooldownEnd -= EndCooldownVisual;
            abilityManager.OnAbilityDurationStart -= StartDurationVisual;
            abilityManager.OnAbilityDurationEnd -= EndDurationVisual;
        }
    }

    private void CreateCooldownOverlays()
    {
        // Create overlay for ability 1
        cooldownOverlay1 = new VisualElement();
        cooldownOverlay1.style.position = Position.Absolute;
        cooldownOverlay1.style.width = Length.Percent(100);
        cooldownOverlay1.style.height = Length.Percent(100);
        cooldownOverlay1.style.backgroundColor = new Color(0, 0, 0, 0.7f);
        cooldownOverlay1.style.display = DisplayStyle.None;
        abilitySlot1?.Add(cooldownOverlay1);

        // Create overlay for ability 2
        cooldownOverlay2 = new VisualElement();
        cooldownOverlay2.style.position = Position.Absolute;
        cooldownOverlay2.style.width = Length.Percent(100);
        cooldownOverlay2.style.height = Length.Percent(100);
        cooldownOverlay2.style.backgroundColor = new Color(0, 0, 0, 0.7f);
        cooldownOverlay2.style.display = DisplayStyle.None;
        abilitySlot2?.Add(cooldownOverlay2);

        // Create overlay for ability 3
        cooldownOverlay3 = new VisualElement();
        cooldownOverlay3.style.position = Position.Absolute;
        cooldownOverlay3.style.width = Length.Percent(100);
        cooldownOverlay3.style.height = Length.Percent(100);
        cooldownOverlay3.style.backgroundColor = new Color(0, 0, 0, 0.7f);
        cooldownOverlay3.style.display = DisplayStyle.None;
        abilitySlot3?.Add(cooldownOverlay3);
    }

    private void UpdateAbilityIcon(int abilityIndex, Ability ability)
    {
        if (ability == null)
        {
            Debug.LogWarning($"Ability is null for index {abilityIndex}");
            return;
        }

        equippedAbilities[abilityIndex] = ability;

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

    private void StartCooldownVisual(int abilityIndex)
    {
        // Get the cooldown duration from the equipped ability
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
        // Change the ability slot border color to yellow when ability is active
        Color yellow = new Color(1f, 1f, 0f);
        switch (abilityIndex)
        {
            case 0:
                if (abilitySlot1 != null)
                {
                    abilitySlot1.style.borderTopColor = yellow;
                    abilitySlot1.style.borderBottomColor = yellow;
                    abilitySlot1.style.borderLeftColor = yellow;
                    abilitySlot1.style.borderRightColor = yellow;
                }
                break;
            case 1:
                if (abilitySlot2 != null)
                {
                    abilitySlot2.style.borderTopColor = yellow;
                    abilitySlot2.style.borderBottomColor = yellow;
                    abilitySlot2.style.borderLeftColor = yellow;
                    abilitySlot2.style.borderRightColor = yellow;
                }
                break;
            case 2:
                if (abilitySlot3 != null)
                {
                    abilitySlot3.style.borderTopColor = yellow;
                    abilitySlot3.style.borderBottomColor = yellow;
                    abilitySlot3.style.borderLeftColor = yellow;
                    abilitySlot3.style.borderRightColor = yellow;
                }
                break;
        }
    }

    private void EndDurationVisual(int abilityIndex)
    {
        // Reset the ability slot border color back to black when duration ends
        Color black = new Color(0f, 0f, 0f);
        switch (abilityIndex)
        {
            case 0:
                if (abilitySlot1 != null)
                {
                    abilitySlot1.style.borderTopColor = black;
                    abilitySlot1.style.borderBottomColor = black;
                    abilitySlot1.style.borderLeftColor = black;
                    abilitySlot1.style.borderRightColor = black;
                }
                break;
            case 1:
                if (abilitySlot2 != null)
                {
                    abilitySlot2.style.borderTopColor = black;
                    abilitySlot2.style.borderBottomColor = black;
                    abilitySlot2.style.borderLeftColor = black;
                    abilitySlot2.style.borderRightColor = black;
                }
                break;
            case 2:
                if (abilitySlot3 != null)
                {
                    abilitySlot3.style.borderTopColor = black;
                    abilitySlot3.style.borderBottomColor = black;
                    abilitySlot3.style.borderLeftColor = black;
                    abilitySlot3.style.borderRightColor = black;
                }
                break;
        }
    }

    private IEnumerator AnimateCooldown(VisualElement overlay, float duration)
    {
        if (overlay == null) yield break;

        // Show the overlay
        overlay.style.display = DisplayStyle.Flex;
        overlay.style.height = Length.Percent(100);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // Shrink the overlay from bottom to top (100% to 0%)
            overlay.style.height = Length.Percent((1f - progress) * 100f);

            yield return null;
        }

        // Ensure it's fully hidden at the end
        overlay.style.display = DisplayStyle.None;
    }
}
