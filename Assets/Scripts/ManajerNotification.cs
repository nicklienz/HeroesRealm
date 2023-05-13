using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum MessageType
{
    Normal,
    Error,
    Success
}
public class ManajerNotification : MonoBehaviour
{
    public static ManajerNotification instance;
    public static ManajerNotification Instance
    {
        get
        {
            // Jika instance belum ada, cari di scene
            if (instance == null)
            {
                instance = FindObjectOfType<ManajerNotification>();
            }
            
            return instance;
        }
    }
    [SerializeField] GameObject message, messageSuccess, messageError;
    public TextMeshProUGUI messageText, messageSuccessText, messageErrorText;

    public IEnumerator aShowMessagePrompt()
    {
        float textHeight = messageText.preferredHeight;
        message.transform.localScale = new Vector3(1,textHeight,1);
        message.SetActive(true);
        yield return new WaitForSeconds(3f);
        messageText.text = string.Empty;
        message.SetActive(false);
    }
    public IEnumerator aShowMessageErrorPrompt()
    {
        float textHeight = messageText.preferredHeight;
        message.transform.localScale = new Vector3(1,textHeight,1);
        messageError.SetActive(true);
        yield return new WaitForSeconds(3f);
        messageErrorText.text = string.Empty;
        messageError.SetActive(false);
    }
    public IEnumerator ShowMessage(MessageType messageType = MessageType.Normal)
    {
    GameObject messageObject;
    TextMeshProUGUI messageTextObject;

    switch (messageType)
    {
        case MessageType.Normal:
            messageObject = message;
            messageTextObject = messageText;
            break;
        case MessageType.Error:
            messageObject = messageError;
            messageTextObject = messageErrorText;
            break;
        case MessageType.Success:
            messageObject = messageSuccess;
            messageTextObject = messageSuccessText;
            break;
        default:
            yield break;
    }

    //float textHeight = messageTextObject.preferredHeight;
    //messageObject.transform.localScale = new Vector3(1, textHeight, 1);
    messageObject.SetActive(true);

    yield return new WaitForSeconds(3f);

    messageTextObject.text = string.Empty;
    messageObject.SetActive(false);
    }
}