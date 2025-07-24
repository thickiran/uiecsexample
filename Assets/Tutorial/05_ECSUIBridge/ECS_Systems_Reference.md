# ECS Systems Reference
## Complete Guide to Business Logic and Processing Systems

---

## 🏭 **System Architecture Overview**

Our ECS systems follow a clear separation of responsibilities:

```
┌─────────────────────────────────────────────────────────────────┐
│                        ECS SYSTEMS LAYER                        │
├─────────────────────────────────────────────────────────────────┤
│  InventoryManagementSystem  │  InventoryUISystem               │
│  • Business Logic           │  • UI Synchronization            │
│  • Data Validation          │  • Visual Updates                │
│  • Request Processing       │  • Event Handling                │
└─────────────────────────────────────────────────────────────────┘
            ▲                              ▲
            │                              │
    ┌───────────────┐              ┌─────────────┐
    │   REQUESTS    │              │   EVENTS    │
    │ (User Actions)│              │ (UI Updates)│
    └───────────────┘              └─────────────┘
```

---

## ⚙️ **InventoryManagementSystem**

### **Purpose**
The core business logic system that handles all inventory operations, validates moves, and maintains data integrity.

### **System Declaration**
```csharp
public partial class InventoryManagementSystem : SystemBase
{
    private EntityQuery moveRequestQuery;
    private EntityQuery pickupRequestQuery;
    private EntityQuery dropRequestQuery;
    
    protected override void OnCreate() {
        // Initialize queries for different request types
    }
    
    protected override void OnUpdate() {
        ProcessMoveRequests();
        ProcessPickupRequests();
        ProcessDropRequests();
    }
}
```

### **Key Responsibilities**

#### **1. Request Processing**
```csharp
private void ProcessMoveRequests()
{
    // Get all pending move requests
    var moveRequests = moveRequestQuery.ToEntityArray(Allocator.TempJob);
    var moveRequestData = moveRequestQuery.ToComponentDataArray<ItemMoveRequest>(Allocator.TempJob);
    var inventoryReferences = moveRequestQuery.ToComponentDataArray<InventoryReference>(Allocator.TempJob);
    
    for (int i = 0; i < moveRequests.Length; i++) {
        var request = moveRequestData[i];
        var inventoryRef = inventoryReferences[i];
        
        if (EntityManager.Exists(inventoryRef.inventoryEntity)) {
            ProcessItemMove(inventoryRef.inventoryEntity, request);
        }
        
        // Clean up request entity
        EntityManager.DestroyEntity(moveRequests[i]);
    }
    
    // Always dispose temporary allocations!
    moveRequests.Dispose();
    moveRequestData.Dispose();
    inventoryReferences.Dispose();
}
```

#### **2. Business Logic Implementation**
```csharp
private void ProcessItemMove(Entity inventoryEntity, ItemMoveRequest request)
{
    var slotsBuffer = EntityManager.GetBuffer<InventorySlot>(inventoryEntity);
    
    // Validate slot indices
    if (request.fromSlot < 0 || request.fromSlot >= slotsBuffer.Length ||
        request.toSlot < 0 || request.toSlot >= slotsBuffer.Length) {
        return; // Invalid request
    }
    
    var fromSlot = slotsBuffer[request.fromSlot];
    var toSlot = slotsBuffer[request.toSlot];
    
    if (fromSlot.isEmpty) {
        return; // Nothing to move
    }
    
    // Handle different scenarios
    if (toSlot.isEmpty) {
        // Move to empty slot
        slotsBuffer[request.toSlot] = fromSlot;
        slotsBuffer[request.fromSlot] = InventorySlot.Empty;
        CreateChangeEvent(inventoryEntity, request.toSlot, fromSlot.itemId, fromSlot.stackCount, true);
        CreateChangeEvent(inventoryEntity, request.fromSlot, fromSlot.itemId, fromSlot.stackCount, false);
    }
    else if (fromSlot.itemId == toSlot.itemId) {
        // Stack items (with validation)
        HandleItemStacking(inventoryEntity, request, fromSlot, toSlot);
    }
    else {
        // Swap items
        slotsBuffer[request.fromSlot] = toSlot;
        slotsBuffer[request.toSlot] = fromSlot;
        CreateChangeEvent(inventoryEntity, request.fromSlot, toSlot.itemId, toSlot.stackCount, true);
        CreateChangeEvent(inventoryEntity, request.toSlot, fromSlot.itemId, fromSlot.stackCount, true);
    }
}
```

