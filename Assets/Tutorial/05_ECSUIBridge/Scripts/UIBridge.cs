using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class InventoryUIBridge : MonoBehaviour
{
    [Header("UI References")]
    public Transform slotContainer;
    public GameObject slotPrefab;
    
    [Header("ECS References")]
    public Entity inventoryEntity;
    
    private List<UISlotReference> slotReferences;
    private bool isInitialized = false;
    private EntityManager entityManager;
    
    void Awake()
    {
        slotReferences = new List<UISlotReference>();
    }
    
    void Start()
    {
        var world = World.DefaultGameObjectInjectionWorld;
        if (world != null)
        {
            entityManager = world.EntityManager;
            
            // Register with UI system
            var uiSystem = world.GetOrCreateSystemManaged<InventoryUISystem>();
            if (inventoryEntity != Entity.Null)
            {
                uiSystem.RegisterInventoryBridge(inventoryEntity, this);
            }
        }
    }
    
    public void Initialize(Entity inventory)
    {
        // Prevent double initialization
        if (isInitialized && inventoryEntity == inventory)
        {
            Debug.Log("InventoryUIBridge: Already initialized with this entity, skipping...");
            return;
        }
        
        inventoryEntity = inventory;
        
        // Ensure EntityManager is initialized
        if (entityManager == null)
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world != null)
            {
                entityManager = world.EntityManager;
                Debug.Log("InventoryUIBridge: EntityManager initialized in Initialize()");
            }
            else
            {
                Debug.LogError("InventoryUIBridge: ECS World not ready yet, deferring initialization");
                StartCoroutine(RetryInitialization(inventory));
                return;
            }
        }
        
        if (entityManager == null)
        {
            Debug.LogError("InventoryUIBridge: EntityManager is still null after world access");
            return;
        }
        
        if (!entityManager.Exists(inventoryEntity))
        {
            Debug.LogError($"InventoryUIBridge: Entity {inventoryEntity} does not exist in ECS world");
            return;
        }
        
        var inventoryComponent = entityManager.GetComponentData<InventoryComponent>(inventoryEntity);
        Debug.Log($"InventoryUIBridge: About to create {inventoryComponent.maxSlots} slots");
        CreateSlots(inventoryComponent.maxSlots);
        isInitialized = true;
        Debug.Log("InventoryUIBridge: Successfully initialized with ECS entity");
        
        // Register with UI system
        var ecsWorld = World.DefaultGameObjectInjectionWorld;
        if (ecsWorld != null)
        {
            var uiSystem = ecsWorld.GetOrCreateSystemManaged<InventoryUISystem>();
            uiSystem.RegisterInventoryBridge(inventoryEntity, this);
        }
    }
    
    private System.Collections.IEnumerator RetryInitialization(Entity inventory)
    {
        Debug.Log("InventoryUIBridge: Retrying initialization...");
        
        // Wait for end of frame to ensure all Start() methods complete
        yield return new WaitForEndOfFrame();
        
        // Wait until ECS world is fully initialized
        int attempts = 0;
        while (World.DefaultGameObjectInjectionWorld == null && attempts < 100)
        {
            yield return null;
            attempts++;
        }
        
        if (World.DefaultGameObjectInjectionWorld != null)
        {
            Debug.Log("InventoryUIBridge: Retrying initialization after ECS world is ready");
            Initialize(inventory);
        }
        else
        {
            Debug.LogError("InventoryUIBridge: Failed to initialize after 100 attempts - ECS world unavailable");
        }
    }
    
    private void SetupGridLayoutForECS()
    {
        if (slotContainer == null)
        {
            Debug.LogError("InventoryUIBridge: slotContainer not assigned!");
            return;
        }
        
        // Check if GridLayoutGroup already exists (from InventoryGridManager)
        var gridLayoutGroup = slotContainer.GetComponent<GridLayoutGroup>();
        if (gridLayoutGroup == null)
        {
            // Create GridLayoutGroup with same settings as InventoryGridManager
            gridLayoutGroup = slotContainer.gameObject.AddComponent<GridLayoutGroup>();
            
            // Configure grid layout to match InventoryGridManager settings
            gridLayoutGroup.cellSize = new Vector2(64, 64);
            gridLayoutGroup.spacing = new Vector2(2, 2);
            gridLayoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayoutGroup.childAlignment = TextAnchor.MiddleCenter;
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayoutGroup.constraintCount = 10; // 10 columns for 50 slots (10x5 grid)
            
            Debug.Log("InventoryUIBridge: Created GridLayoutGroup for ECS slots");
        }
        else
        {
            Debug.Log("InventoryUIBridge: Using existing GridLayoutGroup from InventoryGridManager");
        }
        
        // Add Content Size Fitter for automatic sizing
        var contentSizeFitter = slotContainer.GetComponent<ContentSizeFitter>();
        if (contentSizeFitter == null)
        {
            contentSizeFitter = slotContainer.gameObject.AddComponent<ContentSizeFitter>();
            contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }
    
    private void CreateSlots(int slotCount)
    {
        // Clear existing slots
        ClearSlots();
        
        // Setup grid layout for proper 10x5 arrangement
        SetupGridLayoutForECS();
        
        for (int i = 0; i < slotCount; i++)
        {
            CreateSlot(i);
        }
        
        Debug.Log($"InventoryUIBridge: Created {slotCount} slots with proper grid layout");
    }
    
    private void CreateSlot(int index)
    {
        GameObject slotObject;
        
        if (slotPrefab != null)
        {
            slotObject = Instantiate(slotPrefab, slotContainer);
        }
        else
        {
            slotObject = CreateDefaultECSSlot(index);
        }
        
        // Calculate grid position for proper naming and SlotIndex component
        int x = index % 10; // 10 columns
        int y = index / 10; // Row number
        slotObject.name = $"Slot_{x}_{y}";
        
        // Ensure ItemSlotUI component is present (for Exercise 3-4 compatibility)
        var itemSlotUI = slotObject.GetComponent<ItemSlotUI>();
        if (itemSlotUI == null)
        {
            itemSlotUI = slotObject.AddComponent<ItemSlotUI>();
        }
        
        // Add SlotIndex component to track grid position (Exercise 3 compatibility)
        var slotIndex = slotObject.GetComponent<SlotIndex>();
        if (slotIndex == null)
        {
            slotIndex = slotObject.AddComponent<SlotIndex>();
        }
        slotIndex.x = x;
        slotIndex.y = y;
        
        // Add UISlotReference for ECS integration
        var slotReference = slotObject.GetComponent<UISlotReference>();
        if (slotReference == null)
        {
            slotReference = slotObject.AddComponent<UISlotReference>();
        }
        
        slotReference.Initialize(inventoryEntity, index);
        slotReferences.Add(slotReference);
        
        // Ensure drag handlers are present for ECS integration (Exercise 4 compatibility)
        var slotDragHandler = slotObject.GetComponent<DragHandler>();
        if (slotDragHandler == null)
        {
            slotDragHandler = slotObject.AddComponent<DragHandler>();
            Debug.Log($"InventoryUIBridge: Added DragHandler to slot {slotObject.name}");
        }
        
        var slotDropHandler = slotObject.GetComponent<DropHandler>();
        if (slotDropHandler == null)
        {
            slotDropHandler = slotObject.AddComponent<DropHandler>();
            Debug.Log($"InventoryUIBridge: Added DropHandler to slot {slotObject.name}");
        }
        
        // Subscribe to drag/drop events
        slotDragHandler.OnDragEnd += OnSlotDragEnd;
        slotDropHandler.OnItemDropped += OnSlotItemDropped;
        Debug.Log($"InventoryUIBridge: Subscribed to drag/drop events for slot {slotObject.name}");
    }
    
    private GameObject CreateDefaultECSSlot(int index)
    {
        // Create default slot with same setup as InventoryGridManager
        GameObject slotObject = new GameObject($"Slot_{index}");
        slotObject.transform.SetParent(slotContainer);
        
        var rectTransform = slotObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(64, 64);
        
        // Add Image component for background
        var image = slotObject.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        
        // Add outline
        var outline = slotObject.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = Color.white;
        outline.effectDistance = new Vector2(1, 1);
        
        // Add Button component for interaction
        var button = slotObject.AddComponent<UnityEngine.UI.Button>();
        button.targetGraphic = image;
        
        // Color transition on hover
        var colors = button.colors;
        colors.highlightedColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        button.colors = colors;
        
        // Add drag and drop handlers for Exercise 4 compatibility
        slotObject.AddComponent<DragHandler>();
        slotObject.AddComponent<DropHandler>();
        
        return slotObject;
    }
    
    private void ClearSlots()
    {
        foreach (var slotRef in slotReferences)
        {
            if (slotRef != null)
            {
                DestroyImmediate(slotRef.gameObject);
            }
        }
        slotReferences.Clear();
    }
    
    public void UpdateSlot(int slotIndex, InventorySlot slotData, ItemDataComponent itemData)
    {
        if (slotIndex >= 0 && slotIndex < slotReferences.Count)
        {
            var slotRef = slotReferences[slotIndex];
            if (slotRef != null)
            {
                slotRef.UpdateFromECS(slotData, itemData);
            }
        }
    }
    
    public void ClearSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < slotReferences.Count)
        {
            var slotRef = slotReferences[slotIndex];
            if (slotRef != null && slotRef.slotUI != null)
            {
                slotRef.slotUI.ClearSlot();
            }
        }
    }
    
    public void OnInventoryChanged(InventoryChangedEvent changeEvent)
    {
        // Handle inventory change events
        Debug.Log($"Inventory changed - Slot: {changeEvent.slotIndex}, Item: {changeEvent.itemId}, Count: {changeEvent.stackCount}");
    }
    
    private void OnSlotDragEnd(DragHandler dragHandler)
    {
        // Handle drag end - this is where you might want to create ECS requests
        var sourceSlot = dragHandler.GetSourceSlot();
        if (sourceSlot != null)
        {
            var sourceIndex = GetSlotIndex(sourceSlot.gameObject);
            Debug.Log($"InventoryUIBridge: Drag ended from slot {sourceIndex}");
        }
    }
    
    private void OnSlotItemDropped(DropHandler dropHandler, DragHandler dragHandler)
    {
        // Handle item drop - create ECS move request
        Debug.Log("InventoryUIBridge: OnSlotItemDropped triggered - processing ECS move request");
        
        var targetSlot = dropHandler.GetTargetSlot();
        var sourceSlot = dragHandler.GetSourceSlot();
        
        if (targetSlot != null && sourceSlot != null)
        {
            var targetIndex = GetSlotIndex(targetSlot.gameObject);
            var sourceIndex = GetSlotIndex(sourceSlot.gameObject);
            
            if (targetIndex != -1 && sourceIndex != -1)
            {
                Debug.Log($"InventoryUIBridge: Creating ECS move request from slot {sourceIndex} to slot {targetIndex} (quantity: {sourceSlot.stackCount})");
                RequestItemMove(sourceIndex, targetIndex, sourceSlot.stackCount);
            }
            else
            {
                Debug.LogWarning($"InventoryUIBridge: Invalid slot indices - source: {sourceIndex}, target: {targetIndex}");
            }
        }
        else
        {
            Debug.LogWarning("InventoryUIBridge: OnSlotItemDropped called with null slots");
        }
    }
    
    private int GetSlotIndex(GameObject slotObject)
    {
        Debug.Log($"InventoryUIBridge: GetSlotIndex searching for {slotObject.name} among {slotReferences.Count} slot references");
        
        for (int i = 0; i < slotReferences.Count; i++)
        {
            if (slotReferences[i] != null && slotReferences[i].gameObject == slotObject)
            {
                Debug.Log($"InventoryUIBridge: Found slot at index {i}");
                return i;
            }
        }
        
        // If direct match failed, try to find by ItemSlotUI component
        var itemSlotUI = slotObject.GetComponent<ItemSlotUI>();
        if (itemSlotUI != null)
        {
            for (int i = 0; i < slotReferences.Count; i++)
            {
                if (slotReferences[i] != null && slotReferences[i].slotUI == itemSlotUI)
                {
                    Debug.Log($"InventoryUIBridge: Found slot by ItemSlotUI at index {i}");
                    return i;
                }
            }
        }
        
        Debug.LogWarning($"InventoryUIBridge: Could not find slot index for {slotObject.name}");
        return -1;
    }
    
    private void RequestItemMove(int fromSlot, int toSlot, int quantity)
    {
        if (!isInitialized) 
        {
            Debug.LogError("InventoryUIBridge: Cannot create move request - bridge not initialized");
            return;
        }
        
        Debug.Log($"InventoryUIBridge: Creating ECS ItemMoveRequest entity (from:{fromSlot} to:{toSlot} qty:{quantity})");
        
        // Create move request entity
        var requestEntity = entityManager.CreateEntity();
        entityManager.AddComponentData(requestEntity, new ItemMoveRequest
        {
            fromSlot = fromSlot,
            toSlot = toSlot,
            quantity = quantity
        });
        
        entityManager.AddComponentData(requestEntity, new InventoryReference
        {
            inventoryEntity = inventoryEntity
        });
        
        Debug.Log($"InventoryUIBridge: ECS ItemMoveRequest entity created successfully (Entity: {requestEntity})");
    }
    
    public void RequestItemPickup(int itemId, int quantity, Vector3 worldPosition)
    {
        if (!isInitialized) return;
        
        var requestEntity = entityManager.CreateEntity();
        entityManager.AddComponentData(requestEntity, new ItemPickupRequest
        {
            itemId = itemId,
            quantity = quantity,
            worldPosition = worldPosition
        });
        
        entityManager.AddComponentData(requestEntity, new InventoryReference
        {
            inventoryEntity = inventoryEntity
        });
    }
    
    public bool IsInitialized()
    {
        return isInitialized && inventoryEntity != Entity.Null;
    }
    
    void OnDestroy()
    {
        if (isInitialized)
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world != null)
            {
                var uiSystem = world.GetOrCreateSystemManaged<InventoryUISystem>();
                uiSystem.UnregisterInventoryBridge(inventoryEntity);
            }
        }
    }
}