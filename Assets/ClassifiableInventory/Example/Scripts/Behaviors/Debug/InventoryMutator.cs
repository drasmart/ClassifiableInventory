using UnityEngine;

public class InventoryMutator : MonoBehaviour
{
    public PlayerInventory inventory;
    public ItemType itemType;

    void Update()
    {
        if (inventory == null || itemType == null)
        {
            enabled = false;
            return;
        }
        bool mutated = false;
        if (Input.GetKeyDown(KeyCode.Q))
        {
            inventory.backpack[0] = new Item(itemType);
            mutated = true;
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            inventory.backpack[1] = null;
            mutated = true;
        }
        if (mutated)
        {
            inventory.onDataUpdate.Invoke();
        }
    }
}
