using UnityEngine;
using TMPro;
public class LocaleInvestigatable : MonoBehaviour
{
    public bool investigatable = true;
    public DialogueScriptableObject localeInvestigationDialogue;
    public bool investigated = false;
    public bool destroyed = false;
    public SpriteRenderer localeAppearance;
    public Sprite destroyedAppearance;

    public SpriteRenderer mapIcon;
    public TMP_Text mapText;
    public void UpdateAppearance()
    {
        if (destroyed)
        {
            investigated = true;
            localeAppearance.sprite = destroyedAppearance;
        }
    }

}
