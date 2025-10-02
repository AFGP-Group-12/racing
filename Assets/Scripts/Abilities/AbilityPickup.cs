using UnityEngine;

public class AbilityPickup : MonoBehaviour
{
    public Ability ability;

    public Ability GetAbility()
    {
        return ability; // This if for debugging purposes


        // Ability instanitatedAbility = Instantiate(ability);
        // return instanitatedAbility;
    }
}
