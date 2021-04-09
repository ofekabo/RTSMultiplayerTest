using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using System;
using Random = System.Random;

public class RTSPlayer : NetworkBehaviour
{
  [SerializeField] private LayerMask buildingBlockLayer = new LayerMask();
  [SerializeField] private Building[] buildings = new Building[0];
  [SerializeField] private float minBuildingRangeLimit = 2f;
  [SerializeField] private float buildingRangeLimit = 7f;
  
  [SyncVar(hook = nameof(ClientHandleResourcesUpdated))]
  private int resources = 500;
  
  public event Action<int> ClientOnResourcesUpdated;
  
  
  private List<Unit> myUnits = new List<Unit>();
  private List<Building> myBuildings = new List<Building>();
  private Color _teamColor = new Color();

  public List<Unit> MyUnits { get => myUnits; }
  public List<Building> MyBuildings { get => myBuildings; }
  public int Resources { get => resources; }
  public Color TeamColor { get => _teamColor; }
  

  public bool CanPlaceBuilding(BoxCollider buildingCollider, Vector3 point)
  {
    if (Physics.CheckBox(point + buildingCollider.center, buildingCollider.size / 2,
      Quaternion.identity, buildingBlockLayer))
    {
      return false;
    }
    
    // checking if desired location to build is between two points
    foreach (var building in myBuildings)
    {
      if ((point - building.transform.position).sqrMagnitude 
          <= buildingRangeLimit * buildingRangeLimit 
          &&
          (point - building.transform.position).sqrMagnitude
            >= minBuildingRangeLimit * minBuildingRangeLimit)
      {
        return  true;
      }
    }
    return false;
  }
  
  #region Server
  
  public override void OnStartServer()
  {
    Unit.ServerOnUnitSpawned += ServerHandleUnitSpawned;
    Unit.ServerOnUnitDespawned += ServerHandleUnitDespawned;
    
    Building.ServerOnBuildingSpawned += ServerHandleBuildingSpawned;
    Building.ServerOnBuildingDespawned += ServerHandleBuildingDespawned;

  }
  
  public override void OnStopServer()
  {
    Unit.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
    Unit.ServerOnUnitDespawned -= ServerHandleUnitDespawned;
    
    Building.ServerOnBuildingSpawned -= ServerHandleBuildingSpawned;
    Building.ServerOnBuildingDespawned -= ServerHandleBuildingDespawned;
  }
  
  [Server]
  public void SetTeamColor(Color newTeamColor)
  {
    _teamColor = newTeamColor;
  }

    
  [Server]
  public void SetResources(int newResources)
  {
    resources = newResources;
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
    
    if(resources < buildingToPlace.Price) { return; }
    
    BoxCollider buildingCollider = buildingToPlace.GetComponent<BoxCollider>();
    
    
    
    if(!CanPlaceBuilding(buildingCollider,point)) { return; }
    
    GameObject buildingInstance =
      Instantiate(buildingToPlace.gameObject,point,Quaternion.Euler(buildingToPlace.transform.rotation.x,UnityEngine.Random.Range(180f,245f),buildingToPlace.transform.rotation.z));
    
    NetworkServer.Spawn(buildingInstance,connectionToClient);
    
    SetResources(resources - buildingToPlace.Price);
    
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
