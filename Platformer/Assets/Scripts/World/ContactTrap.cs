using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactTrap : MonoBehaviour
{
    [Tooltip("TThe damage done when a player contacts the sprites collider.")]
    [SerializeField]protected int damage;

    private List<Player> objectsInAttackCollider = new List<Player>();



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for(int x = 0; x < objectsInAttackCollider.Count; x++)
        {
            objectsInAttackCollider[x].TakeDamage(damage);
        }

    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.gameObject.layer ==  LayerMask.NameToLayer("Player"))
        {
            Player playerInTrapZone = collider.gameObject.GetComponent<Player>();
            if(playerInTrapZone != null)
            {
                objectsInAttackCollider.Add(playerInTrapZone);
            }
            
        }       
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        Player playerInTrapZone = collider.gameObject.GetComponent<Player>();
        if(playerInTrapZone != null)
         {
            objectsInAttackCollider.Remove(playerInTrapZone);
        }     
    }
}
