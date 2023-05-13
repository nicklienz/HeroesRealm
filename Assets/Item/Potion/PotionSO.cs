using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Buat Item Baru", menuName ="HeroesRealm/Buat Item Baru/Potion")]
public class PotionSO : ItemSO
{
    public int healthHeal, manaHeal;
    public float healthHealRate, manaHealRate;

    public void Heal(Character character)
    {
        int healthRate = System.Convert.ToInt32((float) character.characterSO.maxHealth * healthHealRate);
        character.characterSO.currentHealth += (healthHeal + healthRate);
        if(character.characterSO.currentHealth > character.characterSO.maxHealth)
        {
            character.characterSO.currentHealth = character.characterSO.maxHealth;
        }
        int manaRate = System.Convert.ToInt32((float) character.characterSO.maxManaPoints * manaHealRate);
        character.characterSO.currentManaPoints+= (manaHeal+ manaRate);
        if(character.characterSO.currentManaPoints > character.characterSO.maxManaPoints)
        {
            character.characterSO.currentManaPoints = character.characterSO.maxManaPoints;
        }
    }
}