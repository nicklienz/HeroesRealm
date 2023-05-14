using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public enum EnemyLevel
{
    easy,
    medium,
    hard
}

public enum Element
{
    hit,
    fire,
    ice,
    soul,
    thunder
}
[CreateAssetMenu(fileName = "Buat Monster Baru", menuName = "HeroesRealm/Buat Monster Baru")]
public class EnemySO : ScriptableObject
{
    public Sprite enemySprite;
    public string enemyName;
    public int enemyHealth;
    public float enemyMinAttackRate;
    public float enemyMaxAttackRate;
    public int enemyExp;
    public int enemyGold;
    public int enemyDef;
    public EnemyLevel enemyLevel;
    public float enemyWalkSpeed, enemyIntervalMove;
    public int enemyMoveRadius, enemyDetectRadius;
    public List<AttackType> enemyAttack;
    public GameObject particleCritical, particleMiss, particleDead, enemyDamageText;
    public Element enemyWeakness;
    public Element enemyImmune;

    public AttackType RandomEnemyAttack()
    {
        int rand = Random.Range(0,enemyAttack.Count);
        return enemyAttack[rand];
    }

    public int CalculateDamageToEnemy(float amount, Element element, int levelWeapon, int levelPlayer, float critRate, float chanceRate)
    {
        float damage = amount;
        float _chanceRate = (float)levelPlayer/(float)levelWeapon;
        if(element == enemyImmune)
        {
            damage *= (Random.Range(0.1f,0.3f)+ critRate);
        } else if (element == enemyWeakness)
        {
            damage *= (Random.Range(0.8f,1.6f)+ critRate);
        } else
        {
            damage *= (Random.Range(0.4f,0.7f)+ critRate);
        }
        damage -= enemyDef;
        float random = Random.Range(0f,1f);
        if ( random > (_chanceRate + chanceRate))
        {
            damage = 0;
        }
        return Mathf.RoundToInt(damage);
    }

    public void EnemyTakeDamage(int attackDamage, int amount, GameObject particleHit, GameObject particleDamage, Vector3 position, float time)
    {
        float rate = (float)amount/ (float)attackDamage;
        GameObject part = Instantiate(particleDamage, position + Vector3.up, Quaternion.Euler(90,0,0));
        TextMeshPro damageText = part.GetComponent<TextMeshPro>();
        damageText.text = amount.ToString();
        Destroy(part,time);
        if (rate > 0 && rate < 1f)
        {
            GameObject go = Instantiate(particleHit, position + Vector3.up, Quaternion.identity);
            Destroy(go, time);
        } else if(rate >= 1f)
        {
            GameObject go = Instantiate(particleHit, position + Vector3.up, Quaternion.identity);
            Destroy(go, time);
            GameObject go1 = Instantiate(particleCritical, position + Vector3.up, Quaternion.identity);      
        } else if (rate == 0)
        {
            GameObject go = Instantiate(particleMiss, position + Vector3.up, Quaternion.identity);
            Destroy(go, time);
        }
    }
}
[System.Serializable]
public class AttackType
{
    public string attackName;
    public Element element;
    public int amountDamage;
    public GameObject damagePrefab;
    public float prefabDestroyTime;
    public AttackType(string _attackName, Element _element, int _amountDamage, GameObject _damagePrefab, float _prefabDestroyTime, int _estimatedDamage)
    {
        attackName = _attackName;
        element = _element;
        amountDamage = _amountDamage;
        damagePrefab = _damagePrefab;
        prefabDestroyTime =_prefabDestroyTime;
    }
}

[System.Serializable]
public class DefenseType
{
    public Element element;
    public float rate;
    public DefenseType(Element _element, float _rate)
    {
        element = _element;
        rate = _rate;
    }
}