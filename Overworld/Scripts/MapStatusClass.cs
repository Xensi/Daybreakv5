using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class MapStatusClass
{
    public enum MapStatus
    {
        Invisible,
        Visible,
        Visited
    }
    public LocaleInvestigatable location;
    public MapStatus status = MapStatus.Invisible;
}