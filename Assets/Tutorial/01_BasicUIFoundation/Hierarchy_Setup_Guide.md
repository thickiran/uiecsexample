# Exercise 1: Proper Hierarchy Setup with Subscenes

## 🎯 **Why Use Subscenes?**
- **Performance**: ECS entities are baked at edit time
- **Streaming**: Load/unload content efficiently
- **Separation**: UI (MonoBehaviour) vs Data (ECS) concerns
- **Best Practice**: Recommended for DOTS projects

## 📋 **Step-by-Step Hierarchy Setup**

### **Step 1: Create Main Scene Components**
```
1. Create Canvas (stays in main scene)
   - Right-click Hierarchy → UI → Canvas
   - Name: "InventoryCanvas"
   - Add InventoryUIManager script

2. Create Inventory Panel
   - Right-click InventoryCanvas → UI → Panel  
   - Name: "InventoryPanel"
   - Configure as shown in README

3. Create Close Button
   - Right-click InventoryPanel → UI → Button
   - Name: "CloseButton"
   - Position in top-right corner
```

### **Step 2: Create Subscene for ECS Data**
```
1. Create Subscene
   - Right-click in Hierarchy → New SubScene → Empty Scene...
   - Name: "InventorySubScene"
   - Save in: Assets/Tutorial/01_BasicUIFoundation/

2. Add to Subscene (for later exercises)
   - ECS entities will go here
   - Inventory data systems
   - Item spawners
```

## 🏗️ **Final Hierarchy Structure**

```
📁 SampleScene
├── 📷 Main Camera
│   └── 🔧 (Transform, Camera, Audio Listener)
├── 🔆 Directional Light  
│   └── 🔧 (Transform, Light)
├── 🎮 EventSystem
│   └── 🔧 (EventSystem, StandaloneInputModule)
├── 📱 InventoryCanvas
│   ├── 🔧 (Canvas, CanvasScaler, GraphicRaycaster)
│   ├── 📜 InventoryUIManager
│   └── 📦 InventoryPanel
│       ├── 🔧 (RectTransform, Image)
│       └── ❌ CloseButton
│           ├── 🔧 (RectTransform, Image, Button)
│           └── 📝 Text
│               └── 🔧 (RectTransform, Text)
└── 📂 InventorySubScene
    └── 🔧 (SubScene)
```

## ⚙️ **Component Configuration**

### **InventoryCanvas Settings:**
- **Canvas**: Render Mode = Screen Space - Overlay
- **CanvasScaler**: 
  - UI Scale Mode = Scale With Screen Size
  - Reference Resolution = 1920x1080
  - Screen Match Mode = Match Width Or Height
  - Match = 0.5

### **InventoryPanel Settings:**
- **RectTransform**: 
  - Anchors = Center (0.5, 0.5)
  - Size = (800, 600)
  - Position = (0, 0)
- **Image**: 
  - Color = (0.2, 0.2, 0.2, 0.9)

### **CloseButton Settings:**
- **RectTransform**: 
  - Anchors = Top-Right
  - Size = (30, 30)
  - Position = (-15, -15)
- **Button**: 
  - Target Graphic = Image
  - OnClick = InventoryUIManager.CloseInventory()

## 🎨 **Visual Inspector Setup**

### **InventoryUIManager Inspector:**
```
📜 Inventory UI Manager (Script)
├── 📋 UI References
│   ├── 📱 Inventory Canvas: InventoryCanvas
│   ├── 📦 Inventory Panel: InventoryPanel  
│   └── ❌ Close Button: CloseButton
└── ⚙️ Settings
    ├── 🎹 Toggle Key: I
    └── 📏 Panel Size: (800, 600)
```

## 🔄 **Exercise 1 Testing Checklist**

After setup, verify:
- ✅ **'I' key toggles inventory** (opens/closes smoothly)
- ✅ **Close button works** (clicking X closes inventory)
- ✅ **Responsive scaling** (resize game window, UI scales)
- ✅ **Proper layering** (inventory appears over game)
- ✅ **Cursor behavior** (visible when inventory open, locked when closed)

## 🚀 **What's Next?**

In **Exercise 2**, we'll:
1. Add **GridContainer** to InventoryPanel
2. Keep UI in main scene
3. Start preparing ECS data structures in subscene
4. Set up the bridge between UI and ECS

## 💡 **Pro Tips**

### **Scene Organization:**
- **Main Scene**: UI, Camera, Lighting
- **Subscene**: ECS entities, pure data
- **Prefabs**: Reusable UI components

### **Performance:**
- UI updates only when needed
- ECS systems handle data efficiently
- Subscenes load independently

### **Debugging:**
- Use Unity Inspector for UI components
- Use Entity Debugger for ECS data
- Console for system messages

## 🎯 **Common Issues & Solutions**

### **Canvas Not Scaling:**
- Check Canvas Scaler settings
- Verify reference resolution
- Ensure screen match mode is correct

### **Button Not Working:**
- Check EventSystem exists
- Verify Button component setup
- Ensure OnClick event is assigned

### **Subscene Not Loading:**
- Check SubScene component is added
- Verify scene file exists
- Ensure Entity package is installed

Ready for Exercise 2? The grid system will build on this foundation! 🎮