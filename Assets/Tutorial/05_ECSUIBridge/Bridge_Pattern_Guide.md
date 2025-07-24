# Bridge Pattern Guide
## Connecting ECS Data with Unity UI

---

## 🌉 **What is the Bridge Pattern?**

The Bridge Pattern separates an abstraction from its implementation, allowing both to vary independently. In our inventory system:

- **Abstraction**: Unity UI (visual slots, drag/drop, interactions)
- **Implementation**: ECS data (entities, components, systems)
- **Bridge**: Components that translate between the two worlds

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   UNITY UI      │────│   BRIDGE LAYER  │────│   ECS DATA      │
│                 │    │                 │    │                 │
│ • ItemSlotUI    │    │ • UIBridge      │    │ • Entities      │
│ • DragHandler   │    │ • SlotReference │    │ • Components    │
│ • Visual State  │    │ • DataUpdater   │    │ • Systems       │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

---

## 🏗️ **Our Bridge Architecture**

### **Component Hierarchy**
```
InventoryUIBridge (Main Bridge)
├── SlotContainer Management
├── ECS Entity References  
├── Event Translation
└── UISlotReference[] (Individual Bridges)
    ├── ItemSlotUI Connection
    ├── ECS Data Mapping
    └── Update Coordination
```

---

## 🎯 **InventoryUIBridge**

### **Purpose**
Main bridge that manages the entire inventory UI and its connection to ECS data.

### **Key Responsibilities**

#### **1. Initialization & Setup**
```csharp
public class InventoryUIBridge : MonoBehaviour
{
    [Header("UI References")]
    public Transform slotContainer;     // Where slots are created
    public GameObject slotPrefab;       // Optional prefab for slots
    
    [Header("ECS References")]
    public Entity inventoryEntity;      // Which ECS inventory to connect to
    
    private List<UISlotReference> slotReferences;
    private bool isInitialized = false;
    private EntityManager entityManager;
}
```

#### **2. ECS-Driven Slot Creation**
```csharp
public void Initialize(Entity inventory)
{
    inventoryEntity = inventory;
    
    // Get inventory metadata from ECS
    var inventoryComponent = entityManager.GetComponentData<InventoryComponent>(inventoryEntity);
    
    // Create UI slots based on ECS data
    CreateSlots(inventoryComponent.maxSlots);
    
    isInitialized = true;
}

private void CreateSlots(int slotCount)
{
    // Setup grid layout for proper arrangement
    SetupGridLayoutForECS();
    
    // Create individual slot bridges
    for (int i = 0; i < slotCount; i++) {
        CreateSlot(i);
    }
}
```

#### **3. Event Translation**
```csharp
// UI Events → ECS Requests
private void OnSlotItemDropped(DropHandler dropHandler, DragHandler dragHandler)
{
    var targetIndex = GetSlotIndex(targetSlot.gameObject);
    var sourceIndex = GetSlotIndex(sourceSlot.gameObject);
    
    if (targetIndex != -1 && sourceIndex != -1) {
        // Translate UI event to ECS request
        RequestItemMove(sourceIndex, targetIndex, sourceSlot.stackCount);
    }
}

private void RequestItemMove(int fromSlot, int toSlot, int quantity)
{
    // Create ECS request entity
    var requestEntity = entityManager.CreateEntity();
    entityManager.AddComponentData(requestEntity, new ItemMoveRequest {
        fromSlot = fromSlot,
        toSlot = toSlot,
        quantity = quantity
    });
    
    // Link to specific inventory
    entityManager.AddComponentData(requestEntity, new InventoryReference {
        inventoryEntity = inventoryEntity
    });
}
```

#### **4. UI Update Coordination**
```csharp
// ECS Data → UI Updates
public void UpdateSlot(int slotIndex, InventorySlot slotData, ItemDataComponent itemData)
{
    if (slotIndex >= 0 && slotIndex < slotReferences.Count) {
        var slotRef = slotReferences[slotIndex];
        if (slotRef != null) {
            // Delegate to individual slot bridge
            slotRef.UpdateFromECS(slotData, itemData);
        }
    }
}

public void ClearSlot(int slotIndex)
{
    if (slotIndex >= 0 && slotIndex < slotReferences.Count) {
        var slotRef = slotReferences[slotIndex];
        if (slotRef != null && slotRef.slotUI != null) {
            slotRef.slotUI.ClearSlot();
        }
    }
}
```

