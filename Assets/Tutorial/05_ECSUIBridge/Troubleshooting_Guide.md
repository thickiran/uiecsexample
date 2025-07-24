# Troubleshooting Guide & Best Practices
## Common Issues and Solutions for ECS Integration

---

## 🚨 **Common Issues & Solutions**

### **Problem 1: Drag & Drop Not Working**

**Symptoms:**
- Items can be dragged but return to original position when dropped
- No error messages in console
- Keyboard shortcuts (1-6) work fine

**Root Cause:**
UI slots don't have `DragHandler` and `DropHandler` components with ECS integration.

**Solution:**
```csharp
// Check if slots have required components
var dragHandler = slotObject.GetComponent<DragHandler>();
var dropHandler = slotObject.GetComponent<DropHandler>();

if (dragHandler == null || dropHandler == null) {
    Debug.LogError($"Slot {slotObject.name} missing drag/drop components");
    // Add them if missing
    if (dragHandler == null) slotObject.AddComponent<DragHandler>();
    if (dropHandler == null) slotObject.AddComponent<DropHandler>();
}
```

**Debug Steps:**
1. Check console for `"InventoryUIBridge: Subscribed to drag/drop events"`
2. Verify `"DropHandler: ECS active - skipping direct UI manipulation"`
3. Look for `"InventoryUIBridge: OnSlotItemDropped triggered"`

---

### **Problem 2: Double Slot Creation (100 slots instead of 50)**

**Symptoms:**
- Inventory shows 100 slots instead of 50
- Some slots work differently than others
- Mixed behavior between exercises

**Root Cause:**
Both `InventoryGridManager` (Exercise 2-4) and `InventoryUIBridge` (Exercise 5) are creating slots.

**Solution:**
```csharp
// Check InventoryGridManager detection
void DetectECSComponents() {
    var inventoryDataUpdater = FindObjectOfType<InventoryDataUpdater>();
    var inventoryUIBridge = FindObjectOfType<InventoryUIBridge>();
    
    if (inventoryDataUpdater != null || inventoryUIBridge != null) {
        skipSlotCreationForECS = true;
        Debug.Log("ECS detected - GridManager will skip slot creation");
    }
}
```

**Debug Steps:**
1. Look for `"InventoryGridManager: ECS components detected - will skip slot creation"`
2. Verify `"InventoryUIBridge: About to create 50 slots"`
3. Check slot count in hierarchy window

---

### **Problem 3: UI Not Updating After ECS Changes**

**Symptoms:**
- Keyboard shortcuts work but UI doesn't update
- Console shows ECS operations but no visual changes
- Items exist in ECS but not visible

**Root Cause:**
`InventoryUISystem` not running or bridge not registered properly.

**Solution:**
```csharp
// Check if UI system is active
var world = World.DefaultGameObjectInjectionWorld;
if (world != null) {
    var uiSystem = world.GetOrCreateSystemManaged<InventoryUISystem>();
    Debug.Log($"UI System enabled: {uiSystem.Enabled}");
    
    // Register bridge if not already registered
    uiSystem.RegisterInventoryBridge(inventoryEntity, this);
}
```

**Debug Steps:**
1. Look for `"InventoryUISystem: Processing X move requests"`
2. Check `"InventoryUIBridge: Successfully initialized with ECS entity"`
3. Verify `"UISlotReference: Found slot at index X"`

---

### **Problem 4: NullReferenceException on Entity Access**

**Symptoms:**
```
NullReferenceException: Object reference not set to an instance of an object
at InventoryUIBridge.RequestItemMove()
```

**Root Cause:**
ECS World not fully initialized when UI bridge tries to access it.

**Solution:**
```csharp
// Add proper initialization checks
public void Initialize(Entity inventory) {
    if (World.DefaultGameObjectInjectionWorld == null) {
        Debug.LogError("ECS World not ready - deferring initialization");
        StartCoroutine(RetryInitialization(inventory));
        return;
    }
    
    entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    if (entityManager == null) {
        Debug.LogError("EntityManager is null");
        return;
    }
    
    // Continue with initialization...
}
```

