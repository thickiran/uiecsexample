using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Drag Settings")]
        public bool isDraggable = true;
        public float dragAlpha = 0.6f;
        
        [Header("Components")]
        public Image dragImage;
        public Canvas dragCanvas;
        
        private Vector3 startPosition;
        private Transform startParent;
        private CanvasGroup canvasGroup;
        private GraphicRaycaster raycaster;
        private ItemSlotUI sourceSlot;
        private GameObject dragPreviewObject;
        private bool? isECSActive = null; // Cached ECS detection result
        
        public System.Action<DragHandler> OnDragStart;
        public System.Action<DragHandler> OnDragEnd;
        public System.Action<DragHandler, Vector2> OnDragUpdate;
        
        void Awake()
        {
            InitializeComponents();
        }
        
        void InitializeComponents()
        {
            // Get or create canvas group for alpha control
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            // Get source slot reference
            sourceSlot = GetComponent<ItemSlotUI>();
            
            // Find drag canvas (usually the main canvas)
            if (dragCanvas == null)
            {
                dragCanvas = GetComponentInParent<Canvas>();
                if (dragCanvas == null)
                {
                    dragCanvas = FindObjectOfType<Canvas>();
                }
            }
            
            // Get raycaster for drop detection
            if (dragCanvas != null)
            {
                raycaster = dragCanvas.GetComponent<GraphicRaycaster>();
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
                    Debug.Log("DragHandler: ECS system detected - will defer to ECS for item moves");
                }
            }
            return isECSActive.Value;
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!isDraggable || sourceSlot == null || sourceSlot.IsEmpty())
            {
                return;
            }
            
            // Store original position and parent
            startPosition = transform.position;
            startParent = transform.parent;
            
            // Create drag preview
            CreateDragPreview();
            
            // Set up for dragging
            canvasGroup.alpha = dragAlpha;
            canvasGroup.blocksRaycasts = false;
            
            // Move to top of hierarchy for proper layering
            transform.SetParent(dragCanvas.transform);
            transform.SetAsLastSibling();
            
            OnDragStart?.Invoke(this);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!isDraggable || sourceSlot == null || sourceSlot.IsEmpty())
            {
                return;
            }
            
            // Follow mouse position
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dragCanvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint);
            
            transform.localPosition = localPoint;
            
            OnDragUpdate?.Invoke(this, eventData.position);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDraggable || sourceSlot == null)
            {
                return;
            }
            
            // Restore visual state
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            
            // Find drop target
            var dropTarget = FindDropTarget(eventData);
            bool dropSuccessful = false;
            
            if (dropTarget != null)
            {
                dropSuccessful = AttemptDrop(dropTarget);
            }
            
            // Return to original position if drop failed
            if (!dropSuccessful)
            {
                ReturnToOriginalPosition();
            }
            
            // Clean up drag preview
            DestroyDragPreview();
            
            OnDragEnd?.Invoke(this);
        }
        
        private void CreateDragPreview()
        {
            if (sourceSlot == null || sourceSlot.currentItem == null) return;
            
            // Create preview object
            dragPreviewObject = new GameObject("DragPreview");
            dragPreviewObject.transform.SetParent(dragCanvas.transform);
            
            // Add image component
            var previewImage = dragPreviewObject.AddComponent<Image>();
            previewImage.sprite = sourceSlot.currentItem.icon;
            previewImage.color = new Color(1f, 1f, 1f, 0.8f);
            previewImage.raycastTarget = false;
            
            // Set size and position
            var rectTransform = previewImage.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(64, 64);
            rectTransform.position = transform.position;
            
            // Add stack count if needed
            if (sourceSlot.stackCount > 1)
            {
                var stackText = new GameObject("StackCount");
                stackText.transform.SetParent(dragPreviewObject.transform);
                
                var text = stackText.AddComponent<Text>();
                text.text = sourceSlot.stackCount.ToString();
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                text.fontSize = 12;
                text.color = Color.white;
                text.alignment = TextAnchor.LowerRight;
                text.raycastTarget = false;
                
                var textRect = text.GetComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0.5f, 0);
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = new Vector2(-2, -2);
            }
        }
        
        private void DestroyDragPreview()
        {
            if (dragPreviewObject != null)
            {
                DestroyImmediate(dragPreviewObject);
            }
        }
        
        private ItemSlotUI FindDropTarget(PointerEventData eventData)
        {
            if (raycaster == null) return null;
            
            // Raycast to find what's under the mouse
            var results = new System.Collections.Generic.List<RaycastResult>();
            raycaster.Raycast(eventData, results);
            
            foreach (var result in results)
            {
                var slot = result.gameObject.GetComponent<ItemSlotUI>();
                if (slot != null && slot != sourceSlot)
                {
                    return slot;
                }
            }
            
            return null;
        }
        
        private bool AttemptDrop(ItemSlotUI targetSlot)
        {
            if (sourceSlot == null || sourceSlot.currentItem == null) return false;
            
            // If ECS is active, skip direct UI manipulation - let ECS system handle it
            if (IsECSActive())
            {
                Debug.Log("DragHandler: ECS active - skipping direct UI manipulation, letting ECS handle the move");
                // Return true to indicate the drop event was handled (ECS will process it)
                return true;
            }
            
            // Original Exercise 4 logic for direct UI manipulation
            Debug.Log("DragHandler: No ECS detected - using direct UI manipulation (Exercise 1-4)");
            
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
        
        private void ReturnToOriginalPosition()
        {
            transform.SetParent(startParent);
            transform.position = startPosition;
        }
        
        public void SetDraggable(bool draggable)
        {
            isDraggable = draggable;
        }
        
        public ItemSlotUI GetSourceSlot()
        {
            return sourceSlot;
        }
    }