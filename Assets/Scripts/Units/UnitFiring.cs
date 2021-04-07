using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class UnitFiring : NetworkBehaviour
{
    [SerializeField] UnitMovement unitMovement;
    [SerializeField] private Targeter targeter;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private  Transform shootingPoint;
    [SerializeField] private float fireRange = 7f;
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private  float rotationSpeed = 20f;

    private Targetable target;
    private float _lastFireTime;


    [ServerCallback]
    private void Update()
    {
        target = targeter.Target;

        _lastFireTime += Time.deltaTime;
        
        if(target == null) { return; }
        
        if (!CanFireAtTarget()) { return; }
        unitMovement.agent.ResetPath();
        
        Quaternion targetRotation = Quaternion.LookRotation
            (target.transform.position - transform.position);

        transform.rotation = Quaternion.RotateTowards
            (transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (_lastFireTime > fireRate)
        {
            Quaternion bulletRotation = Quaternion.LookRotation
                (target.AimAtPoint.position - shootingPoint.position);
            
            GameObject bulletInstance = Instantiate(projectilePrefab, shootingPoint.position, bulletRotation);
            
            NetworkServer.Spawn(bulletInstance, connectionToClient);
            
            _lastFireTime = 0;
        }
        
        
    }

    [Server]
    private bool CanFireAtTarget()
    {
        
        return (targeter.Target.transform.position - transform.position).sqrMagnitude
               <= fireRange * fireRange;
        
    }
    
    
    
}
