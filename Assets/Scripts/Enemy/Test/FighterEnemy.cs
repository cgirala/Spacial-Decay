﻿using UnityEngine;
using System.Collections;
using DanmakU;
using DanmakU.Modifiers;

public class FighterEnemy : Enemy
{
    public DanmakuPrefab bulletPrefab;

    private FireBuilder fireData;
    private float fireCooldown = MAX_FIRE_COOLDOWN;
    private static readonly float MAX_FIRE_COOLDOWN = 1f;

	public void Start()
    {
        fireData = new FireBuilder(bulletPrefab, field);
        fireData.From(transform);
        fireData.Towards(player.transform);
        fireData.WithSpeed(6);
        fireData.WithModifier(new CircularBurstModifier(100, 5, 0, 0));
	}
	
	public override void NormalUpdate()
    {
        fireCooldown -= Time.deltaTime;
	    if(fireCooldown <= 0)
        {
            fireData.Fire();
            fireCooldown = MAX_FIRE_COOLDOWN;
        }
	}

    public override void NormalFixedUpdate()
    {
        Vector3 direction = player.transform.position - transform.position;
        GetComponent<Rigidbody2D>().velocity = direction / direction.magnitude * 3;
        if(direction.magnitude <= 5)
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }
    }
}