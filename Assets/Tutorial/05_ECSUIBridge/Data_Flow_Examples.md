# Data Flow Examples & Practical Scenarios
## Real-World Walkthroughs of ECS Operations

---

## 🎮 **Complete Flow Examples**

### **Example 1: Keyboard Shortcut (Press "1" to Add Health Potion)**

**Step-by-Step Breakdown:**

#### **🔵 Step 1: User Input Detection**
```csharp
// InventoryDataUpdater.Update()
if (Input.GetKeyDown(KeyCode.Alpha1)) {
    AddItemToSlot(1, 1, 0); // itemId=1, quantity=1, slot=0
}
```

#### **🔵 Step 2: SubScene ItemData Lookup & Validation**
```csharp
public void AddItemToSlot(int itemId, int quantity, int slotIndex) {
    // Get item data from SubScene-baked entities for validation
    var itemData = GetItemData(itemId);
    if (itemData == null) {
        Debug.LogError($"AddItemToSlot: Cannot find ItemData for itemId {itemId}");
        return;
    }
    
    var slotsBuffer = entityManager.GetBuffer<InventorySlot>(playerInventoryEntity);
    var currentSlot = slotsBuffer[slotIndex];
    int maxStackSize = itemData.Value.maxStackSize;
    
    if (currentSlot.isEmpty) {
        // Add new item, clamp to max stack size
        int actualQuantity = Mathf.Min(quantity, maxStackSize);
        slotsBuffer[slotIndex] = InventorySlot.Create(itemId, actualQuantity);
        
        if (actualQuantity < quantity) {
            Debug.Log($"Clamped {itemData.Value.itemName} from {quantity} to {actualQuantity} (max: {maxStackSize})");
        }
    } else if (currentSlot.itemId == itemId) {
        // Stack existing item, respect max stack size
        int availableSpace = maxStackSize - currentSlot.stackCount;
        if (availableSpace <= 0) {
            Debug.Log($"{itemData.Value.itemName} slot {slotIndex} at max stack ({maxStackSize})");
            return;
        }
        
        int actualQuantity = Mathf.Min(quantity, availableSpace);
        currentSlot.stackCount += actualQuantity;
        slotsBuffer[slotIndex] = currentSlot;
    }
    
    // Create change event for UI
    CreateChangeEvent(slotIndex, itemId, quantity, true);
}

// Helper method to find SubScene ItemData entities
ItemDataComponent? GetItemData(int itemId) {
    Entity targetEntity = Entity.Null;
    switch (itemId) {
        case 1: targetEntity = healthPotionEntity; break;  // Found from SubScene
        case 2: targetEntity = ironSwordEntity; break;    // Found from SubScene
        case 3: targetEntity = woodEntity; break;         // Found from SubScene
        case 4: targetEntity = leatherArmorEntity; break; // Found from SubScene
    }
    
    if (targetEntity != Entity.Null && entityManager.Exists(targetEntity)) {
        return entityManager.GetComponentData<ItemDataComponent>(targetEntity);
    }
    return null;
}
```

#### **🔵 Step 3: Event Creation**
```csharp
private void CreateChangeEvent(int slotIndex, int itemId, int quantity, bool wasAdded) {
    var eventEntity = entityManager.CreateEntity();
    entityManager.AddComponentData(eventEntity, new InventoryChangedEvent {
        slotIndex = slotIndex,
        itemId = itemId,
        stackCount = quantity,
        wasAdded = wasAdded,
        timestamp = Time.timeAsDouble
    });
}
```

#### **🔵 Step 4: Event Processing (Next Frame)**
```csharp
// InventoryUISystem.ProcessInventoryChanges()
var changeEvents = GetEntityQuery(typeof(InventoryChangedEvent)).ToEntityArray(Allocator.TempJob);

foreach (var eventEntity in changeEvents) {
    var changeEvent = EntityManager.GetComponentData<InventoryChangedEvent>(eventEntity);
    
    // Notify UI bridges
    foreach (var bridge in inventoryBridges.Values) {
        bridge.OnInventoryChanged(changeEvent);
    }
    
    // Clean up event
    EntityManager.DestroyEntity(eventEntity);
}
```

#### **🔵 Step 5: UI Update**
```csharp
// InventoryUISystem.UpdateInventoryEntity()
var inventorySlots = EntityManager.GetBuffer<InventorySlot>(inventoryEntity);
var slotData = inventorySlots[0]; // Slot 0 now has health potion

if (!slotData.isEmpty && itemDataLookup.TryGetValue(slotData.itemId, out var itemData)) {
    bridge.UpdateSlot(0, slotData, itemData);
}
```

