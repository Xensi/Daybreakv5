using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LocaleInvestigatable : MonoBehaviour
{
    public DialogueScriptableObject localeInvestigationDialogue;
    public bool investigated = false;
    public bool destroyed = false;
    public MeshRenderer localeAppearance;
    public Material destroyedAppearance;

    public void UpdateAppearance()
    {
        if (destroyed)
        {
            investigated = true;
            localeAppearance.material = destroyedAppearance;
        }
    }
}
