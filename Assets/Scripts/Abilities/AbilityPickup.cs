using UnityEngine;

public class AbilityPickup : MonoBehaviour
{
    public Ability ability;

    public Ability GetAbility()
    {
        return ability; // This is for debugging purposes

        // USE THIS WHEN YOU DONT WANT TO DEBUG THIS IS SUPER IMPORTANT

        // Ability instanitatedAbility = Instantiate(ability);
        // Destroy(gameObject);
        // return instanitatedAbility;
    }
}