---

## 🎯 **UISlotReference**

### **Purpose**
Individual bridge for each inventory slot, handling the connection between a single UI slot and its ECS data.

### **Key Responsibilities**

#### **1. Slot Identification**
```csharp
public class UISlotReference : MonoBehaviour
{
    [Header("ECS References")]
    public Entity inventoryEntity;  // Which inventory this belongs to
    public int slotIndex;          // Position in the inventory (0-49)
    
    [Header("UI Components")]
    public ItemSlotUI slotUI;      // The visual slot component
    
    public void Initialize(Entity inventory, int index)
    {
        inventoryEntity = inventory;
        slotIndex = index;
        isInitialized = true;
    }
}
```

#### **2. Data Type Conversion**
```csharp
public void UpdateFromECS(InventorySlot slotData, ItemDataComponent itemData)
{
    if (slotUI == null) return;
    
    if (slotData.isEmpty) {
        slotUI.ClearSlot();
    } else {
        // Convert ECS data types to UI data types
        var uiItemData = new ItemData {
            itemId = slotData.itemId,
            itemName = itemData.itemName.ToString(),        // FixedString64Bytes → string
            maxStackSize = itemData.maxStackSize,
            itemType = ConvertItemType(itemData.itemType),  // ECSItemType → ItemType
            description = $"A {itemData.itemType} item"
        };
        
        slotUI.SetItem(uiItemData, slotData.stackCount);
    }
}

private ItemType ConvertItemType(ECSItemType ecsType)
{
    return ecsType switch {
        ECSItemType.Weapon => ItemType.Weapon,
        ECSItemType.Armor => ItemType.Armor,
        ECSItemType.Consumable => ItemType.Consumable,
        ECSItemType.Material => ItemType.Material,
        ECSItemType.Quest => ItemType.Quest,
        _ => ItemType.Material
    };
}
```

#### **3. Request Generation**
```csharp
public void RequestItemMove(int targetSlotIndex, int quantity)
{
    if (!IsInitialized()) return;
    
    var world = World.DefaultGameObjectInjectionWorld;
    var entityManager = world.EntityManager;
    
    // Create move request from this slot
    var requestEntity = entityManager.CreateEntity();
    entityManager.AddComponentData(requestEntity, new ItemMoveRequest {
        fromSlot = slotIndex,
        toSlot = targetSlotIndex,
        quantity = quantity
    });
    
    entityManager.AddComponentData(requestEntity, new InventoryReference {
        inventoryEntity = inventoryEntity
    });
}
```

---

## 🎯 **InventoryDataUpdater**

### **Purpose**
Development and testing bridge that initializes ECS data and provides test functionality.

### **Key Responsibilities**

#### **1. ECS Initialization**
```csharp
public class InventoryDataUpdater : MonoBehaviour
{
    private Entity playerInventoryEntity;
    private EntityManager entityManager;
    private bool isInitialized = false;
    
    void Start()
    {
        StartCoroutine(DelayedInitializeECS());
    }
    
    void InitializeECS()
    {
        CreatePlayerInventory();
        CreateTestItems();
        isInitialized = true;
    }
}
```

#### **2. Test Data Creation**
```csharp
void CreatePlayerInventory()
{
    // Create main inventory entity
    playerInventoryEntity = entityManager.CreateEntity();
    
    // Add components
    entityManager.AddComponentData(playerInventoryEntity, new InventoryComponent {
        maxSlots = 50,
        currentItemCount = 0
    });
    
    entityManager.AddComponentData(playerInventoryEntity, new PlayerInventoryTag());
    
    // Initialize slot buffer
    var slotsBuffer = entityManager.AddBuffer<InventorySlot>(playerInventoryEntity);
    for (int i = 0; i < 50; i++) {
        slotsBuffer.Add(InventorySlot.Empty);
    }
    
    // Connect to UI bridge
    var uiBridge = FindObjectOfType<InventoryUIBridge>();
    if (uiBridge != null) {
        uiBridge.Initialize(playerInventoryEntity);
    }
}
```

