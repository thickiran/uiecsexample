using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public partial class InventoryManagementSystem : SystemBase
{
    private EntityQuery moveRequestQuery;
    private EntityQuery pickupRequestQuery;
    private EntityQuery dropRequestQuery;
    
    protected override void OnCreate()
    {
        moveRequestQuery = GetEntityQuery(typeof(ItemMoveRequest), typeof(InventoryReference));
        pickupRequestQuery = GetEntityQuery(typeof(ItemPickupRequest), typeof(InventoryReference));
        dropRequestQuery = GetEntityQuery(typeof(ItemDropRequest), typeof(InventoryReference));
    }
    
    protected override void OnUpdate()
    {
        ProcessMoveRequests();
        ProcessPickupRequests();
        ProcessDropRequests();
    }
    
    private void ProcessMoveRequests()
    {
        var moveRequests = moveRequestQuery.ToEntityArray(Allocator.TempJob);
        var moveRequestData = moveRequestQuery.ToComponentDataArray<ItemMoveRequest>(Allocator.TempJob);
        var inventoryReferences = moveRequestQuery.ToComponentDataArray<InventoryReference>(Allocator.TempJob);
        
        if (moveRequests.Length > 0)
        {
            Debug.Log($"InventoryManagementSystem: Processing {moveRequests.Length} move requests");
        }
        
        for (int i = 0; i < moveRequests.Length; i++)
        {
            var request = moveRequestData[i];
            var inventoryRef = inventoryReferences[i];
            
            Debug.Log($"InventoryManagementSystem: Processing move request from slot {request.fromSlot} to slot {request.toSlot}");
            
            if (EntityManager.Exists(inventoryRef.inventoryEntity))
            {
                ProcessItemMove(inventoryRef.inventoryEntity, request);
                Debug.Log($"InventoryManagementSystem: Move request processed successfully");
            }
            else
            {
                Debug.LogWarning($"InventoryManagementSystem: Inventory entity {inventoryRef.inventoryEntity} does not exist");
            }
            
            // Clean up request entity
            EntityManager.DestroyEntity(moveRequests[i]);
        }
        
        moveRequests.Dispose();
        moveRequestData.Dispose();
        inventoryReferences.Dispose();
    }
    
    private void ProcessPickupRequests()
    {
        var pickupRequests = pickupRequestQuery.ToEntityArray(Allocator.TempJob);
        var pickupRequestData = pickupRequestQuery.ToComponentDataArray<ItemPickupRequest>(Allocator.TempJob);
        var inventoryReferences = pickupRequestQuery.ToComponentDataArray<InventoryReference>(Allocator.TempJob);
        
        for (int i = 0; i < pickupRequests.Length; i++)
        {
            var request = pickupRequestData[i];
            var inventoryRef = inventoryReferences[i];
            
            if (EntityManager.Exists(inventoryRef.inventoryEntity))
            {
                ProcessItemPickup(inventoryRef.inventoryEntity, request);
            }
            
            // Clean up request entity
            EntityManager.DestroyEntity(pickupRequests[i]);
        }
        
        pickupRequests.Dispose();
        pickupRequestData.Dispose();
        inventoryReferences.Dispose();
    }
    
    private void ProcessDropRequests()
    {
        var dropRequests = dropRequestQuery.ToEntityArray(Allocator.TempJob);
        var dropRequestData = dropRequestQuery.ToComponentDataArray<ItemDropRequest>(Allocator.TempJob);
        var inventoryReferences = dropRequestQuery.ToComponentDataArray<InventoryReference>(Allocator.TempJob);
        
        for (int i = 0; i < dropRequests.Length; i++)
        {
            var request = dropRequestData[i];
            var inventoryRef = inventoryReferences[i];
            
            if (EntityManager.Exists(inventoryRef.inventoryEntity))
            {
                ProcessItemDrop(inventoryRef.inventoryEntity, request);
            }
            
            // Clean up request entity
            EntityManager.DestroyEntity(dropRequests[i]);
        }
        
        dropRequests.Dispose();
        dropRequestData.Dispose();
        inventoryReferences.Dispose();
    }
    
    private void ProcessItemMove(Entity inventoryEntity, ItemMoveRequest request)
    {
        var slotsBuffer = EntityManager.GetBuffer<InventorySlot>(inventoryEntity);
        
        if (request.fromSlot < 0 || request.fromSlot >= slotsBuffer.Length ||
            request.toSlot < 0 || request.toSlot >= slotsBuffer.Length)
        {
            return; // Invalid slot indices
        }
        
        var fromSlot = slotsBuffer[request.fromSlot];
        var toSlot = slotsBuffer[request.toSlot];
        
        if (fromSlot.isEmpty)
        {
            return; // Nothing to move
        }
        
        // Handle different move scenarios
        if (toSlot.isEmpty)
        {
            // Move to empty slot
            slotsBuffer[request.toSlot] = fromSlot;
            slotsBuffer[request.fromSlot] = InventorySlot.Empty;
            CreateChangeEvent(inventoryEntity, request.toSlot, fromSlot.itemId, fromSlot.stackCount, true);
            CreateChangeEvent(inventoryEntity, request.fromSlot, fromSlot.itemId, fromSlot.stackCount, false);
        }
        else if (fromSlot.itemId == toSlot.itemId)
        {
            // Stack items
            var itemDataQuery = GetEntityQuery(typeof(ItemDataComponent));
            var itemDataComponents = itemDataQuery.ToComponentDataArray<ItemDataComponent>(Allocator.TempJob);
            
            int maxStackSize = 99; // Default
            foreach (var itemData in itemDataComponents)
            {
                if (itemData.itemId == fromSlot.itemId)
                {
                    maxStackSize = itemData.maxStackSize;
                    break;
                }
            }
            
            int availableSpace = maxStackSize - toSlot.stackCount;
            int moveAmount = math.min(request.quantity, math.min(fromSlot.stackCount, availableSpace));
            
            if (moveAmount > 0)
            {
                toSlot.stackCount += moveAmount;
                fromSlot.stackCount -= moveAmount;
                
                if (fromSlot.stackCount <= 0)
                {
                    slotsBuffer[request.fromSlot] = InventorySlot.Empty;
                }
                else
                {
                    slotsBuffer[request.fromSlot] = fromSlot;
                }
                
                slotsBuffer[request.toSlot] = toSlot;
                
                CreateChangeEvent(inventoryEntity, request.toSlot, toSlot.itemId, moveAmount, true);
                if (fromSlot.stackCount <= 0)
                {
                    CreateChangeEvent(inventoryEntity, request.fromSlot, fromSlot.itemId, moveAmount, false);
                }
            }
            
            itemDataComponents.Dispose();
        }
        else
        {
            // Swap items
            slotsBuffer[request.fromSlot] = toSlot;
            slotsBuffer[request.toSlot] = fromSlot;
            
            CreateChangeEvent(inventoryEntity, request.fromSlot, toSlot.itemId, toSlot.stackCount, true);
            CreateChangeEvent(inventoryEntity, request.toSlot, fromSlot.itemId, fromSlot.stackCount, true);
        }
    }
    
    private void ProcessItemPickup(Entity inventoryEntity, ItemPickupRequest request)
    {
        var slotsBuffer = EntityManager.GetBuffer<InventorySlot>(inventoryEntity);
        
        // Find empty slot or stackable slot
        for (int i = 0; i < slotsBuffer.Length; i++)
        {
            var slot = slotsBuffer[i];
            
            if (slot.isEmpty)
            {
                // Add to empty slot
                slotsBuffer[i] = InventorySlot.Create(request.itemId, request.quantity);
                CreateChangeEvent(inventoryEntity, i, request.itemId, request.quantity, true);
                return;
            }
            else if (slot.itemId == request.itemId)
            {
                // Try to stack
                var itemDataQuery = GetEntityQuery(typeof(ItemDataComponent));
                var itemDataComponents = itemDataQuery.ToComponentDataArray<ItemDataComponent>(Allocator.TempJob);
                
                int maxStackSize = 99; // Default
                foreach (var itemData in itemDataComponents)
                {
                    if (itemData.itemId == request.itemId)
                    {
                        maxStackSize = itemData.maxStackSize;
                        break;
                    }
                }
                
                int availableSpace = maxStackSize - slot.stackCount;
                int addAmount = math.min(request.quantity, availableSpace);
                
                if (addAmount > 0)
                {
                    slot.stackCount += addAmount;
                    slotsBuffer[i] = slot;
                    CreateChangeEvent(inventoryEntity, i, request.itemId, addAmount, true);
                    
                    // If we couldn't add all, continue looking for more slots
                    if (addAmount < request.quantity)
                    {
                        var remainingRequest = request;
                        remainingRequest.quantity -= addAmount;
                        ProcessItemPickup(inventoryEntity, remainingRequest);
                    }
                    return;
                }
                
                itemDataComponents.Dispose();
            }
        }
    }
    
    private void ProcessItemDrop(Entity inventoryEntity, ItemDropRequest request)
    {
        var slotsBuffer = EntityManager.GetBuffer<InventorySlot>(inventoryEntity);
        
        if (request.slotIndex < 0 || request.slotIndex >= slotsBuffer.Length)
        {
            return; // Invalid slot index
        }
        
        var slot = slotsBuffer[request.slotIndex];
        
        if (slot.isEmpty)
        {
            return; // Nothing to drop
        }
        
        int dropAmount = math.min(request.quantity, slot.stackCount);
        
        slot.stackCount -= dropAmount;
        
        if (slot.stackCount <= 0)
        {
            slotsBuffer[request.slotIndex] = InventorySlot.Empty;
        }
        else
        {
            slotsBuffer[request.slotIndex] = slot;
        }
        
        CreateChangeEvent(inventoryEntity, request.slotIndex, slot.itemId, dropAmount, false);
        
        // TODO: Create world item entity at drop position
        Debug.Log($"Dropped {dropAmount} of item {slot.itemId} at position {request.worldPosition}");
    }
    
    private void CreateChangeEvent(Entity inventoryEntity, int slotIndex, int itemId, int stackCount, bool wasAdded)
    {
        var eventEntity = EntityManager.CreateEntity();
        EntityManager.AddComponentData(eventEntity, new InventoryChangedEvent
        {
            slotIndex = slotIndex,
            itemId = itemId,
            stackCount = stackCount,
            wasAdded = wasAdded,
            timestamp = SystemAPI.Time.ElapsedTime
        });
    }
}