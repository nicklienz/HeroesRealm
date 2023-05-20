using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    public float speed = 5f;  // kecepatan karakter
    [SerializeField] private Image joystickBG; // gambar latar belakang joystick
    [SerializeField] private Image joystick;   // gambar joystick
    public Vector3 inputVector;

    public virtual void OnPointerDown(PointerEventData ped)
    {
        OnDrag(ped);
    }

    public virtual void OnPointerUp(PointerEventData ped)
    {
        inputVector = Vector3.zero;
        joystick.rectTransform.anchoredPosition = Vector3.zero;
    }

    public virtual void OnDrag(PointerEventData ped)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickBG.rectTransform, ped.position, ped.pressEventCamera, out pos))
        {
            pos.x = (pos.x / joystickBG.rectTransform.sizeDelta.x);
            pos.y = (pos.y / joystickBG.rectTransform.sizeDelta.y);

            float x = (joystickBG.rectTransform.pivot.x == 1) ? pos.x * 2 + 1 : pos.x * 2 - 1;
            float y = (joystickBG.rectTransform.pivot.y == 1) ? pos.y * 2 + 1 : pos.y * 2 - 1;

            inputVector = new Vector3(x, 0, y);
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;
            joystick.rectTransform.anchoredPosition = Vector3.zero;
            joystick.rectTransform.anchoredPosition = new Vector3(inputVector.x * (joystickBG.rectTransform.sizeDelta.x / 3), inputVector.z * (joystickBG.rectTransform.sizeDelta.y / 3));
        }
    }
}
