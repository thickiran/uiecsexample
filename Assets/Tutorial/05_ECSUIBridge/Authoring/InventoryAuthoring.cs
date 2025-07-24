using Unity.Entities;
using UnityEngine;

public class InventoryAuthoring : MonoBehaviour
{
    [Header("Inventory Configuration")]
    public int maxSlots = 50;

    class InventoryBaker : Baker<InventoryAuthoring>
    {
        public override void Bake(InventoryAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            
            AddComponent(entity, new InventoryComponent
            {
                maxSlots = authoring.maxSlots,
                currentItemCount = 0
            });
            
            AddComponent<PlayerInventoryTag>(entity);
            
            var buffer = AddBuffer<InventorySlot>(entity);
            for (int i = 0; i < authoring.maxSlots; i++)
            {
                buffer.Add(InventorySlot.Empty);
            }
        }
    }
}