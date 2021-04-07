using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;


public class Health : NetworkBehaviour
{
    [SerializeField] int maxHealth = 100;
    
    [SyncVar(hook = nameof(HandleHealthUpdated))]
    private int _currentHealth;

    public event Action ServerOnDie;
    public event Action<int, int> ClientOnHealthUpdated;
    
    #region Server

    public override void OnStartServer()
    {
        _currentHealth = maxHealth;
        UnitBase.ServerOnPlayerDie += ServerHandlePlayerDie;
    }

    public override void OnStopServer()
    {
        UnitBase.ServerOnPlayerDie -= ServerHandlePlayerDie;

    }

    [Server]
    private void ServerHandlePlayerDie(int connectionID)
    {
        if(connectionToClient.connectionId != connectionID) { return; }
        
        DealDamage(_currentHealth);
    }

    [Server]
    public void DealDamage(int damageAmount)
    {
        if(_currentHealth == 0) { return; }
        
        _currentHealth = Mathf.Max(_currentHealth - damageAmount,0);
        
        if(_currentHealth != 0) { return; }
        
        // dead
        Debug.Log("Dead");
        ServerOnDie?.Invoke();
        
    }
    #endregion

    #region Client

    private void HandleHealthUpdated(int oldHealth, int newHealth)
    {
        ClientOnHealthUpdated?.Invoke(newHealth,maxHealth);
    }
    
    

    #endregion
}
