using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum SkillStatus
{
    ready,
    active,
    coolDown,
    inactive
}

public enum SKillEffectType
{
    pemain,
    musuh
}

public enum SkillActivationType
{
    waktu
}

public class SkillSO : ScriptableObject
{
    public Job job;
    public SkillStatus skillStatus;
    public SKillEffectType skillEffectType;
    public SkillActivationType skillActivationType;
    public string skillName;
    public string skillDesc;
    public Sprite skillSprite;
    public float coolDownTime;
    public float activeTime;
    public float coolDownRemaining;
    public float activeRemaining;
    public bool unlocked;
    public int unlockLevel;
    public int skillLevel = 0;
    const  int maxSkillLevel = 5;
    public float levelMultiplier;
    public float countDownMultiplier;
    public List<SkillSO> requiredSkillToUnlock;

    public void UnlockSkill(Character character)
    {
        if (!unlocked && character.characterSO.level >= unlockLevel && character.characterSO.skillPointLeft > 0)
        {
            if (requiredSkillToUnlock.Count > 0)
            {
                foreach (SkillSO requiredSkill in requiredSkillToUnlock)
                {
                    if (requiredSkill.skillLevel != maxSkillLevel)
                    {
                        return; // Exit if any required skill is not unlocked
                    }
                }
            }
            character.characterSO.skillPointLeft--;
            character.characterSO.skillPointUsed++;
            skillStatus = SkillStatus.coolDown;
            skillLevel++;
            unlocked = true;
        }
    }
    public virtual void ApplySkillEffect(Character character)
    {
        
    }
    
    public virtual void ApplySkillEffect(Enemy enemyToAttack)
    {

    }

    public virtual void RemoveSkillEffect(Character character)
    {
        
    }

    public virtual void RemoveSkillEffect(Enemy enemyToAttack)
    {
        
    }
    
    public void UpgradeSkill (Character character)
    {
        if(unlocked && skillLevel < maxSkillLevel && character.characterSO.skillPointLeft > 0)
        {
            character.characterSO.skillPointUsed++;
            character.characterSO.skillPointLeft--;
            skillLevel++;
        }   
    }
    public IEnumerator ActivateSkill (Character character)
    {
        if(skillStatus == SkillStatus.ready)
        {
            skillStatus = SkillStatus.active;
            if(skillEffectType == SKillEffectType.pemain && skillActivationType == SkillActivationType.waktu)
            {
                ApplySkillEffect(character); 
                yield return new WaitForSeconds(activeTime);
                RemoveSkillEffect(character);
            } else if (skillEffectType == SKillEffectType.musuh && skillActivationType == SkillActivationType.waktu)
            {
                ApplySkillEffect(character.enemyToAttack);
                    yield return new WaitForSeconds(activeTime);
                RemoveSkillEffect(character.enemyToAttack);
            }
            //Debug.Log(character.characterSO.curStr);
            //Debug.Log(character.characterSO.curStr);
        }
    }
    public void RefreshSkillTime()
    {
        if(unlocked)
        {
            if(skillStatus == SkillStatus.coolDown)
            {
                coolDownRemaining -= Time.deltaTime;
                if(coolDownRemaining <= 0)
                {
                    activeRemaining = activeTime;
                    skillStatus = SkillStatus.ready;
                }
            } else if(skillStatus == SkillStatus.active)
            {
                activeRemaining -= Time.deltaTime;
                if(activeRemaining <= 0)
                {
                    coolDownRemaining = coolDownTime;
                    skillStatus = SkillStatus.coolDown;
                }
            }
        } else
        {
            skillStatus = SkillStatus.inactive;
        }
    }
}
