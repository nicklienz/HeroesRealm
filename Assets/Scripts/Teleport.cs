using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    public List<Destination> destinationList;
    // Start is called before the first frame update

    public void TeleportTo(Character character, Destination destination, AutoGridWalk autoGridWalk)
    {
        if(character.characterSO.gold >= destination.gold)
        {
            character.characterSO.gold -= destination.gold;
            character.transform.position = destination.destination.position + new Vector3 (0,0,-3f);
            autoGridWalk.Teleporting(destination.destination.position+ new Vector3 (0,0,-3f));
            ManajerNotification.Instance.messageSuccessText.text = "Berhasil teleport ke " + destination.destName;
            StartCoroutine(ManajerNotification.Instance.ShowMessage(MessageType.Success));
            character.CheckCharacterLevel();
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
            AutoGridWalk autoGrid = other.gameObject.GetComponent<AutoGridWalk>();
            Character character = other.gameObject.GetComponent<Character>();
            TeleportTo(character, destinationList[0], autoGrid);
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
