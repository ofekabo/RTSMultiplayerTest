using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class Unit : NetworkBehaviour
{
   [Header("Config")]
   [SerializeField] Health health;
   [SerializeField] int resourceCost = 10;
   [Header("Refrences")]
   [SerializeField] UnitFiring unitFiring;
   [SerializeField] UnitMovement unitMovement;
   [SerializeField] Targeter targeter;
   [SerializeField] private UnityEvent onSelected = null;
   [SerializeField] private UnityEvent onDeselected = null;

   public static event Action<Unit> ServerOnUnitSpawned;
   public static event Action<Unit> ServerOnUnitDespawned;
   public static event Action<Unit> AuthorityOnUnitSpawned;
   public static event Action<Unit> AuthorityOnUnitDespawned;

   
   public UnitMovement UnitMovement { get => unitMovement; }
   public UnitFiring UnitFiring { get=> unitFiring; }
   public Targeter Targeter { get => targeter; }

   public int ResourceCost { get => resourceCost; }


   #region Server

   public override void OnStartServer()
   {
      ServerOnUnitSpawned?.Invoke(this);
      
      health.ServerOnDie += ServerHandleDie;
   }

   public override void OnStopServer()
   {
      ServerOnUnitDespawned?.Invoke(this);
      
      health.ServerOnDie -= ServerHandleDie;
   }

   [Server]
   private void ServerHandleDie()
   {
      NetworkServer.Destroy(gameObject);
   }
   
   #endregion
   
   
   #region Client

   public override void OnStartAuthority()
   {
      if(!hasAuthority) { return; }
      AuthorityOnUnitSpawned?.Invoke(this);
   }

   public override void OnStopClient()
   {
      if(!hasAuthority) { return; }
      AuthorityOnUnitDespawned?.Invoke(this);
   }
   
   [Client]
   public void Select()
   {
      if(!hasAuthority) { return; }
      onSelected?.Invoke();
   }

   [Client]
   public void Deselect()
   {
      if(!hasAuthority) { return; }
      onDeselected?.Invoke();
   }

 

   #endregion
}
