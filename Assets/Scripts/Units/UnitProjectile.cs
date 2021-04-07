using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Mirror;
using UnityEngine;

public class UnitProjectile : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] int damage = 20;
    [SerializeField] private float launchForce = 10f;
    [SerializeField] private float lifeTime = 5f;

    private void Start()
    {
        rb.velocity = transform.forward * launchForce;
    }

    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf),lifeTime);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity))
        {
            //  if this connection to client matches this connection its ours
            if(networkIdentity.connectionToClient == connectionToClient) { return; }
        }

        if (other.TryGetComponent<Health>(out Health health))
        {
            health.DealDamage(damage);
        }
        
        DestroySelf();
    }
    
    [Server]
    private void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
}
