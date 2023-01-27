using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualMapManager : MonoBehaviour
{
    public static ManualMapManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
    //bool mapPulledUp = false;
    public Camera mapCamera;
    public List<MapStatusClass> mapLocations;
    private void Start()
    {
        UpdateLocationsStatus();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMapCamera();
        }
    }
    private void UpdateLocationsStatus()
    {
        foreach (MapStatusClass item in mapLocations)
        {
            switch (item.status)
            {
                case MapStatusClass.MapStatus.Invisible:
                    item.location.mapIcon.gameObject.SetActive(false);
                    item.location.mapIcon.color = Color.black;
                    break;
                case MapStatusClass.MapStatus.Visible:
                    item.location.mapIcon.gameObject.SetActive(true);
                    item.location.mapIcon.color = Color.gray;
                    break;
                case MapStatusClass.MapStatus.Visited:
                    item.location.mapIcon.gameObject.SetActive(true);
                    item.location.mapIcon.color = Color.cyan;
                    break;
                default:
                    break;
            }
        }
    }
    public void ChangeLocationStatus(int locationInt, MapStatusClass.MapStatus status)
    { 
        mapLocations[locationInt].status = status;
        UpdateLocationsStatus();
    }

    public bool HasLocationBeenVisited(int id)
    {
        if (mapLocations[id].status == MapStatusClass.MapStatus.Visited)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public void ToggleMapCamera()
    {
        mapCamera.enabled = !mapCamera.enabled;
        if (mapCamera.enabled)
        {

            OverworldManager.Instance.HideArmyInfo();
        }
        else
        {
            OverworldManager.Instance.ShowArmyInfoAndUpdateArmyBars();
        }
    }

}
