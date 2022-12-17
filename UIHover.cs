using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;// Required when using Event data.

public class UIHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool hovering = false; 
    // Start is called before the first frame update
     
    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
        FightManager.Instance.hoveringUI = true; 
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
        FightManager.Instance.hoveringUI = false; 
    }
    private void OnDisable()
    { 
        hovering = false;
        FightManager.Instance.hoveringUI = false;
    }
}