---

### **Problem 5: SubScene ItemData Entities Not Found**

**Symptoms:**
- Console shows: `"ItemData entity not found in SubScene!"`
- Keyboard controls don't work
- Error: `"Cannot find ItemData for itemId X"`

**Root Cause:**
SubScene ItemData GameObjects not properly configured with ItemDataAuthoring components.

**Solution:**
```csharp
// Debug SubScene ItemData discovery
void DebugSubSceneItemData() {
    var itemDataQuery = entityManager.CreateEntityQuery(typeof(ItemDataComponent));
    var entities = itemDataQuery.ToEntityArray(Allocator.Temp);
    var itemDataComponents = itemDataQuery.ToComponentDataArray<ItemDataComponent>(Allocator.Temp);
    
    Debug.Log($"Found {entities.Length} ItemData entities in SubScene:");
    for (int i = 0; i < entities.Length; i++) {
        var itemData = itemDataComponents[i];
        Debug.Log($"  Entity {i}: ID={itemData.itemId}, Name={itemData.itemName}, MaxStack={itemData.maxStackSize}");
    }
    
    entities.Dispose();
    itemDataComponents.Dispose();
}
```

**Debug Steps:**
1. Check InventorySubScene.unity has 4 GameObjects with ItemDataAuthoring
2. Verify ItemDataAuthoring components have correct itemId values (1, 2, 3, 4)
3. Ensure SubScene is saved and Auto Load Scene is enabled
4. Look for `"Found X ItemData entities in SubScene"` message on play

**Manual Fix:**
1. Open InventorySubScene.unity
2. Create GameObjects: ItemData_HealthPotion, ItemData_IronSword, ItemData_Wood, ItemData_LeatherArmor
3. Add ItemDataAuthoring component to each with correct configuration
4. Save SubScene

---

### **Problem 6: Max Stack Size Not Working**

**Symptoms:**
- Items stack beyond their intended limits
- Health Potions stack to 999 instead of 10
- No stack validation occurring

**Root Cause:**
AddItemToSlot method not finding SubScene ItemData for validation.

**Solution:**
```csharp
// Verify ItemData lookup is working
public void TestItemDataLookup() {
    for (int itemId = 1; itemId <= 4; itemId++) {
        var itemData = GetItemData(itemId);
        if (itemData != null) {
            Debug.Log($"ItemID {itemId}: {itemData.Value.itemName}, MaxStack: {itemData.Value.maxStackSize}");
        } else {
            Debug.LogError($"ItemID {itemId}: NOT FOUND!");
        }
    }
}
```

**Debug Steps:**
1. Check if SubScene ItemData entities exist (Problem 5)
2. Verify GetItemData() method returns valid data
3. Look for stack validation debug messages in AddItemToSlot
4. Test with `Press '1' multiple times - should stop at 10`

---

### **Problem 7: Items Not Stacking Correctly**

**Symptoms:**
- Items swap instead of stacking
- Stack count doesn't increase when expected
- Max stack size ignored

**Root Cause:**
Item data lookup failing or max stack size not properly configured.

**Solution:**
```csharp
// Debug SubScene ItemData lookup during stacking
var itemDataQuery = GetEntityQuery(typeof(ItemDataComponent));
var allItemData = itemDataQuery.ToComponentDataArray<ItemDataComponent>(Allocator.TempJob);

Debug.Log($"Found {allItemData.Length} SubScene ItemData definitions");
foreach (var itemData in allItemData) {
    Debug.Log($"SubScene Item {itemData.itemId}: {itemData.itemName}, max stack: {itemData.maxStackSize}");
}

// Check if item exists in SubScene entities
bool itemFound = false;
int maxStackFromSubScene = 1;
foreach (var itemData in allItemData) {
    if (itemData.itemId == targetItemId) {
        itemFound = true;
        maxStackFromSubScene = itemData.maxStackSize;
        Debug.Log($"Found SubScene ItemData for ID {targetItemId}, max stack: {maxStackFromSubScene}");
        break;
    }
}

if (!itemFound) {
    Debug.LogError($"No SubScene ItemData found for ID {targetItemId} - check InventorySubScene setup");
    Debug.LogError("Ensure InventorySubScene has GameObject with ItemDataAuthoring for this itemId");
}

allItemData.Dispose();
```

