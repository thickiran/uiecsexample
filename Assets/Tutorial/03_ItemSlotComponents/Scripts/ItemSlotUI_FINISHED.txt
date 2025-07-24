// ItemSlotUI.cs - FINISHED STATE
// Exercise 3: Item Slot Components - Complete Implementation
// Students can copy-paste this if they get stuck or miss the exercise

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [Header("Slot Configuration")]
        public SlotState currentState = SlotState.Empty;
        public int slotIndex = 0;
        
        [Header("Visual Components")]
        public Image backgroundImage;
        public Image itemImage;
        public Text stackCountText;
        public Image highlightImage;
        
        [Header("State Colors")]
        public Color emptyColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        public Color occupiedColor = new Color(0.4f, 0.4f, 0.4f, 0.8f);
        public Color highlightColor = new Color(0.6f, 0.6f, 0.6f, 0.8f);
        public Color selectedColor = new Color(0.8f, 0.8f, 0.2f, 0.8f);
        
        [Header("Item Data")]
        public ItemData currentItem;
        public int stackCount = 0;
        
        [Header("Drag Settings")]
        public bool isDraggable = true;
        public float dragAlpha = 0.6f;

        private bool isDragging = false;
        private CanvasGroup canvasGroup;
        private Vector2 originalPosition;
        
        private Button slotButton;
        private bool isSelected = false;
        private bool isHovered = false;
        
        public System.Action<ItemSlotUI> OnSlotClicked;
        public System.Action<ItemSlotUI> OnSlotHovered;
        public System.Action<ItemSlotUI> OnSlotUnhovered;
        
        void Awake()
        {
            InitializeComponents();
        }
        
        void InitializeComponents()
        {
            // Get or create button component
            slotButton = GetComponent<Button>();
            if (slotButton == null)
            {
                slotButton = gameObject.AddComponent<Button>();
            }
            
            // Set button to have no graphic target to prevent UI blocking
            slotButton.targetGraphic = null;
            
            // Get background image
            if (backgroundImage == null)
            {
                backgroundImage = GetComponent<Image>();
            }
            
            // Create item image if it doesn't exist
            if (itemImage == null)
            {
                GameObject itemImageObj = new GameObject("ItemImage");
                itemImageObj.transform.SetParent(transform, false);
                
                itemImage = itemImageObj.AddComponent<Image>();
                itemImage.raycastTarget = false;
                
                // Set up item image rect transform
                var itemRect = itemImage.GetComponent<RectTransform>();
                itemRect.anchorMin = Vector2.zero;
                itemRect.anchorMax = Vector2.one;
                itemRect.offsetMin = Vector2.one * 4; // 4px margin
                itemRect.offsetMax = Vector2.one * -4;
            }
            
            // Create stack count text if it doesn't exist
            if (stackCountText == null)
            {
                GameObject stackTextObj = new GameObject("StackCount");
                stackTextObj.transform.SetParent(transform, false);
                
                stackCountText = stackTextObj.AddComponent<Text>();
                stackCountText.raycastTarget = false;
                stackCountText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                stackCountText.fontSize = 12;
                stackCountText.color = Color.white;
                stackCountText.alignment = TextAnchor.LowerRight;
                
                // Position in bottom-right corner
                var textRect = stackCountText.GetComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0.5f, 0);
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = new Vector2(-2, -2);
            }
            
            // Create highlight image if it doesn't exist
            if (highlightImage == null)
            {
                GameObject highlightObj = new GameObject("Highlight");
                highlightObj.transform.SetParent(transform, false);
                
                highlightImage = highlightObj.AddComponent<Image>();
                highlightImage.raycastTarget = false;
                
                // Set up highlight rect transform
                var highlightRect = highlightImage.GetComponent<RectTransform>();
                highlightRect.anchorMin = Vector2.zero;
                highlightRect.anchorMax = Vector2.one;
                highlightRect.offsetMin = Vector2.zero;
                highlightRect.offsetMax = Vector2.zero;
            }
            
            // Get or create canvas group for drag operations
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            // Initialize state
            SetState(SlotState.Empty);
        }
        
        public void SetState(SlotState newState)
        {
            currentState = newState;
            UpdateVisuals();
        }
        
        public void SetItem(ItemData item, int count = 1)
        {
            currentItem = item;
            stackCount = count;
            
            if (item != null)
            {
                SetState(SlotState.Occupied);
                if (itemImage != null)
                {
                    itemImage.sprite = item.icon;
                    itemImage.color = Color.white;
                }
                
                UpdateStackCount();
            }
            else
            {
                ClearSlot();
            }
        }
        
        public void ClearSlot()
        {
            currentItem = null;
            stackCount = 0;
            SetState(SlotState.Empty);
            
            if (itemImage != null)
            {
                itemImage.sprite = null;
                itemImage.color = Color.clear;
            }
            
            UpdateStackCount();
        }
        
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateVisuals();
        }
        
        private void UpdateVisuals()
        {
            if (backgroundImage == null) return;
            
            Color targetColor = emptyColor;
            
            switch (currentState)
            {
                case SlotState.Empty:
                    targetColor = emptyColor;
                    break;
                case SlotState.Occupied:
                    targetColor = occupiedColor;
                    break;
                case SlotState.Highlighted:
                    targetColor = highlightColor;
                    break;
            }
            
            if (isSelected)
            {
                targetColor = selectedColor;
            }
            
            backgroundImage.color = targetColor;
            
            // Update highlight visibility
            if (highlightImage != null)
            {
                highlightImage.gameObject.SetActive(isHovered && !isSelected);
                if (isHovered && !isSelected)
                {
                    highlightImage.color = new Color(1f, 1f, 1f, 0.2f);
                }
            }
        }
        
        private void UpdateStackCount()
        {
            if (stackCountText == null) return;
            
            if (currentItem != null && stackCount > 1)
            {
                stackCountText.text = stackCount.ToString();
                stackCountText.gameObject.SetActive(true);
            }
            else
            {
                stackCountText.gameObject.SetActive(false);
            }
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            OnSlotClicked?.Invoke(this);
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            UpdateVisuals();
            OnSlotHovered?.Invoke(this);
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            UpdateVisuals();
            OnSlotUnhovered?.Invoke(this);
        }
        
        public void OnDrop(PointerEventData eventData)
        {
            var draggedObject = eventData.pointerDrag?.GetComponent<ItemSlotUI>();
            if (draggedObject != null && draggedObject != this)
            {
                ProcessItemTransfer(draggedObject);
                Debug.Log($"Dropped item from slot {draggedObject.slotIndex} to slot {slotIndex}");
            }
        }
        
        public bool IsEmpty()
        {
            return currentState == SlotState.Empty || currentItem == null;
        }
        
        public bool CanAcceptItem(ItemData item)
        {
            if (IsEmpty()) return true;
            
            if (currentItem != null && currentItem.itemId == item.itemId)
            {
                return stackCount < item.maxStackSize;
            }
            
            return false;
        }
        
        public int GetAvailableStackSpace()
        {
            if (IsEmpty()) return 99;
            if (currentItem == null) return 0;
            
            return currentItem.maxStackSize - stackCount;
        }
        
        private void ProcessItemTransfer(ItemSlotUI sourceSlot)
        {
            if (sourceSlot == null || sourceSlot.currentItem == null) return;
            
            var sourceItem = sourceSlot.currentItem;
            var sourceCount = sourceSlot.stackCount;
            
            if (IsEmpty())
            {
                // Move to empty slot
                SetItem(sourceItem, sourceCount);
                sourceSlot.ClearSlot();
                HandleSelectionTransfer(sourceSlot, "move");
                Debug.Log($"Moved {sourceItem.itemName} x{sourceCount} to empty slot");
            }
            else if (currentItem.itemId == sourceItem.itemId)
            {
                // Stack items
                int availableSpace = GetAvailableStackSpace();
                int moveAmount = Mathf.Min(sourceCount, availableSpace);
                
                if (moveAmount > 0)
                {
                    SetItem(currentItem, stackCount + moveAmount);
                    
                    int remainingCount = sourceCount - moveAmount;
                    if (remainingCount > 0)
                    {
                        sourceSlot.SetItem(sourceItem, remainingCount);
                        HandleSelectionTransfer(sourceSlot, "partial_stack");
                    }
                    else
                    {
                        sourceSlot.ClearSlot();
                        HandleSelectionTransfer(sourceSlot, "full_stack");
                    }
                    
                    Debug.Log($"Stacked {moveAmount} {sourceItem.itemName} (remaining: {remainingCount})");
                }
            }
            else
            {
                // Swap items
                var tempItem = currentItem;
                var tempCount = stackCount;
                
                SetItem(sourceItem, sourceCount);
                sourceSlot.SetItem(tempItem, tempCount);
                HandleSelectionTransfer(sourceSlot, "swap");
                
                Debug.Log($"Swapped {tempItem.itemName} with {sourceItem.itemName}");
            }
        }
        
        private void HandleSelectionTransfer(ItemSlotUI sourceSlot, string transferType)
        {
            // Find the SlotStateManager to update its selectedSlot reference
            var slotManager = FindObjectOfType<SlotStateManager>();
            if (slotManager == null) return;
            
            bool sourceWasSelected = sourceSlot.isSelected;
            bool targetWasSelected = this.isSelected;
            
            switch (transferType)
            {
                case "move":
                    // Move operation: if source was selected, target becomes selected
                    if (sourceWasSelected)
                    {
                        sourceSlot.SetSelected(false);
                        this.SetSelected(true);
                        slotManager.selectedSlot = this;
                        Debug.Log($"Selection transferred from slot {sourceSlot.slotIndex} to slot {slotIndex}");
                    }
                    break;
                    
                case "full_stack":
                    // Full stack: all items moved, so selection follows if source was selected
                    if (sourceWasSelected)
                    {
                        sourceSlot.SetSelected(false);
                        this.SetSelected(true);
                        slotManager.selectedSlot = this;
                        Debug.Log($"Selection transferred via stacking from slot {sourceSlot.slotIndex} to slot {slotIndex}");
                    }
                    break;
                    
                case "partial_stack":
                    // Partial stack: items remain in source, so keep selection there if it was selected
                    // But if target was selected and is getting more items, keep target selected
                    if (sourceWasSelected && !targetWasSelected)
                    {
                        // Keep source selected since items remain there
                        Debug.Log($"Partial stack: keeping selection on source slot {sourceSlot.slotIndex}");
                    }
                    else if (targetWasSelected)
                    {
                        // Keep target selected since it was already selected
                        Debug.Log($"Partial stack: keeping selection on target slot {slotIndex}");
                    }
                    break;
                    
                case "swap":
                    // Swap operation: selection states should swap along with items
                    sourceSlot.SetSelected(targetWasSelected);
                    this.SetSelected(sourceWasSelected);
                    
                    if (sourceWasSelected)
                    {
                        slotManager.selectedSlot = this;
                        Debug.Log($"Selection swapped: now slot {slotIndex} is selected");
                    }
                    else if (targetWasSelected)
                    {
                        slotManager.selectedSlot = sourceSlot;
                        Debug.Log($"Selection swapped: now slot {sourceSlot.slotIndex} is selected");
                    }
                    break;
            }
        }
        
        // Drag & Drop Implementation
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!isDraggable || IsEmpty()) return;
            
            isDragging = true;
            originalPosition = transform.position;
            
            // Reduce opacity during drag
            canvasGroup.alpha = dragAlpha;
            canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            
            // Move the slot with the mouse
            transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            
            isDragging = false;
            
            // Restore opacity and position
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            transform.position = originalPosition;
            
            // Drop handling is now done by OnDrop() method via IDropHandler interface
            Debug.Log($"Ended drag for slot {slotIndex}");
        }

        public bool IsDragging()
        {
            return isDragging;
        }
    }