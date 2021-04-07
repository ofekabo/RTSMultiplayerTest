using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Targeter : NetworkBehaviour
{
    private Targetable _target;
    public Targetable Target { get => _target;}
    #region Server

    public override void OnStartServer()
    {
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;

    }

    [Command]
    public void CmdSetTarget(GameObject targetGameObject)
    {
        if(!targetGameObject.TryGetComponent<Targetable>(out Targetable newTarget)) { return; }
        
        _target = newTarget;
    }

    [Server]
    private void ServerHandleGameOver()
    {
        ClearTarget();
    }

    [Server]
    public void ClearTarget()
    {
        _target = null;
    }
    
    #endregion

    
    #region Client

    

    #endregion
}
