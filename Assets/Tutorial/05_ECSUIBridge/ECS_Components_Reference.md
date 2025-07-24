# ECS Components Reference
## Complete Guide to All ECS Data Structures

---

## 📊 **Core Inventory Components**

### **InventoryComponent**
```csharp
public struct InventoryComponent : IComponentData
{
    public int maxSlots;        // Maximum number of slots (50 in our case)
    public int currentItemCount; // Number of occupied slots
}
```

**Purpose**: Main inventory metadata
**Usage**: One per inventory entity
**Example**:
```csharp
entityManager.AddComponentData(inventoryEntity, new InventoryComponent {
    maxSlots = 50,
    currentItemCount = 0
});
```

---

### **InventorySlot** 
```csharp
public struct InventorySlot : IBufferElementData
{
    public int itemId;          // Which item type (-1 = empty)
    public int stackCount;      // How many items in this stack
    public bool isEmpty;        // Quick check for empty state
    
    // Helper methods
    public static InventorySlot Empty => new InventorySlot {
        itemId = -1, stackCount = 0, isEmpty = true
    };
    
    public static InventorySlot Create(int id, int count) => new InventorySlot {
        itemId = id, stackCount = count, isEmpty = false
    };
}
```

**Purpose**: Individual slot data in the inventory grid
**Usage**: Dynamic buffer of 50 elements per inventory
**Key Concepts**:
- **IBufferElementData**: Allows dynamic arrays in ECS
- **Buffer**: Like `List<InventorySlot>` but ECS-optimized
- **Indexing**: `buffer[0]` = top-left slot, `buffer[49]` = bottom-right slot

**Example**:
```csharp
// Get the buffer of all slots
var slotsBuffer = entityManager.GetBuffer<InventorySlot>(inventoryEntity);

// Add item to first slot
slotsBuffer[0] = InventorySlot.Create(itemId: 1, count: 5);

// Clear a slot
slotsBuffer[5] = InventorySlot.Empty;

// Check if slot is empty
if (slotsBuffer[10].isEmpty) {
    // Slot is available
}
```

---

### **PlayerInventoryTag**
```csharp
public struct PlayerInventoryTag : IComponentData { }
```

**Purpose**: Identifies which inventory belongs to the player
**Usage**: Distinguishes player inventory from chests, banks, etc.
**Pattern**: "Tag Component" - no data, just identification

**Example**:
```csharp
// Find the player's inventory
var query = GetEntityQuery(typeof(InventoryComponent), typeof(PlayerInventoryTag));
var playerInventory = query.GetSingletonEntity();
```

---

## 🎯 **Event Components**

### **InventoryChangedEvent**
```csharp
public struct InventoryChangedEvent : IComponentData
{
    public int slotIndex;       // Which slot changed
    public int itemId;          // What item was affected
    public int stackCount;      // How many items were involved
    public bool wasAdded;       // true = added, false = removed
    public double timestamp;    // When the change occurred
}
```

**Purpose**: Notifies UI systems when inventory data changes
**Lifecycle**: Created by business logic, consumed by UI systems, then destroyed
**Usage**: Event-driven UI updates

**Example**:
```csharp
// Create change event when item is added
var eventEntity = EntityManager.CreateEntity();
EntityManager.AddComponentData(eventEntity, new InventoryChangedEvent {
    slotIndex = 0,
    itemId = 1,
    stackCount = 3,
    wasAdded = true,
    timestamp = Time.timeAsDouble
});

// UI system processes and destroys event
var changeEvents = GetEntityQuery(typeof(InventoryChangedEvent)).ToEntityArray(Allocator.TempJob);
foreach (var eventEntity in changeEvents) {
    var changeEvent = EntityManager.GetComponentData<InventoryChangedEvent>(eventEntity);
    ProcessUIUpdate(changeEvent);
    EntityManager.DestroyEntity(eventEntity); // Clean up
}
```

---

## 🎮 **Request Components**

### **ItemMoveRequest**
```csharp
public struct ItemMoveRequest : IComponentData
{
    public int fromSlot;        // Source slot index (0-49)
    public int toSlot;          // Target slot index (0-49)
    public int quantity;        // How many to move (for partial stacks)
}
```

**Purpose**: Request to move items between slots
**Created By**: UI drag & drop system
**Processed By**: InventoryManagementSystem

---

### **ItemPickupRequest**
```csharp
public struct ItemPickupRequest : IComponentData
{
    public int itemId;          // What item to add
    public int quantity;        // How many to add
    public float3 worldPosition; // Where the pickup occurred (for effects)
}
```

**Purpose**: Request to add items to inventory
**Created By**: Keyboard shortcuts (1-6), world item pickups
**Processed By**: InventoryManagementSystem

---