#### **3. Item Stacking Logic**
```csharp
private void HandleItemStacking(Entity inventoryEntity, ItemMoveRequest request, 
                              InventorySlot fromSlot, InventorySlot toSlot)
{
    // Look up item data to get max stack size
    var itemDataQuery = GetEntityQuery(typeof(ItemDataComponent));
    var itemDataComponents = itemDataQuery.ToComponentDataArray<ItemDataComponent>(Allocator.TempJob);
    
    int maxStackSize = 99; // Default
    foreach (var itemData in itemDataComponents) {
        if (itemData.itemId == fromSlot.itemId) {
            maxStackSize = itemData.maxStackSize;
            break;
        }
    }
    
    // Calculate how much can be stacked
    int availableSpace = maxStackSize - toSlot.stackCount;
    int moveAmount = math.min(request.quantity, math.min(fromSlot.stackCount, availableSpace));
    
    if (moveAmount > 0) {
        var slotsBuffer = EntityManager.GetBuffer<InventorySlot>(inventoryEntity);
        
        // Update stack counts
        toSlot.stackCount += moveAmount;
        fromSlot.stackCount -= moveAmount;
        
        // Update slots
        if (fromSlot.stackCount <= 0) {
            slotsBuffer[request.fromSlot] = InventorySlot.Empty;
        } else {
            slotsBuffer[request.fromSlot] = fromSlot;
        }
        slotsBuffer[request.toSlot] = toSlot;
        
        // Create events
        CreateChangeEvent(inventoryEntity, request.toSlot, toSlot.itemId, moveAmount, true);
        if (fromSlot.stackCount <= 0) {
            CreateChangeEvent(inventoryEntity, request.fromSlot, fromSlot.itemId, moveAmount, false);
        }
    }
    
    itemDataComponents.Dispose();
}
```

#### **4. Event Creation**
```csharp
private void CreateChangeEvent(Entity inventoryEntity, int slotIndex, int itemId, int stackCount, bool wasAdded)
{
    var eventEntity = EntityManager.CreateEntity();
    EntityManager.AddComponentData(eventEntity, new InventoryChangedEvent {
        slotIndex = slotIndex,
        itemId = itemId,
        stackCount = stackCount,
        wasAdded = wasAdded,
        timestamp = SystemAPI.Time.ElapsedTime
    });
}
```

### **System Queries**
```csharp
protected override void OnCreate()
{
    // Query for move requests
    moveRequestQuery = GetEntityQuery(
        typeof(ItemMoveRequest), 
        typeof(InventoryReference)
    );
    
    // Query for pickup requests  
    pickupRequestQuery = GetEntityQuery(
        typeof(ItemPickupRequest), 
        typeof(InventoryReference)
    );
    
    // Query for drop requests
    dropRequestQuery = GetEntityQuery(
        typeof(ItemDropRequest), 
        typeof(InventoryReference)
    );
}
```

---

## 🖼️ **InventoryUISystem**

### **Purpose**
Synchronizes ECS data with Unity UI components, ensuring the visual representation always matches the authoritative ECS state.

### **System Declaration**
```csharp
public partial class InventoryUISystem : SystemBase
{
    private EntityQuery inventoryQuery;
    private EntityQuery itemDataQuery;
    private Dictionary<Entity, InventoryUIBridge> inventoryBridges;
    
    protected override void OnCreate() {
        // Initialize queries and bridge registry
    }
    
    protected override void OnUpdate() {
        UpdateInventoryUI();      // Sync data to UI
        ProcessInventoryChanges(); // Handle change events
    }
}
```

