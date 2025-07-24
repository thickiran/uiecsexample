# Exercise 3: Item Slot Components

## Learning Objectives
- Add interactive item slot components with multiple visual states
- Configure hover, click, and selection feedback systems
- Set up item data management and stacking mechanics
- Connect Unity's Event System for UI interactions

## Prerequisites
Before starting Exercise 3, ensure you have:
- ✅ Completed Exercise 2 with working grid system
- ✅ StudentSlot.prefab created and functional
- ✅ 50 slots generating correctly in grid
- ✅ Basic hover effects working on slots

## Instructor Demo (5 minutes)
Watch as the instructor demonstrates:
1. Adding finished ItemSlotUI script to slot prefabs
2. Configuring component references in Inspector
3. Setting up SlotStateManager for testing
4. Testing visual feedback and keyboard shortcuts

## Your Exercise (10 minutes)
Complete the following tasks by copying finished scripts and configuring references:

### Task 1: Add ItemSlotUI Script to Slot Prefab

#### Step 1A: Copy the Finished ItemSlotUI Script
1. **Navigate** to Assets/Tutorial/03_ItemSlotComponents/Scripts/
2. **Find** ItemSlotUI_FINISHED.txt file
3. **Open** the file and **copy all content** (Ctrl+A, Ctrl+C)
4. **Create** new C# script called "ItemSlotUI.cs" in the same folder
5. **Paste** the finished content into ItemSlotUI.cs and **save**

#### Step 1B: Add Script to Your Slot Prefab
1. **Navigate** to Assets/Tutorial/02_GridLayoutSystem/Prefabs/
2. **Double-click** on your StudentSlot.prefab to enter Prefab Mode
3. **Select** the root prefab object in Hierarchy
4. **Click** "Add Component" in Inspector
5. **Search** for "ItemSlotUI" and **add** the script component

#### Step 1C: Configure Component References
**Note**: The ItemSlotUI script will automatically create missing UI elements, but you need to assign the Background Image reference.

1. **In the ItemSlotUI component in Inspector:**
   - **Background Image**: Drag the root prefab's Image component to this field
   - Leave other fields empty - they will be auto-created by the script
2. **Save Prefab** (Ctrl+S)

#### Step 1D: Verify Auto-Generated Components
When you play the scene, the ItemSlotUI script will automatically create:
- **ItemImage** child object for item icons
- **StackCount** child object for stack numbers  
- **Highlight** child object for hover effects
- **CanvasGroup** component for drag operations

### Task 2: Set Up SlotStateManager for Testing

#### Step 2A: Copy the Finished SlotStateManager Script
1. **Navigate** to Assets/Tutorial/03_ItemSlotComponents/Scripts/
2. **Find** SlotStateManager_FINISHED.txt file
3. **Open** the file and **copy all content** (Ctrl+A, Ctrl+C)
4. **Create** new C# script called "SlotStateManager.cs" in the same folder
5. **Paste** the finished content into SlotStateManager.cs and **save**

#### Step 2B: Add SlotStateManager to Scene
1. **In the Hierarchy**, create **Empty GameObject** (anywhere at root level)
2. **Rename** to "SlotManager"  
3. **Add Component** → search for "SlotStateManager"
4. **Add** the SlotStateManager script component

**Note**: The SlotStateManager can be placed anywhere in the hierarchy as it uses `FindObjectsOfType<ItemSlotUI>()` to locate all slots in the scene.

#### Step 2C: Test the System
1. **Save scene** and **Play**
2. **Check Console** - should see: "Found 50 slots and wired up event handlers"
3. **Test keyboard shortcuts**:
   - **Press '1'**: Should add 4 different items to first 4 slots
   - **Press '2'**: Should add Health Potion x5 to first slot (shows stacking)
   - **Press '3'**: Should clear all slots
   - **Press '4'**: Should add drag & drop test items
4. **Test mouse interactions**:
   - **Hover**: Slots should show white overlay highlight
   - **Click**: Slots should select (yellow background) + console message
   - **Click again**: Should deselect (return to normal color)

