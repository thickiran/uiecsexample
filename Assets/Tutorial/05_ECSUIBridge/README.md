# Exercise 5: ECS-UI Bridge Integration

## Learning Objectives
- Connect Unity UI system with DOTS Entity Component System
- Implement real-time data synchronization between ECS and UI
- Create bridge pattern for hybrid ECS-MonoBehaviour architecture
- Handle ECS events and system updates in UI layer

## Instructor Demo (5 minutes)
Watch as the instructor demonstrates:
1. Creating ECS components for inventory data
2. Setting up UI bridge system for data synchronization
3. Implementing event-driven updates between ECS and UI
4. Testing real-time inventory management

## Your Exercise (5 minutes)
Complete the following tasks:

### Task 1: Set Up ECS Components
1. ECS inventory components are already created in `InventoryUIComponent.cs`:
   - `InventoryComponent` (max slots, current count)
   - `InventorySlot` (buffer element for slot data)
   - `ItemDataComponent` (item properties)
   - `InventoryChangedEvent` (change notifications)

2. Verify components compile correctly and are accessible in your scripts

### Task 2: Create UI Bridge System
1. Add `InventoryUIBridge` component to your inventory UI
2. Configure bridge settings:
   - Link to slot container
   - Reference slot prefab
   - Set up ECS entity reference

3. Create `UISlotReference` components for each slot
4. Connect UI slots to ECS slot indices

### Task 3: Implement Data Synchronization
1. Create `InventoryUISystem` that runs in ECS world
2. Set up entity queries for:
   - Inventory entities with slot buffers
   - Item data lookup
   - Change events processing

3. Implement real-time UI updates from ECS data
4. Handle inventory change events

### Task 4: Create SubScene ItemData Entities
1. Open the `InventorySubScene.unity` SubScene
2. Create GameObjects with `ItemDataAuthoring` components:
   - **ItemData_HealthPotion**: ID=1, Name="Health Potion", MaxStack=10, Type=Consumable, Value=50
   - **ItemData_IronSword**: ID=2, Name="Iron Sword", MaxStack=1, Type=Weapon, Value=200
   - **ItemData_Wood**: ID=3, Name="Wood", MaxStack=99, Type=Material, Value=5
   - **ItemData_LeatherArmor**: ID=4, Name="Leather Armor", MaxStack=1, Type=Armor, Value=150

3. Save the SubScene - Unity will bake these into ECS entities

### Task 5: Test ECS Integration
1. `InventoryDataUpdater` is already in the scene and will find your SubScene ItemData entities
2. Test keyboard controls with proper stack size validation:
   - Press '1': Add health potion to slot 0 (max 10 stack)
   - Press '2': Add iron sword to slot 1 (max 1 stack)
   - Press '3': Add 5 wood to slot 2 (max 99 stack)
   - Press '4': Add leather armor to slot 3 (max 1 stack)
   - Press '5': Clear entire inventory
   - Press '6': Fill with test items

3. Verify max stack sizes are enforced correctly
4. Check console for SubScene ItemData entity discovery messages

### Task 6: Connect Drag & Drop to ECS
1. Modify drag handlers to create ECS requests
2. Implement `ItemMoveRequest` processing
3. Test dragging items between slots
4. Verify ECS data updates correctly

## Success Criteria
- ✅ ECS components compile without errors
- ✅ SubScene ItemData entities are created and baked correctly
- ✅ InventoryDataUpdater finds SubScene ItemData entities on initialization
- ✅ UI bridge initializes with ECS inventory entity
- ✅ Keyboard controls update UI through ECS with proper stack size validation
- ✅ Max stack sizes are enforced (Health Potion: 10, Iron Sword: 1, Wood: 99, Leather Armor: 1)
- ✅ Real-time synchronization works both ways
- ✅ Drag & drop operations update ECS data
- ✅ Change events propagate correctly

## ECS Architecture Overview
```
SubScene (InventorySubScene.unity):
├── ItemData_HealthPotion (GameObject with ItemDataAuthoring)
├── ItemData_IronSword (GameObject with ItemDataAuthoring)
├── ItemData_Wood (GameObject with ItemDataAuthoring)
└── ItemData_LeatherArmor (GameObject with ItemDataAuthoring)

Runtime ECS World:
├── PlayerInventory (Entity - created by InventoryDataUpdater)
│   ├── InventoryComponent
│   ├── InventorySlot (Buffer)
│   └── PlayerInventoryTag
├── ItemData (Entities - baked from SubScene authoring components)
│   └── ItemDataComponent
└── Systems
    ├── InventoryUISystem
    └── InventoryManagementSystem (future)

UI World:
├── InventoryUIBridge
├── UISlotReference (per slot)
└── ItemSlotUI (MonoBehaviour)
```

## Data Flow
1. **SubScene Baking**: ItemDataAuthoring components bake into ECS ItemData entities
2. **Runtime Discovery**: InventoryDataUpdater finds SubScene ItemData entities using EntityQuery
3. **ECS → UI**: InventoryUISystem reads ECS data, updates UI bridge
4. **UI → ECS**: UI interactions create ECS request entities with max stack validation
5. **Events**: InventoryChangedEvent notifies UI of changes
6. **Synchronization**: Bridge ensures UI reflects ECS state

## Key Components

### ECS Components
```csharp
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
}
```

### Bridge Pattern
```csharp
public class InventoryUIBridge : MonoBehaviour
{
    public Entity inventoryEntity;
    public void Initialize(Entity inventory);
    public void UpdateSlot(int index, InventorySlot data, ItemDataComponent itemData);
}
```

## Common Issues & Solutions
- **"ItemData entity not found" warnings**: Check SubScene ItemData GameObjects have ItemDataAuthoring components with correct IDs
- **SubScene not baking**: Ensure InventorySubScene.unity is saved and Auto Load Scene is enabled
- **Max stack size not working**: Verify ItemDataAuthoring components have correct maxStackSize values
- **Bridge not initializing**: Check entity creation order and InventoryDataUpdater initialization
- **UI not updating**: Verify InventoryUISystem is running and SubScene entities are found
- **Drag & drop not working**: Ensure ECS requests are created
- **Performance issues**: Check query efficiency and update frequency
- **Data mismatch**: Verify slot index mapping is correct

## Performance Considerations
- Use entity queries efficiently
- Batch UI updates when possible
- Avoid updating unchanged slots
- Cache item data lookups
- Process events in batches

## Advanced Features (Optional)
- Item tooltips with ECS data
- Inventory sorting and filtering
- Multiple inventory support
- Persistence system integration
- Network synchronization ready

## Next Steps
This completes the inventory system foundation. You can now extend it with:
- Player movement and item pickup
- Item crafting system
- Equipment slots
- Merchant interactions
- Save/load functionality

## Testing Workflow
1. Create SubScene ItemData GameObjects with ItemDataAuthoring components
2. Save InventorySubScene.unity to trigger baking
3. Play the scene
4. Check console for "Found X ItemData entities in SubScene" messages
5. Press number keys to add items with stack validation:
   - Press '1' multiple times - should stop at 10 health potions
   - Press '2' multiple times - should stay at 1 iron sword
   - Press '3' multiple times - wood stacks up to 99
   - Press '4' multiple times - should stay at 1 leather armor
6. Verify UI updates immediately with proper stack limits
7. Test drag & drop between slots
8. Check console for ECS events and validation messages
9. Confirm data persistence during play