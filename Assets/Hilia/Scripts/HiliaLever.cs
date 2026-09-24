using UnityEngine;
using UnityEngine.EventSystems;

public class HiliaLever : MonoBehaviour, IPointerClickHandler
{
    public HiliaMission mission;
    public void OnPointerClick(PointerEventData data)
    {
        if(data.button==PointerEventData.InputButton.Left)mission.UseLever();
    }
}
