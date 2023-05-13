
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
    [SerializeField] bool isCollide;
    [SerializeField] bool attacking;
    Character character;
    public int curEnemyHealth;
    [SerializeField] Slider enemyHealthSlider;
    [SerializeField] TextMeshProUGUI enemyName;
    [SerializeField] Image imageIcon;
    public string enemyId;
    public Vector3Int startPos, destPos, prevDest;
    BoxCollider boxCollider;
    private void Start() 
    {
        boxCollider = gameObject.GetComponent<BoxCollider>();
        boxCollider.size = new Vector3(enemySO.enemyDetectRadius, 1f, enemySO.enemyDetectRadius);
        enemyId = enemySO.enemyName+ GetInstanceID();
        gameObject.name = enemyId;
        curEnemyHealth = enemySO.enemyHealth;
        enemyName.text = enemySO.enemyName;
        if(enemySO.enemyLevel == EnemyLevel.easy)
        {
            imageIcon.color = Color.gray;
        } else if(enemySO.enemyLevel == EnemyLevel.medium)
        {
            imageIcon.color = Color.yellow;
        } else if(enemySO.enemyLevel == EnemyLevel.hard)
        {
            imageIcon.color = Color.red;
        }
        DisplayEnemyStat();
        enemyState = EnemyState.patroli;
    }

    private void DisplayEnemyStat()
    {
        enemyHealthSlider.value = (float)curEnemyHealth/ (float)enemySO.enemyHealth;
    }

    private void OnCollisionEnter(Collision other) 
    {
        if(other.gameObject.tag == "Player")
        {
            character = other.gameObject.GetComponent<Character>();
            isCollide = true;
            enemyState = EnemyState.attack;
        }    
    }

    private void OnCollisionExit(Collision other) 
    {
        if(other.gameObject.tag == "Pemain")
        {
            isCollide = false;
            attacking = false;
        }            
    }
    private void OnTriggerEnter(Collider other) 
    {
        //Debug.Log("enter");
        if(other.gameObject.tag == "Player")
        {
            enemyState = EnemyState.kejar;
            Vector3 playerPos = other.gameObject.transform.position;
            Debug.Log(this.gameObject.name +" enter player");
        }            
    }

    private void OnTriggerExit(Collider other) 
    {
        //Debug.Log("exit");
        if(other.gameObject.tag == "Player")
        {
            enemyState = EnemyState.patroli;
            Debug.Log(this.gameObject.name +" exit player");
        }            
    }    
    private void FixedUpdate()
    {
        DisplayEnemyStat();
        if(isCollide && !attacking)
        {
            StartCoroutine(AttackingPlayer());
        } else if (!isCollide)
        {
            StopCoroutine(AttackingPlayer());
        }
    }

    private IEnumerator AttackingPlayer()
    {
        attacking = true;
        yield return new WaitForSeconds(intervalAttack);
        if(isCollide)
        {
            AttackType enemyAttack = enemySO.RandomEnemyAttack();
            int damage = character.characterSO.CalculateDamageToPlayer(enemyAttack.amountDamage, enemyAttack.element, enemySO.enemyMinAttackRate, enemySO.enemyMaxAttackRate);
            character.characterSO.PlayerTakeDamage(damage, enemyAttack.damagePrefab, enemySO.enemyDamageText, character.transform.position, enemyAttack.prefabDestroyTime);
        }
        attacking = false;
    }

    public void EnemyDead(Character character)
    {
        Destroy(gameObject, 0f);
        GameObject go = Instantiate(enemySO.particleDead, transform.position, Quaternion.identity);
        Destroy(go, 1f);
        character.characterSO.gold += Mathf.RoundToInt(enemySO.enemyGold + enemySO.enemyGold * character.characterSO.goldRate); 
    }
}