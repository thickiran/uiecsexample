using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

public partial class InventoryUISystem : SystemBase
{
    private EntityQuery inventoryQuery;
    private EntityQuery itemDataQuery;
    private Dictionary<Entity, InventoryUIBridge> inventoryBridges;
    
    protected override void OnCreate()
    {
        inventoryQuery = GetEntityQuery(
            ComponentType.ReadOnly<InventoryComponent>(),
            ComponentType.ReadOnly<InventorySlot>()
        );
        
        itemDataQuery = GetEntityQuery(
            ComponentType.ReadOnly<ItemDataComponent>()
        );
        
        inventoryBridges = new Dictionary<Entity, InventoryUIBridge>();
    }
    
    protected override void OnUpdate()
    {
        UpdateInventoryUI();
        ProcessInventoryChanges();
    }
    
    private void UpdateInventoryUI()
    {
        var inventoryEntities = inventoryQuery.ToEntityArray(Allocator.TempJob);
        var itemDataEntities = itemDataQuery.ToEntityArray(Allocator.TempJob);
        var itemDataComponents = itemDataQuery.ToComponentDataArray<ItemDataComponent>(Allocator.TempJob);
        
        // Create lookup for item data
        var itemDataLookup = new Dictionary<int, ItemDataComponent>();
        for (int i = 0; i < itemDataEntities.Length; i++)
        {
            var itemData = itemDataComponents[i];
            itemDataLookup[itemData.itemId] = itemData;
        }
        
        foreach (var inventoryEntity in inventoryEntities)
        {
            UpdateInventoryEntity(inventoryEntity, itemDataLookup);
        }
        
        inventoryEntities.Dispose();
        itemDataEntities.Dispose();
        itemDataComponents.Dispose();
    }
    
    private void UpdateInventoryEntity(Entity inventoryEntity, Dictionary<int, ItemDataComponent> itemDataLookup)
    {
        if (!inventoryBridges.TryGetValue(inventoryEntity, out var bridge))
        {
            // Try to find existing bridge in scene
            bridge = Object.FindObjectOfType<InventoryUIBridge>();
            if (bridge != null)
            {
                bridge.Initialize(inventoryEntity);
                inventoryBridges[inventoryEntity] = bridge;
            }
            else
            {
                return; // No bridge found
            }
        }
        
        if (bridge == null || !bridge.IsInitialized())
        {
            return;
        }
        
        var inventoryComponent = EntityManager.GetComponentData<InventoryComponent>(inventoryEntity);
        var inventorySlots = EntityManager.GetBuffer<InventorySlot>(inventoryEntity);
        
        // Update each slot
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            var slotData = inventorySlots[i];
            var itemData = default(ItemDataComponent);
            
            if (!slotData.isEmpty && itemDataLookup.TryGetValue(slotData.itemId, out itemData))
            {
                bridge.UpdateSlot(i, slotData, itemData);
            }
            else
            {
                bridge.ClearSlot(i);
            }
        }
    }
    
    private void ProcessInventoryChanges()
    {
        var changeEvents = GetEntityQuery(typeof(InventoryChangedEvent)).ToEntityArray(Allocator.TempJob);
        
        foreach (var eventEntity in changeEvents)
        {
            var changeEvent = EntityManager.GetComponentData<InventoryChangedEvent>(eventEntity);
            OnInventoryChanged(changeEvent);
            
            // Remove processed event
            EntityManager.DestroyEntity(eventEntity);
        }
        
        changeEvents.Dispose();
    }
    
    private void OnInventoryChanged(InventoryChangedEvent changeEvent)
    {
        // Find the bridge that needs updating
        foreach (var bridge in inventoryBridges.Values)
        {
            if (bridge != null && bridge.IsInitialized())
            {
                bridge.OnInventoryChanged(changeEvent);
            }
        }
    }
    
    public void RegisterInventoryBridge(Entity inventoryEntity, InventoryUIBridge bridge)
    {
        inventoryBridges[inventoryEntity] = bridge;
    }
    
    public void UnregisterInventoryBridge(Entity inventoryEntity)
    {
        inventoryBridges.Remove(inventoryEntity);
    }
    
    protected override void OnDestroy()
    {
        inventoryBridges?.Clear();
    }
}