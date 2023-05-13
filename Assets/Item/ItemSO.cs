using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TipeItem
{
    Weapon,
    Armour,
    Head,
    Legs,
    Shield,
    Arm,
    Ring,
    Potion,
    Powerup,
    Skill,
    Quest
}

public enum Consume
{
    Consumable,
    Permanent
}

public class ItemSO : ScriptableObject
{
    public TipeItem itemType;
    public Consume consume;
    public string itemName;
    public string itemDesc;
    public GameObject prefab;
    public Sprite itemIcon;
    public int hargaBeli;
    public int hargaJual;
    public int levelUnlock;
}