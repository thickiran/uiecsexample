using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class UISlotReference : MonoBehaviour
{
    [Header("ECS References")]
    public Entity inventoryEntity;
    public int slotIndex;
    
    [Header("UI Components")]
    public ItemSlotUI slotUI;
    
    private bool isInitialized = false;
    
    void Awake()
    {
        if (slotUI == null)
        {
            slotUI = GetComponent<ItemSlotUI>();
        }
    }
    
    public void Initialize(Entity inventory, int index)
    {
        inventoryEntity = inventory;
        slotIndex = index;
        isInitialized = true;
    }
    
    public bool IsInitialized()
    {
        return isInitialized && inventoryEntity != Entity.Null;
    }
    
    public void UpdateFromECS(InventorySlot slotData, ItemDataComponent itemData)
    {
        if (slotUI == null) return;
        
        if (slotData.isEmpty)
        {
            slotUI.ClearSlot();
        }
        else
        {
            // Convert ECS item data to UI item data
            var uiItemData = new ItemData
            {
                itemId = slotData.itemId,
                itemName = itemData.itemName.ToString(),
                maxStackSize = itemData.maxStackSize,
                itemType = ConvertItemType(itemData.itemType),
                description = $"A {itemData.itemType} item"
            };
            
            slotUI.SetItem(uiItemData, slotData.stackCount);
        }
    }
    
    private ItemType ConvertItemType(ECSItemType ecsType)
    {
        return ecsType switch
        {
            ECSItemType.Weapon => ItemType.Weapon,
            ECSItemType.Armor => ItemType.Armor,
            ECSItemType.Consumable => ItemType.Consumable,
            ECSItemType.Material => ItemType.Material,
            ECSItemType.Quest => ItemType.Quest,
            _ => ItemType.Material
        };
    }
    
    public void RequestItemMove(int targetSlotIndex, int quantity)
    {
        if (!IsInitialized()) return;
        
        var world = World.DefaultGameObjectInjectionWorld;
        var entityManager = world.EntityManager;
        
        // Create move request entity
        var requestEntity = entityManager.CreateEntity();
        entityManager.AddComponentData(requestEntity, new ItemMoveRequest
        {
            fromSlot = slotIndex,
            toSlot = targetSlotIndex,
            quantity = quantity
        });
        
        // Add reference to source inventory
        entityManager.AddComponentData(requestEntity, new InventoryReference
        {
            inventoryEntity = inventoryEntity
        });
    }
    
    public void RequestItemDrop(int quantity, Vector3 worldPosition)
    {
        if (!IsInitialized()) return;
        
        var world = World.DefaultGameObjectInjectionWorld;
        var entityManager = world.EntityManager;
        
        // Create drop request entity
        var requestEntity = entityManager.CreateEntity();
        entityManager.AddComponentData(requestEntity, new ItemDropRequest
        {
            slotIndex = slotIndex,
            quantity = quantity,
            worldPosition = worldPosition
        });
        
        entityManager.AddComponentData(requestEntity, new InventoryReference
        {
            inventoryEntity = inventoryEntity
        });
    }
}

public struct InventoryReference : IComponentData
{
    public Entity inventoryEntity;
}