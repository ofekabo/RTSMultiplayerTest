﻿using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
  [Header("Refrences")]
  [SerializeField] private Health health;
  [SerializeField] private Unit unitPrefab = null;
  [SerializeField] private Transform unitSpawnPoint = null;
  [SerializeField] private TMP_Text remainingUnitsText = null;
  [SerializeField] private Image unitProgressImage = null;
  [SerializeField] private int maxUnitQueue = 5;
  [SerializeField] private float spawnMoveRange = 6;
  [SerializeField] private float unitSpawnTime = 5f;

  [SyncVar(hook = nameof(ClientHandleQueuedUnitsUpdated))]
  private int queuedUnits;
  [SyncVar]
  private float unitTimer;
  
  private float progressImageVelocity;

  private void Update()
  {
    if (isServer)
    {
      ProduceUnits();
    }

    if (isClient)
    {
      UpdateTimerDisplay();
    }
  }

  #region Server

  public override void OnStartServer()
  {
    health.ServerOnDie += ServerHandleDie;
  }

  public override void OnStopServer()
  {
    health.ServerOnDie -= ServerHandleDie;
  }

  [Server]
  private void ProduceUnits()
  {
    if(queuedUnits == 0){ return; }
    
    unitTimer += Time.deltaTime;
    
    if(unitTimer < unitSpawnTime) { return;}
    
    GameObject unitInstance = Instantiate
      (unitPrefab.gameObject,unitSpawnPoint.position,unitSpawnPoint.rotation);
    
    NetworkServer.Spawn(unitInstance,connectionToClient);
    
    Vector3 spawnOffset = UnityEngine.Random.insideUnitSphere * spawnMoveRange;
    spawnOffset.y = unitSpawnPoint.position.y;
    UnitMovement unitMovement = unitInstance.GetComponent<UnitMovement>();
    
    unitMovement.ServerMove(unitSpawnPoint.position + spawnOffset);
    
    queuedUnits--;
    unitTimer = 0;
  }

  [Server]
  private void ServerHandleDie()
  {
    NetworkServer.Destroy(gameObject);
  }
  
  [Command]
  private void CmdSpawnUnit()
  {
    if(queuedUnits == maxUnitQueue) { return; }
    
    RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();
    if(player.Resources < unitPrefab.ResourceCost) { return;}
    
    queuedUnits++;
    
    player.SetResources(player.Resources - unitPrefab.ResourceCost);
  }
  

  #endregion

  #region Client

  private void UpdateTimerDisplay()
  {
    float newProgress = unitTimer / unitSpawnTime;

    if (newProgress < unitProgressImage.fillAmount)
    {
      unitProgressImage.fillAmount = newProgress;
    }
    else
    {
      unitProgressImage.fillAmount = Mathf.SmoothDamp(
        unitProgressImage.fillAmount,
        newProgress,
        ref progressImageVelocity,0.1f);
    }
  }
  
  public void OnPointerClick(PointerEventData eventData)
  {
    if(eventData.button != PointerEventData.InputButton.Left) { return; }
    
    if(!hasAuthority) { return; }
    
    CmdSpawnUnit();
  }

  private void ClientHandleQueuedUnitsUpdated(int oldUnits, int newUnits)
  {
    remainingUnitsText.text = newUnits.ToString();
  }

  #endregion

 
}
