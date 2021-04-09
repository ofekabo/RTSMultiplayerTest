using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using Unity.Mathematics;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Random = System.Random;

public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Building building = null;
    [SerializeField] private Image iconImage  = null;
    [SerializeField] private TMP_Text priceText = null;
    [SerializeField] private LayerMask floorMask = new LayerMask();
    
    private Camera _mainCam;
    private RTSPlayer _player;
    private GameObject buildingPreviewInstance;
    private Renderer[] buildingRendererInstace;
    private BoxCollider buildingCollider;

    private void Start()
    {
        _mainCam = Camera.main;
        
        iconImage.sprite = building.Icon;
        priceText.text = building.Price.ToString();
        
        buildingCollider = building.GetComponent<BoxCollider>();
    }

    private void Update()
    {
        if (_player == null)
        {
            _player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        }
        
        if(buildingPreviewInstance == null) { return; }
        
        UpdateBuildingPreview();
     }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(eventData.button != PointerEventData.InputButton.Left) { return; }
        
        if(_player.Resources < building.Price) { return; }
        
        buildingPreviewInstance = Instantiate(building.BuildingPreview);
        buildingPreviewInstance.transform.rotation = Quaternion.Euler(0,UnityEngine.Random.Range(180f,245f),0);
        buildingRendererInstace = buildingPreviewInstance.GetComponentsInChildren<Renderer>();
        buildingPreviewInstance.SetActive(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(buildingPreviewInstance == null) { return; }
        Ray ray = _mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
        {
            _player.CmdTryPlaceBuilding(building.ID,hit.point);
        }
        
        Destroy(buildingPreviewInstance);
    }

    private void UpdateBuildingPreview()
    {
        Ray ray = _mainCam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) { return; }
        buildingPreviewInstance.transform.position = hit.point;
        
        
        if(!buildingPreviewInstance.activeSelf)
            buildingPreviewInstance.SetActive(true);
        
        Color color = _player.CanPlaceBuilding(buildingCollider,hit.point) ? Color.green : Color.red;

        foreach (var renderer in buildingRendererInstace)
        {
            renderer.material.color = color;
        }
    }
}
