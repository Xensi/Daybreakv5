using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageableEntity : MonoBehaviour
{ 
    public float health = 10;
    public bool alive = true;
    public GlobalDefines.Team team = GlobalDefines.Team.EnemyOfAll;
    public float armor = 0; 
    public float fireDamageMultiplier = 1;

    public void InflictDamageOnThis(float damage = 0, float APdamage = 0, bool fireDamage = false)
    {
        if (alive)
        {
            float total = Mathf.Clamp(damage - armor, 0, 999) + APdamage;
            
            if (fireDamage)
            { 
                health -= total * fireDamageMultiplier;
            }
            else
            {
                health -= total;
            }

            OnDamageEffect(fireDamage);
            if (health <= 0)
            {
                alive = false;
                OnDeathEffect(fireDamage);
            }
        }
    }
    public virtual void OnDamageEffect(bool flamed = false)
    {

    }
    public virtual void OnDeathEffect(bool flamed = false)
    {  
    }
}
