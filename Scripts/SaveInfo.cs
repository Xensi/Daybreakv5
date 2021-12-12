// Connected to the Cube and includes a DontDestroyOnLoad()
// LoadScene() is called by the first  script and switches to the second.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SaveInfo : MonoBehaviour
{
    private static bool created = false;

    public List<UnitScriptableObject> list = new List<UnitScriptableObject>();

    public UnitInformationScript unitPrefab;

    public List<UnitInformationScript> listOfSavedUnits = new List<UnitInformationScript>();

    void Awake()
    {
        if (!created)
        {
            DontDestroyOnLoad(this.gameObject);
            created = true;
            Debug.Log("Awake: " + this.gameObject);
            GenerateModifiableScripObjsAsChildren();
        }
    }


    private void GenerateModifiableScripObjsAsChildren()
    {
        var i = 0;
        foreach (var item in list)
        {
            var newItem = Instantiate(unitPrefab);
            newItem.transform.parent = this.transform;
            newItem.name = item.name;
            newItem.models = item.models;
            newItem.morale = item.morale;
            newItem.energy = item.energy;

            newItem.maxModels = item.models;
            newItem.maxMorale = item.morale;
            newItem.maxEnergy = item.energy;
            newItem.placementID = i;

            listOfSavedUnits.Add(newItem);
            i++;
        }
    }


    public void LoadScene()
    {
        if (SceneManager.GetActiveScene().name == "test")
        {
            SceneManager.LoadScene("test2", LoadSceneMode.Single);
        }
    }
}