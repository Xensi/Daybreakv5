using UnityEngine;
public class LocaleInvestigatable : MonoBehaviour
{
    public DialogueScriptableObject localeInvestigationDialogue;
    public bool investigated = false;
    public bool destroyed = false;
    public SpriteRenderer localeAppearance;
    public Sprite destroyedAppearance;

    public SpriteRenderer mapIcon;
    public void UpdateAppearance()
    {
        if (destroyed)
        {
            investigated = true;
            localeAppearance.sprite = destroyedAppearance;
        }
    }
}
