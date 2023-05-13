using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class Scrapyard : MonoBehaviour
{
    Character character;
    [SerializeField] Transform slotParent;
    [SerializeField] GameObject panelShop;
    [SerializeField] GameObject slotItem;
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] Button exitShop;

    private void Start()
    {
        character = GameObject.Find("Pemain").GetComponent<Character>(); 
    }
    public void SellItem(ItemSO item, int slotId)
    {
        ManajerInventory.Instance.inventory.RemoveItem(slotId);
        character.characterSO.gold += item.hargaJual;
        DisplayShop();
        ManajerInventory.Instance.DisplayInventory();
        character.CheckCharacterLevel();
    }
    private void OnTriggerEnter(Collider other) 
    {
         if(other.gameObject.tag == "Player")
         {
            DisplayShop();
            panelShop.gameObject.SetActive(true);
         }
    }
    private void ExitShop()
    {
        panelShop.gameObject.SetActive(false);
    }

    private void DisplayShop()
    {
        exitShop.onClick.RemoveAllListeners();
        exitShop.onClick.AddListener(()=> ExitShop());
        if(slotParent.childCount > 0)
        {
            foreach (Transform child in slotParent)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        for(int i = 0; i < ManajerInventory.Instance.inventory.tas.Count; i++)
        {
            if(ManajerInventory.Instance.inventory.tas[i].item == null)
            {
                continue;
            }
            GameObject slot = Instantiate(slotItem, Vector3.zero, Quaternion.identity);
            GameObject slotDesc = slot.transform.Find("Slot_Desc").gameObject;
            slot.gameObject.name = ManajerInventory.Instance.inventory.tas[i].item.itemName;
            slot.transform.SetParent(slotParent);
            slot.transform.localScale = new Vector3(1f,1f,1f);
            titleText.text = "Scrapyard";
            Image icon = slot.transform.Find("Icon").GetComponent<Image>();
            Button sellButton = slot.transform.Find("Button_Price").GetComponent<Button>();
            TextMeshProUGUI textNamaItem = slot.transform.Find("Nama_Item").GetComponentInChildren<TextMeshProUGUI>();   
            TextMeshProUGUI textHarga = slot.transform.Find("Button_Price").gameObject.GetComponentInChildren<TextMeshProUGUI>();
            icon.sprite = ManajerInventory.Instance.inventory.tas[i].item.itemIcon;
            textHarga.text = ManajerInventory.Instance.inventory.tas[i].item.hargaJual.ToString();
            textNamaItem.text = ManajerInventory.Instance.inventory.tas[i].item.itemName;
            int index = i; // Simpan nilai i dalam variabel index untuk menghindari closure bugs
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(() => SellItem(ManajerInventory.Instance.inventory.tas[index].item, ManajerInventory.Instance.inventory.tas[index].slotId));

            // Tambahkan event trigger PointerEnter dan panggil fungsi ShowPopup saat event terjadi
            EventTrigger eventTrigger = slot.GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((data) => { ShowPopup(ManajerInventory.Instance.inventory.tas[index].item, slotDesc); });
            eventTrigger.triggers.Add(entry);

            // Menambahkan event handler untuk event PointerUp
            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerExit;
            entry.callback.AddListener((data) => { DisablePopUp(slotDesc); });
            slot.GetComponent<EventTrigger>().triggers.Add(entry);

        }
        if(slotParent.childCount == 0)
        {
            titleText.text = "Scrapyard (Nothing to sell)";
        }
    }
    private void ShowPopup(ItemSO itemSO, GameObject go)
    {
        string text = string.Empty;
        TextMeshProUGUI goText = go.GetComponent<TextMeshProUGUI>();
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
        goText.text = text;
        go.SetActive(true);
    }

    private void DisablePopUp(GameObject go)
    {
        go.SetActive(false);
    }
}
