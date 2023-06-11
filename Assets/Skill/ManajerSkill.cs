using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ManajerSkill : MonoBehaviour
{
    public static ManajerSkill instance;
    public static ManajerSkill Instance
    {
        get
        {
            // Jika instance belum ada, cari di scene
            if (instance == null)
            {
                instance = FindObjectOfType<ManajerSkill>();
            }
            
            return instance;
        }
    }
    [SerializeField] private SkillSO skillAttackSelected;
    [SerializeField] private SkillSO skillDefenseSelected;
    [SerializeField] private Image skillAttackSelectedIcon;
    [SerializeField] private Image skillDefSelectedIcon;
    [SerializeField] private Image skillAttackSlider;
    [SerializeField] private Image skillDefSlider;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button defenseButton;
    [SerializeField] private TextMeshProUGUI skillPoints;
    [SerializeField] Character character;
    [SerializeField] private List<SkillSO> skillAttackList;
    [SerializeField] private List<SkillSO> skillDefenseList;
    [SerializeField] private GameObject skillTemplate;
    [SerializeField] private Transform slotAttackParent;
    [SerializeField] private GameObject slotSkill;
    public CharacterSO tempCharSOAttack;
    public CharacterSO tempCharSODefense;

    private void Start() 
    {
        DisplaySkillList();
    }

    private void Update() 
    {
        if(skillAttackSelected != null)
        {
            skillAttackSelected.RefreshSkillTime();     
            if(skillAttackSelected.skillStatus == SkillStatus.coolDown)
            {
                skillAttackSlider.color = Color.gray;
                skillAttackSelectedIcon.color = Color.gray;                
                skillAttackSlider.fillAmount = 1 - (skillAttackSelected.coolDownRemaining/skillAttackSelected.coolDownTime);
            } else if(skillAttackSelected.skillStatus == SkillStatus.ready)
            {
                skillAttackSlider.color = Color.white;
                skillAttackSelectedIcon.color = Color.white;  
                skillAttackSlider.fillAmount = 1;
            } else if (skillAttackSelected.skillStatus == SkillStatus.active)
            {
                skillAttackSlider.fillAmount = skillAttackSelected.activeRemaining/skillAttackSelected.activeTime;
                skillAttackSlider.color = Color.green;
            }   
        }
    }
    
    public void DisplayActiveSkill()
    {
        if(skillAttackSelected == null)
        {
            return;        
        }
        skillAttackSelectedIcon.sprite = skillAttackSelected.skillSprite;
        attackButton.onClick.RemoveAllListeners();
        attackButton.onClick.AddListener(() => StartCoroutine(skillAttackSelected.ActivateSkill(character)));    
    }

    public void DisplaySkillList()
    {
        if(slotAttackParent.childCount > 0)
        {
            foreach (Transform child in slotAttackParent)
            {
                //GameObject.Destroy(child.gameObject);
                Destroy(child.gameObject,0f);
            }
        }
        for(int i = 0; i < skillAttackList.Count; i++)
        {
            int index = i;
            GameObject slot = Instantiate(slotSkill, Vector3.zero, Quaternion.identity);
            Image icon = slot.transform.Find("Skill_Icon").GetComponent<Image>();
            List<Image> levels = new List<Image>();
            levels.Add(slot.transform.Find("Skill_Level1").GetComponent<Image>());
            levels.Add(slot.transform.Find("Skill_Level2").GetComponent<Image>());
            levels.Add(slot.transform.Find("Skill_Level3").GetComponent<Image>());
            levels.Add(slot.transform.Find("Skill_Level4").GetComponent<Image>());
            levels.Add(slot.transform.Find("Skill_Level5").GetComponent<Image>());
            TextMeshProUGUI textSkillName = slot.transform.Find("Skill_Name").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI textSkillDesc = slot.transform.Find("Skill_Desc").GetComponent<TextMeshProUGUI>();
            Button upgradeButton = slot.transform.Find("Button_Upgrade").GetComponent<Button>();
            Button selectButton = slot.transform.Find("Button_Select").GetComponent<Button>();
            slot.name = skillAttackList[i].skillName;
            slot.transform.SetParent(slotAttackParent);
            slot.transform.localScale = new Vector3(1f,1f,1f);
            icon.sprite = skillAttackList[i].skillSprite;
            textSkillName.text = skillAttackList[i].skillName;
            textSkillDesc.text = skillAttackList[i].skillDesc;
            upgradeButton.onClick.AddListener(() => UpgradeSkill(skillAttackList[index]));
            selectButton.onClick.AddListener(() => SelectSkill(skillAttackList[index]));
            if(!skillAttackList[i].unlocked)
            {
                slot.GetComponent<Image>().color = Color.grey;
                selectButton.gameObject.SetActive(false);
                string requiredText = string.Empty;
                foreach (SkillSO skill in skillAttackList[i].requiredSkillToUnlock)
                {
                    requiredText += skill.skillName + ", ";
                }
                textSkillDesc.text = skillAttackList[i].skillDesc +". Minimum level: "+ skillAttackList[i].unlockLevel +", Required skill   " + requiredText;
            }

            if(skillAttackList[i].skillLevel == 5)
            {
                upgradeButton.gameObject.SetActive(false);
            }

            for (int j = 0; j < levels.Count; j++)
            {
                if (j < skillAttackList[i].skillLevel)
                {
                    //levels[j].color = Color.clear;
                }
                else
                {
                    levels[j].color = Color.gray;
                }
            }
        }
        skillPoints.text = "Left: "+character.characterSO.skillPointLeft +", Used : "+character.characterSO.skillPointUsed;
    }

    private void SelectSkill(SkillSO skill)
    {
        if(skillAttackSelected != null)
        {
            if(skillAttackSelected.skillStatus == SkillStatus.active)
            {
                skillAttackSelected.RemoveSkillEffect(character);
                skillAttackSelected.coolDownRemaining = skillAttackSelected.coolDownTime;
                skillAttackSelected.skillStatus = SkillStatus.coolDown;            
            }
        }
        skillAttackSelected = skill;
        skillAttackSelected.coolDownRemaining = skillAttackSelected.coolDownTime;
        skillAttackSelected.skillStatus = SkillStatus.coolDown;
        DisplayActiveSkill();
    }

    private void UpgradeSkill(SkillSO skill)
    {
        if(skill.unlocked && character.characterSO.skillPointLeft > 0)
        {
            skill.skillLevel++;
            character.characterSO.skillPointLeft--;
            character.characterSO.skillPointUsed++;            
        } else
        {
            skill.UnlockSkill(character);
        }
        DisplaySkillList();
    }
    public void ResetSkill()
    {
        foreach(SkillSO skillAttack in skillAttackList)
        {
            skillAttack.unlocked = false;
            skillAttack.skillLevel = 0;
            skillAttack.skillStatus = SkillStatus.inactive;
        }
        DisplaySkillList();
    }
    public void ExitSkillList()
    {
        if(slotAttackParent.childCount > 0)
        {
            foreach (Transform child in slotAttackParent)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        skillTemplate.gameObject.SetActive(false);
    }

    public void EnterSkillList()
    {
        skillTemplate.gameObject.SetActive(true);
        DisplaySkillList();
    }
}
