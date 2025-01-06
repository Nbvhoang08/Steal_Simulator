using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    public float speed = 5f;
    public Joystick joystick;
    public Animator animator;
    private Rigidbody _rb;
    public bool isMoving = false;
    private GameObject _itemToLoot;
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        if(joystick == null)
        {
            joystick = FindObjectOfType<Joystick>();
        }
    }
    void Update()
    {
        if(joystick == null)
        {
            joystick = FindObjectOfType<Joystick>();
        }
        animator.SetBool("run", isMoving);
    }
    public void LootItem(GameObject item)
    {
         _itemToLoot = item;
        StartCoroutine(CheckLootDistance());
    }
    private IEnumerator CheckLootDistance()
    {
        while (true)
        {
            if (_itemToLoot != null)
            {
                float distance = Vector3.Distance(transform.position, _itemToLoot.transform.position);
                if (distance < 1f)
                {
                    yield return new WaitForSeconds(2f);
                    distance = Vector3.Distance(transform.position, _itemToLoot.transform.position);
                    if (distance < 1f)
                    {
                        _itemToLoot.transform.SetParent(transform);
                        _itemToLoot.transform.localPosition = Vector3.zero;
                        _itemToLoot = null;
                    }
                }
            }
            yield return null;
        }
    }
    void FixedUpdate()
    {
        float horizontal = joystick.Horizontal;
        float vertical = joystick.Vertical;
        
        Vector3 moveDirection = new Vector3(horizontal, 0, vertical);
        if (moveDirection.magnitude > 0.1f)
        {
            // Normalize the move direction to ensure consistent speed
            moveDirection.Normalize();
            isMoving = true;
            // Apply movement through Rigidbody velocity
            Vector3 targetVelocity = moveDirection * speed;
            targetVelocity.y = _rb.velocity.y; // Retain current vertical velocity (e.g., gravity effect)
            _rb.velocity = targetVelocity;

            // Rotate the player to face the movement direction
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, speed * 100 * Time.fixedDeltaTime);
        }
        else
        {
            // Stop horizontal movement if joystick is idle
            Vector3 idleVelocity = _rb.velocity;
            isMoving = false;
            idleVelocity.x = 0;
            idleVelocity.z = 0;
            _rb.velocity = idleVelocity;
            
        }
    
    }  
   
}