#### **🔵 Step 6: Visual Representation**
```csharp
// UISlotReference.UpdateFromECS()
var uiItemData = new ItemData {
    itemId = slotData.itemId,           // 1
    itemName = itemData.itemName,       // "Health Potion" 
    maxStackSize = itemData.maxStackSize, // 10
    itemType = ItemType.Consumable,
    description = "Restores 50 HP"
};

slotUI.SetItem(uiItemData, slotData.stackCount); // Shows "Health Potion x1"
```

**🎯 Result**: Health potion appears in slot 0 with stack count 1 (respecting max stack size of 10 from SubScene ItemData)

---

### **Example 2: Drag & Drop Operation (Slot 0 → Slot 5)**

**Step-by-Step Breakdown:**

#### **🔴 Step 1: Drag Initiation**
```csharp
// DragHandler.OnBeginDrag()
public void OnBeginDrag(PointerEventData eventData) {
    if (!isDraggable || sourceSlot == null || sourceSlot.IsEmpty()) return;
    
    // Store original position
    startPosition = transform.position;
    startParent = transform.parent;
    
    // Create visual feedback
    CreateDragPreview();
    canvasGroup.alpha = dragAlpha;
    
    OnDragStart?.Invoke(this);
}
```

#### **🔴 Step 2: Drop Detection**
```csharp
// DropHandler.OnDrop()
public void OnDrop(PointerEventData eventData) {
    var draggedObject = eventData.pointerDrag;
    var dragHandler = draggedObject.GetComponent<DragHandler>();
    
    // Check if ECS is active
    if (IsECSActive()) {
        // Let ECS handle the move
        bool dropSuccessful = HandleDrop(dragHandler); // Returns true for ECS
        
        if (dropSuccessful) {
            OnItemDropped?.Invoke(this, dragHandler); // Triggers ECS request
        }
    }
}
```

#### **🔴 Step 3: ECS Request Creation**
```csharp
// InventoryUIBridge.OnSlotItemDropped()
private void OnSlotItemDropped(DropHandler dropHandler, DragHandler dragHandler) {
    var targetSlot = dropHandler.GetTargetSlot();  // Slot 5
    var sourceSlot = dragHandler.GetSourceSlot();  // Slot 0
    
    var targetIndex = GetSlotIndex(targetSlot.gameObject); // 5
    var sourceIndex = GetSlotIndex(sourceSlot.gameObject); // 0
    
    RequestItemMove(sourceIndex, targetIndex, sourceSlot.stackCount);
}

private void RequestItemMove(int fromSlot, int toSlot, int quantity) {
    var requestEntity = entityManager.CreateEntity();
    entityManager.AddComponentData(requestEntity, new ItemMoveRequest {
        fromSlot = fromSlot,    // 0
        toSlot = toSlot,        // 5
        quantity = quantity     // 1
    });
    
    entityManager.AddComponentData(requestEntity, new InventoryReference {
        inventoryEntity = inventoryEntity
    });
}
```

#### **🔴 Step 4: Business Logic Processing**
```csharp
// InventoryManagementSystem.ProcessItemMove()
private void ProcessItemMove(Entity inventoryEntity, ItemMoveRequest request) {
    var slotsBuffer = EntityManager.GetBuffer<InventorySlot>(inventoryEntity);
    
    var fromSlot = slotsBuffer[request.fromSlot]; // Slot 0: Health Potion x1
    var toSlot = slotsBuffer[request.toSlot];     // Slot 5: Empty
    
    if (toSlot.isEmpty) {
        // Move to empty slot
        slotsBuffer[request.toSlot] = fromSlot;        // Slot 5 = Health Potion x1
        slotsBuffer[request.fromSlot] = InventorySlot.Empty; // Slot 0 = Empty
        
        // Create events
        CreateChangeEvent(inventoryEntity, request.toSlot, fromSlot.itemId, fromSlot.stackCount, true);
        CreateChangeEvent(inventoryEntity, request.fromSlot, fromSlot.itemId, fromSlot.stackCount, false);
    }
    
    // Clean up request
    EntityManager.DestroyEntity(requestEntity);
}
```

