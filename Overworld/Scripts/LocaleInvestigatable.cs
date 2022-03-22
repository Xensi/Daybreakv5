using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LocaleInvestigatable : MonoBehaviour
{
    public DialogueScriptableObject localeInvestigationDialogue;
    public bool investigated = false;
    public bool destroyed = false;
    public SpriteRenderer localeAppearance;
    public Sprite destroyedAppearance;

    public void UpdateAppearance()
    {
        if (destroyed)
        {
            investigated = true;
            localeAppearance.sprite = destroyedAppearance;
        }
    }
}
