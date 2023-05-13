using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Equipment : MonoBehaviour
{
    public static Equipment instance;
    public static Equipment Instance
    {
        get
        {
            // Jika instance belum ada, cari di scene
            if (instance == null)
            {
                instance = FindObjectOfType<Equipment>();
            }
            
            return instance;
        }
    }
    public WeaponSO weaponEquipped, defaultWeapon;
    public ArmourSO headEquipped, shieldEquipped, armEquipped, bodyEquipped, legsEquipped, ringEquipped;
    public Image slotWpn, slotHead, slotShield, slotArm, slotBody, slotLegs, slotRing;
    public Sprite nullHead, nullShield, nullArm, nullBody, nullLegs, nullRing;
    public int TotalWeaponStat(Element element)
    {
        int attack = 0;
        if(weaponEquipped == null)
        {
            attack = 0;
        } else if (weaponEquipped != null)
        {
            foreach(AttackType attackType in weaponEquipped.elementAttack)
            {
                if(attackType.element == element)
                {
                    attack += attackType.amountDamage;
                }
            }
        }
        return attack;
    }

    private void Start()
    {
        DisplayEquipment();
        ManajerInventory.Instance.DisplayInventory();
    }
    public List<float> TotalArmourStatElement()
    {
        List<float> list = new List<float>();
        list.Add((headEquipped?.hitDefRate ?? 0) + (shieldEquipped?.hitDefRate ?? 0) + (armEquipped?.hitDefRate ?? 0) + (bodyEquipped?.hitDefRate ?? 0) + (legsEquipped?.hitDefRate ?? 0) + (ringEquipped?.hitDefRate ?? 0));
        list.Add((headEquipped?.fireDefRate ?? 0) + (shieldEquipped?.fireDefRate ?? 0) + (armEquipped?.fireDefRate ?? 0) + (bodyEquipped?.fireDefRate ?? 0) + (legsEquipped?.fireDefRate ?? 0) + (ringEquipped?.fireDefRate ?? 0));
        list.Add((headEquipped?.iceDefRate ?? 0) + (shieldEquipped?.iceDefRate ?? 0) + (armEquipped?.iceDefRate ?? 0) + (bodyEquipped?.iceDefRate ?? 0) + (legsEquipped?.iceDefRate ?? 0) + (ringEquipped?.iceDefRate ?? 0));
        list.Add((headEquipped?.soulDefRate ?? 0) + (shieldEquipped?.soulDefRate ?? 0) + (armEquipped?.soulDefRate ?? 0) + (bodyEquipped?.soulDefRate ?? 0) + (legsEquipped?.soulDefRate ?? 0) + (ringEquipped?.soulDefRate ?? 0));
        list.Add((headEquipped?.thunderDefRate ?? 0) + (shieldEquipped?.thunderDefRate ?? 0) + (armEquipped?.thunderDefRate ?? 0) + (bodyEquipped?.thunderDefRate ?? 0) + (legsEquipped?.thunderDefRate ?? 0) + (ringEquipped?.thunderDefRate ?? 0));
        list.Add((headEquipped?.chanceRate ?? 0) + (shieldEquipped?.chanceRate ?? 0) + (armEquipped?.chanceRate ?? 0) + (bodyEquipped?.chanceRate ?? 0) + (legsEquipped?.chanceRate ?? 0) + (ringEquipped?.chanceRate ?? 0));
        list.Add((headEquipped?.criticalRate ?? 0) + (shieldEquipped?.criticalRate ?? 0) + (armEquipped?.criticalRate ?? 0) + (bodyEquipped?.criticalRate ?? 0) + (legsEquipped?.criticalRate ?? 0) + (ringEquipped?.criticalRate ?? 0));
        list.Add((headEquipped?.goldRate ?? 0) + (shieldEquipped?.goldRate ?? 0) + (armEquipped?.goldRate ?? 0) + (bodyEquipped?.goldRate ?? 0) + (legsEquipped?.goldRate ?? 0) + (ringEquipped?.goldRate ?? 0));
        list.Add((headEquipped?.expRate ?? 0) + (shieldEquipped?.expRate ?? 0) + (armEquipped?.expRate ?? 0) + (bodyEquipped?.expRate ?? 0) + (legsEquipped?.expRate ?? 0) + (ringEquipped?.expRate ?? 0));
        return list;
    }
    private void DisplayEquipment()
    {
        slotWpn.sprite = weaponEquipped != null ? weaponEquipped.itemIcon : defaultWeapon.itemIcon;
        slotHead.sprite = headEquipped != null ? headEquipped.itemIcon : nullHead;
        slotShield.sprite = shieldEquipped != null ? shieldEquipped.itemIcon : nullShield;
        slotArm.sprite = armEquipped != null ? armEquipped.itemIcon : nullArm;
        slotBody.sprite = bodyEquipped != null ? bodyEquipped.itemIcon : nullBody;
        slotLegs.sprite = legsEquipped != null ? legsEquipped.itemIcon : nullLegs;
        slotRing.sprite = ringEquipped != null ? ringEquipped.itemIcon : nullRing;
    }
    public void ChangeWeapon(WeaponSO newWeapon, Inventory inventory, int slotId)
    {
        if(weaponEquipped != null && weaponEquipped != defaultWeapon)
        {
            WeaponSO oldWeapon = weaponEquipped;
            inventory.AddItemSlotId(oldWeapon, inventory.tas[slotId-1].slotId);
        }
        weaponEquipped = newWeapon;           
        DisplayEquipment();
        Character character = GameObject.Find("Player").GetComponent<Character>();
        character.HitungTotalStatusEquipment();
    }

    public void ChangeArmourRibet(ArmourSO armour, Inventory inventory, int slotId)
    {
        if(armour.itemType == TipeItem.Head)
        {
            if(headEquipped != null)
            {
                ArmourSO old = headEquipped;
                inventory.AddItemSlotId(old, inventory.tas[slotId-1].slotId);
            }
            headEquipped = armour;
        } else if(armour.itemType == TipeItem.Shield)
        {
            if(shieldEquipped != null)
            {
                ArmourSO old = shieldEquipped;
                inventory.AddItemSlotId(old, inventory.tas[slotId-1].slotId);
            }
            shieldEquipped = armour;
        } else if(armour.itemType == TipeItem.Arm)
        {
            if(armEquipped != null)
            {
                ArmourSO old = armEquipped;
                inventory.AddItemSlotId(old, inventory.tas[slotId-1].slotId);
            }
            armEquipped = armour;
        } else if(armour.itemType == TipeItem.Armour)
        {
            if(bodyEquipped != null)
            {
                ArmourSO old = bodyEquipped;
                inventory.AddItemSlotId(old, inventory.tas[slotId-1].slotId);
            }
            bodyEquipped = armour;
        } else if(armour.itemType == TipeItem.Legs)
        {
            if(legsEquipped != null)
            {
                ArmourSO old = legsEquipped;
                inventory.AddItemSlotId(old, inventory.tas[slotId-1].slotId);
            }
            legsEquipped = armour;
        }else if(armour.itemType == TipeItem.Ring)
        {
            if(ringEquipped != null)
            {
                ArmourSO old = ringEquipped;
                inventory.AddItemSlotId(old, inventory.tas[slotId-1].slotId);
            }
            ringEquipped = armour;
        }
    }

    public void ChangeArmour(ArmourSO armour, Inventory inventory, int slotId)
    {
        ArmourSO oldArmour = null;

        switch (armour.itemType)
        {
            case TipeItem.Head:
                oldArmour = headEquipped;
                headEquipped = armour;
                break;
            case TipeItem.Shield:
                oldArmour = shieldEquipped;
                shieldEquipped = armour;
                break;
            case TipeItem.Arm:
                oldArmour = armEquipped;
                armEquipped = armour;
                break;
            case TipeItem.Armour:
                oldArmour = bodyEquipped;
                bodyEquipped = armour;
                break;
            case TipeItem.Legs:
                oldArmour = legsEquipped;
                legsEquipped = armour;
                break;
            case TipeItem.Ring:
                oldArmour = ringEquipped;
                ringEquipped = armour;
                break;
            default:
                break;
        }

        if (oldArmour != null)
        {
            inventory.AddItemSlotId(oldArmour, inventory.tas[slotId - 1].slotId);
        }
        DisplayEquipment();
    }
    public void RemoveWeapon()
    {
        if(ManajerInventory.Instance.inventory.SlotKosong() == true && weaponEquipped != defaultWeapon)
        {
            ManajerInventory.Instance.inventory.AddItem(weaponEquipped);
            weaponEquipped = defaultWeapon;
            DisplayEquipment();
            ManajerInventory.Instance.DisplayInventory();
        }
    }

    public void RemoveHeadArmour()
    {
        if(ManajerInventory.Instance.inventory.SlotKosong() == true && headEquipped != null)
        {
            ManajerInventory.Instance.inventory.AddItem(headEquipped);
            headEquipped = null;
            DisplayEquipment();
            ManajerInventory.Instance.DisplayInventory();
        }
    }
    public void RemoveShieldArmour()
    {
        if(ManajerInventory.Instance.inventory.SlotKosong() == true && shieldEquipped != null)
        {
            ManajerInventory.Instance.inventory.AddItem(shieldEquipped);
            shieldEquipped = null;
            DisplayEquipment();
            ManajerInventory.Instance.DisplayInventory();
        }
    }
    public void RemoveArmArmour()
    {
        if(ManajerInventory.Instance.inventory.SlotKosong() == true && armEquipped != null)
        {
            ManajerInventory.Instance.inventory.AddItem(armEquipped);
            armEquipped = null;
            DisplayEquipment();
            ManajerInventory.Instance.DisplayInventory();
        }
    }
    public void RemoveBodyArmour()
    {
        if(ManajerInventory.Instance.inventory.SlotKosong() == true && bodyEquipped != null)
        {
            ManajerInventory.Instance.inventory.AddItem(bodyEquipped);
            bodyEquipped = null;
            DisplayEquipment();
            ManajerInventory.Instance.DisplayInventory();
        }
    }
    public void RemoveLegsArmour()
    {
        if(ManajerInventory.Instance.inventory.SlotKosong() == true && legsEquipped != null)
        {
            ManajerInventory.Instance.inventory.AddItem(legsEquipped);
            legsEquipped = null;
            DisplayEquipment();
            ManajerInventory.Instance.DisplayInventory();
        }
    }
    public void RemoveRingArmour()
    {
        if(ManajerInventory.Instance.inventory.SlotKosong() == true && ringEquipped != null)
        {
            ManajerInventory.Instance.inventory.AddItem(ringEquipped);
            ringEquipped = null;
            DisplayEquipment();
            ManajerInventory.Instance.DisplayInventory();
        }
    }
}