---

## 🔧 **Debugging Tools & Techniques**

### **1. ECS Entity Debugger**
```csharp
// Add this method to InventoryDataUpdater for debugging
[ContextMenu("Debug Inventory State")]
public void DebugInventoryState() {
    if (!isInitialized) {
        Debug.Log("Inventory not initialized");
        return;
    }
    
    var slotsBuffer = entityManager.GetBuffer<InventorySlot>(playerInventoryEntity);
    Debug.Log($"=== Inventory Debug (Entity: {playerInventoryEntity}) ===");
    
    for (int i = 0; i < slotsBuffer.Length; i++) {
        var slot = slotsBuffer[i];
        if (!slot.isEmpty) {
            Debug.Log($"Slot {i}: Item {slot.itemId} x{slot.stackCount}");
        }
    }
    
    var inventoryComponent = entityManager.GetComponentData<InventoryComponent>(playerInventoryEntity);
    Debug.Log($"Total item count: {inventoryComponent.currentItemCount}/{inventoryComponent.maxSlots}");
}
```

### **2. UI Bridge Debugger**
```csharp
// Add this method to InventoryUIBridge for debugging
[ContextMenu("Debug Bridge State")]
public void DebugBridgeState() {
    Debug.Log($"=== UI Bridge Debug ===");
    Debug.Log($"Initialized: {isInitialized}");
    Debug.Log($"Inventory Entity: {inventoryEntity}");
    Debug.Log($"Slot References: {slotReferences.Count}");
    
    for (int i = 0; i < slotReferences.Count; i++) {
        var slotRef = slotReferences[i];
        if (slotRef != null && slotRef.slotUI != null && !slotRef.slotUI.IsEmpty()) {
            Debug.Log($"UI Slot {i}: {slotRef.slotUI.currentItem.itemName} x{slotRef.slotUI.stackCount}");
        }
    }
}
```

### **3. Event System Tracer**
```csharp
// Add to InventoryManagementSystem for event tracing
private void LogEventCreation(int slotIndex, int itemId, int stackCount, bool wasAdded) {
    Debug.Log($"[EVENT] Slot {slotIndex}: {(wasAdded ? "+" : "-")}{stackCount} of item {itemId} at {Time.time:F2}s");
}

// Add to InventoryUISystem for event processing
private void LogEventProcessing(InventoryChangedEvent changeEvent) {
    Debug.Log($"[UI UPDATE] Processing: Slot {changeEvent.slotIndex}, Item {changeEvent.itemId}, " +
              $"Count {changeEvent.stackCount}, Added: {changeEvent.wasAdded}");
}
```

---

## 📊 **Performance Best Practices**

### **1. Memory Management**
```csharp
// ✅ Always dispose temporary allocations
public void ProcessRequests() {
    var requests = requestQuery.ToEntityArray(Allocator.TempJob);
    var requestData = requestQuery.ToComponentDataArray<ItemMoveRequest>(Allocator.TempJob);
    
    try {
        // Process data
        for (int i = 0; i < requests.Length; i++) {
            ProcessRequest(requestData[i]);
        }
    } finally {
        // Always dispose in finally block
        requests.Dispose();
        requestData.Dispose();
    }
}

// ❌ Forgetting to dispose
public void BadExample() {
    var requests = requestQuery.ToEntityArray(Allocator.TempJob);
    // Process requests but never dispose - memory leak!
}
```

