using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodSprite : Enemy
{
    private bool isFacingRight = true;


    private enum State
    {Idle, Walk, Charge,ChargeAttack, Attack, Dead}
    
    private State currentState = State.Idle;

    [Tooltip("How fast we want to move when we're charging.")]
    [SerializeField]private float chargeSpeed;

    private List<GameObject> objectsInAttackCollider = new List<GameObject>();

    [Tooltip("How far ahead of itself the woodsprite will search for a target.")]
    [SerializeField]private float SearchDistance;


    [Tooltip("The delay in time between our attacks.")]
    [SerializeField] private float attackDelay = 0.5f;
    
    //The time since we last attacked.
    private float timeSinceLastAttack = 0.0f;

    [Tooltip("The damage done with each attack.")]
    [SerializeField] private int attackDamage = 1;

    [Tooltip("The additional damage done when charging.")]
    [SerializeField] private int chargeDamage = 1;


    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    new void Update()
    {
        
        if(isDead == false)
        {
            CheckDeath();
        }

        if(currentState != State.Dead && currentState != State.Attack)
        {
            //Add the frameTime to the time since last attack.
            timeSinceLastAttack += Time.deltaTime;

            if(objectsInAttackCollider.Count != 0)
            {
                Attack();
            }
            //If we have nothing to attack look for something to attack
            else
            {
                //Find the player
                if(SearchForTarget() == true)
                {
                    currentState = State.Charge;
                }

                if(currentState == State.Walk || currentState == State.Charge)
                {
                    Walk();
                }
            }



            if(currentState == State.Charge)
            {
                animator.SetBool("isCharging", true);
            }
            else
            {
                animator.SetBool("isCharging", false);
            }
        }

        //Set his horizontal motion to zero if he's dead and on the ground.
        if((isDead == true && CheckGrounded() == true) || (currentState != State.Walk && currentState != State.Charge))
        {
            StopHorizontalMovement();
        }
    }

    //Searches for a target directly infront of him.
    private bool SearchForTarget()
    {
        RaycastHit2D raycast;

        if(isFacingRight == true)
        {
            raycast = Physics2D.Raycast(this.transform.position + Vector3.right*(collider.size.x), this.transform.right, (collider.size.x + SearchDistance), LayerMask.GetMask("World","Player"));
            Debug.DrawRay(this.transform.position + Vector3.right*(collider.size.x) + 0.1f*Vector3.up, this.transform.right*(collider.size.x + SearchDistance), Color.red);
        }
        else //facing left
        {
            raycast = Physics2D.Raycast(this.transform.position - Vector3.right*(collider.size.x), this.transform.right*-1f, (collider.size.x+ SearchDistance), LayerMask.GetMask("World","Player"));
            Debug.DrawRay(this.transform.position - Vector3.right*(collider.size.x) + 0.1f*Vector3.up, this.transform.right*(collider.size.x+ SearchDistance)*-1f, Color.red);   
        }

        if(raycast.collider != null)
        {
           /* Debug.Log();
            Debug.Log("Hit " + raycast.collider.name + '\n' + " Hit layer: " + raycast.collider.gameObject.layer) + '\n' + ;
            Debug.Log("Player Layer Mask" +  LayerMask.NameToLayer("Player"));*/

            if(raycast.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                return true;
            }
        }

        return false;
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.gameObject.layer ==  LayerMask.NameToLayer("Player"))
        {
            objectsInAttackCollider.Add(collider.gameObject);
        }       
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        objectsInAttackCollider.Remove(collider.gameObject);
    }

    //Start the attack animation
    private void Attack()
    {  
        //Stop moving
        if(timeSinceLastAttack > attackDelay)
        {
            if(currentState == State.Charge)
            {
                currentState = State.ChargeAttack;
            }
            else
            {
                currentState = State.Attack;
            }
            animator.SetTrigger("Attack");
            timeSinceLastAttack = 0;

            StopHorizontalMovement();
        }
    }

    //Stop attacking so we can go do other stuff
    public void AttackFinished()
    {
        if(objectsInAttackCollider.Count == 0)
        {
            currentState = State.Walk;
            Debug.Log("Empty Target list. We're moving on!");
        }
        else
        {
            Debug.Log("Mainting attack state");
            currentState = State.Attack;
        }
    }

    public void IdleFinished()
    {
        currentState = State.Walk;
    }
    //Deal the damage when our animation reach the right frame.
    public void DealDamage()
    {
        foreach(GameObject target in objectsInAttackCollider)
            {
                DestructableObject targetDestructable = target.GetComponent<DestructableObject>();
                if(targetDestructable != null)
                {
                    if(currentState == State.ChargeAttack)
                    {
                        targetDestructable.TakeDamage(attackDamage + chargeDamage);
                    }
                    else
                    if(currentState == State.Attack)
                    {
                        targetDestructable.TakeDamage(attackDamage);
                    }
                    
                }
                else
                {
                    Debug.LogError(target.gameObject.name + " is on the Player layer but doesn't have a DestructableObject attached!");
                }
            }
    }
    

    public override void CheckDeath()
    {
        if(health <= 0)
        {
            currentState = State.Dead;
            Perish();
        }
    }

    protected override void Perish()
    {
        isDead = true;
        animator.SetBool("isDead", true);
    }


    private void Walk()
    {
        Vector2 movement= Vector2.zero;
                
        bool frontCollision = checkFrontCollision();
        bool hasFloor = checkForFloor();

        //Turn around if we run into a wall or a pit.
        //We only want to check the floor if frontCollision returns false. Otherwise the checkForFloor will run inside the wall collider and will return false.
        if((frontCollision == true) || (!hasFloor && !frontCollision))
        {
            //Flip the sprite and remember we flipped it.
            Vector3 localScale = this.gameObject.transform.localScale;
            localScale.x = this.gameObject.transform.localScale.x*-1;

            this.gameObject.transform.localScale = localScale;
            isFacingRight = !isFacingRight;

            //Play the idle animation before we start walking
            currentState = State.Idle;
        }

        //Preserve our vertical momentum
        movement.y = rb2d.velocity.y;

        //Figure out whether we want to move to the right or
        if(isFacingRight == true)
        {
            if(currentState == State.Charge)
            {
                movement.x = chargeSpeed;
                
            }
            else
            if(currentState == State.Walk)
            {
                movement.x = movementSpeed;
            }
        }
        else
        {
            
            if(currentState == State.Charge)
            {
                movement.x = -chargeSpeed;
                
            }
            else
            if(currentState == State.Walk)
            {
                movement.x = -movementSpeed;
            }
        }

        animator.SetFloat ("velocityX", Mathf.Abs(movement.x));
        //Set the velocity of our gameobject
        rb2d.velocity = movement;       
    }

    //Raycasts infront of the characters collider forward to look for any walls on the "World" layer.
    bool checkFrontCollision()
    {
        RaycastHit2D raycast;

        //Send the raycast out in the direction we're facing.
        if(isFacingRight == true)
        {
            raycast = Physics2D.Raycast(this.transform.position, this.transform.right, (collider.size.x+ 0.1f), LayerMask.GetMask("World"));
            Debug.DrawRay(this.transform.position, this.transform.right*(collider.size.x+ 0.1f));
        }
        else //facing left
        {
            raycast = Physics2D.Raycast(this.transform.position, this.transform.right*-1f, (collider.size.x+ 0.1f), LayerMask.GetMask("World"));
            Debug.DrawRay(this.transform.position, this.transform.right*(collider.size.x+ 0.1f)*-1f);   
        }
        
        //

        if(raycast != null && raycast.collider != null)
        {
            return true;
        }
        else
        {
            return false;
        }      
    }

    //Raycasts infront of the character downward to check for any pits.
    bool checkForFloor()
    {
        Vector2 floorCheckPosition;
        RaycastHit2D raycast;

        //Move the floor check position to the front of our character so he looks in the right position.
        if(isFacingRight == true)
        {
            floorCheckPosition = new Vector2(this.gameObject.transform.position.x + collider.size.x + 0.1f, this.gameObject.transform.position.y);
        }
        else //facing left
        {
            floorCheckPosition = new Vector2(this.gameObject.transform.position.x - collider.size.x - 0.1f, this.gameObject.transform.position.y);
        }
        
        //Check to see if there's a tile infront of us.
        raycast = Physics2D.Raycast(floorCheckPosition, Vector2.down, (collider.size.y/2)+0.1f, LayerMask.GetMask("World"));
        Debug.DrawRay(floorCheckPosition, Vector2.down*((collider.size.y/2)+0.1f)); 

        if(raycast != null && raycast.collider != null)
        {
            return true;
        }
        else
        {
            return false;
        }   
    }

    private void StopHorizontalMovement()
    {
        Vector2 movement= Vector2.zero;
        movement.y = rb2d.velocity.y;
        movement.x = 0;

        animator.SetFloat ("velocityX", Mathf.Abs(movement.x));
        //Set the velocity of our gameobject
        rb2d.velocity = movement;
    }
}
