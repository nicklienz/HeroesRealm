using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Buat skill baru", menuName = "HeroesRealm/Buat Skill Baru/Attack")]
public class SkillAttack : SkillSO
{
    [SerializeField] private float str, def, magic, criticalRate, chanceRate, attackRange, regenDelay, regenRate ,goldRate, expRate;
    public override void ApplySkillEffect(Character character)
    {
        ManajerSkill.Instance.tempCharSOAttack.curStr = (str != 0) ? System.Convert.ToInt32(character.characterSO.curStr * (str + levelMultiplier * skillLevel)) : 0;
        ManajerSkill.Instance.tempCharSOAttack.curDef = (def != 0) ? System.Convert.ToInt32(character.characterSO.curDef * (def + levelMultiplier * skillLevel)) : 0;
        ManajerSkill.Instance.tempCharSOAttack.curMagic = (magic != 0) ? System.Convert.ToInt32(character.characterSO.curMagic * (magic + levelMultiplier * skillLevel)) : 0;
        ManajerSkill.Instance.tempCharSOAttack.criticalRate = (criticalRate != 0) ? character.characterSO.criticalRate * (criticalRate + levelMultiplier * skillLevel) : 0;
        ManajerSkill.Instance.tempCharSOAttack.chanceRate = (chanceRate != 0) ? character.characterSO.chanceRate * (chanceRate + levelMultiplier * skillLevel) : 0;
        ManajerSkill.Instance.tempCharSOAttack.regenDelay = (regenDelay != 0) ? character.characterSO.regenDelay * (regenDelay + levelMultiplier * skillLevel) : 0;
        ManajerSkill.Instance.tempCharSOAttack.regenRate = (regenRate != 0) ? character.characterSO.regenRate * (regenRate + levelMultiplier * skillLevel) : 0;
        ManajerSkill.Instance.tempCharSOAttack.expRate = (expRate != 0) ? character.characterSO.expRate * (expRate + levelMultiplier * skillLevel) : 0;
        ManajerSkill.Instance.tempCharSOAttack.goldRate = (goldRate != 0) ? character.characterSO.expRate * (goldRate + levelMultiplier * skillLevel) : 0;                         
    }

    public override void RemoveSkillEffect(Character character)
    {
        ManajerSkill.Instance.tempCharSOAttack.curStr = 0;
        ManajerSkill.Instance.tempCharSOAttack.curDef = 0;
        ManajerSkill.Instance.tempCharSOAttack.curMagic = 0;
        ManajerSkill.Instance.tempCharSOAttack.criticalRate = 0;
        ManajerSkill.Instance.tempCharSOAttack.chanceRate = 0;
        ManajerSkill.Instance.tempCharSOAttack.regenDelay = 0;
        ManajerSkill.Instance.tempCharSOAttack.regenRate = 0;
        ManajerSkill.Instance.tempCharSOAttack.expRate= 0;
        ManajerSkill.Instance.tempCharSOAttack.goldRate= 0;                         
    }

    public override void ApplySkillEffect(Enemy enemyToAttack)
    {
        enemyToAttack.enemySO.enemyDef-= System.Convert.ToInt32(enemyToAttack.enemySO.enemyDef *def);
    }
    public override void RemoveSkillEffect(Enemy enemyToAttack)
    {
        
    }
}
