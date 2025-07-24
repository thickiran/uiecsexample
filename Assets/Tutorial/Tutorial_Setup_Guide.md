# Tutorial Setup Guide

## Before Starting the Tutorial

### 1. Project Setup Verification
Ensure your Unity project has the following:
- ✅ Unity 6.0 or later
- ✅ DOTS packages installed (entities, entities.graphics)
- ✅ Input System package
- ✅ UI toolkit (uGUI) package

### 2. Scene Preparation
1. Create a new scene or use the existing SampleScene
2. Ensure you have a main camera in the scene
3. Add an EventSystem GameObject (Create → UI → Event System)
4. Set up basic lighting if needed

### 3. Folder Structure Check
Your project should have this structure:
```
Assets/
├── Tutorial/
│   ├── 01_BasicUIFoundation/
│   ├── 02_GridLayoutSystem/
│   ├── 03_ItemSlotComponents/
│   ├── 04_DragDropSystem/
│   ├── 05_ECSUIBridge/
│   └── README.md
├── Scenes/
└── Scripts/
```

## Exercise Preparation Checklist

### Before Each Exercise:
1. **Read the README** in the exercise folder
2. **Understand the learning objectives**
3. **Check that previous exercises are working**
4. **Have Unity Inspector open** for component configuration
5. **Clear the Console** for clean error tracking

### During Each Exercise:
1. **Follow the README step-by-step**
2. **Test frequently** - don't wait until the end
3. **Ask questions** if something isn't clear
4. **Save your work** regularly
5. **Check the Success Criteria** before moving on

## Quick Reference

### Important Unity Windows
- **Inspector**: For component configuration
- **Console**: For error messages and debug output
- **Hierarchy**: For scene organization
- **Project**: For asset management
- **Game View**: For testing UI

### Keyboard Shortcuts
- `F2`: Rename selected object
- `Ctrl+D`: Duplicate selected object
- `Ctrl+S`: Save scene
- `Ctrl+Shift+S`: Save scene as
- `Space`: Play/pause game

### Test Keys (Added in Each Exercise)
- `I`: Toggle inventory (Exercise 1+)
- `1-6`: Add test items (Exercise 3+)
- `Mouse`: Drag and drop (Exercise 4+)

## Common Setup Issues

### Package Issues
**Problem**: DOTS packages not found
**Solution**: 
1. Open Package Manager
2. Switch to "Unity Registry"
3. Search for "Entities" and install
4. Also install "Entities Graphics"

### Input System Issues
**Problem**: Input doesn't work
**Solution**:
1. Go to Edit → Project Settings → XR Plug-in Management
2. In Input System Package, click "Yes" to enable

### UI Issues
**Problem**: UI doesn't appear
**Solution**:
1. Check if Canvas is set to "Screen Space - Overlay"
2. Verify EventSystem exists in scene
3. Ensure UI elements have proper anchoring

### Script Compilation Issues
**Problem**: Scripts won't compile
**Solution**:
1. Check Console for specific error messages
2. Verify all required namespaces are included
3. Ensure DOTS packages are properly installed

## Exercise-Specific Setup

### Exercise 1: Basic UI Foundation
**Before starting:**
- Ensure EventSystem is in scene
- Check Canvas Scaler settings
- Verify Input System is enabled

### Exercise 2: Grid Layout System
**Before starting:**
- Complete Exercise 1 successfully
- Have a working inventory panel
- Understand Layout Groups

### Exercise 3: Item Slot Components
**Before starting:**
- Complete Exercise 2 successfully
- Have a working grid system
- Understand Unity Event System

### Exercise 4: Drag & Drop System
**Before starting:**
- Complete Exercise 3 successfully
- Have working item slots
- Understand pointer events

### Exercise 5: ECS-UI Bridge Integration
**Before starting:**
- Complete Exercise 4 successfully
- Have working drag & drop
- Understand ECS basics

## Troubleshooting Guide

### If UI Doesn't Scale Properly:
1. Check Canvas Scaler component
2. Verify reference resolution (1920x1080)
3. Ensure screen match mode is set correctly

### If Items Don't Drag:
1. Check if item slots have DragHandler component
2. Verify Canvas Group is properly configured
3. Ensure raycasting is not blocked

### If ECS Integration Fails:
1. Verify DOTS packages are installed
2. Check that World.DefaultGameObjectInjectionWorld exists
3. Ensure systems are properly registered

## Performance Tips

### During Development:
- Keep Console open to catch errors early
- Test on different screen resolutions
- Use Unity Profiler for performance analysis
- Comment code for clarity

### For Production:
- Minimize UI updates per frame
- Use object pooling for dynamic UI elements
- Batch ECS operations when possible
- Cache frequently accessed components

## Resources

### Unity Documentation:
- [Unity UI System](https://docs.unity3d.com/Manual/UISystem.html)
- [DOTS Documentation](https://docs.unity3d.com/Packages/com.unity.entities@latest)
- [Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest)

### Additional Learning:
- Unity Learn: UI courses
- DOTS samples repository
- Unity forums and community

## Final Checklist

Before starting the tutorial:
- [ ] Project setup complete
- [ ] All packages installed
- [ ] Scene prepared with EventSystem
- [ ] Tutorial folders created
- [ ] Unity windows arranged
- [ ] Ready to follow instructions

Ready to begin? Start with Exercise 1!