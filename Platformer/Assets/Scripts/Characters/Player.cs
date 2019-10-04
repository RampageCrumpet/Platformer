using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Rigidbody2D))]

/*  RampageCrumpet 
    2/18/2019

    This class is the base class all of the specialized player classes inherit from.

    This class requires a Rigidbody2d
*/  


public class Player : Actor
{
    //Tells us which direction our sprite is facing. 
    private bool facingRight = true;

    [Tooltip("The ammount of damage our character does with one attack.")]
    [SerializeField] private int attackDamage = 5;

    [Tooltip("The delay in time between our attacks.")]
    [SerializeField] private float attackDelay = 2.0f;



    [Tooltip("The length of time we want our character to be invulnerable for after taking damage.")]
    [SerializeField] private float invulnerableTime= 2.0f;

    //The time since we we last took damage.
    private float timeSinceLastDamage = 0.0f;
    
    //The time since we last attacked.
    private float timeSinceLastAttack = 0.0f;

    BoxCollider2D attackCollider;


    // Start is called before the first frame update
    new void Start()
    {
        attackCollider = this.GetComponentInChildren<BoxCollider2D>();
        base.Start();
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
        Movement();  

        timeSinceLastAttack += Time.deltaTime;
        timeSinceLastDamage += Time.deltaTime;

        if(Input.GetAxis("Attack") != 0 && timeSinceLastAttack > attackDelay)
        {
            timeSinceLastAttack = 0;
            Attack();
        }      
    }

    public override void TakeDamage(int damage)
    {
        if(timeSinceLastDamage > invulnerableTime)
        {
            Debug.Log(this.gameObject.name + " has taken damage");

            timeSinceLastDamage = 0.0f;
            health = health - damage;
            CheckDeath();
        }
    }

    //Handles the movement for our player character.
    protected void Movement()
    {
        Vector2 movement= Vector2.zero;
        
        //Preserve our vertical momentum
        movement.y = rb2d.velocity.y;
        movement.x = Input.GetAxis("Horizontal")*movementSpeed;

        if(Input.GetButtonDown("Jump") == true && grounded == true)
        {
             movement.y = jumpTakeOffSpeed;
        }
        
        //Set the players velocity.
        rb2d.velocity = movement;

        //Set the variables so our animator knows when to change the animations.
        animator.SetFloat ("velocityX", Mathf.Abs(movement.x));
        animator.SetFloat ("velocityY", Mathf.Abs(movement.y));

        if((movement.x < 0 && facingRight == true) || (movement.x > 0 && facingRight != true))
        {
            //Flip the sprite and remember we flipped it.
            Vector3 localScale = this.gameObject.transform.localScale;
            localScale.x = this.gameObject.transform.localScale.x*-1;

            this.gameObject.transform.localScale = localScale;

            facingRight = !facingRight;
        }
    }

    private void Attack()
    {
        //Play the attack animation
        animator.SetTrigger("Attack");

        //Set up what we want to add to our list of objects to attack.
        ContactFilter2D attackFilter = new ContactFilter2D(); 
        attackFilter.SetLayerMask(LayerMask.GetMask("Character"));

        
        Collider2D[] hitColliders = new Collider2D[10];

        //Populate the array with the hit objects.
        Physics2D.OverlapCollider(attackCollider,attackFilter, hitColliders);

        foreach(Collider2D hitCollder in hitColliders)
        {
            if(hitCollder != null)
            {
                Enemy enemy = hitCollder.gameObject.GetComponent<Enemy>();
                if(enemy != null)
                {
                    enemy.TakeDamage(attackDamage);
                }
            }
        }
    }
}