### **2. Query Optimization**
```csharp
// ✅ Specific, efficient queries
private EntityQuery playerInventoryQuery = GetEntityQuery(
    typeof(InventoryComponent),
    typeof(PlayerInventoryTag),
    typeof(InventorySlot)
);

// ✅ Read-only when possible
private EntityQuery itemDataQuery = GetEntityQuery(
    ComponentType.ReadOnly<ItemDataComponent>()
);

// ❌ Overly broad queries
private EntityQuery badQuery = GetEntityQuery(typeof(IComponentData)); // Too generic
```

### **3. Batch Operations**
```csharp
// ✅ Process all requests in one batch
private void ProcessAllMoveRequests() {
    var requests = moveRequestQuery.ToEntityArray(Allocator.TempJob);
    
    foreach (var request in requests) {
        ProcessSingleRequest(request);
    }
    
    requests.Dispose();
}

// ❌ Processing requests one by one
private void ProcessOneRequest() {
    var request = moveRequestQuery.GetSingletonEntity(); // Called multiple times
}
```

### **4. Event System Optimization**
```csharp
// ✅ Batch event creation
private List<InventoryChangedEvent> pendingEvents = new List<InventoryChangedEvent>();

public void CreateChangeEvent(int slot, int itemId, int count, bool added) {
    pendingEvents.Add(new InventoryChangedEvent {
        slotIndex = slot,
        itemId = itemId,
        stackCount = count,
        wasAdded = added,
        timestamp = Time.timeAsDouble
    });
}

public void FlushPendingEvents() {
    foreach (var eventData in pendingEvents) {
        var eventEntity = EntityManager.CreateEntity();
        EntityManager.AddComponentData(eventEntity, eventData);
    }
    pendingEvents.Clear();
}

// ❌ Creating events one by one immediately
public void BadEventCreation(int slot, int itemId, int count, bool added) {
    var eventEntity = EntityManager.CreateEntity(); // Expensive if called frequently
    EntityManager.AddComponentData(eventEntity, new InventoryChangedEvent { ... });
}
```

---

## 🧪 **Testing Strategies**

### **1. Unit Testing ECS Logic**
```csharp
[Test]
public void TestItemStacking() {
    // Setup
    var world = new World("Test");
    var entityManager = world.EntityManager;
    var system = world.GetOrCreateSystem<InventoryManagementSystem>();
    
    // Create test inventory
    var inventory = entityManager.CreateEntity();
    entityManager.AddComponentData(inventory, new InventoryComponent { maxSlots = 50 });
    var slots = entityManager.AddBuffer<InventorySlot>(inventory);
    
    // Initialize slots
    for (int i = 0; i < 50; i++) {
        slots.Add(InventorySlot.Empty);
    }
    
    // Add items to stack
    slots[0] = InventorySlot.Create(itemId: 1, count: 3);
    slots[1] = InventorySlot.Create(itemId: 1, count: 2);
    
    // Create move request
    var request = new ItemMoveRequest { fromSlot = 0, toSlot = 1, quantity = 3 };
    
    // Execute
    system.ProcessItemMove(inventory, request);
    
    // Assert
    Assert.IsTrue(slots[0].isEmpty, "Source slot should be empty");
    Assert.AreEqual(1, slots[1].itemId, "Target should have correct item");
    Assert.AreEqual(5, slots[1].stackCount, "Target should have combined stack");
    
    // Cleanup
    world.Dispose();
}
```

