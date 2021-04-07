using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitSelectionHandler : MonoBehaviour
{
    [SerializeField] RectTransform unitSelectionArea;
    [SerializeField] private LayerMask layerMask = new LayerMask();
    
    private Vector2 _selectionAreaStartPos;
    
   private Camera _mainCam;
   private RTSPlayer _player;
   
   public List<Unit> selectedUnits { get; } = new List<Unit>();

   private void Start()
   {
      _mainCam = Camera.main;
      Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
      GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
   }


   private void Update()
   {
       if (_player == null)
       {
           try
           {
               _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
           }
           catch (NullReferenceException nullReferenceException)
           {
               Debug.LogWarning("Player is null , might trying to get component before player instantiated \n" + nullReferenceException);
           }
         
       }
       
       if (Mouse.current.leftButton.wasPressedThisFrame)
       {
           StartSelectionArea();
       }
       else if (Mouse.current.leftButton.wasReleasedThisFrame)
       {
           ClearSelectionArea();
           
       }
       else if (Mouse.current.leftButton.isPressed)
       {
           UpdateSelectionArea();
       }
   }

   private void StartSelectionArea()
   {
       if (!Keyboard.current.leftShiftKey.isPressed)
       {
           DeselectSelectedUnits();
       }
      
       
       unitSelectionArea.gameObject.SetActive(true);
       
       _selectionAreaStartPos = Mouse.current.position.ReadValue();
       
       UpdateSelectionArea();
   }
   private void UpdateSelectionArea()
   {
       Vector2 mousePos = Mouse.current.position.ReadValue();
       
       float areaWidth = mousePos.x - _selectionAreaStartPos.x;
       float areaHeight = mousePos.y - _selectionAreaStartPos.y;
       
       unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth),Mathf.Abs(areaHeight));
       
       unitSelectionArea.anchoredPosition = _selectionAreaStartPos + 
                                            new Vector2(areaWidth /2 ,areaHeight /2);
   }

   private void DeselectSelectedUnits()
   {
       foreach (var selectedUnit in selectedUnits)
       {
           selectedUnit.Deselect();
       }

       selectedUnits.Clear();
   }

   private void ClearSelectionArea()
   {
       unitSelectionArea.gameObject.SetActive(false);

       if (unitSelectionArea.sizeDelta.magnitude == 0)
       {
           Ray ray = _mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());
       
           if(!Physics.Raycast(ray,out RaycastHit hit,Mathf.Infinity,layerMask)) { return; }
       
           // if the thing we hit is not a Unit compononent : do nothing
           if(!hit.collider.TryGetComponent(out Unit unit)) { return; } 
       
           if(!unit.hasAuthority) { return; }
       
           selectedUnits.Add(unit);

           foreach (var selectedUnit in selectedUnits)
           {
               selectedUnit.Select();
           }
           
           return;
       }
      
       
       Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta /2);
       Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

       foreach (var unit in _player.MyUnits)
       {
           if(selectedUnits.Contains(unit)) { continue; }
           Vector3 screenPosition = _mainCam.WorldToScreenPoint(unit.transform.position);

           if (screenPosition.x > min.x &&
               screenPosition.x < max.x &&
               screenPosition.y > min.y &&
               screenPosition.y < max.y)
           {
                selectedUnits.Add(unit);
                unit.Select();
           }
       }
   }

   private void AuthorityHandleUnitDespawned(Unit unit)
   {
       selectedUnits.Remove(unit);
   }

   private void ClientHandleGameOver(string winnerName)
   {
       enabled = false;
   }
   
   private void OnDestroy()
   {
       Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
       GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
   }

}
