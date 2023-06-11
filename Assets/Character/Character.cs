using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class Character : MonoBehaviour
{
    public CharacterSO characterSO;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider manaSlider;
    [SerializeField] private Slider expSlider;
    [SerializeField] private TextMeshProUGUI userNameText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI lvlText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI expText;
    [SerializeField] private List<Transform> enemyCollide;
    [SerializeField] private float intervalAttack;
    public Enemy enemyToAttack;
    [SerializeField] private Slider enemyHealthSlider;
    [SerializeField] private TextMeshProUGUI enemyName;
    [SerializeField] private Image enemyIcon, enemyLevel;
    [SerializeField] private GameObject panelPick;
    [SerializeField] private Button yesPick, noPick;
    [SerializeField] private bool isRegen;
    public bool isCollide, isInTrigger;
    ManajerGame manajerGame;
    
    private void Start()
    {
        isCollide = false;
        isInTrigger = false;
        HitungTotalStatusEquipment();
        CheckCharacterLevel();
        manajerGame = GameObject.Find("ManajerGame").GetComponent<ManajerGame>();
        //Button yesPick = panelPick.transform.Find("Yes").GetComponent<Button>();
        //Button noPick = panelPick.transform.Find("No").GetComponent<Button>();
    }
    public void CheckCharacterLevel()
    {
        int levelNew = ManajerLevel.Instance.GetLevelForExp(characterSO.experiencePoints);
        characterSO.CalculateLevelStatus(levelNew);
        if(levelNew != characterSO.level)
        {
            characterSO.RestoreHealthMana();
            if (characterSO.skillPointLeft + characterSO.skillPointUsed <= levelNew)
            {
                characterSO.skillPointLeft = levelNew - characterSO.skillPointUsed;
            } else
            {
                ManajerSkill.Instance.ResetSkill();
                characterSO.skillPointLeft = levelNew;
                characterSO.skillPointUsed = 0;
            }
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
        } else if (isCollide)
        {
            StopCoroutine(Regen());
        }
    }

    private IEnumerator Regen()
    {
        isRegen = true;
        yield return new WaitForSeconds(characterSO.regenDelay - ManajerSkill.Instance.tempCharSOAttack.regenDelay);
        if(!isCollide && enemyToAttack == null)
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
    private void ShowEnemyStat()
    {
        enemyHealthSlider.gameObject.SetActive(true);
        enemyName.gameObject.SetActive(true);
        enemyIcon.gameObject.SetActive(true);
        enemyLevel.gameObject.SetActive(true);
        enemyIcon.sprite = enemyToAttack.enemySO.enemySprite;
        enemyLevel.color = enemyToAttack.color;
        enemyName.text = enemyToAttack.enemySO.enemyName;
        enemyHealthSlider.value = (float)enemyToAttack.curEnemyHealth/ (float)enemyToAttack.enemySO.enemyHealth;
    }
    private IEnumerator HideEnemyStat()
    {
        yield return new WaitForSeconds(1.5f);
        enemyHealthSlider.gameObject.SetActive(false);
        enemyName.gameObject.SetActive(false);
        enemyIcon.gameObject.SetActive(false);
        enemyLevel.gameObject.SetActive(false);
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
        isCollide = true;
        CollideEnemy();
        if(other.gameObject.tag == "Tree")
        {
            ManajerNotification.Instance.messageErrorText.text = other.gameObject.name;
            StartCoroutine(ManajerNotification.instance.ShowMessage(MessageType.Error));
        }    
    }
    private void OnCollisionExit(Collision other) 
    {
        CollideEnemy();              
        isCollide = false;
    }

    public IEnumerator AttackingEnemy(Enemy enemy)
    {
        if (enemy.curEnemyHealth > 0 && enemyToAttack != null && characterSO.currentManaPoints >= Equipment.Instance.weaponEquipped.manaCost)
        {
            yield return new WaitForSeconds(intervalAttack);
            ShowEnemyStat();
            AttackType playerAttack = characterSO.RandomPlayerAttack(Equipment.Instance.weaponEquipped, enemy.enemySO.enemyWeakness, enemy.enemySO.enemyImmune);
            int damage = enemy.enemySO.CalculateDamageToEnemy(playerAttack.amountDamage + characterSO.curStr + ManajerSkill.Instance.tempCharSOAttack.curStr,playerAttack.element,Equipment.Instance.weaponEquipped.levelUnlock, characterSO.level, characterSO.criticalRate, characterSO.chanceRate);
            characterSO.currentManaPoints -= Equipment.Instance.weaponEquipped.manaCost;
            CheckCharacterLevel();
            CheckCharacterDead();
            enemy.curEnemyHealth -= damage;
            enemy.enemySO.EnemyTakeDamage(playerAttack.amountDamage, damage, playerAttack.damagePrefab, characterSO.playerDamageText, enemy.transform.position, playerAttack.prefabDestroyTime);
            ShowEnemyStat();
        }
        else if (enemy.curEnemyHealth <= 0)
        {
            enemy.EnemyDead(this);
            characterSO.experiencePoints += Mathf.RoundToInt(enemy.enemySO.enemyExp + (enemy.enemySO.enemyExp * (characterSO.expRate + ManajerSkill.Instance.tempCharSOAttack.expRate)));
            RemoveEnemy(this.transform); 
            ManajerEnemy.Instance.RemoveEnemy();  
            StartCoroutine(HideEnemyStat());        
        } else if(enemyToAttack == null)
        {
            yield break;
        }
        CheckCharacterLevel();
        CheckCharacterDead();
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
    private void CollideEnemy()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.6f);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
            {
                if(enemyToAttack == null)
                {
                    isCollide = true;
                    enemyToAttack = collider.gameObject.GetComponent<Enemy>();
                }
                break;
            } else
            {
                enemyToAttack = null;
                isCollide = false;
            }
        }
    }
}