using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelSpawner : MonoBehaviour
{
    public FormationPosition formPos;
    public GameObject prefabModel;
    public Transform spawnPos;
    public Transform sendPos;
    public GlobalDefines.Team team = GlobalDefines.Team.Zhanguo;
    public List<SoldierModel> extant;

    public int storedModels = 4;

    public AttackableObject attackable;

    /*private void OnEnable()
    {
        if (formPos != null)
        {
            SpawnModel();
        }
    }*/
    private void OnTriggerEnter(Collider other)
    {
        SoldierModel model = other.GetComponentInParent<SoldierModel>();
        if (model != null && !awakened && attackable.alive)
        {
            int time = disturbanceTime;
            if (model.currentModelState == SoldierModel.ModelState.Charging)
            {
                time /= 2;
            }   
            Invoke("AddDisturbance", Random.Range(0, time));
        }
    }
    private void AddDisturbance()
    {
        Debug.Log("disturbed");
        disturbanceLevel++;
        if (disturbanceLevel >= disturbanceThreshold)
        {
            Awaken();
        }
    }
    public int disturbanceTime = 120;
    public int disturbanceLevel = 0;
    public int disturbanceThreshold = 40;
    public bool awakened = false;
    private void Awaken() //triggers when hearing nearby troops
    {
        awakened = true;
        while (storedModels > 0)
        {
            storedModels--;
            Invoke("SpawnModel", Random.Range(0, 10));
        } 
    }
    private void SpawnModel()
    { 
        if (attackable.alive)
        {
            GameObject obj = Instantiate(prefabModel, spawnPos.position, Quaternion.identity);
            SoldierModel model = obj.GetComponentInChildren<SoldierModel>();
            if (model != null)
            {
                model.team = team;
                //model.target = sendPos;
                extant.Add(model);
            }
        }
    }
    private void Start()
    {
        InvokeRepeating("SlowUpdate", 0, 2);   
    }
    public AttackableObject obj;
    private void SlowUpdate()
    {
        foreach (SoldierModel item in extant)
        {
            if (item != null)
            {  
                float dist = Mathf.Infinity;
                obj = null;
                for (int i = 0; i < InteractablesManager.Instance.interactables.Length; i++)
                {
                    if (!InteractablesManager.Instance.interactables[i].alive)
                    {
                        continue;
                    }
                    float newDist = Helper.Instance.GetSquaredMagnitude(item.transform.position, InteractablesManager.Instance.interactables[i].transform.position);
                    if (newDist < dist)
                    {
                        dist = newDist;
                        obj = InteractablesManager.Instance.interactables[i];
                    }
                }
                if (obj != null)
                {
                    item.target = obj.transform;
                }
            }
            else
            {
                extant.Remove(item);
                break;
            }
        }
    }
    private void Update()
    {
        foreach (SoldierModel item in extant)
        {
            if (item != null)
            { 
                item.UpdateModelStateManual();
            }
            else
            {
                extant.Remove(item);
                break;
            }
        }
    }
}
