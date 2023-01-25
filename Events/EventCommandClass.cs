[System.Serializable]
public class EventCommandClass
{
    public enum EventCommands
    {
        AddMorale,
        AddSupplies,
        AddSpoils,
        AddUnit,
        RevealLocation
    }
    public EventCommands command;
    public int commandNum;
}
