using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;// Required when using Event data.

public class UIHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool hovering = false;
    [SerializeField] private FightManager fightManager;
    // Start is called before the first frame update
     
    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
        if (fightManager != null)
        {
            fightManager.hoveringUI = true;
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
        if (fightManager != null)
        {
            fightManager.hoveringUI = false;
        }
    }
}
