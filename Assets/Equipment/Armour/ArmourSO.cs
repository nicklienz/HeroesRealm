using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Buat Item Baru", menuName ="HeroesRealm/Buat Item Baru/Armour")]
public class ArmourSO : ItemSO
{
    public Job job;
    public float hitDefRate, fireDefRate, iceDefRate, soulDefRate,  thunderDefRate;
    public float chanceRate, criticalRate, goldRate, expRate;
}