#### **🔴 Step 5: UI Synchronization**
```csharp
// InventoryUISystem processes change events
// Event 1: Slot 5 gained Health Potion x1
// Event 2: Slot 0 lost Health Potion x1

// Update slot 5
bridge.UpdateSlot(5, newSlotData, itemData); // Shows Health Potion

// Clear slot 0  
bridge.ClearSlot(0); // Shows empty slot
```

**🎯 Result**: Health potion moves from slot 0 to slot 5

---

### **Example 3: Stacking Items (Drag Health Potion onto Another Health Potion)**

**Scenario**: Slot 0 has Health Potion x3, Slot 1 has Health Potion x2, drag from 0 to 1

#### **🟡 Step 1-3: Same as Example 2** 
(Drag initiation, drop detection, request creation)

#### **🟡 Step 4: Stacking Logic with SubScene ItemData**
```csharp
// InventoryManagementSystem.ProcessItemMove()
var fromSlot = slotsBuffer[0]; // Health Potion x3
var toSlot = slotsBuffer[1];   // Health Potion x2

if (fromSlot.itemId == toSlot.itemId) {
    // Look up max stack size from SubScene-baked ItemData entities
    var itemDataQuery = GetEntityQuery(typeof(ItemDataComponent));
    var entities = itemDataQuery.ToEntityArray(Allocator.TempJob);
    var itemDataComponents = itemDataQuery.ToComponentDataArray<ItemDataComponent>(Allocator.TempJob);
    
    int maxStackSize = 1; // Default fallback
    for (int i = 0; i < itemDataComponents.Length; i++) {
        if (itemDataComponents[i].itemId == fromSlot.itemId) {
            maxStackSize = itemDataComponents[i].maxStackSize; // Health Potion: 10
            Debug.Log($"Found ItemData: {itemDataComponents[i].itemName} with max stack {maxStackSize}");
            break;
        }
    }
    
    // Calculate stacking with SubScene data
    int availableSpace = maxStackSize - toSlot.stackCount; // 10 - 2 = 8
    int moveAmount = math.min(fromSlot.stackCount, availableSpace); // min(3, 8) = 3
    
    // Update counts
    toSlot.stackCount += moveAmount;     // 2 + 3 = 5
    fromSlot.stackCount -= moveAmount;   // 3 - 3 = 0
    
    // Update slots
    slotsBuffer[1] = toSlot;                        // Slot 1: Health Potion x5
    slotsBuffer[0] = InventorySlot.Empty;           // Slot 0: Empty (all moved)
    
    // Create events
    CreateChangeEvent(inventoryEntity, 1, toSlot.itemId, moveAmount, true);   // +3 to slot 1
    CreateChangeEvent(inventoryEntity, 0, fromSlot.itemId, moveAmount, false); // -3 from slot 0
    
    // Cleanup
    entities.Dispose();
    itemDataComponents.Dispose();
}
```

**🎯 Result**: Slot 0 becomes empty, Slot 1 shows Health Potion x5 (validated against SubScene max stack size of 10)

---

## 🔄 **Timing and Frame Analysis**

### **Frame-by-Frame Breakdown**

**Frame N: User Action**
```
User drags item from slot 0 to slot 1
→ UI events triggered
→ ItemMoveRequest entity created
```

**Frame N+1: ECS Processing**
```
InventoryManagementSystem.OnUpdate()
→ Processes ItemMoveRequest
→ Updates InventorySlot buffer
→ Creates InventoryChangedEvent entities
→ Destroys ItemMoveRequest entity
```

**Frame N+2: UI Update**
```
InventoryUISystem.OnUpdate()
→ Processes InventoryChangedEvent entities
→ Calls bridge.UpdateSlot() and bridge.ClearSlot()
→ Destroys InventoryChangedEvent entities

UISlotReference.UpdateFromECS()
→ Converts ECS data to UI data
→ Calls slotUI.SetItem() or slotUI.ClearSlot()
```

**Frame N+3: Visual Result**
```
User sees the item in the new slot
→ Drag preview disappears
→ UI shows final state
```

---

## 📊 **Data State Tracking**

### **Before Operation**
```
ECS State:
├── Slot[0]: InventorySlot { itemId=1, stackCount=3, isEmpty=false }
├── Slot[1]: InventorySlot { itemId=-1, stackCount=0, isEmpty=true }
└── Slot[2]: InventorySlot { itemId=-1, stackCount=0, isEmpty=true }

UI State:
├── SlotUI[0]: Shows "Health Potion x3"
├── SlotUI[1]: Shows empty slot
└── SlotUI[2]: Shows empty slot
```

