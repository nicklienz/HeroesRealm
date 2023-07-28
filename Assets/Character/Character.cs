using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class Character : MonoBehaviour
{
    public CharacterSO characterSO;
    ReferenceUIPemain referenceUIPemain;
    [SerializeField] private GameObject playerDamageText;
    [SerializeField] private List<Transform> enemyCollide;
    [SerializeField] private float intervalAttack;
    public Enemy enemyToAttack;
    [SerializeField] private bool isRegen;
    public bool isCollide, isInTrigger;
    private ManajerGame manajerGame;
    private Animator animator;
    private string currentState = "idle";
    private void Start()
    {
        transform.position = characterSO.ReturnPosition();
        referenceUIPemain = GameObject.Find("ManajerDisplayUIPemain").GetComponent<ReferenceUIPemain>();
        SpawnCharacter();
        animator = GetComponentInChildren<Animator>();
        isCollide = false;
        isInTrigger = false;
        HitungTotalStatusEquipment();
        CheckCharacterLevel();
        manajerGame = GameObject.Find("ManajerGame").GetComponent<ManajerGame>();
        ChangeAnimationState(currentState);
    }

    private void SpawnCharacter()
    {
        GameObject go = Instantiate(Resources.Load<GameObject>(characterSO.modelPrefab), this.transform, false);
        go.transform.SetParent(this.transform, false);
        go.transform.localPosition = new Vector3(0.5f,0,0.5f);
        Equipment.Instance.GetEquipmentTransform(go.transform);
    }

    public void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;
        animator.Play(newState);
        currentState = newState;
    }

    public void ExitDisplayCharacterStatus()
    {
        referenceUIPemain.charStatusPanel.gameObject.SetActive(false);
    }    
    public void DisplayCharacterStatus()
    {
        HitungTotalStatusEquipment();
        referenceUIPemain.gameObject.SetActive(true);
        referenceUIPemain.nameText.text = characterSO.userName.ToString();
        referenceUIPemain.jobText.text = characterSO.job.ToString();
        referenceUIPemain.levelText.text = characterSO.level.ToString();
        referenceUIPemain.strText.text = characterSO.curStr.ToString();
        referenceUIPemain.defText.text = characterSO.curDef.ToString();
        referenceUIPemain.magicText.text = characterSO.curMagic.ToString();
        referenceUIPemain.criticalText.text = $"{(characterSO.criticalRate*100).ToString()}%";
        referenceUIPemain.chanceText.text = $"{(characterSO.chanceRate*100).ToString()}%";
        referenceUIPemain.expRateText.text = $"{(characterSO.expRate*100).ToString()}%";
        referenceUIPemain.goldRateText.text = $"{(characterSO.goldRate*100).ToString()}%";
        referenceUIPemain.hitAttText.text = $"{characterSO.hitAtk.ToString()}";
        referenceUIPemain.fireAtt.text = $"{characterSO.fireAtk.ToString()}";
        referenceUIPemain.iceAtt.text = $"{characterSO.iceAtk.ToString()}";
        referenceUIPemain.soulAtt.text = $"{characterSO.soulAtk.ToString()}";
        referenceUIPemain.thunderAtt.text = $"{characterSO.thunderAtk.ToString()}";
        referenceUIPemain.hitDef.text = $"{(characterSO.hitDef*100).ToString()}%";
        referenceUIPemain.fireDef.text = $"{(characterSO.fireDef*100).ToString()}%";
        referenceUIPemain.iceDef.text = $"{(characterSO.iceDef*100).ToString()}%";
        referenceUIPemain.soulDef.text = $"{(characterSO.soulDef*100).ToString()}%";
        referenceUIPemain.thunderDef.text = $"{(characterSO.thunderDef*100).ToString()}%";
    }
    public void CheckCharacterLevel()
    {
        int levelNew = ManajerLevel.Instance.GetLevelForExp(characterSO.experiencePoints);
        characterSO.CalculateLevelStatus(levelNew);
        if(levelNew != characterSO.level)
        {
            ChangeAnimationState("level_up");
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
        referenceUIPemain.expSlider.value = ((float)characterSO.experiencePoints-beforeExp)/((float)ManajerLevel.Instance.GetExpForLevel(characterSO.level+1)-beforeExp);
        referenceUIPemain.expText.text = characterSO.experiencePoints.ToString() +"/"+ ManajerLevel.Instance.GetExpForLevel(characterSO.level+1).ToString();
        referenceUIPemain.healthSlider.value = ((float)characterSO.currentHealth/(float)characterSO.maxHealth);
        referenceUIPemain.manaSlider.value = ((float)characterSO.currentManaPoints/(float)characterSO.maxManaPoints);
        referenceUIPemain.userNameText.text = characterSO.userName;
        referenceUIPemain.healthText.text = characterSO.currentHealth+"/"+characterSO.maxHealth;
        referenceUIPemain.lvlText.text = characterSO.level.ToString();
        referenceUIPemain.goldText.text = characterSO.gold.ToString();
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
        //ChangeAnimationState("idle");
        yield return new WaitForSeconds(characterSO.regenDelay - ManajerSkill.Instance.tempCharSOAttack.regenDelay);
        if(!isCollide && enemyToAttack == null)
        {
            //animator.Play("idle");
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
        referenceUIPemain.gameObject.SetActive(true);
        referenceUIPemain.enemyName.gameObject.SetActive(true);
        referenceUIPemain.enemyIcon.gameObject.SetActive(true);
        referenceUIPemain.enemyLevel.gameObject.SetActive(true);
        referenceUIPemain.enemyIcon.sprite = enemyToAttack.enemySO.enemySprite;
        referenceUIPemain.enemyLevel.color = enemyToAttack.color;
        referenceUIPemain.enemyName.text = enemyToAttack.enemySO.enemyName;
        referenceUIPemain.enemyHealthSlider.value = (float)enemyToAttack.curEnemyHealth/ (float)enemyToAttack.enemySO.enemyHealth;
    }
    private IEnumerator HideEnemyStat()
    {
        yield return new WaitForSeconds(1.5f);
        referenceUIPemain.enemyHealthSlider.gameObject.SetActive(false);
        referenceUIPemain.enemyName.gameObject.SetActive(false);
        referenceUIPemain.enemyIcon.gameObject.SetActive(false);
        referenceUIPemain.enemyLevel.gameObject.SetActive(false);
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
        ChangeAnimationState("battle");
        if (enemy.curEnemyHealth > 0 && enemyToAttack != null && characterSO.currentManaPoints >= Equipment.Instance.weaponEquipped.manaCost)
        {
            yield return new WaitForSeconds(intervalAttack);
            ShowEnemyStat();
            AttackType playerAttack = characterSO.RandomPlayerAttack(Equipment.Instance.weaponEquipped, enemy.enemySO.enemyWeakness, enemy.enemySO.enemyImmune);
            int damage = enemy.enemySO.CalculateDamageToEnemy(playerAttack.amountDamage + characterSO.curStr + ManajerSkill.Instance.tempCharSOAttack.curStr,playerAttack.element,Equipment.Instance.weaponEquipped.levelUnlock, characterSO.level, characterSO.criticalRate, characterSO.chanceRate);
            characterSO.currentManaPoints -= Equipment.Instance.weaponEquipped.manaCost;
            ChangeAnimationState("attack");
            CheckCharacterLevel();
            CheckCharacterDead();
            enemy.curEnemyHealth -= damage;
            enemy.enemySO.EnemyTakeDamage(playerAttack.amountDamage, damage, playerAttack.damagePrefab, playerDamageText, enemy.transform.position, playerAttack.prefabDestroyTime);
            ShowEnemyStat();
        }
        if (enemy.curEnemyHealth <= 0)
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
            ChangeAnimationState("dead");
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
            referenceUIPemain.panelPick.SetActive(true);
            ConfirmPickItem(other.gameObject);
        }
    }

    private void ConfirmPickItem(GameObject go)
    {
        referenceUIPemain.yesPick.onClick.RemoveAllListeners();
        referenceUIPemain.noPick.onClick.RemoveAllListeners();
        referenceUIPemain.yesPick.onClick.AddListener(() => PickItem(go));
        referenceUIPemain.noPick.onClick.AddListener(() => CancelPick());   
    }

    private void CancelPick()
    {
        referenceUIPemain.panelPick.SetActive(false);
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
        referenceUIPemain.panelPick.SetActive(false);
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

    public void LogoutAccount()
    {
        AccountDatabase.LogoutAccount(characterSO.userName);
    }
}