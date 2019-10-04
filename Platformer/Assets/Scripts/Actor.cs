using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*  RampageCrumpet
    2/18/2019
    This class is the base class for both the characters and the enemies.
    
*/

public class Actor : DestructableObject
{
    [Tooltip("The number of unity units the actor can move in one second when traveling at full speed.")]
    [SerializeField]protected float movementSpeed;
  

    [Tooltip("How fast the character is moving after they jump.")]
    [SerializeField]protected float jumpTakeOffSpeed;

    //TODO:This assumes I'll always have a capsule collider. Fix it.
    protected CapsuleCollider2D collider;


    protected Rigidbody2D rb2d;
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;

    //Is out sprite currently on the ground.
    //If it's false our sprite is in the air
    protected bool grounded = false;

    //A flag set to true if we die. 
    protected bool isDead = false;


    // Start is called before the first frame update
    protected void Start()
    {
        collider = this.GetComponent<CapsuleCollider2D>();
        rb2d = this.GetComponent<Rigidbody2D>();
        animator = this.GetComponent<Animator> ();
        spriteRenderer = this.GetComponent<SpriteRenderer>();
    }

    
    // Update is called once per frame
    protected void Update()
    {
        grounded = CheckGrounded();
        //Debug.Log(grounded);
    }

    public bool CheckGrounded()
    {
        RaycastHit2D capsuleCast = Physics2D.CapsuleCast(collider.bounds.center,collider.size,collider.direction, 0.0f, Vector2.down, 0.1f, LayerMask.GetMask("World"));
       
       
        if(capsuleCast != null && capsuleCast.collider != null)
        {
            return true;
        }
        else
        {
            return false;
        }      
    }
}
