using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class ItemDataAuthoring : MonoBehaviour
{
    [Header("Item Configuration")]
    public int itemId;
    public string itemName;
    public int maxStackSize = 1;
    public ECSItemType itemType = ECSItemType.None;
    public int value = 0;

    class ItemDataBaker : Baker<ItemDataAuthoring>
    {
        public override void Bake(ItemDataAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            
            AddComponent(entity, new ItemDataComponent
            {
                itemId = authoring.itemId,
                itemName = authoring.itemName,
                maxStackSize = authoring.maxStackSize,
                itemType = authoring.itemType,
                value = authoring.value
            });
        }
    }
}