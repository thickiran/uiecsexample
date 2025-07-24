using UnityEngine;

/// <summary>
/// Shared types used across all tutorial exercises
/// This prevents duplicate definitions when namespaces are removed
/// </summary>

[System.Serializable]
public class ItemData
{
    public int itemId;
    public string itemName;
    public Sprite icon;
    public int maxStackSize = 1;
    public ItemType itemType;
    public string description;
}

public enum ItemType
{
    Weapon,
    Armor,
    Consumable,
    Material,
    Quest
}

public enum SlotState
{
    Empty,
    Occupied,
    Highlighted
}