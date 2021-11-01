using UnityEngine;
using System.IO; //for saving files to operating system
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static void SaveUnit(Piece piece)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/unit.mw"; //save to a operating system path that won't change
        FileStream stream = new FileStream(path, FileMode.Create); //creates the file

        UnitStats data = new UnitStats(piece); //format data as shown in unit stats

        formatter.Serialize(stream, data); //saves to file

        stream.Close(); //close stream

    }

    public static UnitStats LoadUnit() //seems designed to only work with the one file, so figure out how to do multiple
    {
        string path = Application.persistentDataPath + "/unit.mw"; //open path
        if (File.Exists(path)) //if file exists here
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            UnitStats data = formatter.Deserialize(stream) as UnitStats;

            stream.Close();

            return data;
        }
        else
        {
            Debug.LogError("Save file not found in " + path);
            return null;
        }
    }

}
