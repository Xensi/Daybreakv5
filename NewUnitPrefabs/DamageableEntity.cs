using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageableEntity : MonoBehaviour
{ 
    public float health = 10;
    public bool alive = true;
    public GlobalDefines.Team team = GlobalDefines.Team.EnemyOfAll;
    public float armor = 0;

    public void InflictDamageOnThis(float damage)
    {
        if (alive)
        {
            float total = Mathf.Clamp(damage - armor, 0, 999);
            
            health -= total;
            if (health <= 0)
            {
                alive = false;
                OnDeathEffect();
            }
        }
    }
    public virtual void OnDeathEffect()
    {  
    }
}
