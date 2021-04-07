﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitCommandController : MonoBehaviour
{
    [SerializeField] private UnitSelectionHandler unitSelectionHandle = null;
    [SerializeField] private LayerMask layerMask = new LayerMask();

    private Camera _mainCam;
    private void Start()
    {
        _mainCam = Camera.main;
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    

    private void Update()
    {
        if(!Mouse.current.rightButton.wasPressedThisFrame) { return; }
        
        Ray ray = _mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        
        if(!Physics.Raycast(ray,out RaycastHit hit,Mathf.Infinity, layerMask)) { return; }

        if (hit.collider.TryGetComponent<Targetable>(out Targetable target))
        {
            if (target.hasAuthority)
            {
                TryMove(hit.point);
                return;
            }
            
            TryTarget(target);
            return;
        }
        TryMove(hit.point);
    }

    private void TryTarget(Targetable target)
    {
        foreach (var unit in unitSelectionHandle.selectedUnits)
        {
            unit.Targeter.CmdSetTarget(target.gameObject);
        }
    }

    private void TryMove(Vector3 hitPoint)
    {
        foreach (var unit in unitSelectionHandle.selectedUnits)
        {
            unit.UnitMovement.CmdMove(hitPoint);
        }
    }

    private void ClientHandleGameOver(string winnerName)
    {
        enabled = false;
    }
    
    private void OnDestroy()
    {
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }
    
}