### **Key Responsibilities**

#### **1. UI Synchronization**
```csharp
private void UpdateInventoryUI()
{
    var inventoryEntities = inventoryQuery.ToEntityArray(Allocator.TempJob);
    var itemDataEntities = itemDataQuery.ToEntityArray(Allocator.TempJob);
    var itemDataComponents = itemDataQuery.ToComponentDataArray<ItemDataComponent>(Allocator.TempJob);
    
    // Create lookup table for item data
    var itemDataLookup = new Dictionary<int, ItemDataComponent>();
    for (int i = 0; i < itemDataEntities.Length; i++) {
        var itemData = itemDataComponents[i];
        itemDataLookup[itemData.itemId] = itemData;
    }
    
    // Update each inventory
    foreach (var inventoryEntity in inventoryEntities) {
        UpdateInventoryEntity(inventoryEntity, itemDataLookup);
    }
    
    // Clean up
    inventoryEntities.Dispose();
    itemDataEntities.Dispose();
    itemDataComponents.Dispose();
}
```

#### **2. Bridge Management**
```csharp
private void UpdateInventoryEntity(Entity inventoryEntity, Dictionary<int, ItemDataComponent> itemDataLookup)
{
    // Find or register UI bridge for this inventory
    if (!inventoryBridges.TryGetValue(inventoryEntity, out var bridge)) {
        bridge = Object.FindObjectOfType<InventoryUIBridge>();
        if (bridge != null) {
            bridge.Initialize(inventoryEntity);
            inventoryBridges[inventoryEntity] = bridge;
        } else {
            return; // No bridge available
        }
    }
    
    if (bridge == null || !bridge.IsInitialized()) {
        return;
    }
    
    // Get current inventory state
    var inventoryComponent = EntityManager.GetComponentData<InventoryComponent>(inventoryEntity);
    var inventorySlots = EntityManager.GetBuffer<InventorySlot>(inventoryEntity);
    
    // Update each slot in the UI
    for (int i = 0; i < inventorySlots.Length; i++) {
        var slotData = inventorySlots[i];
        
        if (!slotData.isEmpty && itemDataLookup.TryGetValue(slotData.itemId, out var itemData)) {
            bridge.UpdateSlot(i, slotData, itemData);
        } else {
            bridge.ClearSlot(i);
        }
    }
}
```

#### **3. Event Processing**
```csharp
private void ProcessInventoryChanges()
{
    var changeEvents = GetEntityQuery(typeof(InventoryChangedEvent)).ToEntityArray(Allocator.TempJob);
    
    foreach (var eventEntity in changeEvents) {
        var changeEvent = EntityManager.GetComponentData<InventoryChangedEvent>(eventEntity);
        OnInventoryChanged(changeEvent);
        
        // Remove processed event
        EntityManager.DestroyEntity(eventEntity);
    }
    
    changeEvents.Dispose();
}

private void OnInventoryChanged(InventoryChangedEvent changeEvent)
{
    // Notify all registered bridges
    foreach (var bridge in inventoryBridges.Values) {
        if (bridge != null && bridge.IsInitialized()) {
            bridge.OnInventoryChanged(changeEvent);
        }
    }
}
```

#### **4. Bridge Registration**
```csharp
public void RegisterInventoryBridge(Entity inventoryEntity, InventoryUIBridge bridge)
{
    inventoryBridges[inventoryEntity] = bridge;
    Debug.Log($"Registered UI bridge for inventory {inventoryEntity}");
}

public void UnregisterInventoryBridge(Entity inventoryEntity)
{
    if (inventoryBridges.Remove(inventoryEntity)) {
        Debug.Log($"Unregistered UI bridge for inventory {inventoryEntity}");
    }
}
```

