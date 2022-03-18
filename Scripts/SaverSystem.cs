using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; //needed to do file stuff
using System.Runtime.Serialization.Formatters.Binary; //needed for 
using UnityEngine.SceneManagement;

public class SaverSystem: MonoBehaviour
{
    public List<Piece> pieces;
    const string UNIT_SUB = "/unit";
    const string UNIT_COUNT_SUB = "/unit.count";

    /*void Awake()
    {
        LoadUnit();
    }

    void OnApplicationQuit()
    {
        SaveUnit();
    }
*/
    //when we remove a piece we should remove it from list of pieces to save

    public void SaveUnit()
    {
        Debug.Log("Saving");
        BinaryFormatter formatter = new BinaryFormatter();
        
        string path = Application.persistentDataPath + UNIT_SUB; //directory that won't change
        string countPath = Application.persistentDataPath + UNIT_COUNT_SUB; //directory that won't change

        FileStream countStream = new FileStream(countPath, FileMode.Create);
        formatter.Serialize(countStream, pieces.Count);
        countStream.Close();


        for (int i = 0; i < pieces.Count; i++)
        {
            FileStream stream = new FileStream(path + i, FileMode.Create); //create file
            //adding index to path saves each fish individually
            UnitData data = new UnitData(pieces[i]); //this automatically sets up unit data using piece as input
                                                     //using index lets us save a new file for each piece
            formatter.Serialize(stream, data); //writes data to file
            stream.Close(); //closes stream (MUST)
        }
    }

    public void LoadUnit()
    {
        Debug.Log("Loading");
        BinaryFormatter formatter = new BinaryFormatter();

        string path = Application.persistentDataPath + UNIT_SUB; //directory that won't change
        string countPath = Application.persistentDataPath + UNIT_COUNT_SUB; //directory that won't change

        int unitCount = 0;

        if (File.Exists(countPath))
        {
            FileStream countStream = new FileStream(countPath, FileMode.Open);
            unitCount = (int)formatter.Deserialize(countStream); //cast it as int
            countStream.Close();
        }
        else
        {
            Debug.LogError("Path not found in " + countPath);
        }

        for (int i = 0; i < pieces.Count; i++)
        {
            if (File.Exists(path + i))
            {

                FileStream stream = new FileStream(path + i, FileMode.Open);
                UnitData data = formatter.Deserialize(stream) as UnitData;
                stream.Close();

                Debug.Log(data.name); //HERE IS WHERE YOU DO THINGS WITH THE LOADED DATA
                //Debug.Log(data.models);
                //Debug.Log(data.morale);
                //Debug.Log(data.energy);

            }
            else
            {
                Debug.LogError("Path not found in " + path + i);
            }
        }

    }
}
