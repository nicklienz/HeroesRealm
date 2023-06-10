using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Buat skill baru", menuName = "HeroesRealm/Buat Skill Baru/Attack")]
public class SkillAttack : SkillSO
{
    [SerializeField] private float str, def, magic, criticalRate, chanceRate, attackRange, regenDelay, regenRate;
    [SerializeField] private float strBase, defBase, magicBase, criticalRateBase, chanceRateBase, attackRangeBase, regenDelayBase, regenRateBase;
    public override void ApplySkillEffect(Character character)
    {
        character.characterSO.curStr += System.Convert.ToInt32(character.characterSO.curStr * (strBase + skillLevel * str));
        character.characterSO.curDef += System.Convert.ToInt32(character.characterSO.curDef * (defBase + skillLevel * def));
        character.characterSO.curMagic += System.Convert.ToInt32(character.characterSO.curMagic * (magicBase + skillLevel * magic));
        character.characterSO.criticalRate += System.Convert.ToInt32(character.characterSO.criticalRate * (criticalRateBase + skillLevel * criticalRate));
        character.characterSO.chanceRate += System.Convert.ToInt32(character.characterSO.chanceRate* (chanceRateBase + skillLevel * chanceRateBase));
        character.characterSO.regenDelay += System.Convert.ToInt32(character.characterSO.regenDelay* (regenDelayBase + skillLevel * regenDelay));
        character.characterSO.regenRate += System.Convert.ToInt32(character.characterSO.regenRate* (regenRateBase + skillLevel * regenRateBase));
    }

    public override void RemoveSkillEffect(Character character)
    {
        character.characterSO.curStr -= System.Convert.ToInt32(character.characterSO.curStr *(strBase + skillLevel * str));
        character.characterSO.curDef -= System.Convert.ToInt32(character.characterSO.curDef * (defBase + skillLevel * def));
        character.characterSO.curMagic -= System.Convert.ToInt32(character.characterSO.curMagic * (magicBase + skillLevel * magic));
        character.characterSO.criticalRate -= System.Convert.ToInt32(character.characterSO.criticalRate * (criticalRateBase + skillLevel * criticalRate));
        character.characterSO.chanceRate -= System.Convert.ToInt32(character.characterSO.chanceRate* (chanceRateBase + skillLevel * chanceRateBase));
        character.characterSO.regenDelay -= System.Convert.ToInt32(character.characterSO.regenDelay*(regenDelayBase + skillLevel * regenDelay));
        character.characterSO.regenRate -= System.Convert.ToInt32(character.characterSO.regenRate* (regenRateBase + skillLevel * regenRateBase));
    }
}
