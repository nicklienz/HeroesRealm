
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Tilemaps;

public enum EnemyState
{
    patroli,
    kejar,
    attack
}
public class Enemy : MonoBehaviour
{
    public EnemyState enemyState;
    public EnemySO enemySO;
    [SerializeField] float intervalAttack;
    public int curEnemyHealth;
    public Color color;
    public string enemyId;
    public Vector3Int startPos, destPos, prevDest;
    public Transform playerCollided, playerDetected;
    private void Start() 
    {
        enemyId = enemySO.enemyName+ GetInstanceID();
        gameObject.name = enemyId;
        curEnemyHealth = enemySO.enemyHealth;
        if(enemySO.enemyLevel == EnemyLevel.easy)
        {
            color = Color.gray;
        } else if(enemySO.enemyLevel == EnemyLevel.medium)
        {
            color = Color.yellow;
        } else if(enemySO.enemyLevel == EnemyLevel.hard)
        {
            color = Color.red;
        }
        enemyState = EnemyState.patroli;
    }

    public IEnumerator AttackingPlayer(Character character)
    {
        while(playerCollided != null)
        {
            AttackType enemyAttack = enemySO.RandomEnemyAttack();
            int damage = character.characterSO.CalculateDamageToPlayer(enemyAttack.amountDamage, enemyAttack.element, enemySO.enemyMinAttackRate, enemySO.enemyMaxAttackRate);
            character.characterSO.PlayerTakeDamage(damage, enemyAttack.damagePrefab, enemySO.enemyDamageText, character.transform.position, enemyAttack.prefabDestroyTime);
            StartCoroutine(character.AttackingEnemy(this));
            yield return new WaitForSeconds(intervalAttack);
        }
    }

    public void EnemyDead(Character character)
    {
        Destroy(gameObject, 0f);
        GameObject go = Instantiate(enemySO.particleDead, transform.position, Quaternion.identity);
        Destroy(go, 1f);
        character.characterSO.gold += Mathf.RoundToInt(enemySO.enemyGold + enemySO.enemyGold * character.characterSO.goldRate); 
    }
}