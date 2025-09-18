using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAbilityManager : MonoBehaviour
{
    #region Variables
    private PlayerContext contextScript;
    private PlayerInput input;
    private List<Ability> abilityList;

    public int abilityIndex = 0;
    private Ability emptyAbility;

    #endregion Variables


    #region MonoBehaviour

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        contextScript = GetComponent<PlayerContext>();

        input = contextScript.input;

        emptyAbility = ScriptableObject.CreateInstance<EmptyAbility>();

        abilityList = new List<Ability> { emptyAbility, emptyAbility, emptyAbility };
    }

    // Update is called once per frame
    void Update()
    {
        IndexCheck();

    }
    #endregion MonoBehaviour

    #region Functions

    void IndexCheck()
    {
        if (abilityIndex > 2)
        {
            abilityIndex = 0;
        }
    }

    void AddAbility(Ability ability)
    {
        if (abilityList[abilityIndex].abilityIndex == -1)
        {
            abilityList[abilityIndex] = ability;
            abilityIndex += 1;
        }
        else
        {
            // If you have an ability already what happens
        }
    }
    public void debugAdd(Ability ability) // Delete this once done
    {
        AddAbility(ability);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AbilityBox"))
        {
            AddAbility(other.GetComponent<AbilityPickup>().GetAbility());
        }
    }

    public void StartAbility1()
    {
        if (abilityList[0].abilityIndex == -1)
        {
            return;
        }

        abilityList[0].Activate(contextScript, 0);
    }
    public void EndAbility1()
    {
        if (abilityList[0].abilityIndex == -1)
        {
            return;
        }
        abilityList[0].DeActivate(contextScript);
    }
    public void StartAbility2()
    {
        if (abilityList[1].abilityIndex == -1)
        {
            return;
        }

        abilityList[1].Activate(contextScript, 1);
    }
    public void EndAbility2()
    {
        if (abilityList[1].abilityIndex == -1)
        {
            return;
        }
        abilityList[1].DeActivate(contextScript);
    }
    public void StartAbility3()
    {
        if (abilityList[2].abilityIndex == -1)
        {
            return;
        }

        abilityList[2].Activate(contextScript, 2);
    }
    public void EndAbility3()
    {
        if (abilityList[2].abilityIndex == -1)
        {
            return;
        }
        abilityList[2].DeActivate(contextScript);
    }

    public void StartAbilityDuration(int abilityIndex, float duration)
    {
        StartCoroutine(AbilityDuration(abilityIndex, duration));
    }
    IEnumerator AbilityDuration(int abilityIndex, float duration)
    {
        yield return new WaitForSeconds(duration);
        abilityList[abilityIndex].AbilityEnd();
    }


    public void StartAbilityCooldown(int abilityIndex, float cooldown)
    {
        StartCoroutine(AbilityCooldown(abilityIndex, cooldown));
    }
    IEnumerator AbilityCooldown(int abilityIndex, float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        abilityList[abilityIndex].CooldownEnd();
    }


    #endregion Functions
}
