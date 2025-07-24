using UnityEngine;
using UnityEngine.EventSystems;

public class DropHandler : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Drop Settings")]
        public bool canAcceptDrops = true;
        public Color validDropColor = new Color(0.2f, 0.8f, 0.2f, 0.3f);
        public Color invalidDropColor = new Color(0.8f, 0.2f, 0.2f, 0.3f);
        
        [Header("Visual Feedback")]
        public UnityEngine.UI.Image dropIndicator;
        
        private ItemSlotUI targetSlot;
        private bool isDraggedOver = false;
        private DragHandler currentDraggedItem;
        private bool? isECSActive = null; // Cached ECS detection result
        
        public System.Action<DropHandler, DragHandler> OnItemDropped;
        public System.Action<DropHandler, DragHandler> OnDragEnter;
        public System.Action<DropHandler, DragHandler> OnDragExit;
        
        void Awake()
        {
            InitializeComponents();
        }
        
        void InitializeComponents()
        {
            // Get target slot reference
            targetSlot = GetComponent<ItemSlotUI>();
            
            // Create drop indicator if it doesn't exist
            if (dropIndicator == null)
            {
                CreateDropIndicator();
            }
            
            // Hide indicator initially
            if (dropIndicator != null)
            {
                dropIndicator.gameObject.SetActive(false);
            }
        }
        
        private bool IsECSActive()
        {
            // Cache the result to avoid repeated FindObjectOfType calls
            if (isECSActive == null)
            {
                var inventoryDataUpdater = FindObjectOfType<InventoryDataUpdater>();
                var inventoryUIBridge = FindObjectOfType<InventoryUIBridge>();
                isECSActive = inventoryDataUpdater != null || inventoryUIBridge != null;
                
                if (isECSActive.Value)
                {
                    Debug.Log("DropHandler: ECS system detected - will defer to ECS for item moves");
                }
            }
            return isECSActive.Value;
        }
        
        private void CreateDropIndicator()
        {
            GameObject indicatorObj = new GameObject("DropIndicator");
            indicatorObj.transform.SetParent(transform, false);
            
            dropIndicator = indicatorObj.AddComponent<UnityEngine.UI.Image>();
            dropIndicator.color = validDropColor;
            dropIndicator.raycastTarget = false;
            
            // Set up rect transform to cover entire slot
            var rectTransform = dropIndicator.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            // Move to back so it appears behind other elements
            indicatorObj.transform.SetAsFirstSibling();
        }
        
        public void OnDrop(PointerEventData eventData)
        {
            if (!canAcceptDrops || targetSlot == null)
            {
                return;
            }
            
            // Find the dragged object
            var draggedObject = eventData.pointerDrag;
            if (draggedObject == null) return;
            
            var dragHandler = draggedObject.GetComponent<DragHandler>();
            if (dragHandler == null) return;
            
            // Attempt to handle the drop
            bool dropSuccessful = HandleDrop(dragHandler);
            
            if (dropSuccessful)
            {
                Debug.Log($"DropHandler: Drop successful, invoking OnItemDropped event for {gameObject.name}");
                OnItemDropped?.Invoke(this, dragHandler);
            }
            else
            {
                Debug.LogWarning($"DropHandler: Drop failed for {gameObject.name}");
            }
            
            // Hide drop indicator
            HideDropIndicator();
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!canAcceptDrops) return;
            
            // Check if we're dragging something
            if (eventData.pointerDrag != null)
            {
                var dragHandler = eventData.pointerDrag.GetComponent<DragHandler>();
                if (dragHandler != null)
                {
                    currentDraggedItem = dragHandler;
                    isDraggedOver = true;
                    
                    // Show appropriate drop indicator
                    ShowDropIndicator(CanAcceptDrop(dragHandler));
                    
                    OnDragEnter?.Invoke(this, dragHandler);
                }
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (isDraggedOver)
            {
                isDraggedOver = false;
                HideDropIndicator();
                
                if (currentDraggedItem != null)
                {
                    OnDragExit?.Invoke(this, currentDraggedItem);
                }
                
                currentDraggedItem = null;
            }
        }
        
        private bool HandleDrop(DragHandler dragHandler)
        {
            if (targetSlot == null) return false;
            
            var sourceSlot = dragHandler.GetSourceSlot();
            if (sourceSlot == null || sourceSlot.currentItem == null) return false;
            
            // If ECS is active, skip direct UI manipulation - let ECS system handle it
            if (IsECSActive())
            {
                Debug.Log("DropHandler: ECS active - skipping direct UI manipulation, letting ECS handle the move");
                // Return true to indicate the drop event was handled (ECS will process it)
                return true;
            }
            
            // Original Exercise 4 logic for direct UI manipulation
            Debug.Log("DropHandler: No ECS detected - using direct UI manipulation (Exercise 1-4)");
            
            // Use the same logic as in DragHandler
            var sourceItem = sourceSlot.currentItem;
            var sourceCount = sourceSlot.stackCount;
            
            // Check if target can accept the item
            if (targetSlot.IsEmpty())
            {
                // Move item to empty slot
                targetSlot.SetItem(sourceItem, sourceCount);
                sourceSlot.ClearSlot();
                return true;
            }
            else if (targetSlot.currentItem.itemId == sourceItem.itemId)
            {
                // Try to stack items
                var availableSpace = targetSlot.GetAvailableStackSpace();
                if (availableSpace > 0)
                {
                    var moveCount = Mathf.Min(sourceCount, availableSpace);
                    targetSlot.stackCount += moveCount;
                    targetSlot.SetItem(targetSlot.currentItem, targetSlot.stackCount);
                    
                    sourceCount -= moveCount;
                    if (sourceCount > 0)
                    {
                        sourceSlot.SetItem(sourceItem, sourceCount);
                    }
                    else
                    {
                        sourceSlot.ClearSlot();
                    }
                    
                    return true;
                }
            }
            else
            {
                // Swap items
                var targetItem = targetSlot.currentItem;
                var targetCount = targetSlot.stackCount;
                
                targetSlot.SetItem(sourceItem, sourceCount);
                sourceSlot.SetItem(targetItem, targetCount);
                
                return true;
            }
            
            return false;
        }
        
        private bool CanAcceptDrop(DragHandler dragHandler)
        {
            if (targetSlot == null) return false;
            
            var sourceSlot = dragHandler.GetSourceSlot();
            if (sourceSlot == null || sourceSlot.currentItem == null) return false;
            
            var sourceItem = sourceSlot.currentItem;
            
            // Empty slot can accept anything
            if (targetSlot.IsEmpty()) return true;
            
            // Same item type can potentially stack
            if (targetSlot.currentItem.itemId == sourceItem.itemId)
            {
                return targetSlot.GetAvailableStackSpace() > 0;
            }
            
            // Different items can be swapped
            return true;
        }
        
        private void ShowDropIndicator(bool validDrop)
        {
            if (dropIndicator != null)
            {
                dropIndicator.gameObject.SetActive(true);
                dropIndicator.color = validDrop ? validDropColor : invalidDropColor;
            }
        }
        
        private void HideDropIndicator()
        {
            if (dropIndicator != null)
            {
                dropIndicator.gameObject.SetActive(false);
            }
        }
        
        public void SetCanAcceptDrops(bool canAccept)
        {
            canAcceptDrops = canAccept;
        }
        
        public ItemSlotUI GetTargetSlot()
        {
            return targetSlot;
        }
        
        public bool IsValidDropTarget(DragHandler dragHandler)
        {
            return canAcceptDrops && CanAcceptDrop(dragHandler);
        }
    }