using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ManajerInventory : MonoBehaviour
{
    public static ManajerInventory instance;
    public static ManajerInventory Instance
    {
        get
        {
            // Jika instance belum ada, cari di scene
            if (instance == null)
            {
                instance = FindObjectOfType<ManajerInventory>();
            }
            
            return instance;
        }
    }
    public Inventory inventory;
    [SerializeField] GameObject slotTas;
    [SerializeField] Transform slotParent;
    [SerializeField] Character character;

    private void Start()
    {
        DisplayInventory();
    }
    public void DisplayInventory()
    {
        if(slotParent.childCount > 0)
        {
            foreach (Transform child in slotParent)
            {
                //GameObject.Destroy(child.gameObject);
                Destroy(child.gameObject,0f);
            }
        }
        for(int i = 0; i < inventory.tas.Count; i++)
        {
            GameObject slot = Instantiate(slotTas, Vector3.zero, Quaternion.identity);
            Image icon = slot.transform.Find("ItemIcon").GetComponent<Image>();
            Button button = slot.GetComponent<Button>();
            GameObject panel = slot.transform.Find("Panel").gameObject;
            GameObject slotDesc = panel.transform.Find("Info_button").gameObject;
            Button useButton = panel.transform.Find("Use_button").GetComponent<Button>();
            Button dropButton = panel.transform.Find("Drop_button").GetComponent<Button>();
            Button infoButton = panel.transform.Find("Info_button").GetComponent<Button>();
            Item slotItem = slot.GetComponent<Item>();
            slot.transform.SetParent(slotParent);
            slot.transform.localScale = new Vector3(1f,1f,1f);
            if(inventory.tas[i].item != null)
            {
                icon.gameObject.SetActive(true);
                button.interactable = true;
                icon.sprite = inventory.tas[i].item.itemIcon;    
                slotItem.item = inventory.tas[i].item;
            } else 
            {
                icon.gameObject.SetActive(false);
                button.interactable = false;
            }
            slot.name =  inventory.tas[i].slotId.ToString();
            button.onClick.AddListener(() => ShowHideButton(panel));
            useButton.onClick.AddListener(() => UseItem(slot,slotItem.item));
            dropButton.onClick.AddListener(() => DropItem(slot,slotItem.item));
            infoButton.onClick.AddListener(() => InfoItem(slotDesc,slotItem.item));
        }
    }

    private void ShowHideButton(GameObject go)
    {
       go.SetActive(!go.activeSelf? true : false);
    }

    private void UseItem(GameObject item, ItemSO itemSO)
    {
        int index = System.Convert.ToInt32(item.gameObject.name);
        inventory.RemoveItem(index);
        if(itemSO.itemType == TipeItem.Weapon)
        {
            WeaponSO weapon = itemSO as WeaponSO;
            Equipment.Instance.ChangeWeapon(weapon, inventory, index);
        } else if (itemSO.itemType == TipeItem.Potion)
        {
            PotionSO potionSO = itemSO as PotionSO;
            potionSO.Heal(character);
        } else if (itemSO.itemType == TipeItem.Head || itemSO.itemType == TipeItem.Arm || itemSO.itemType == TipeItem.Shield || itemSO.itemType == TipeItem.Armour || itemSO.itemType == TipeItem.Legs || itemSO.itemType == TipeItem.Ring) 
        {
            ArmourSO armourSO = itemSO as ArmourSO;
            Equipment.Instance.ChangeArmour(armourSO, inventory, index);
        }
        DisplayInventory();
    }

    private void DropItem(GameObject item, ItemSO itemSO)
    {
        int index = System.Convert.ToInt32(item.gameObject.name);
        if(!character.isInTrigger)
        {
            inventory.RemoveItem(index);
            Debug.Log("berhasil");
            GameObject go = Instantiate(itemSO.prefab, character.transform.position + new Vector3(0.5f,0,0.5f), Quaternion.identity);
            go.name = itemSO.name;
            go.transform.GetChild(0).tag = itemSO.itemType.ToString();
            DisplayInventory();
        }        
    }

    private void InfoItem(GameObject go, ItemSO itemSO)
    {
        string text = string.Empty;
        text = itemSO.name + "\r\n";
        if(itemSO.itemType == TipeItem.Weapon)
        {
            WeaponSO weaponSO = itemSO as WeaponSO;
            foreach (AttackType attack in weaponSO.elementAttack)
            {
                text += attack.element +" : " + attack.amountDamage.ToString() + "\r\n";
            }
        } else if (itemSO.itemType == TipeItem.Head ||
            itemSO.itemType == TipeItem.Shield ||
            itemSO.itemType == TipeItem.Arm ||
            itemSO.itemType == TipeItem.Armour ||
            itemSO.itemType == TipeItem.Legs ||
            itemSO.itemType == TipeItem.Ring)
        {
            ArmourSO armourSO = itemSO as ArmourSO;
            if (armourSO.hitDefRate != 0)
            {
                text += "Hit : " + (armourSO.hitDefRate * 100).ToString() + "%";
            }
            if (armourSO.fireDefRate != 0)
            {
                text += "\r\nFire :" + (armourSO.fireDefRate * 100).ToString() + "%";
            }
            if (armourSO.iceDefRate != 0)
            {
                text += "\r\nIce : " + (armourSO.iceDefRate * 100).ToString() + "%";
            }
            if (armourSO.soulDefRate != 0)
            {
                text += "\r\nSoul : " + (armourSO.soulDefRate * 100).ToString() + "%";
            }
            if (armourSO.thunderDefRate != 0)
            {
                text += "\r\nThunder : " + (armourSO.thunderDefRate * 100).ToString() + "%";
            }
            text += "\r\n";
        } else if (itemSO.itemType == TipeItem.Potion)
        {
            PotionSO potionSO= itemSO as PotionSO;
            if (potionSO.healthHeal != 0)
            {
                text += "Health +" + potionSO.healthHeal.ToString();
            }
            if (potionSO.manaHeal!= 0)
            {
                text += "\r\nMana +" + potionSO.manaHeal.ToString();
            }
            if (potionSO.healthHealRate != 0)
            {
                text += "\r\nHealth +" + (potionSO.healthHealRate * 100).ToString() + "%";
            }
            if (potionSO.manaHealRate != 0)
            {
                text += "\r\nMana +" + (potionSO.manaHealRate * 100).ToString() + "%";
            }
            //text = "Health +" + potionSO.healthHeal.ToString() + "\r\nMana +" + potionSO.manaHeal.ToString()+ "\r\nHealth +"  + potionSO.healthHealRate.ToString() + "%\r\n% Mana +" + potionSO.manaHealRate.ToString() + "%\r\n";
        }
        text += itemSO.itemDesc;
        ManajerNotification.Instance.messageText.text = text;
        StartCoroutine(ManajerNotification.Instance.ShowMessage());
    }
}