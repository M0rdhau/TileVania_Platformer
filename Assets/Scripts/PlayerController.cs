﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [Header("Movement")]

    [SerializeField] float walkSpeedMax = 3f;
    [SerializeField] float runSpeedMax = 6f;
    [SerializeField] float acceleration = 0.2f;
    [SerializeField] float jumpVelocity = 6f;
    [SerializeField] float climbSpeed = 3f;
    //vertical velocity necessary to roll
    [SerializeField] float rollTime = 0.8f;
    Vector2 accelerationVector;
    float fallTime;
    bool isRunning = false;
    bool isClimbing = false;
    bool isCrouching = false;
    bool isJumping = false;
    bool isFalling = false;
    bool jumpAxisInUse = false;

    [Header("Combat")]

    [SerializeField] Transform attackPoint;
    [SerializeField] float attackRange = 0.8f;
    [SerializeField] int attackDamage = 5;
    [SerializeField] float attackRate = 2f;
    float nextAttackTime = 0f;
    LayerMask enemyLayers;

    SpriteRenderer _renderer;
    Animator _animator;
    Collider2D bodyCollider;
    Collider2D feetCollider;
    Collider2D handsCollider;
    Rigidbody2D _rigidBody;


    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        bodyCollider = GetComponent<Collider2D>();
        feetCollider = transform.GetChild(2).GetComponent<Collider2D>();
        handsCollider = transform.GetChild(3).GetComponent<Collider2D>();
        _renderer = GetComponentInChildren<SpriteRenderer>();
        _rigidBody = GetComponent<Rigidbody2D>();
        enemyLayers = LayerMask.GetMask("Enemies");
    }

    // Update is called once per frame
    void Update()
    {
        HandleAcceleration();
        HandleMovementInput();
        HandleAttackInput();
        CheckForFalling();
    }

    private void HandleAttackInput()
    {
        if (Time.time >= nextAttackTime)
        {
            if (Input.GetAxis("Fire3") > 0)
            {
                Attack("kick");
                nextAttackTime = Time.time + 1f / attackRate;
            }
            if (Input.GetAxis("Fire2") > 0)
            {
                Attack("punch");
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }
    }

    private void CheckAttackPoint()
    {
        var diff = transform.position.x - attackPoint.position.x;
        if ((_renderer.flipX && diff < 0) || (!_renderer.flipX && diff > 0))
        {
            FlipAttackPoint(diff);
        }
    }

    void FlipAttackPoint(float diff)
    {
        Vector2 pos = attackPoint.position;
        pos.x += 2*diff;
        attackPoint.position = pos;
    }

    private void Attack(string attName)
    {
        CheckAttackPoint();
        _animator.SetTrigger(attName);
        Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);
        foreach (Collider2D enemy in enemiesHit)
        {
            if (enemy.GetComponent<Health>())
            {
                enemy.GetComponent<Health>().DamageHealth(attackDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var collisionLayer = collision.gameObject.layer;
        if ( (collisionLayer == 8 || collisionLayer == 9 )  && (isFalling || isJumping))
        {
            isFalling = false;
            isJumping = false;
            _animator.SetBool("isFalling", false);
            if (collisionLayer == 8)
            {
                if (Time.time - fallTime < rollTime)
                {
                    _animator.SetTrigger("landed_Noroll");
                }
                else
                {
                    _animator.SetTrigger("landed");
                }
            }
        }
    }

    #region Movement

    void HandleMovementInput()
    {
        HandleClimb();

        if (Input.GetAxis("Horizontal") != 0)
        {
            HandleHorizontalMovement(Input.GetAxis("Horizontal"));
        }
        else
        {
            Vector2 vel = _rigidBody.velocity;
            vel.x = 0;
            _rigidBody.velocity = vel;
            _animator.SetBool("isRunning", false);
            _animator.SetBool("isWalking", false);
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }

        if (Input.GetAxis("Jump") != 0)
        {
            if (!jumpAxisInUse)
            {
                Jump();
                jumpAxisInUse = true;
            }
        }
        if (Input.GetAxis("Jump") == 0)
        {
            jumpAxisInUse = false;
        }
    }

    private void HandleClimb()
    {
        float axisThrow = Input.GetAxis("Vertical");
        if (isTouchingLadders())
        {
            if (!isTouchingGround() && isTouchingLadders())
            {
                _rigidBody.gravityScale = 0;
                isClimbing = true;
                _animator.SetBool("isClimbing", isClimbing);
            }
            
            if (axisThrow != 0)
            {
                _animator.speed = 1;
                var climbVec = _rigidBody.velocity;
                climbVec.y = climbSpeed*axisThrow;
                _rigidBody.velocity = climbVec;

            }
            else if (isClimbing)
            {
                _animator.speed = 0;
                var climbVec = _rigidBody.velocity;
                climbVec.y = 0;
                _rigidBody.velocity = climbVec;
            }
        }
        else
        {
            _rigidBody.gravityScale = 1;
            _animator.speed = 1;
            isClimbing = false;
            _animator.SetBool("isClimbing", isClimbing);
            _animator.SetTrigger("landed_Noroll");
        }

    }

    private void CheckForFalling()
    {
        if (_rigidBody.velocity.y < -1*Mathf.Epsilon && !isTouchingGround() && !isFalling && !isClimbing)
        {
            isJumping = false;
            isFalling = true;
            _animator.SetBool("isFalling", true);
            fallTime = Time.time;
        }
    }

    private void Jump()
    {
        if (isTouchingGround() || isTouchingLadders())
        {
            var jumpVec = _rigidBody.velocity;
            jumpVec.y = jumpVelocity;
            _rigidBody.velocity = jumpVec;
            isJumping = true;
            _animator.SetTrigger("jump");
        }
    }

    private void HandleHorizontalMovement(float axisThrow)
    {
        accelerationVector.x = axisThrow * acceleration;
        if (isTouchingGround())
        {
            _animator.SetBool("isRunning", false);
            _animator.SetBool("isWalking", true);
            if (isRunning)
            {
                _animator.SetBool("isRunning", true);
            }
        }

        if (accelerationVector.x > 0)
        {
            _renderer.flipX = false;
        }
        else if (accelerationVector.x < 0)
        {
            _renderer.flipX = true;
        }
    }

    void HandleAcceleration()
    {
        Vector2 vel = _rigidBody.velocity;

        if (!isRunning)
        {
            vel += accelerationVector;
            vel.x = Mathf.Clamp(vel.x, -walkSpeedMax, walkSpeedMax);
        }
        else
        {
            vel += accelerationVector * 2;
            vel.x = Mathf.Clamp(vel.x, -runSpeedMax, runSpeedMax);
        }

        _rigidBody.velocity = vel;
    }

    private bool isTouchingLadders()
    {
        return handsCollider.IsTouchingLayers(LayerMask.GetMask("Ladders"));
    }

    private bool isTouchingGround()
    {
        return feetCollider.IsTouchingLayers(LayerMask.GetMask("Ground"));
    }

    #endregion
}
