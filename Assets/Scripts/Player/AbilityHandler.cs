using System.Collections.Generic;
using UnityEngine;

public class AbilityHandler : MonoBehaviour
{
    #region Variables
    private List<Ability> abilityList = new List<Ability>(3);
    private int abilityIndex = 0;

    #endregion Variables


    #region MonoBehaviour
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

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
        if (abilityList[abilityIndex] != null)
        {
            // When you have an ability there what do you do
        }
        else
        {
            abilityList[abilityIndex] = ability;
            abilityIndex += 1;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("AbilityBox"))
        {
            AddAbility(other.GetComponent<AbilityPickup>().GetAbility());
        }
    }

    #endregion Functions
}
