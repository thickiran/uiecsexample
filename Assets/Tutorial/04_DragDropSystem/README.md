# Exercise 4: Drag & Drop System

## Learning Objectives
- Implement Unity's drag & drop interfaces (IBeginDragHandler, IDragHandler, IEndDragHandler)
- Create visual feedback during drag operations
- Handle item swapping and stacking through drag & drop
- Implement drop validation and visual indicators

## Instructor Demo (5 minutes)
Watch as the instructor demonstrates:
1. Adding drag interfaces to ItemSlotUI
2. Creating drag preview with visual feedback
3. Implementing drop detection and validation
4. Handling different drop scenarios (swap, stack, empty)

## Your Exercise (5 minutes)
**Note**: The drag & drop functionality has been pre-implemented in your ItemSlotUI.cs. Your task is to test and understand the system.

### Task 1: Understand the Implementation
1. Review the ItemSlotUI.cs modifications:
   - Drag interfaces are already implemented
   - CanvasGroup component is auto-created
   - Drag settings are configured (60% alpha, draggable state)

### Task 2: Test the Drag & Drop System
1. Run the scene and press **4** key to set up test items
2. Try dragging items between slots
3. Test different scenarios:
   - Drag to empty slots (move)
   - Drag same items together (stack)
   - Drag different items (swap)

### Task 3: Verify All Features Work
1. **Empty slots**: Items move completely
2. **Same items**: Health potions stack up to max (10)
3. **Different items**: Sword and armor swap positions
4. **Visual feedback**: 60% opacity during drag
5. **Position restore**: Items return to original position after drop

### Task 4: Test Keyboard Shortcuts
1. Press **1**: Add basic test items
2. Press **2**: Test stackable items (5 health potions)
3. Press **3**: Clear all slots
4. Press **4**: Set up drag & drop test scenario

### Task 5: Understand the Code Structure
1. Review `OnBeginDrag()` - starts drag, reduces opacity
2. Review `OnDrag()` - moves item with mouse
3. Review `OnEndDrag()` - handles drop logic
4. Review `HandleDrop()` - processes move/stack/swap operations

## Success Criteria
- ✅ Items can be dragged with smooth visual feedback (60% opacity)
- ✅ Empty slots accept any item (move operation)
- ✅ Same items stack up to maximum (health potions stack to 10)
- ✅ Different items swap positions (sword ↔ armor)
- ✅ Dragged items follow mouse cursor
- ✅ Items return to original position after drop
- ✅ Keyboard shortcuts work (1-4 keys for testing)

## Key Components
```csharp
// ItemSlotUI now implements drag interfaces directly
public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, 
    IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Drag Settings")]
    public bool isDraggable = true;
    public float dragAlpha = 0.6f;
    
    public void OnBeginDrag(PointerEventData eventData);
    public void OnDrag(PointerEventData eventData);
    public void OnEndDrag(PointerEventData eventData);
    
    private void HandleDrop(PointerEventData eventData);
}
```

## Drag & Drop Flow
1. **Begin Drag**: Store position, reduce opacity (60%), disable raycasts
2. **During Drag**: Move item with mouse cursor
3. **End Drag**: Restore opacity, restore position, handle drop logic

## Drop Validation Logic
```csharp
private void HandleDrop(PointerEventData eventData)
{
    var targetSlot = eventData.pointerEnter?.GetComponent<ItemSlotUI>();
    
    if (targetSlot.IsEmpty())
    {
        // Move to empty slot
        targetSlot.SetItem(currentItem, stackCount);
        ClearSlot();
    }
    else if (targetSlot.currentItem.itemId == currentItem.itemId)
    {
        // Stack items (if space available)
        int availableSpace = targetSlot.GetAvailableStackSpace();
        int moveAmount = Mathf.Min(stackCount, availableSpace);
        // ... stacking logic
    }
    else
    {
        // Swap items
        // ... swapping logic
    }
}
```

## Visual Feedback States
- **Dragging**: 60% opacity, no raycasts, follows mouse
- **Normal**: 100% opacity, raycasts enabled
- **After Drop**: Returns to original position, full opacity restored

## Common Issues & Solutions
- **Drag doesn't start**: Check if slot has item and isDraggable is true
- **Items don't move**: Verify CanvasGroup is properly created
- **Items don't swap**: Check HandleDrop logic in ItemSlotUI
- **Stacking not working**: Verify stack count and max stack size logic
- **Visual feedback missing**: Ensure dragAlpha is set to 0.6f

## Test Scenarios
1. **Basic drag**: Drag item from slot to slot (move operation)
2. **Stacking**: Drag health potion onto another health potion (max 10)
3. **Swapping**: Drag sword onto armor slot (items swap)
4. **Empty slot**: Drag any item to empty slot (move operation)
5. **Keyboard testing**: Use keys 1-4 to set up different test scenarios

## Next Steps
In Exercise 5, we'll connect this UI system to the DOTS backend for real-time data synchronization.