### **ItemDropRequest**
```csharp
public struct ItemDropRequest : IComponentData
{
    public int slotIndex;       // Which slot to drop from
    public int quantity;        // How many to drop (-1 = all)
    public float3 worldPosition; // Where to drop the item
}
```

**Purpose**: Request to remove items from inventory
**Created By**: Right-click drop, inventory actions
**Processed By**: InventoryManagementSystem

---

## 🏷️ **Item Definition Components**

### **ItemDataComponent**
```csharp
public struct ItemDataComponent : IComponentData
{
    public int itemId;                      // Unique identifier
    public FixedString64Bytes itemName;     // Display name (ECS-safe string)
    public int maxStackSize;                // How many can stack together
    public ECSItemType itemType;            // Category of item
    public int value;                       // Base value for trading
}
```

**Purpose**: Defines the properties of each item type
**Usage**: One entity per item type in the game
**Key Concepts**:
- **FixedString64Bytes**: ECS-compatible string type
- **Reference Data**: Looked up by itemId when needed

**Example**:
```csharp
// Create health potion definition
var healthPotionEntity = entityManager.CreateEntity();
entityManager.AddComponentData(healthPotionEntity, new ItemDataComponent {
    itemId = 1,
    itemName = "Health Potion",
    maxStackSize = 10,
    itemType = ECSItemType.Consumable,
    value = 50
});

// Look up item data by ID
var itemDataQuery = GetEntityQuery(typeof(ItemDataComponent));
var allItemData = itemDataQuery.ToComponentDataArray<ItemDataComponent>(Allocator.TempJob);

foreach (var itemData in allItemData) {
    if (itemData.itemId == 1) {
        Debug.Log($"Found: {itemData.itemName}");
        break;
    }
}
```

---

### **ECSItemType**
```csharp
public enum ECSItemType : byte
{
    None = 0,
    Weapon = 1,
    Armor = 2,
    Consumable = 3,
    Material = 4,
    Quest = 5
}
```

**Purpose**: Categorizes items for filtering, sorting, and game logic
**Type**: `byte` for memory efficiency
**Extensible**: Easy to add new types

---

## 🔗 **Bridge Components**

### **InventoryReference**
```csharp
public struct InventoryReference : IComponentData
{
    public Entity inventoryEntity;  // Which inventory this refers to
}
```

**Purpose**: Links request entities to specific inventories
**Usage**: Attached to requests to specify target inventory

**Example**:
```csharp
// Create move request for specific inventory
var requestEntity = entityManager.CreateEntity();
entityManager.AddComponentData(requestEntity, new ItemMoveRequest { ... });
entityManager.AddComponentData(requestEntity, new InventoryReference {
    inventoryEntity = playerInventoryEntity
});
```

---

## 🧠 **Design Patterns in Our Components**

### **1. Data-Only Components**
```csharp
// ✅ Good: Pure data
public struct InventorySlot : IBufferElementData {
    public int itemId;
    public int stackCount;
}

// ❌ Bad: Mixed data and logic
public struct InventorySlot : IBufferElementData {
    public int itemId;
    public void MoveToSlot(int target) { ... } // Logic doesn't belong here
}
```

### **2. Event-Driven Architecture**
```csharp
// Systems create events
CreateChangeEvent(slotIndex, itemId, quantity, wasAdded);

// Other systems consume events
ProcessInventoryChanges(); // UI updates based on events
```

### **3. Request-Response Pattern**
```csharp
// UI creates requests
var requestEntity = CreateMoveRequest(fromSlot, toSlot);

// Business logic processes requests
ProcessMoveRequests(); // Validates and executes moves

// Events notify about results
CreateChangeEvent(); // UI updates automatically
```

### **4. Reference Entities**
```csharp
// Item definitions stored as entities
Entity healthPotionDef = CreateItemDefinition(id: 1, name: "Health Potion");

// Inventory slots reference by ID
InventorySlot slot = InventorySlot.Create(itemId: 1, count: 5);
```

---

## 💡 **Performance Considerations**

### **Memory Layout**
- **Components**: Stored contiguously in memory chunks
- **Systems**: Process chunks linearly for CPU cache efficiency
- **Buffers**: Dynamic arrays optimized for ECS

### **Query Efficiency**
```csharp
// ✅ Efficient: Specific queries
var inventoryQuery = GetEntityQuery(typeof(InventoryComponent), typeof(PlayerInventoryTag));

// ❌ Inefficient: Overly broad queries
var allEntities = GetEntityQuery(typeof(IComponentData)); // Too generic
```

### **Memory Management**
```csharp
// ✅ Always dispose temporary allocations
var entities = query.ToEntityArray(Allocator.TempJob);
// ... use entities
entities.Dispose(); // Important!

// ✅ Use appropriate allocators
Allocator.TempJob    // Single frame operations
Allocator.Persistent // Data that lives longer
```

This component system provides a solid foundation for any inventory system while maintaining clean separation between data and logic!