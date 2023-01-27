using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharactersManager : MonoBehaviour
{
    public static CharactersManager Instance { get; private set; }
    public List<CharacterClass> characters;
    private void Awake()
    {
        Instance = this;
    }

    public void ImprisonCharacter(int id)
    {
        characters[id].imprisonedByPlayer = true;
    }
    public void HelpCharacter(int id)
    {
        characters[id].receivedHelpFromPlayer = true;
    }
    public bool HasHelpedCharacter(int id)
    {
        return characters[id].receivedHelpFromPlayer;
    }

    public bool CharacterIsImprisoned(int id)
    {
        return characters[id].imprisonedByPlayer;
    }
    public bool CheckPrisonerCount(int num)
    {
        int i = 0;
        foreach (CharacterClass item in characters)
        {
            if (item.imprisonedByPlayer)
            {
                i++;
            }
        }
        if (i >= num)
        {
            return true;
        }
        else
        {
            return false;

        }
    }
}
