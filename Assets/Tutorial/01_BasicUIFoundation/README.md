# Exercise 1: Basic UI Foundation

## Learning Objectives
- Set up a Canvas with proper settings for inventory UI
- Create responsive inventory panel with proper anchoring
- Configure Canvas Scaler for multi-resolution support
- Implement basic show/hide functionality

## Instructor Demo (5 minutes)
Watch as the instructor demonstrates:
1. Creating a Canvas with Screen Space Overlay
2. Adding Canvas Scaler for responsive design
3. Setting up inventory panel with proper anchoring
4. Configuring basic styling and background

## Your Exercise (5 minutes)
Complete the following tasks:

### Task 1: Create Canvas Setup
1. Create a new GameObject named "InventoryCanvas"
2. Add Canvas component and set to Screen Space Overlay
3. Add Canvas Scaler component with these settings:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920x1080
   - Screen Match Mode: Match Width Or Height
   - Match: 0.5
4. **IMPORTANT**: Keep Canvas in main scene (not subscene) for UI

### Task 2: Create Inventory Panel
1. Create child GameObject named "InventoryPanel" under Canvas
2. Add RectTransform component and configure:
   - Anchor: Center (0.5, 0.5)
   - Pivot: Center (0.5, 0.5)
   - Position: (0, 0)
   - Size: (800, 600)
3. Add Image component for background
4. Set background color to dark gray with 90% opacity

### Task 3: Add Close Button
1. Create "CloseButton" as child of InventoryPanel
2. Position in top-right corner of panel
3. Add Button component and "X" text
4. Connect to close functionality

### Task 4: Create Subscene (DOTS Preparation)
1. Right-click in Hierarchy → New SubScene → Empty Scene
2. Name it "InventorySubScene"
3. Save in Assets/Tutorial/01_BasicUIFoundation/
4. Leave empty for now (we'll use it in Exercise 5)

### Task 5: Test Your Setup
1. Attach InventoryUIManager script to InventoryCanvas
2. Assign references in inspector
3. Test with 'I' key to toggle inventory
4. Verify responsive scaling works on different screen sizes

## Success Criteria
- ✅ Canvas scales properly on different resolutions
- ✅ Inventory panel is centered and properly sized
- ✅ Background is semi-transparent dark color
- ✅ 'I' key toggles inventory visibility
- ✅ Close button works correctly

## Common Issues & Solutions
- **Canvas doesn't scale**: Check Canvas Scaler settings
- **Panel not centered**: Verify anchor and pivot settings
- **Background not visible**: Ensure Image component is added
- **Toggle key not working**: Check InventoryUIManager script attachment
- **Inventory won't open after first close**: CRITICAL - Make sure the script disables the InventoryPanel, NOT the InventoryCanvas. If you disable the Canvas, the script stops working!

## Next Steps
Once completed, you'll move to Exercise 2 where we'll add a grid layout system for inventory slots.