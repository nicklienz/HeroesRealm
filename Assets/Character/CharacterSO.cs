using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public enum Job
{
    Warrior,
    Wizard,
    Archer
}

[CreateAssetMenu(fileName = "Buat Karakter Baru", menuName = "HeroesRealm/Buat Karakter Baru")]
public class CharacterSO : ScriptableObject
{
    public Job job;
    public string userName;
    public int gold;
    public int level;
    [SerializeField] int startHealth;
    public int maxHealth;
    public int currentHealth;
    [SerializeField] int healthMultiplier;
    [SerializeField] int startMana;
    public int maxManaPoints;
    public int currentManaPoints;
    [SerializeField] int manaMultiplier;
    public int experiencePoints = 10;
    public int startStr;
    public int startDef;
    public int startMagic;
    public int curStr;
    public int curDef;
    public int curMagic;
    public int strMultiplier;
    public int defMultiplier;
    public int magicMultiplier;
    public float criticalRate, chanceRate, goldRate, expRate;
    public int hitAtk, fireAtk, iceAtk, soulAtk,  thunderAtk;
    public float hitDef, fireDef, iceDef, soulDef, thunderDef;
    public GameObject particleCritical, particleMiss, playerDamageText;
    public float regenDelay;
    public float regenRate;
    public Vector3 position;
    public int skillPointUsed;
    public int skillPointLeft;
    // Metode untuk menghitung total damage yang akan diterima oleh karakter
    public int CalculateDamageToPlayer(float amount, Element element, float randMin, float randMax)
    {
        float damage = amount;
        switch(element)
        {
            case Element.hit:
            damage = amount-(amount * hitDef);
            break;
            case Element.fire:
            damage = amount-(amount * fireDef);
            break;  
            case Element.ice:
            damage = amount-(amount * iceDef);
            break;
            case Element.soul:
            damage = amount-(amount * soulDef);
            break;
            case Element.thunder:
            damage = amount-(amount * thunderDef);
            break;
        }
        damage -= UnityEngine.Random.Range(randMin, randMax) * (curDef + + ManajerSkill.Instance.tempCharSOAttack.curDef);
        if(damage < 1)
        {
            damage = 0;
        }
        return Mathf.RoundToInt(damage);
    }

    public void PlayerTakeDamage(int amount, GameObject particleHit, GameObject particleDamage, Vector3 position, float time)
    {
        float rate = (float)amount/ (float)maxHealth;
        GameObject part = Instantiate(particleDamage, position, Quaternion.identity);
        TextMeshPro damageText = part.GetComponentInChildren<TextMeshPro>();
        damageText.text = amount.ToString();
        Destroy(part,time);
        if (rate < 0.3f)
        {
            currentHealth -= amount;
            GameObject go = Instantiate(particleHit, position, Quaternion.identity);
            Destroy(go, time);         
        } else if(rate >= 0.3f)
        {
            currentHealth -= amount;
            GameObject go = Instantiate(particleHit, position, Quaternion.identity);
            Destroy(go, time);
            GameObject go1 = Instantiate(particleCritical, position, Quaternion.identity);
            Destroy(go1, time);                      
        } else if (rate == 0)
        {
            GameObject go = Instantiate(particleMiss, position, Quaternion.identity);
            Destroy(go, time);
        }
    }

    public void CalculateLevelStatus(int lvl)
    {
        maxHealth = lvl * healthMultiplier + startHealth;
        maxManaPoints = lvl * manaMultiplier + startMana;
        curStr = lvl * strMultiplier + startStr;
        curDef = lvl * defMultiplier + startDef;
        curMagic = lvl * magicMultiplier + startMagic;   
    }

    public void RestoreHealthMana()
    {
        currentHealth = maxHealth;
        currentManaPoints = maxManaPoints;
    }
    public void RegenerationHealthMana()
    {
        if(currentHealth < maxHealth)
        {
            currentHealth += Convert.ToInt32(maxHealth * (regenRate + ManajerSkill.Instance.tempCharSOAttack.regenRate));
        }
        if(currentHealth >= maxHealth)
        {
            currentHealth = maxHealth;
        }
        if (currentManaPoints < maxManaPoints)
        {
            currentManaPoints += Convert.ToInt32(maxManaPoints * regenRate);
        }
        if(currentManaPoints >= maxManaPoints)
        {
            currentManaPoints = maxManaPoints;
        }
    }

    public AttackType RandomPlayerAttack(WeaponSO weaponSO, Element enemyWeakness, Element enemyImmune)
    {
        AttackType highestDamage = weaponSO.elementAttack.OrderByDescending(at=>at.amountDamage).First();
        foreach(AttackType attack in weaponSO.elementAttack)
        {
            if(attack.element == enemyWeakness)
            {
                return attack;
            } else if (attack.element == enemyImmune)
        {
            continue;
        }
        }
        return highestDamage;
    }
}
