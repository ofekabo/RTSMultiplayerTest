using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using System;

public class RTSPlayer : NetworkBehaviour
{
  [SerializeField] private Building[] buildings = new Building[0];
  
  [SyncVar(hook = nameof(ClientHandleResourcesUpdated))]
  private int resources = 500;
  
  public event Action<int> ClientOnResourcesUpdated;
  
  private List<Unit> myUnits = new List<Unit>();
  private List<Building> myBuildings = new List<Building>();

  public List<Unit> MyUnits { get => myUnits; }
  public List<Building> MyBuildings { get => myBuildings; }
  public int Resources { get => resources; }

  [Server]
  public void SetResources(int newResources)
  {
    resources = newResources;
  }
  
  #region Server


  public override void OnStartServer()
  {
    Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
    Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
    
    Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
    Building.ServerOnBuildingDespawned += ServerHandleBuildingDespawned;

  }

  [Command]
  public void CmdTryPlaceBuilding(int buildingID,Vector3 point)
  {
    Building buildingToPlace = null;

    foreach (var building in buildings)
    {
      if (building.ID == buildingID)
      {
        buildingToPlace = building;
        break;
      }
    }
    
    if(buildingToPlace == null) { return; }
    
    GameObject buildingInstance =
      Instantiate(buildingToPlace.gameObject,point,buildingToPlace.transform.rotation);
    
    NetworkServer.Spawn(buildingInstance,connectionToClient);
    
  }
  

  public override void OnStopServer()
  {
    Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
    Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
    
    Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
    Building.ServerOnBuildingDespawned -= ServerHandleBuildingDespawned;
  }

  private void ServerHandleUnitSpawned(Unit unit)
  {
    // if unit connection id not the same as player connection id then do nothing
    if(unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }
    
    myUnits.Add(unit);
  }
  private void ServerHandleUnitDespawned(Unit unit)
  {
    if(unit.connectionToClient.connectionId != connectionToClient.connectionId) { return; }
    
    myUnits.Remove(unit);
  }
  
  private void ServerHandleBuildingSpawned(Building building)
  {
    if(building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }
    
    myBuildings.Add(building);
  }
  private void ServerHandleBuildingDespawned(Building building)
  {
    if(building.connectionToClient.connectionId != connectionToClient.connectionId) { return; }
    
    myBuildings.Remove(building);
  }
  #endregion

  #region Client

  public override void OnStartAuthority()
  {
    if(NetworkServer.active) { return; }
    
    Unit.AuthorityOnUnitSpawned +=AuthorityHandleUnitSpawned;
    Unit.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
    
    Building.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
    Building.AuthorityOnBuildingDespawned += AuthorityHandleBuildingDespawned;
  }

  public override void OnStopClient()
  {
    if (!isClientOnly || !hasAuthority) { return; }
    
    Unit.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
    Unit.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
    
    Building.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
    Building.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
  }

  private void ClientHandleResourcesUpdated(int oldResources, int newResources)
  {
    ClientOnResourcesUpdated?.Invoke(newResources);
  }

  private void AuthorityHandleUnitSpawned(Unit unit)
  {
    myUnits.Add(unit);
  }
  private void AuthorityHandleUnitDespawned(Unit unit)
  {
    myUnits.Remove(unit);
  }
  
  private void AuthorityHandleBuildingSpawned(Building building)
  {
    MyBuildings.Add(building);
  }
  private void AuthorityHandleBuildingDespawned(Building building)
  {
    MyBuildings.Remove(building);
  }
  
  #endregion
}