### **During Operation (Request Processing)**
```
Request Queue:
└── ItemMoveRequest { fromSlot=0, toSlot=1, quantity=3 }

ECS State: (Same as before - not yet processed)
UI State: (Same as before - drag preview active)
```

### **After Operation**
```
ECS State:
├── Slot[0]: InventorySlot { itemId=-1, stackCount=0, isEmpty=true }
├── Slot[1]: InventorySlot { itemId=1, stackCount=3, isEmpty=false }
└── Slot[2]: InventorySlot { itemId=-1, stackCount=0, isEmpty=true }

UI State:
├── SlotUI[0]: Shows empty slot
├── SlotUI[1]: Shows "Health Potion x3"
└── SlotUI[2]: Shows empty slot

Event History:
├── InventoryChangedEvent { slotIndex=1, itemId=1, stackCount=3, wasAdded=true }
└── InventoryChangedEvent { slotIndex=0, itemId=1, stackCount=3, wasAdded=false }
```

---

## 🎯 **Error Handling Examples**

### **Invalid Move Attempt**
```csharp
// User tries to move from empty slot
if (fromSlot.isEmpty) {
    Debug.LogWarning("Cannot move from empty slot");
    return; // No request created, UI shows feedback
}

// User tries to move to invalid slot index
if (request.toSlot < 0 || request.toSlot >= slotsBuffer.Length) {
    Debug.LogWarning($"Invalid target slot: {request.toSlot}");
    return; // Request ignored, no change events
}
```

### **Stacking Overflow**
```csharp
// Trying to stack more than max stack size
int availableSpace = maxStackSize - toSlot.stackCount;
if (availableSpace <= 0) {
    Debug.Log("Cannot stack - target slot is full");
    // Could implement slot swapping instead
    SwapItems(fromSlot, toSlot);
}
```

### **Missing SubScene ItemData**
```csharp
// ItemData entity not found in SubScene (common setup issue)
var itemData = GetItemData(slotData.itemId);
if (itemData == null) {
    Debug.LogError($"SubScene ItemData not found for ID: {slotData.itemId}");
    Debug.LogError("Check: 1) InventorySubScene has GameObject with ItemDataAuthoring");
    Debug.LogError("       2) ItemDataAuthoring.itemId matches the requested ID");
    Debug.LogError("       3) SubScene is saved and baking completed");
    // Show placeholder or error icon
    slotUI.ShowErrorState();
    return;
}

// Use SubScene ItemData for validation
int maxStack = itemData.Value.maxStackSize;
string itemName = itemData.Value.itemName.ToString();
```

---

## 🧪 **Testing Scenarios**

### **Unit Test Example**
```csharp
[Test]
public void TestItemMove_EmptyToEmpty()
{
    // Setup
    var world = new World("Test");
    var system = world.GetOrCreateSystem<InventoryManagementSystem>();
    var inventory = CreateTestInventory();
    
    // Add item to slot 0
    SetSlot(inventory, 0, itemId: 1, count: 5);
    
    // Create move request
    var request = new ItemMoveRequest { fromSlot = 0, toSlot = 5, quantity = 5 };
    
    // Execute
    system.ProcessItemMove(inventory, request);
    
    // Assert
    Assert.IsTrue(IsSlotEmpty(inventory, 0));
    Assert.AreEqual(1, GetSlotItemId(inventory, 5));
    Assert.AreEqual(5, GetSlotCount(inventory, 5));
}
```

### **Integration Test Example**
```csharp
[UnityTest]
public IEnumerator TestDragDrop_Integration()
{
    // Setup scene with UI and ECS
    var scene = SetupInventoryScene();
    
    // Add item to slot 0
    AddTestItem(itemId: 1, slot: 0);
    yield return null; // Wait for UI update
    
    // Simulate drag from slot 0 to slot 5
    SimulateDragDrop(fromSlot: 0, toSlot: 5);
    yield return null; // Wait for ECS processing
    yield return null; // Wait for UI update
    
    // Verify result
    Assert.IsTrue(IsUISlotEmpty(0));
    Assert.AreEqual("Health Potion", GetUISlotText(5));
}
```

This comprehensive data flow documentation helps students understand not just what happens, but exactly how and when each piece of the system responds to user actions!