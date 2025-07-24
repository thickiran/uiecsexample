// SlotStateManager.cs - FINISHED STATE
// Exercise 3: Item Slot Components - Complete Implementation
// Students can copy-paste this if they get stuck or miss the exercise

using UnityEngine;
using System.Collections.Generic;

public class SlotStateManager : MonoBehaviour
    {
        [Header("Slot Management")]
        public List<ItemSlotUI> allSlots = new List<ItemSlotUI>();
        public ItemSlotUI selectedSlot;
        
        [Header("Test Items")]
        public ItemData[] testItems;
        
        void Start()
        {
            CreateTestItems();
            // Delay slot initialization to ensure grid is created first, 
            // but before InventoryUIManager hides the panel
            StartCoroutine(DelayedInitializeSlots());
        }
        
        System.Collections.IEnumerator DelayedInitializeSlots()
        {
            // Wait for end of frame to ensure InventoryGridManager has created slots
            yield return new WaitForEndOfFrame();
            
            // Initialize slots while they are still active/visible
            InitializeSlots();
            
            // Now tell InventoryUIManager it's safe to hide the panel
            var uiManager = FindObjectOfType<InventoryUIManager>();
            if (uiManager != null)
            {
                uiManager.HideInventoryAfterSlotInitialization();
                Debug.Log("SlotStateManager coordinated with InventoryUIManager to hide panel after slot initialization");
            }
        }
        
        void InitializeSlots()
        {
            // Find all slot components in the scene
            var foundSlots = FindObjectsByType<ItemSlotUI>(FindObjectsSortMode.None);
            allSlots.Clear();
            allSlots.AddRange(foundSlots);
            
            // Sort slots by their grid position (top-left to bottom-right)
            allSlots.Sort((slot1, slot2) => 
            {
                var index1 = slot1.GetComponent<SlotIndex>();
                var index2 = slot2.GetComponent<SlotIndex>();
                
                if (index1 == null || index2 == null)
                {
                    // Fallback to name-based sorting if SlotIndex is missing
                    return string.Compare(slot1.name, slot2.name, System.StringComparison.Ordinal);
                }
                
                // Sort by row first (y), then by column (x)
                // This gives us top-left to bottom-right order
                if (index1.y != index2.y)
                    return index1.y.CompareTo(index2.y);
                else
                    return index1.x.CompareTo(index2.x);
            });
            
            // Subscribe to slot events with correct sequential indices
            for (int i = 0; i < allSlots.Count; i++)
            {
                var slot = allSlots[i];
                slot.slotIndex = i;  // Now correctly assigns 0, 1, 2, 3... in grid order
                slot.OnSlotClicked += OnSlotClicked;
                slot.OnSlotHovered += OnSlotHovered;
                slot.OnSlotUnhovered += OnSlotUnhovered;
            }
            
            Debug.Log($"Found {allSlots.Count} slots and wired up event handlers in correct grid order");
        }
        
        void CreateTestItems()
        {
            testItems = new ItemData[]
            {
                new ItemData
                {
                    itemId = 1,
                    itemName = "Health Potion",
                    maxStackSize = 10,
                    itemType = ItemType.Consumable,
                    description = "Restores 50 HP"
                },
                new ItemData
                {
                    itemId = 2,
                    itemName = "Iron Sword",
                    maxStackSize = 1,
                    itemType = ItemType.Weapon,
                    description = "A sturdy iron sword"
                },
                new ItemData
                {
                    itemId = 3,
                    itemName = "Wood",
                    maxStackSize = 99,
                    itemType = ItemType.Material,
                    description = "Basic crafting material"
                },
                new ItemData
                {
                    itemId = 4,
                    itemName = "Leather Armor",
                    maxStackSize = 1,
                    itemType = ItemType.Armor,
                    description = "Light protective armor"
                }
            };
        }
        
        private void OnSlotClicked(ItemSlotUI slot)
        {
            // Handle slot selection
            if (selectedSlot != null)
            {
                selectedSlot.SetSelected(false);
            }
            
            if (selectedSlot == slot)
            {
                selectedSlot = null;
            }
            else
            {
                selectedSlot = slot;
                slot.SetSelected(true);
            }
            
            Debug.Log($"Slot {slot.slotIndex} clicked. Selected: {selectedSlot != null}");
        }
        
        private void OnSlotHovered(ItemSlotUI slot)
        {
            if (slot.currentItem != null)
            {
                Debug.Log($"Hovering over {slot.currentItem.itemName} (Stack: {slot.stackCount})");
            }
        }
        
        private void OnSlotUnhovered(ItemSlotUI slot)
        {
            // Handle unhover if needed
        }
        
        // Methods for students to test during exercise
        public void AddTestItem(int itemIndex, int slotIndex)
        {
            if (itemIndex >= 0 && itemIndex < testItems.Length && 
                slotIndex >= 0 && slotIndex < allSlots.Count)
            {
                var item = testItems[itemIndex];
                var slot = allSlots[slotIndex];
                
                if (slot.CanAcceptItem(item))
                {
                    if (slot.IsEmpty())
                    {
                        slot.SetItem(item, 1);
                    }
                    else
                    {
                        slot.stackCount++;
                        slot.SetItem(slot.currentItem, slot.stackCount);
                    }
                }
            }
        }
        
        public void ClearSlot(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < allSlots.Count)
            {
                allSlots[slotIndex].ClearSlot();
            }
        }
        
        public void ClearAllSlots()
        {
            foreach (var slot in allSlots)
            {
                slot.ClearSlot();
            }
        }
        
        public void TestSlotStates()
        {
            // Add some test items to demonstrate different states
            if (allSlots.Count >= 4)
            {
                AddTestItem(0, 0); // Health potion
                AddTestItem(1, 1); // Iron sword
                AddTestItem(2, 2); // Wood
                AddTestItem(3, 3); // Leather armor
            }
        }
        
        public void TestStackableItems()
        {
            // Test stacking
            if (allSlots.Count >= 1)
            {
                for (int i = 0; i < 5; i++)
                {
                    AddTestItem(0, 0); // Add 5 health potions to first slot
                }
            }
        }
        
        void Update()
        {
            // Quick test keys for students
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TestSlotStates();
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TestStackableItems();
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                ClearAllSlots();
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                TestDragAndDrop();
            }
        }
        
        public void TestDragAndDrop()
        {
            // Add items to test drag and drop
            ClearAllSlots();
            
            if (allSlots.Count >= 6)
            {
                AddTestItem(0, 0); // Health potion
                AddTestItem(0, 1); // Another health potion (for stacking test)
                AddTestItem(1, 2); // Iron sword
                AddTestItem(3, 3); // Leather armor
                AddTestItem(2, 4); // Wood
                
                Debug.Log("Drag & Drop test items added. Try dragging items between slots!");
                Debug.Log("- Drag health potions together to stack");
                Debug.Log("- Drag sword onto armor to swap");
                Debug.Log("- Drag items to empty slots to move");
            }
        }
    }