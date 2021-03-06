﻿using UnityEngine;
using DanmakU;
using System.Collections;

/// <summary>
/// An enemy that fires bullets at the player and can be hurt by the player's dash mechanic.
/// </summary>
public partial class Enemy : DanmakuCollider
{
    [HideInInspector]
    public Player Player;
    [HideInInspector]
    public DanmakuField Field;
    [HideInInspector]
    public Wave Wave;
    [HideInInspector]
    public int Difficulty;
    
    // Enemy health values
    public int MaxHealth;
    [HideInInspector]
    public int Health;

    // Enemy rotation values
    [SerializeField]
    protected bool FacePlayer; // Enemy constantly rotates toward the player if true - overrides TargetRotation
    protected Quaternion TargetRotation; // Enemy constantly rotates toward this rotation if it is not null

    // Storage for enemy velocity when the level is paused
    private Vector3 oldVelocity;

    // Enemy health bar reference and size
    private GameObject healthBar;
    private float healthBarSize = 1.0f;
    
    [SerializeField]
    private GameObject healthBarPrefab;

    /// <summary>
    /// Called when the enemy is instantiated (before Start). Initializes the enemy.
    /// </summary>
    public override sealed void Awake()
    {
        base.Awake();
        Player = LevelController.Singleton.Player;
        Field = LevelController.Singleton.Field;
        Wave = LevelController.Singleton.Event.GetComponent<Wave>();
        Difficulty = Wave.Difficulty;
        TagFilter = "Friendly";

        healthBar = (GameObject)Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
        healthBar.transform.parent = transform;
        healthBar.transform.localScale = new Vector3(healthBarSize, 1, 1);

        Health = MaxHealth;
    }

    /// <summary>
    /// Called when the enemy is instantiated. Starts the Run coroutine.
    /// </summary>
    public virtual void Start()
    {
        StartCoroutine(Run());
    }

    /// <summary>
    /// Generic coroutine to control the enemy.
    /// </summary>
    protected virtual IEnumerator Run()
    {
        return null;
    }

    public virtual void Update()
    {
        // Stores the enemy's velocity when the level is paused
        if(LevelController.Singleton.Paused && oldVelocity == Vector3.zero)
        {
            oldVelocity = GetComponent<Rigidbody2D>().velocity;
            GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        }
        else if(!LevelController.Singleton.Paused && oldVelocity != Vector3.zero)
        {
            GetComponent<Rigidbody2D>().velocity = oldVelocity;
            oldVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// Called in fixed-time intervals. Updates the enemy rotation and handles other physics-related functions.
    /// </summary>
    /// <param name="warning">The warning prefab to spawn</param>
    /// <returns>The warning that was spawned</returns>
    public virtual void FixedUpdate()
    {
        if(!LevelController.Singleton.Paused)
        {
            if(FacePlayer)
                TargetRotation = Quaternion.LookRotation(Vector3.forward, Player.transform.position - transform.position);
            if(TargetRotation != null)
                transform.rotation = Quaternion.Slerp(transform.rotation, TargetRotation, Time.fixedDeltaTime * 8);
        }
    }

    /// <summary>
    /// Damages the enemy by a value.
    /// </summary>
    /// <param name="damage">The amount of damage to deal</param>
    public virtual void Damage(int damage)
    {
        Health -= damage;

        float healthProportion = (float)Health / MaxHealth;
        healthBar.GetComponentInChildren<HealthIndicator>().Activate(healthProportion);

        if(Health <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Kills the enemy and removes the enemy from the list of active enemies in the wave.
    /// </summary>
    public virtual void Die()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Called when the enemy's GameObject is destroyed. Makes sure that finalization code is executed.
    /// </summary>
    public void OnDestroy()
    {
        Wave.UnregisterEnemy(this);
    }

    #region Rotation methods

    /// <summary>
    /// Adds a value to the enemy's rotation.
    /// </summary>
    /// <param name="degrees">The angle of increase in degrees</param>
    protected void AddRotation(float degrees)
    {
        TargetRotation *= Quaternion.Euler(new Vector3(0, 0, degrees));
    }

    /// <summary>
    /// Sets the enemy's rotation.
    /// </summary>
    /// <param name="degrees">The angle of rotation in degrees</param>
    protected void SetRotation(float degrees)
    {
        TargetRotation = Quaternion.Euler(new Vector3(0, 0, degrees));
    }

    /// <summary>
    /// Rotates the enemy in a direction.
    /// </summary>
    /// <param name="direction">A vector representing the direction to rotate towards</param>
    protected void SetRotation(Vector3 direction)
    {
        SetRotation(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90);
    }

    /// <summary>
    /// Rotates the enemy toward a point.
    /// </summary>
    /// <param name="point">The point to rotate the enemy towards</param>
    protected void RotateTowards(Vector3 point)
    {
        SetRotation(point - transform.position);
    }

    #endregion

    /// <summary>
    /// Handles collision with a bullet.
    /// </summary>
    /// <param name="danmaku">The bullet that the enemy collided with</param>
    /// <param name="info">Collision information</param>
    protected override void DanmakuCollision(Danmaku danmaku, RaycastHit2D info)
    {
        Damage(danmaku.Damage);
        danmaku.Deactivate();
    }
}