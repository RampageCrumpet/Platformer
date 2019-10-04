using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*  RampageCrumpet
    2/18/2019

    This class is the root class for all of our player characters and enemies
    It also has the code to handle simple destructible objects such as barrels and the like.
*/ 
public class DestructableObject : MonoBehaviour
{
    
    [Tooltip("The ammount of damage our object can take.")]
    [SerializeField] protected int health;


    [Tooltip("The maximum ammount of health our object can have.")]
    [SerializeField] protected int maximumHealth;


    public virtual void TakeDamage(int damage)
    {
        Debug.Log(this.gameObject.name + " has taken " + damage + " damage.");
        health = health - damage;
        CheckDeath();
    }

    //Check to see if we have any health remaining. 
    public virtual void CheckDeath()
    {
        if(health <= 0)
        {
            Perish();
        }
    }

    //Destroy the object when it runs out of health.
    protected virtual void Perish()
    {
        //TODO: Play death animation
        Destroy(this.gameObject);
    }
}
