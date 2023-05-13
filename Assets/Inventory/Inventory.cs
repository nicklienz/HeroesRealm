using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Buat Tas Baru", menuName ="HeroesRealm/Buat Tas Baru")]
public class Inventory : ScriptableObject
{
    public List<SlotTas> tas = new List<SlotTas>();
    [SerializeField] int ukuranTas;

    public bool SlotKosong()
    {
        for(int i =0; i < tas.Count; i++)
        {            
            if(tas[i].item == null)
            {
                return true;
            }
        }
        return false;
    }
    public void AddItem(ItemSO item)
    {
        for(int i =0; i < tas.Count; i++)
        {
            if(tas[i].item == null)
            {
                tas[i].item = item;
                break;
            }
        }
    }
    
    public void AddItemSlotId(ItemSO item, int slotId)
    {
        tas[slotId-1].item = item;
    }
    public void AddItemType(ItemSO item)
    {
        if(item.itemType == TipeItem.Weapon)
        {
            
        } else if (item.itemType == TipeItem.Potion)
        {
            //heal mana atau health
        } else if (item.itemType == TipeItem.Quest)
        {
            //cek sesuai quest, jika tidak quest maka tidak dipakai
        } else if (item.itemType == TipeItem.Skill)
        {
            //use skill untuk damage enemy atau heal, attacktype class
        }  else if (item.itemType == TipeItem.Powerup)
        {
            //buff atau debuff
        } 
    }

    public void RemoveItem (int slotId)
    {
        tas[slotId-1].item = null;      
    }
}

[System.Serializable]
public class SlotTas
{
    public int slotId;
    public ItemSO item;
    public SlotTas (int _slotId, ItemSO _item)
    {
        slotId = _slotId;
        item = _item;
    }
}