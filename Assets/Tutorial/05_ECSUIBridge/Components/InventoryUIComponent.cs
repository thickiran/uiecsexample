using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

public struct InventoryComponent : IComponentData
{
    public int maxSlots;
    public int currentItemCount;
}

public struct InventorySlot : IBufferElementData
{
    public int itemId;
    public int stackCount;
    public bool isEmpty;
    
    public static InventorySlot Empty => new InventorySlot
    {
        itemId = -1,
        stackCount = 0,
        isEmpty = true
    };
    
    public static InventorySlot Create(int id, int count)
    {
        return new InventorySlot
        {
            itemId = id,
            stackCount = count,
            isEmpty = false
        };
    }
}

public struct PlayerInventoryTag : IComponentData { }

public struct InventoryChangedEvent : IComponentData
{
    public int slotIndex;
    public int itemId;
    public int stackCount;
    public bool wasAdded;
    public double timestamp;
}

public struct ItemPickupRequest : IComponentData
{
    public int itemId;
    public int quantity;
    public float3 worldPosition;
}

public struct ItemDropRequest : IComponentData
{
    public int slotIndex;
    public int quantity;
    public float3 worldPosition;
}

public struct ItemMoveRequest : IComponentData
{
    public int fromSlot;
    public int toSlot;
    public int quantity;
}

// Item data component for ECS
public struct ItemDataComponent : IComponentData
{
    public int itemId;
    public FixedString64Bytes itemName;
    public int maxStackSize;
    public ECSItemType itemType;
    public int value;
}

public enum ECSItemType : byte
{
    None = 0,
    Weapon = 1,
    Armor = 2,
    Consumable = 3,
    Material = 4,
    Quest = 5
}