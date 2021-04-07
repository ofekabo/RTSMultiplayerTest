using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ServerAnimationHandler : NetworkBehaviour
{
    [SerializeField] List<Unit> allUnits = new List<Unit>();

    private void Start()
    {
        Unit.ServerOnUnitSpawned += AddUnit;
        Unit.ServerOnUnitDespawned += RemoveUnit;
    }

    private void AddUnit(Unit unit)
    {
        allUnits.Add(unit);
    }

    private void RemoveUnit(Unit unit)
    {
        allUnits.Remove(unit);
    }

    [ServerCallback] // remove if aniatmions dont work
    private void Update()
    {
        foreach (Unit unit in allUnits)
        {
            unit.UnitMovement.UpdateAnimator();
        }
        
    }
    
}