### **2. Integration Testing**
```csharp
[UnityTest]
public IEnumerator TestCompleteInventoryFlow() {
    // Setup scene
    var testScene = SceneManager.CreateScene("TestInventory");
    SceneManager.SetActiveScene(testScene);
    
    // Create inventory system
    var inventoryGO = new GameObject("InventorySystem");
    var dataUpdater = inventoryGO.AddComponent<InventoryDataUpdater>();
    var uiBridge = inventoryGO.AddComponent<InventoryUIBridge>();
    
    // Wait for initialization
    yield return new WaitForSeconds(0.1f);
    
    // Test keyboard input
    dataUpdater.AddItemToSlot(itemId: 1, quantity: 5, slotIndex: 0);
    yield return null; // Wait for ECS processing
    yield return null; // Wait for UI update
    
    // Verify UI state
    Assert.AreEqual("Health Potion", GetUISlotItemName(0));
    Assert.AreEqual(5, GetUISlotStackCount(0));
    
    // Test drag and drop
    SimulateDragDrop(fromSlot: 0, toSlot: 5);
    yield return null; // ECS processing
    yield return null; // UI update
    
    // Verify move
    Assert.IsTrue(IsUISlotEmpty(0));
    Assert.AreEqual("Health Potion", GetUISlotItemName(5));
    
    // Cleanup
    SceneManager.UnloadSceneAsync(testScene);
}
```

---

## 🔍 **Common Code Smells & Fixes**

### **1. Mixed Responsibilities**
```csharp
// ❌ Bad: UI component accessing ECS directly
public class ItemSlotUI : MonoBehaviour {
    public void OnDrop() {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entityManager.SetComponentData(...); // UI shouldn't touch ECS directly
    }
}

// ✅ Good: Proper separation through bridge
public class ItemSlotUI : MonoBehaviour {
    public void OnDrop() {
        OnItemDropped?.Invoke(this, dragData); // Let bridge handle ECS
    }
}
```

### **2. Tight Coupling**
```csharp
// ❌ Bad: Hard-coded dependencies
public class InventoryUIBridge {
    void Update() {
        var system = World.DefaultGameObjectInjectionWorld.GetExistingSystem<InventoryManagementSystem>();
        system.ProcessCustomLogic(); // Direct system access
    }
}

// ✅ Good: Event-driven communication
public class InventoryUIBridge {
    void OnSlotItemDropped() {
        RequestItemMove(fromSlot, toSlot, quantity); // Create request, let system handle it
    }
}
```

### **3. Resource Leaks**
```csharp
// ❌ Bad: Not disposing allocations
public void ProcessData() {
    var entities = query.ToEntityArray(Allocator.TempJob);
    var components = query.ToComponentDataArray<SomeComponent>(Allocator.TempJob);
    
    // Process data but never dispose - memory leak!
}

// ✅ Good: Proper disposal
public void ProcessData() {
    var entities = query.ToEntityArray(Allocator.TempJob);
    var components = query.ToComponentDataArray<SomeComponent>(Allocator.TempJob);
    
    try {
        // Process data
    } finally {
        entities.Dispose();
        components.Dispose();
    }
}
```

---

## 📋 **Development Checklist**

### **✅ Before Implementing New Features**
- [ ] Is the data structure defined as an ECS component?
- [ ] Does the business logic belong in a system?
- [ ] Is the UI update handled through events?
- [ ] Are request/response patterns used for user actions?
- [ ] Are SubScene ItemData entities properly configured?

### **✅ Before Submitting Code**
- [ ] All temporary allocations are disposed
- [ ] Debug logs are helpful but not excessive
- [ ] Error cases are handled gracefully
- [ ] Memory usage is reasonable
- [ ] Performance impact is minimal
- [ ] SubScene ItemData validation is working correctly

### **✅ When Debugging Issues**
- [ ] Check console for ECS-related debug messages
- [ ] Verify SubScene ItemData entities exist and are found
- [ ] Check "Found X ItemData entities in SubScene" message
- [ ] Verify entity/component existence
- [ ] Trace the complete data flow
- [ ] Test with simplified scenarios
- [ ] Use Unity's ECS debugger tools

### **✅ SubScene-Specific Debugging**
- [ ] InventorySubScene.unity has 4 GameObjects with ItemDataAuthoring
- [ ] ItemDataAuthoring components have correct itemId values (1,2,3,4)
- [ ] SubScene is saved and Auto Load Scene is enabled
- [ ] Console shows ItemData discovery messages on play
- [ ] Max stack size validation is working correctly

Following these practices will help you build robust, maintainable inventory systems that scale well and are easy to debug when issues arise!