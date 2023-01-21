using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualMapManager : MonoBehaviour
{
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
                    item.location.mapIcon.enabled = false;
                    item.location.mapIcon.color = Color.black;
                    break;
                case MapStatusClass.MapStatus.Visible:
                    item.location.mapIcon.enabled = true;
                    item.location.mapIcon.color = Color.gray;
                    break;
                case MapStatusClass.MapStatus.Visited:
                    item.location.mapIcon.enabled = true;
                    item.location.mapIcon.color = Color.blue;
                    break;
                default:
                    break;
            }
        }
    }
    private void ToggleMapCamera()
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