### Task 3: Troubleshooting and Verification

#### Step 3A: Common Issues and Solutions
**Problem: Console shows "Found 0 slots and wired up event handlers"**
- **Solution**: StudentSlot.prefab missing ItemSlotUI component
- **Fix**: Ensure ItemSlotUI script was added to your StudentSlot.prefab and slots were regenerated

**Problem: Console shows "Found X slots" but keyboard shortcuts don't work**
- **Solution**: SlotStateManager not receiving input
- **Fix**: Make sure SlotStateManager GameObject is active and the script is enabled

**Problem: Hover works but click selection (yellow) doesn't work**
- **Solution**: Event handlers not wired or Button component blocking
- **Fix**: Check console for "Slot X clicked" messages. If missing, verify event subscription

**Problem: Items don't appear when pressing keys**
- **Solution**: Background Image reference not assigned or slots not found
- **Fix**: Assign root Image component to ItemSlotUI script's Background Image field

#### Step 3B: Final Testing Checklist
**Visual States:**
- ✅ **Empty slots**: Dark gray background
- ✅ **Occupied slots**: Medium gray background (after adding items)
- ✅ **Hover effect**: White highlight overlay appears
- ✅ **Selected state**: Yellow background when clicked

**Keyboard Shortcuts:**
- ✅ **Press '1'**: Adds 4 different items to first 4 slots
- ✅ **Press '2'**: Adds Health Potion x5 to first slot (stack count shows)
- ✅ **Press '3'**: Clears all slots
- ✅ **Press '4'**: Adds drag & drop test items

**Mouse Interactions:**
- ✅ **Hover**: White overlay appears on any slot
- ✅ **Click**: Yellow selection (only one slot at a time)
- ✅ **Stack display**: Numbers appear when count > 1

## Success Criteria
When your exercise is complete, you should have:
- ✅ **ItemSlotUI script** copied from FINISHED version and added to StudentSlot.prefab
- ✅ **SlotStateManager script** copied from FINISHED version and added to scene
- ✅ **Background Image reference** assigned in ItemSlotUI component
- ✅ **All keyboard shortcuts working** (1,2,3,4 keys)
- ✅ **Visual states working**: hover (white overlay), selection (yellow background)
- ✅ **Item display working**: icons and stack counts appear correctly
- ✅ **Drag & drop ready**: All components prepared for Exercise 4

## Key Components Added
- **ItemSlotUI.cs** - Core slot interaction script with auto-UI-generation
- **SlotStateManager.cs** - Testing and management system with keyboard shortcuts  
- **Auto-generated UI elements** - ItemImage, StackCount, Highlight objects

## Expected Scene Hierarchy After Exercise 3
```
📁 SampleScene
├── 📷 Main Camera
├── 🔆 Directional Light  
├── 🎮 EventSystem
├── 🎛️ SlotManager (NEW - can be placed anywhere at root level)
│   └── 📜 SlotStateManager Script
│       ├── All Slots: (Auto-populated with 50 ItemSlotUI components)
│       └── Test Items: (4 auto-generated test items)
├── 📱 InventoryCanvas
│   └── 📦 InventoryPanel
│       └── 📊 GridContainer
│           ├── 🔲 Slot_0_0 (Enhanced with ItemSlotUI)
│           │   ├── 📜 ItemSlotUI Script
│           │   ├── 🖼️ ItemImage (Auto-created)
│           │   ├── 📝 StackCount (Auto-created)
│           │   └── ✨ Highlight (Auto-created)
│           ├── 🔲 Slot_1_0 (Enhanced with ItemSlotUI)
│           └── ... (48 more enhanced slots)
└── 📂 InventorySubScene (Empty)
```

## Exercise 3 Complete! 🎉
You've successfully set up a fully interactive slot system by:
- ✅ **Copying finished scripts** instead of writing from scratch
- ✅ **Configuring component references** in Inspector
- ✅ **Testing with keyboard shortcuts** and mouse interactions
- ✅ **Preparing for drag & drop** functionality in Exercise 4

Your inventory system now has professional-grade slot interactions!