using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Teleport : MonoBehaviour
{
    public List<Destination> destinationList;
    public GameObject template;
    private Transform slotParent;
    [SerializeField] private GameObject slotTeleport;
    private TextMeshProUGUI title;
    private Button exitTeleport;

    private PlayerMovement playerMovement;
    // Start is called before the first frame update

    public void DisplayTeleportTemplate(Character character)
    {
        template.SetActive(true);
        exitTeleport = template.transform.Find("TopBar").transform.Find("Button_Back").GetComponent<Button>();
        exitTeleport.onClick.RemoveAllListeners();
        exitTeleport.onClick.AddListener((()=> ExitTeleport()));
        title = template.transform.Find("Title").GetComponent<TextMeshProUGUI>();
        title.text = this.gameObject.name + " Harbour";
        slotParent = template.transform.Find("Panel").transform.Find("ScrollRect").transform.Find("Content").transform.Find("Slot Parent").transform;
        if(slotParent.childCount > 0)
        {
            foreach (Transform child in slotParent)
            {
                //GameObject.Destroy(child.gameObject);
                Destroy(child.gameObject,0f);
            }
        }
        for(int i = 0; i < destinationList.Count; i++)
        {
            int currentIndex = i;
            GameObject slot = Instantiate(slotTeleport, Vector3.zero, Quaternion.identity);
            slot.transform.localScale = new Vector3(1f,1f,1f);
            slot.transform.SetParent(slotParent);
            slot.gameObject.name = "Tujuan " + destinationList[i].destName;
            Button buttonOk = slot.transform.Find("Button_OK").GetComponent<Button>();
            TextMeshProUGUI destName = slot.transform.Find("Nama_Dest").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI textPrice = slot.transform.Find("Text_Price").GetComponent<TextMeshProUGUI>();
            destName.text = destinationList[i].destName;
            textPrice.text = destinationList[i].gold.ToString();
            slot.transform.SetParent(slotParent);
            buttonOk.onClick.RemoveAllListeners();
            buttonOk.onClick.AddListener(() => TeleportTo(character, destinationList[currentIndex]));
            //Debug.Log(character +" "+ destinationList[i].destName +" "+ autoGridWalk);
        }
    }

    public void TeleportTo(Character character, Destination destination)
    {
        //Debug.Log(character +" "+ destination +" "+ autoGridWalk);
        if(character.characterSO.gold >= destination.gold)
        {
            playerMovement.TeleportPos(destination.destination.position + new Vector3 (2f,0,0));
            character.characterSO.gold -= destination.gold;
            character.transform.position = destination.destination.position + new Vector3 (2f,0,0);
            ManajerNotification.Instance.messageSuccessText.text = "Berhasil teleport ke " + destination.destName;
            StartCoroutine(ManajerNotification.Instance.ShowMessage(MessageType.Success));
            character.CheckCharacterLevel();
            ExitTeleport();
        } else
        {
            ManajerNotification.Instance.messageErrorText.text = "Tidak dapat teleport ke " + destination.destName + " karena gold tidak cukup!";
            StartCoroutine(ManajerNotification.Instance.ShowMessage(MessageType.Error));
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if(other.gameObject.tag == "Player")
        {
            Character character = other.gameObject.GetComponent<Character>();
            playerMovement = other.gameObject.GetComponent<PlayerMovement>();
            DisplayTeleportTemplate(character);
        }                    
    }

    private void ExitTeleport()
    {
        template.SetActive(false);
        if(slotParent.childCount > 0)
        {
            foreach (Transform child in slotParent)
            {
                //GameObject.Destroy(child.gameObject);
                Destroy(child.gameObject,0f);
            }
        }
    }
}


[System.Serializable]
public class Destination
{
    public Transform destination;
    public string destName;
    public int gold;
    public Destination (Transform _destination, string _destName, int _gold)
    {
        destination = _destination;
        destName = _destName;
        gold = _gold;
    }
}
