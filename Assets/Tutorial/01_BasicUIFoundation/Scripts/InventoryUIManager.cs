using UnityEngine;
using UnityEngine.UI;

public class InventoryUIManager : MonoBehaviour
    {
        [Header("UI References")]
        public Canvas inventoryCanvas;
        public RectTransform inventoryPanel;
        public Button closeButton;
        
        [Header("Settings")]
        public KeyCode toggleKey = KeyCode.I;
        public Vector2 panelSize = new Vector2(800, 600);
        
        private bool isInventoryOpen = false;
        
        void Start()
        {
            InitializeUIWithoutHiding();
        }
        
        void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleInventory();
            }
        }
        
        void InitializeUIWithoutHiding()
        {
            // Set up canvas for screen space overlay
            if (inventoryCanvas != null)
            {
                inventoryCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                inventoryCanvas.sortingOrder = 100;
                
                // Add Canvas Scaler for responsive design
                var canvasScaler = inventoryCanvas.GetComponent<CanvasScaler>();
                if (canvasScaler == null)
                {
                    canvasScaler = inventoryCanvas.gameObject.AddComponent<CanvasScaler>();
                }
                
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.referenceResolution = new Vector2(1920, 1080);
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvasScaler.matchWidthOrHeight = 0.5f;
            }
            
            // Set up inventory panel
            if (inventoryPanel != null)
            {
                inventoryPanel.anchorMin = new Vector2(0.5f, 0.5f);
                inventoryPanel.anchorMax = new Vector2(0.5f, 0.5f);
                inventoryPanel.pivot = new Vector2(0.5f, 0.5f);
                inventoryPanel.anchoredPosition = Vector2.zero;
                inventoryPanel.sizeDelta = panelSize;
                
                // Add background image if it doesn't exist
                var backgroundImage = inventoryPanel.GetComponent<Image>();
                if (backgroundImage == null)
                {
                    backgroundImage = inventoryPanel.gameObject.AddComponent<Image>();
                }
                
                // Set up background styling
                backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
                backgroundImage.raycastTarget = true;
            }
            
            // Set up close button
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseInventory);
            }
            
            // NOTE: Do not hide inventory here - let SlotStateManager coordinate this
        }
        
        public void HideInventoryAfterSlotInitialization()
        {
            // Called by SlotStateManager after it has found all slots
            SetInventoryVisibility(false);
        }
        
        public void ToggleInventory()
        {
            isInventoryOpen = !isInventoryOpen;
            SetInventoryVisibility(isInventoryOpen);
        }
        
        public void OpenInventory()
        {
            isInventoryOpen = true;
            SetInventoryVisibility(true);
        }
        
        public void CloseInventory()
        {
            isInventoryOpen = false;
            SetInventoryVisibility(false);
        }
        
        private void SetInventoryVisibility(bool visible)
        {
            if (inventoryPanel != null)
            {
                inventoryPanel.gameObject.SetActive(visible);
            }
            
            // Lock cursor when inventory is open
            if (visible)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        
        // Helper method for students to call during exercise
        public void SetPanelColor(Color color)
        {
            if (inventoryPanel != null)
            {
                var image = inventoryPanel.GetComponent<Image>();
                if (image != null)
                {
                    image.color = color;
                }
            }
        }
        
        // Helper method for students to resize panel
        public void SetPanelSize(Vector2 size)
        {
            panelSize = size;
            if (inventoryPanel != null)
            {
                inventoryPanel.sizeDelta = size;
            }
        }
    }