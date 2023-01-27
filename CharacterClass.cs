using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class CharacterClass
{
    public string name;
    public int ransomWorth; //spoils received for ransoming
    public enum Faction
    {
        Altgard,
        Maukland,
        Zhanguo
    }
    public Faction faction;

    public List<int> knownLocations; //places this person knows

    public enum Crime
    {
        Thievery,
        Heresy,
        Murder,
        PrisonerOfWar
    }
    public List<Crime> crimes;

    public bool receivedHelpFromPlayer = false;

    public bool imprisonedByPlayer = false;

    public DialogueScriptableObject trialDialogue;

}