### **System Queries**
```csharp
protected override void OnCreate()
{
    // Query for inventories with slots
    inventoryQuery = GetEntityQuery(
        ComponentType.ReadOnly<InventoryComponent>(),
        ComponentType.ReadOnly<InventorySlot>()
    );
    
    // Query for item definitions
    itemDataQuery = GetEntityQuery(
        ComponentType.ReadOnly<ItemDataComponent>()
    );
    
    inventoryBridges = new Dictionary<Entity, InventoryUIBridge>();
}
```

---

## 🔄 **System Interaction Patterns**

### **1. Request-Process-Event Pattern**
```
User Action → Request Entity → Management System → Data Change → Event Entity → UI System → Visual Update
```

Example flow:
1. **User drags item**: Creates `ItemMoveRequest`
2. **ManagementSystem**: Processes request, updates data
3. **ManagementSystem**: Creates `InventoryChangedEvent`
4. **UISystem**: Processes event, updates UI
5. **UISystem**: Destroys event entity

### **2. Data-Driven UI Updates**
```csharp
// ECS data changes
slotsBuffer[5] = InventorySlot.Create(itemId: 1, count: 3);

// UI automatically reflects changes (next frame)
// No manual UI.SetSlot() calls needed!
```

### **3. System Dependencies**
```
InventoryManagementSystem (writes data)
           ↓
InventoryUISystem (reads data, updates UI)
           ↓
UI Components (visual representation)
```

---

## ⚡ **Performance Optimizations**

### **1. Query Efficiency**
```csharp
// ✅ Efficient: Specific queries with required components
private EntityQuery inventoryQuery = GetEntityQuery(
    ComponentType.ReadOnly<InventoryComponent>(),
    ComponentType.ReadOnly<InventorySlot>()
);

// ❌ Inefficient: Too broad
private EntityQuery allEntities = GetEntityQuery(ComponentType.ReadOnly<IComponentData>());
```

### **2. Memory Management**
```csharp
// ✅ Always dispose temporary arrays
var entities = query.ToEntityArray(Allocator.TempJob);
try {
    // Process entities
} finally {
    entities.Dispose(); // Critical!
}
```

### **3. Batch Processing**
```csharp
// ✅ Process all requests in one frame
var allRequests = moveRequestQuery.ToEntityArray(Allocator.TempJob);
foreach (var request in allRequests) {
    ProcessRequest(request);
}

// ❌ Process one at a time
ProcessSingleRequest(); // Called multiple times per frame
```

### **4. Change Detection**
```csharp
// ✅ Event-driven updates (only when data changes)
CreateChangeEvent(slotIndex, itemId, count, wasAdded);

// ❌ Polling every frame
void Update() {
    CheckAllSlotsForChanges(); // Expensive!
}
```

---

## 🎯 **System Design Benefits**

### **1. Single Responsibility**
- **ManagementSystem**: Only handles business logic
- **UISystem**: Only handles visual updates
- **Clear boundaries**: No mixed concerns

### **2. Testability**
```csharp
// Easy to test business logic without UI
[Test]
public void TestItemMove() {
    var world = new World("Test");
    var system = world.GetOrCreateSystem<InventoryManagementSystem>();
    
    // Create test data
    var inventory = CreateTestInventory();
    var request = new ItemMoveRequest { fromSlot = 0, toSlot = 1 };
    
    // Process request
    system.ProcessItemMove(inventory, request);
    
    // Assert results
    Assert.IsTrue(slotIsEmpty(0));
    Assert.AreEqual(expectedItem, GetSlotItem(1));
}
```

### **3. Modularity**
- Add new systems without modifying existing ones
- Replace UI system without changing business logic
- Support multiple UIs (mobile, desktop, VR) with same data

### **4. Performance**
- Systems process data in optimized chunks
- Memory layout optimized for CPU cache
- Minimal garbage collection

This system architecture provides a robust foundation that scales from simple inventory operations to complex game economies!