#### **3. Development Tools**
```csharp
void Update()
{
    if (!isInitialized) return;
    
    // Keyboard shortcuts for testing
    if (Input.GetKeyDown(KeyCode.Alpha1)) {
        AddItemToSlot(1, 1, 0); // Health potion to slot 0
    }
    
    if (Input.GetKeyDown(KeyCode.Alpha5)) {
        ClearInventory();
    }
    
    if (Input.GetKeyDown(KeyCode.Alpha6)) {
        FillInventoryWithTestItems();
    }
}

public void AddItemToSlot(int itemId, int quantity, int slotIndex)
{
    // Direct ECS manipulation for testing
    var slotsBuffer = entityManager.GetBuffer<InventorySlot>(playerInventoryEntity);
    
    if (slotIndex >= 0 && slotIndex < slotsBuffer.Length) {
        slotsBuffer[slotIndex] = InventorySlot.Create(itemId, quantity);
        CreateChangeEvent(slotIndex, itemId, quantity, true);
    }
}
```

---

## 🔄 **Bridge Communication Patterns**

### **1. UI → ECS (User Actions)**
```
User Drag & Drop
       ↓
DropHandler.OnDrop()
       ↓
InventoryUIBridge.OnSlotItemDropped()
       ↓
InventoryUIBridge.RequestItemMove()
       ↓
ECS ItemMoveRequest Entity
       ↓
InventoryManagementSystem.ProcessMoveRequests()
```

### **2. ECS → UI (Data Updates)**
```
ECS Data Change
       ↓
InventoryManagementSystem.CreateChangeEvent()
       ↓
InventoryUISystem.ProcessInventoryChanges()
       ↓
InventoryUIBridge.UpdateSlot()
       ↓
UISlotReference.UpdateFromECS()
       ↓
ItemSlotUI.SetItem()
```

### **3. Initialization Flow**
```
Scene Start
       ↓
InventoryDataUpdater.DelayedInitializeECS()
       ↓
InventoryDataUpdater.CreatePlayerInventory()
       ↓
InventoryUIBridge.Initialize()
       ↓
InventoryUIBridge.CreateSlots()
       ↓
UISlotReference.Initialize()
```

---

## ⚡ **Bridge Pattern Benefits**

### **1. Separation of Concerns**
- **UI Layer**: Focuses on user interaction and visual feedback
- **ECS Layer**: Focuses on data integrity and business logic
- **Bridge Layer**: Handles translation between the two

### **2. Independent Evolution**
- Change UI framework without touching ECS code
- Modify ECS structure without breaking UI
- Add new UI features using existing ECS data

### **3. Testing & Debugging**
```csharp
// Test ECS logic without UI
[Test]
public void TestItemMove() {
    var world = new World("Test");
    // Test business logic directly
}

// Test UI updates without user interaction
public void SimulateInventoryChange() {
    var changeEvent = new InventoryChangedEvent { /* ... */ };
    uiBridge.OnInventoryChanged(changeEvent);
}
```

### **4. Multiple UI Support**
```csharp
// Same ECS data, different UIs
public class MobileInventoryBridge : MonoBehaviour { /* Touch-optimized */ }
public class VRInventoryBridge : MonoBehaviour { /* VR-optimized */ }
public class DesktopInventoryBridge : MonoBehaviour { /* Mouse/keyboard */ }
```

---

## 🎯 **Key Design Principles**

### **1. Single Responsibility**
- Each bridge component has one clear purpose
- UIBridge manages the collection, UISlotReference manages individuals
- DataUpdater handles initialization, not runtime logic

### **2. Dependency Inversion**
- UI depends on abstractions (ItemData), not ECS specifics
- ECS doesn't know about UI implementation details
- Bridge handles the translation layer

### **3. Event-Driven Updates**
- Changes flow through events, not polling
- Loose coupling between systems
- Clear audit trail of what changed when

### **4. Fail-Safe Defaults**
- Bridge continues working if ECS data is missing
- UI gracefully handles invalid states
- Comprehensive error logging for debugging

This bridge architecture provides a robust, maintainable connection between Unity's traditional GameObject/MonoBehaviour system and the modern ECS architecture, giving you the best of both worlds!