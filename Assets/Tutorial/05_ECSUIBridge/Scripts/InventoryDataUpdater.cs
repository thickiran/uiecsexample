using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class InventoryDataUpdater : MonoBehaviour
{
    [Header("Test Data")]
    public int testItemId = 1;
    public int testQuantity = 1;
    public int testSlot = 0;
    
    private Entity playerInventoryEntity;
    private EntityManager entityManager;
    private bool isInitialized = false;
    
    // Cache for SubScene ItemData entities
    private Entity healthPotionEntity;
    private Entity ironSwordEntity;
    private Entity woodEntity;
    private Entity leatherArmorEntity;
    
    void Start()
    {
        StartCoroutine(DelayedInitializeECS());
    }
    
    System.Collections.IEnumerator DelayedInitializeECS()
    {
        Debug.Log("InventoryDataUpdater: Starting delayed ECS initialization...");
        
        // Wait for end of frame to ensure all Start() methods complete
        yield return new WaitForEndOfFrame();
        
        // Wait until ECS world is fully initialized
        int attempts = 0;
        while (World.DefaultGameObjectInjectionWorld == null && attempts < 100)
        {
            yield return null;
            attempts++;
        }
        
        if (World.DefaultGameObjectInjectionWorld != null)
        {
            Debug.Log("InventoryDataUpdater: ECS World ready, initializing...");
            InitializeECS();
        }
        else
        {
            Debug.LogError("InventoryDataUpdater: Failed to initialize - ECS world unavailable after 100 attempts");
        }
    }
    
    void InitializeECS()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world != null)
        {
            entityManager = world.EntityManager;
            
            if (entityManager == null)
            {
                Debug.LogError("InventoryDataUpdater: EntityManager is null even after world access");
                return;
            }
            
            Debug.Log("InventoryDataUpdater: Creating player inventory and finding SubScene items...");
            CreatePlayerInventory();
            FindSubSceneItemData();
            isInitialized = true;
            Debug.Log("InventoryDataUpdater: ECS initialization completed successfully");
        }
        else
        {
            Debug.LogError("InventoryDataUpdater: ECS World is still null in InitializeECS()");
        }
    }
    
    void CreatePlayerInventory()
    {
        // Create player inventory entity
        playerInventoryEntity = entityManager.CreateEntity();
        
        // Add inventory component
        entityManager.AddComponentData(playerInventoryEntity, new InventoryComponent
        {
            maxSlots = 50,
            currentItemCount = 0
        });
        
        // Add player tag
        entityManager.AddComponentData(playerInventoryEntity, new PlayerInventoryTag());
        
        // Add inventory slots buffer
        var slotsBuffer = entityManager.AddBuffer<InventorySlot>(playerInventoryEntity);
        
        // Initialize all slots as empty
        for (int i = 0; i < 50; i++)
        {
            slotsBuffer.Add(InventorySlot.Empty);
        }
        
        // Find and initialize UI bridge
        var uiBridge = FindObjectOfType<InventoryUIBridge>();
        if (uiBridge != null)
        {
            uiBridge.Initialize(playerInventoryEntity);
        }
    }
    
    void FindSubSceneItemData()
    {
        Debug.Log("InventoryDataUpdater: Searching for SubScene ItemData entities...");
        
        // Query for all ItemData entities
        var itemDataQuery = entityManager.CreateEntityQuery(typeof(ItemDataComponent));
        var entities = itemDataQuery.ToEntityArray(Allocator.Temp);
        var itemDataComponents = itemDataQuery.ToComponentDataArray<ItemDataComponent>(Allocator.Temp);
        
        Debug.Log($"InventoryDataUpdater: Found {entities.Length} ItemData entities in SubScene");
        
        // Cache entities by itemId
        for (int i = 0; i < entities.Length; i++)
        {
            var itemData = itemDataComponents[i];
            switch (itemData.itemId)
            {
                case 1:
                    healthPotionEntity = entities[i];
                    Debug.Log($"Found Health Potion entity: {itemData.itemName}");
                    break;
                case 2:
                    ironSwordEntity = entities[i];
                    Debug.Log($"Found Iron Sword entity: {itemData.itemName}");
                    break;
                case 3:
                    woodEntity = entities[i];
                    Debug.Log($"Found Wood entity: {itemData.itemName}");
                    break;
                case 4:
                    leatherArmorEntity = entities[i];
                    Debug.Log($"Found Leather Armor entity: {itemData.itemName}");
                    break;
                default:
                    Debug.Log($"Found unknown item entity: ID {itemData.itemId}, Name: {itemData.itemName}");
                    break;
            }
        }
        
        entities.Dispose();
        itemDataComponents.Dispose();
        
        // Verify we found the expected items
        if (healthPotionEntity == Entity.Null) Debug.LogWarning("Health Potion entity not found in SubScene!");
        if (ironSwordEntity == Entity.Null) Debug.LogWarning("Iron Sword entity not found in SubScene!");
        if (woodEntity == Entity.Null) Debug.LogWarning("Wood entity not found in SubScene!");
        if (leatherArmorEntity == Entity.Null) Debug.LogWarning("Leather Armor entity not found in SubScene!");
    }
    
    ItemDataComponent? GetItemData(int itemId)
    {
        Entity targetEntity = Entity.Null;
        
        switch (itemId)
        {
            case 1: targetEntity = healthPotionEntity; break;
            case 2: targetEntity = ironSwordEntity; break;
            case 3: targetEntity = woodEntity; break;
            case 4: targetEntity = leatherArmorEntity; break;
            default: 
                Debug.LogWarning($"GetItemData: Unknown itemId {itemId}");
                return null;
        }
        
        if (targetEntity == Entity.Null || !entityManager.Exists(targetEntity))
        {
            Debug.LogWarning($"GetItemData: Entity for itemId {itemId} not found or invalid");
            return null;
        }
        
        return entityManager.GetComponentData<ItemDataComponent>(targetEntity);
    }
    
    void Update()
    {
        if (!isInitialized) return;
        
        // Test controls
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AddItemToSlot(1, 1, 0); // Health potion to slot 0
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            AddItemToSlot(2, 1, 1); // Iron sword to slot 1
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            AddItemToSlot(3, 5, 2); // 5 wood to slot 2
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            AddItemToSlot(4, 1, 3); // Leather armor to slot 3
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            ClearInventory();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            FillInventoryWithTestItems();
        }
    }
    
    public void AddItemToSlot(int itemId, int quantity, int slotIndex)
    {
        if (!isInitialized || !entityManager.Exists(playerInventoryEntity)) return;
        
        // Get item data for stack size validation
        var itemData = GetItemData(itemId);
        if (itemData == null)
        {
            Debug.LogError($"AddItemToSlot: Cannot find ItemData for itemId {itemId}");
            return;
        }
        
        var slotsBuffer = entityManager.GetBuffer<InventorySlot>(playerInventoryEntity);
        
        if (slotIndex >= 0 && slotIndex < slotsBuffer.Length)
        {
            var currentSlot = slotsBuffer[slotIndex];
            int maxStackSize = itemData.Value.maxStackSize;
            
            if (currentSlot.isEmpty)
            {
                // Add new item, clamp to max stack size
                int actualQuantity = Mathf.Min(quantity, maxStackSize);
                slotsBuffer[slotIndex] = InventorySlot.Create(itemId, actualQuantity);
                
                if (actualQuantity < quantity)
                {
                    Debug.Log($"AddItemToSlot: Clamped {itemData.Value.itemName} quantity from {quantity} to {actualQuantity} (max stack: {maxStackSize})");
                }
            }
            else if (currentSlot.itemId == itemId)
            {
                // Stack existing item, respect max stack size
                int availableSpace = maxStackSize - currentSlot.stackCount;
                if (availableSpace <= 0)
                {
                    Debug.Log($"AddItemToSlot: {itemData.Value.itemName} slot {slotIndex} is already at max stack size ({maxStackSize})");
                    return;
                }
                
                int actualQuantity = Mathf.Min(quantity, availableSpace);
                currentSlot.stackCount += actualQuantity;
                slotsBuffer[slotIndex] = currentSlot;
                
                if (actualQuantity < quantity)
                {
                    Debug.Log($"AddItemToSlot: Added {actualQuantity} {itemData.Value.itemName} (wanted {quantity}, max stack: {maxStackSize})");
                }
            }
            else
            {
                Debug.Log($"Slot {slotIndex} is occupied by a different item");
                return;
            }
            
            // Create change event with actual quantity added
            CreateChangeEvent(slotIndex, itemId, quantity, true);
            
            // Update inventory count
            var inventoryComponent = entityManager.GetComponentData<InventoryComponent>(playerInventoryEntity);
            if (currentSlot.isEmpty)
            {
                inventoryComponent.currentItemCount++;
                entityManager.SetComponentData(playerInventoryEntity, inventoryComponent);
            }
        }
    }
    
    public void RemoveItemFromSlot(int slotIndex, int quantity = -1)
    {
        if (!isInitialized || !entityManager.Exists(playerInventoryEntity)) return;
        
        var slotsBuffer = entityManager.GetBuffer<InventorySlot>(playerInventoryEntity);
        
        if (slotIndex >= 0 && slotIndex < slotsBuffer.Length)
        {
            var currentSlot = slotsBuffer[slotIndex];
            
            if (!currentSlot.isEmpty)
            {
                int removeAmount = quantity == -1 ? currentSlot.stackCount : quantity;
                removeAmount = Mathf.Min(removeAmount, currentSlot.stackCount);
                
                currentSlot.stackCount -= removeAmount;
                
                if (currentSlot.stackCount <= 0)
                {
                    slotsBuffer[slotIndex] = InventorySlot.Empty;
                    
                    // Update inventory count
                    var inventoryComponent = entityManager.GetComponentData<InventoryComponent>(playerInventoryEntity);
                    inventoryComponent.currentItemCount--;
                    entityManager.SetComponentData(playerInventoryEntity, inventoryComponent);
                }
                else
                {
                    slotsBuffer[slotIndex] = currentSlot;
                }
                
                // Create change event
                CreateChangeEvent(slotIndex, currentSlot.itemId, removeAmount, false);
            }
        }
    }
    
    public void ClearInventory()
    {
        if (!isInitialized || !entityManager.Exists(playerInventoryEntity)) return;
        
        var slotsBuffer = entityManager.GetBuffer<InventorySlot>(playerInventoryEntity);
        
        for (int i = 0; i < slotsBuffer.Length; i++)
        {
            slotsBuffer[i] = InventorySlot.Empty;
        }
        
        // Update inventory count
        var inventoryComponent = entityManager.GetComponentData<InventoryComponent>(playerInventoryEntity);
        inventoryComponent.currentItemCount = 0;
        entityManager.SetComponentData(playerInventoryEntity, inventoryComponent);
    }
    
    public void FillInventoryWithTestItems()
    {
        AddItemToSlot(1, 3, 0);  // 3 health potions
        AddItemToSlot(2, 1, 1);  // 1 iron sword
        AddItemToSlot(3, 10, 2); // 10 wood
        AddItemToSlot(4, 1, 3);  // 1 leather armor
        AddItemToSlot(1, 5, 4);  // 5 more health potions
    }
    
    private void CreateChangeEvent(int slotIndex, int itemId, int quantity, bool wasAdded)
    {
        var eventEntity = entityManager.CreateEntity();
        entityManager.AddComponentData(eventEntity, new InventoryChangedEvent
        {
            slotIndex = slotIndex,
            itemId = itemId,
            stackCount = quantity,
            wasAdded = wasAdded,
            timestamp = Time.timeAsDouble
        });
    }
    
    public Entity GetPlayerInventoryEntity()
    {
        return playerInventoryEntity;
    }
    
    public bool IsInitialized()
    {
        return isInitialized;
    }
}