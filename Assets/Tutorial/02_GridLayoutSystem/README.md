# Exercise 2: Grid Layout System

## Learning Objectives
- Implement Grid Layout Group for organized inventory slots
- Configure automatic sizing with Content Size Fitter
- Create responsive grid that adapts to different slot counts
- Understand grid constraints and layout options

## Instructor Demo (5 minutes)
Watch as the instructor demonstrates:
1. Adding Grid Layout Group component
2. Configuring cell size, spacing, and constraints
3. Setting up Content Size Fitter for automatic sizing
4. Creating temporary slot placeholders

## Your Exercise (5 minutes)
Complete the following tasks:

### Task 1: Setup Grid Container
1. Create "GridContainer" GameObject as child of InventoryPanel
2. Add RectTransform and configure:
   - Anchor: Fill (stretch to fill parent)
   - Margins: 20px on all sides
3. This will be your grid parent

### Task 2: Add Grid Layout Group
1. Add GridLayoutGroup component to GridContainer
2. Configure settings:
   - Cell Size: (64, 64)
   - Spacing: (2, 2)
   - Start Corner: Upper Left
   - Start Axis: Horizontal
   - Child Alignment: Middle Center
   - Constraint: Fixed Column Count
   - Constraint Count: 10

### Task 3: Add Content Size Fitter
1. Add ContentSizeFitter component to GridContainer
2. Set both Horizontal and Vertical Fit to "Preferred Size"
3. This will auto-size the container to fit all slots

### Task 4: Create Slot Prefab Manually
1. **Create Empty GameObject:**
   - Right-click in Hierarchy → Create Empty
   - Name: "SlotPrefab"
   - RectTransform component is automatically added

2. **Configure RectTransform:**
   - Set Size Delta to (64, 64) to match cellSize
   - Set Anchors to Center (0.5, 0.5)
   - Set Pivot to Center (0.5, 0.5)

3. **Add Image Component (Background):**
   - Add Component → UI → Image
   - Color: (0.3, 0.3, 0.3, 0.8) - Gray with transparency
   - Source Image: None (solid color background)

4. **Add Outline Component (Border):**
   - Add Component → UI → Effects → Outline
   - Effect Color: White (1, 1, 1, 1)
   - Effect Distance: (1, 1)

5. **Add Button Component (Interactions):**
   - Add Component → UI → Button
   - Target Graphic: Image (drag the Image component)
   - Configure Button Colors:
     - Normal Color: (0.3, 0.3, 0.3, 0.8) - Same as background
     - Highlighted Color: (0.5, 0.5, 0.5, 0.8) - Lighter on hover
     - Pressed Color: (0.2, 0.2, 0.2, 0.8) - Darker when clicked

6. **Create Prefab:**
   - Drag SlotPrefab from Hierarchy to Project window
   - Save in Assets/Tutorial/02_GridLayoutSystem/Prefabs/
   - Name: "StudentSlot.prefab" 
   - Delete original from Hierarchy (prefab is now saved)

7. **Assign to InventoryGridManager:**
   - Select GridContainer in Hierarchy
   - In InventoryGridManager script component
   - Drag StudentSlot.prefab to "Slot Prefab" field

8. **Test Slot Generation:**
   - Play the scene
   - Script automatically generates 50 slots using your prefab
   - Each slot named "Slot_X_Y" (like Slot_0_0, Slot_1_0, etc.)

### Task 5: Test Grid Responsiveness
1. Attach InventoryGridManager to GridContainer
2. Set grid dimensions to 10x5 (50 slots total)
3. Test changing grid size at runtime
4. Verify slots arrange properly in grid pattern

## Success Criteria
- ✅ Grid displays 10 columns and 5 rows
- ✅ Slots are evenly spaced with 2px gaps
- ✅ Container auto-sizes to fit all slots
- ✅ Hover effects work on individual slots
- ✅ Grid adapts when size parameters change

## Code Example
```csharp
// Basic slot creation in grid
for (int y = 0; y < gridHeight; y++)
{
    for (int x = 0; x < gridWidth; x++)
    {
        GameObject slot = Instantiate(slotPrefab, gridParent);
        slot.name = $"Slot_{x}_{y}";
    }
}
```

## Common Issues & Solutions
- **No slots appear**: Check if Slot Prefab is assigned in InventoryGridManager Inspector
- **Slots wrong size**: Verify RectTransform Size Delta is (64, 64) in your prefab
- **No hover effects**: Ensure Button Target Graphic is set to Image component
- **Slots not responsive**: Check Button component is added and Interactable is true
- **Slots not arranging in grid**: Check GridLayoutGroup constraint settings
- **Container not resizing**: Verify ContentSizeFitter is properly configured
- **Spacing issues**: Adjust spacing values in GridLayoutGroup
- **Slots overlapping**: Check cell size matches slot prefab size

## How Slot Generation Works
The InventoryGridManager script:
1. Checks if you assigned a Slot Prefab in Inspector
2. If no prefab assigned, creates a default one automatically (fallback)
3. Uses nested loops to instantiate your prefab 50 times (10 width × 5 height)
4. Names each slot "Slot_X_Y" and adds SlotIndex component for identification
5. Arranges slots using GridLayoutGroup component settings

## Bonus Challenges
- Try changing constraint to Fixed Row Count
- Experiment with different Start Corner options
- Test with different aspect ratios

## Next Steps
In Exercise 3, we'll enhance individual slots with different states and interactions.