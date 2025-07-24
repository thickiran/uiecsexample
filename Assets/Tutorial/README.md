# DOTS Inventory System Tutorial

## Overview
This tutorial teaches you how to build a complete inventory system using Unity's Data-Oriented Technology Stack (DOTS) for a Diablo/V-Rising style game. The tutorial is structured as 5 exercises, each taking 10 minutes (5 minutes instructor demo + 5 minutes student practice).

## Prerequisites
- Unity 6.0 or later
- Basic knowledge of Unity UI system
- Familiarity with C# programming
- Understanding of basic game development concepts

## Required Packages
The following packages are already installed in this project:
- `com.unity.entities` (1.3.14) - Core ECS framework
- `com.unity.entities.graphics` (1.3.2) - Hybrid rendering support
- `com.unity.inputsystem` (1.14.0) - New input system
- `com.unity.ugui` (2.0.0) - UI system

## Tutorial Structure (45 minutes total)

### Exercise 1: Basic UI Foundation (10 minutes)
**Files:** `Assets/Tutorial/01_BasicUIFoundation/`
- Create Canvas with proper settings
- Set up responsive inventory panel
- Configure basic show/hide functionality
- **Key Learning:** Canvas setup and responsive design

### Exercise 2: Grid Layout System (10 minutes)  
**Files:** `Assets/Tutorial/02_GridLayoutSystem/`
- Implement Grid Layout Group
- Configure automatic sizing
- Create responsive grid system
- **Key Learning:** Unity layout components and grid organization

### Exercise 3: Item Slot Components (10 minutes)
**Files:** `Assets/Tutorial/03_ItemSlotComponents/`
- Create interactive item slots
- Implement visual state management
- Add hover and click interactions
- **Key Learning:** UI event system and component interaction

### Exercise 4: Drag & Drop System (10 minutes)
**Files:** `Assets/Tutorial/04_DragDropSystem/`
- Implement drag & drop interfaces
- Create visual feedback system
- Handle item swapping and stacking
- **Key Learning:** Advanced UI interactions and event handling

### Exercise 5: ECS-UI Bridge Integration (15 minutes)
**Files:** `Assets/Tutorial/05_ECSUIBridge/`
- Connect UI system to DOTS backend
- Implement real-time data synchronization
- Create hybrid architecture
- **Key Learning:** ECS integration with traditional UI

## Getting Started

1. **Open the project** in Unity 6.0+
2. **Start with Exercise 1** - Each exercise builds on the previous
3. **Follow the README** in each exercise folder for detailed instructions
4. **Test frequently** - Each exercise includes testing steps
5. **Ask questions** during the 5-minute practice periods

## Exercise Flow
Each exercise follows this pattern:
- **5 minutes**: Instructor demonstrates the concepts and implementation
- **5 minutes**: Students implement the exercise following the README
- **Quick check**: Verify everyone completed before moving to next exercise

## Key Concepts Covered

### Unity UI System
- Canvas and Canvas Scaler setup
- Layout Groups and Content Size Fitter
- Event System interfaces (IPointerEnterHandler, IDragHandler, etc.)
- Responsive design principles

### DOTS/ECS Architecture
- Entity Component System basics
- Component and System creation
- Buffer elements for dynamic data
- Event-driven architecture
- Hybrid ECS-MonoBehaviour patterns

### Game Development Patterns
- Bridge pattern for UI-ECS communication
- Event-driven updates
- State management
- Data synchronization

## Testing Your Implementation

Each exercise includes specific test scenarios:
- **Exercise 1**: Test inventory toggle and responsive scaling
- **Exercise 2**: Test grid layout with different sizes
- **Exercise 3**: Test slot interactions and visual feedback
- **Exercise 4**: Test drag & drop operations
- **Exercise 5**: Test ECS data synchronization

## Common Issues & Solutions

### Canvas Not Scaling
- Check Canvas Scaler settings
- Verify reference resolution is set correctly
- Ensure screen match mode is appropriate

### Layout Not Working
- Verify Layout Group components are properly configured
- Check Content Size Fitter settings
- Ensure child objects have correct anchor settings

### Drag & Drop Issues
- Verify event interfaces are implemented
- Check that raycasting is not blocked
- Ensure proper layer setup

### ECS Integration Problems
- Verify DOTS packages are installed
- Check entity creation order
- Ensure systems are properly registered

## Performance Considerations
- **UI Updates**: Minimize unnecessary UI refreshes
- **ECS Queries**: Use efficient entity queries
- **Memory**: Be mindful of allocation in update loops
- **Event Handling**: Batch event processing when possible

## Extension Ideas
After completing all exercises, consider extending the system with:
- Item tooltips and detailed information
- Inventory sorting and filtering
- Multiple inventory types (player, chest, merchant)
- Item crafting system
- Equipment slots and character stats
- Save/load functionality

## Support Files
- **Scripts**: Core functionality for each exercise
- **Prefabs**: Pre-configured UI elements
- **Examples**: Sample scenes and configurations
- **README**: Detailed instructions for each exercise

## Final Result
By the end of this tutorial, you'll have:
- A fully functional inventory system
- Understanding of DOTS architecture
- Knowledge of UI-ECS integration patterns
- Experience with modern Unity development practices
- A foundation for building more complex game systems

## Next Steps
This inventory system serves as a foundation for larger game systems. Consider exploring:
- Player movement and item pickup systems
- Equipment and character progression
- Multiplayer inventory synchronization
- Advanced UI animations and effects
- Integration with other game systems

Good luck with your inventory system development!