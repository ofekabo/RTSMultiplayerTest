using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuVisual : MonoBehaviour
{
   [SerializeField] Transform aimObject;
   [SerializeField] Transform shootingPoint;
   [SerializeField] LayerMask aimLayerMask = new LayerMask();
   [SerializeField] GameObject bulletHitEffect = null;
   [SerializeField] GameObject shootingEffect = null;
   
   

   
   
   private void Update()
   {
      Vector3 mousePos = Mouse.current.position.ReadValue();
      
      Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
      
      if(Physics.Raycast(ray,out RaycastHit hit, Mathf.Infinity, aimLayerMask))
      {
         aimObject.position = hit.point;
      }

      if (Mouse.current.leftButton.wasPressedThisFrame)
      {
         GameObject shootingFXInstance = Instantiate(shootingEffect,shootingPoint.position,Quaternion.identity);
         GameObject bulletFXInstace = Instantiate(bulletHitEffect,hit.point,Quaternion.identity);
         
         Destroy(bulletFXInstace,1.5f);
         Destroy(bulletFXInstace,1.5f);
      }
      
     
   }
   
   
}
