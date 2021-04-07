using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : NetworkBehaviour
{
   public NavMeshAgent agent = null;
   public Animator anim;
   [SerializeField] private Targeter targeter = null;
   [Header("Checks")]
   [SerializeField] float chaseRange = 10f;

   
   
   #region Server

   public override void OnStartServer()
   {
      GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
   }

   public override void OnStopServer()
   {
      GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
   }

   [ServerCallback]
   private void Update()
   {
      
      Targetable target = targeter.Target;
      if (target != null)
      {
         if ((target.transform.position - transform.position).sqrMagnitude > chaseRange * chaseRange)
         {
            agent.SetDestination(target.transform.position);
         }
         else if (agent.hasPath)
         {
            agent.ResetPath();
         }
         
        return; 
      }
      
      if(!agent.hasPath) { return; }
      if(agent.remainingDistance > agent.stoppingDistance) { return; }
      
      agent.ResetPath();
   }

   [Command]
   public void CmdMove(Vector3 position)
   {
      ServerMove(position);
   }

   [Server]
   public void ServerMove(Vector3 position)
   {
      targeter.ClearTarget();
      
      if (!NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, NavMesh.AllAreas)) { return; }
      
      agent.SetDestination(position);
   }

   [Server]
   private void ServerHandleGameOver()
   {
      agent.ResetPath();
   }
   
   #endregion

   #region Client

   public void UpdateAnimator()
   {
      Vector3 velocity = agent.velocity;
      Vector3 localVelocity = transform.InverseTransformDirection(velocity);
      float speed = localVelocity.z;
      anim.SetFloat("ForwardSpeed", speed);
   }
   
   #endregion
   
}
