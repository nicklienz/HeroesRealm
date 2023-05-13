using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Buat Item Baru", menuName ="HeroesRealm/Buat Item Baru/Weapon")]
public class WeaponSO : ItemSO
{
    public Job job;
    public int manaCost;
    public List<AttackType> elementAttack;
    public float chanceRate, criticalRate, goldRate, expRate;
}
