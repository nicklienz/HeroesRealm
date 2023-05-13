using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class Character : MonoBehaviour
{
    public CharacterSO characterSO;
    [SerializeField] Slider healthSlider;
    [SerializeField] Slider manaSlider;
    [SerializeField] Slider expSlider;
    [SerializeField] TextMeshProUGUI userNameText;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI lvlText;
    [SerializeField] TextMeshProUGUI goldText;
    [SerializeField] TextMeshProUGUI expText;
    [SerializeField] List<Transform> enemyCollide;
    [SerializeField] float intervalAttack;
    [SerializeField] Enemy enemyToAttack;
    [SerializeField] GameObject panelPick;
    [SerializeField] Button yesPick, noPick;
    [SerializeField] bool isRegen;
    public bool isCollide, attacking, isInTrigger;
    ManajerGame manajerGame;
    
    private void Start()
    {
        isCollide = false;
        attacking = false;
        isInTrigger = false;
        HitungTotalStatusEquipment();
        CheckCharacterLevel();
        manajerGame = GameObject.Find("ManajerGame").GetComponent<ManajerGame>();
        Button yesPick = panelPick.transform.Find("Yes").GetComponent<Button>();
        Button noPick = panelPick.transform.Find("No").GetComponent<Button>();
    }
    public void CheckCharacterLevel()
    {
        int levelNew = ManajerLevel.Instance.GetLevelForExp(characterSO.experiencePoints);
        characterSO.CalculateLevelStatus(levelNew);
        if(levelNew != characterSO.level)
        {
            characterSO.RestoreHealthMana();
        }
        characterSO.level = levelNew;
        float beforeExp = ManajerLevel.Instance.GetExpForLevel(characterSO.level);
        expSlider.value = ((float)characterSO.experiencePoints-beforeExp)/((float)ManajerLevel.Instance.GetExpForLevel(characterSO.level+1)-beforeExp);
        expText.text = characterSO.experiencePoints.ToString() +"/"+ ManajerLevel.Instance.GetExpForLevel(characterSO.level+1).ToString();
        healthSlider.value = ((float)characterSO.currentHealth/(float)characterSO.maxHealth);
        manaSlider.value = ((float)characterSO.currentManaPoints/(float)characterSO.maxManaPoints);
        userNameText.text = characterSO.userName;
        healthText.text = characterSO.currentHealth+"/"+characterSO.maxHealth;
        lvlText.text = characterSO.level.ToString();
        goldText.text = characterSO.gold.ToString();
    }

    public void HitungTotalStatusEquipment()
    {
        characterSO.hitAtk = Equipment.Instance.TotalWeaponStat(Element.hit);
        characterSO.fireAtk = Equipment.Instance.TotalWeaponStat(Element.fire);
        characterSO.iceAtk = Equipment.Instance.TotalWeaponStat(Element.ice);
        characterSO.soulAtk = Equipment.Instance.TotalWeaponStat(Element.soul);
        characterSO.thunderAtk = Equipment.Instance.TotalWeaponStat(Element.thunder);
        characterSO.hitDef = Equipment.Instance.TotalArmourStatElement()[0];
        characterSO.fireDef = Equipment.Instance.TotalArmourStatElement()[1];
        characterSO.iceDef = Equipment.Instance.TotalArmourStatElement()[2];
        characterSO.soulDef = Equipment.Instance.TotalArmourStatElement()[3];
        characterSO.thunderDef = Equipment.Instance.TotalArmourStatElement()[4];
        characterSO.chanceRate = Equipment.Instance.weaponEquipped.chanceRate + Equipment.Instance.TotalArmourStatElement()[5];
        characterSO.criticalRate = Equipment.Instance.weaponEquipped.criticalRate + Equipment.Instance.TotalArmourStatElement()[6];
        characterSO.goldRate = Equipment.Instance.weaponEquipped.goldRate + Equipment.Instance.TotalArmourStatElement()[7];
        characterSO.expRate = Equipment.Instance.weaponEquipped.expRate + Equipment.Instance.TotalArmourStatElement()[8];
    }
    private void Update() 
    {
        if(!isCollide && !isRegen)
        {
            StartCoroutine(Regen());
        } else if (isCollide && !attacking)
        {
            CheckCharacterDead();
            StopCoroutine(Regen());
            if(enemyToAttack.curEnemyHealth > 0 && enemyToAttack != null)
            {
                StartCoroutine(AttackingEnemy(enemyToAttack));
            } else if (enemyToAttack.curEnemyHealth <= 0 && enemyToAttack != null)
            {
                StopCoroutine(AttackingEnemy(enemyToAttack));
                enemyToAttack.EnemyDead(this);
                characterSO.experiencePoints += Mathf.RoundToInt(enemyToAttack.enemySO.enemyExp + (enemyToAttack.enemySO.enemyExp * characterSO.expRate));
                CheckCharacterLevel();
                RemoveEnemy(this.transform); 
                ManajerEnemy.Instance.RemoveEnemy();
                isCollide = false;            
            }
        }
    }

    private IEnumerator Regen()
    {
        isRegen = true;
        yield return new WaitForSeconds(characterSO.regenDelay);
        if(!isCollide)
        {
            characterSO.RegenerationHealthMana();
            CheckCharacterLevel();
        }
        yield return null;
        isRegen = false;
    }
    public void AddEnemy(Transform enemy)
    {
        if (!enemyCollide.Contains(enemy))
        {
            enemyCollide.Add(enemy);
        }
    }

    public void RemoveEnemy(Transform enemy)
    {
        if (enemyCollide.Contains(enemy))
        {
            enemyCollide.Remove(enemy);
        }
    }
    private void OnCollisionEnter(Collision other) 
    {
        if(other.gameObject.tag == "Tree")
        {
            ManajerNotification.Instance.messageErrorText.text = other.gameObject.name;
            StartCoroutine(ManajerNotification.instance.ShowMessage(MessageType.Error));
        }    
        if(other.gameObject.tag == "Enemy")
        {
            isCollide = true;
            AddEnemy(other.transform);
            enemyToAttack = other.gameObject.GetComponent<Enemy>();
        }    
    }
    private void OnCollisionExit(Collision other) 
    {
        if(other.gameObject.tag == "Enemy")
        {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            isCollide = false;
            attacking = false;
            RemoveEnemy(other.transform);
            enemyToAttack = null;
        }                
    }

    private IEnumerator AttackingEnemy(Enemy enemy)
    {
        attacking = true;
        yield return new WaitForSeconds(intervalAttack);
        if(isCollide)
        {
            AttackType playerAttack = characterSO.RandomPlayerAttack(Equipment.Instance.weaponEquipped, enemy.enemySO.enemyWeakness, enemy.enemySO.enemyImmune);
            int damage = enemy.enemySO.CalculateDamageToEnemy(playerAttack.amountDamage + characterSO.curStr,playerAttack.element,Equipment.Instance.weaponEquipped.levelUnlock, characterSO.level, characterSO.criticalRate, characterSO.chanceRate);
            enemy.curEnemyHealth -= damage;
            enemy.enemySO.EnemyTakeDamage(playerAttack.amountDamage, damage, playerAttack.damagePrefab, characterSO.playerDamageText, enemy.transform.position, playerAttack.prefabDestroyTime);
        }
        attacking = false;
    }

    private void CheckCharacterDead()
    {
        if(characterSO.currentHealth <= 0)
        {
            characterSO.experiencePoints = System.Convert.ToInt32(characterSO.experiencePoints * 0.9f);
            characterSO.CalculateLevelStatus(characterSO.level);
            manajerGame.RestartGame();
            characterSO.currentHealth = characterSO.maxHealth;
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if(other.gameObject.tag == "Weapon" || 
        other.gameObject.tag == "Potion" || 
        other.gameObject.tag == "Head"  || 
        other.gameObject.tag == "Shield"|| 
        other.gameObject.tag == "Body"|| 
        other.gameObject.tag == "Legs"|| 
        other.gameObject.tag == "Ring"|| 
        other.gameObject.tag == "Arm")
        {
            isInTrigger = true;
            panelPick.SetActive(true);
            ConfirmPickItem(other.gameObject);
        }
    }

    private void ConfirmPickItem(GameObject go)
    {
        yesPick.onClick.RemoveAllListeners();
        noPick.onClick.RemoveAllListeners();
        yesPick.onClick.AddListener(() => PickItem(go));
        noPick.onClick.AddListener(() => CancelPick());   
    }

    private void CancelPick()
    {
        panelPick.SetActive(false);
    }
    private void PickItem(GameObject other)
    {
        if(ManajerInventory.Instance.inventory.SlotKosong() == true)
        {
            ManajerInventory.Instance.inventory.AddItem(other.gameObject.GetComponent<Item>().item);
            ManajerInventory.Instance.DisplayInventory();
            Destroy(other.gameObject, 0f);
            isInTrigger = false;
            ManajerNotification.Instance.messageSuccessText.text = other.gameObject.name;
        } else
        {
            StartCoroutine(ManajerNotification.instance.ShowMessage(MessageType.Error));
            ManajerNotification.Instance.messageErrorText.text = "Tas penuh!";
        }
        panelPick.SetActive(false);
    }
    private void OnTriggerExit(Collider other) 
    {

        isInTrigger = false;
        CancelPick();
    }
}