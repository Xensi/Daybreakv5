[System.Serializable]
public class EventCommandClass
{
    public enum EventCommands
    {
        AddMorale,
        AddSupplies,
        AddSpoils,
        AddUnit
    }
    public EventCommands command;
    public int commandNum;
}
