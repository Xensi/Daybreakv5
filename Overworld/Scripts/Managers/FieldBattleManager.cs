using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldBattleManager : MonoBehaviour
{
    [SerializeField] private GameObject FieldBattleParent;
    [SerializeField] private GameObject OverworldParent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartFieldBattle()
    {
        FieldBattleParent.SetActive(true);
        OverworldParent.SetActive(false);
    }
}
