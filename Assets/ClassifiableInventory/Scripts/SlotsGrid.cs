using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Classification;

[RequireComponent(typeof(GridLayoutGroup))]
public class SlotsGrid : BaseSlot
{
    public GameObject slotPrefab;

    private List<Slot> spawnedSlots = new List<Slot>();
    private Classifiable classifiable;
    private Slot slotPrime;

    private void Awake()
    {
        classifiable = GetComponent<Classifiable>();
        slotPrime = slotPrefab?.GetComponent<Slot>();
    }
    private void Start()
    {
        UpdateAllSlots();
    }
    public override void UpdateAllSlots()
    {
        if (slotPrime == null || classifiable == null)
        {
            return;
        }
        int count = 0;
        GetAccess(targetScript, property, propertyType,
            null,
            (list, dataType) => count = list.Count,
            (array, dataType) => count = array.Length,
            null);
        for (int i = 0; i < count; i++)
        {
            if (i < spawnedSlots.Count)
            {
                var nextSlot = spawnedSlots[i];
                var nextClassifiable = nextSlot.GetComponent<Classifiable>();

                SetupSlot(nextSlot, i);

                nextClassifiable.Clear();
                nextClassifiable.AddAllFrom(classifiable);

                nextSlot.gameObject.SetActive(true);

                DragManager.Instance.UpdateSlot(nextSlot, nextSlot.enabled);
            }
            else
            {
                var nextSlot = Instantiate(slotPrime);
                var nextClassifiable = nextSlot.GetComponent<Classifiable>();

                nextSlot.gameObject.SetActive(false);

                SetupSlot(nextSlot, i);
                nextClassifiable.Clear();
                nextClassifiable.AddAllFrom(classifiable);
                nextSlot.transform.SetParent(transform, false);

                nextSlot.gameObject.SetActive(true);

                spawnedSlots.Add(nextSlot);
            }
        }
        for(int i = count; i < spawnedSlots.Count; i++)
        {
            spawnedSlots[i].gameObject.SetActive(false);
        }
    }
    private void SetupSlot(Slot slot, int index)
    {
        slot.targetScript = targetScript;
        slot.property = property;
        slot.propertyType = propertyType;
        slot.index = index;
        slot.fallbackSlotContainer = fallbackSlotContainer;
        slot.keepShadowWhileDragging = keepShadowWhileDragging;
        slot.isReadOnly = isReadOnly;
    }
    public override Slot FindFreeSlotFor(DraggableModel model)
    {
        System.Type storageType = null;
        IList storage = null;
        GetAccess(targetScript, property, propertyType,
            null,
            (list, dataType) => { storageType = dataType; storage = list; },
            (array, dataType) => { storageType = dataType; storage = array as IList; },
            null);
        int index = -1;
        if (storage != null)
        {
            for(int i = 0, n = storage.Count; i < n; i++)
            {
                var nextObj = storage[i];
                if ((nextObj == null) || ((nextObj as DraggableModel)?.IsNull == true))
                {
                    index = i;
                    break;
                }
            }
        }
        if (!(index != -1 && storageType != null && storageType.IsAssignableFrom(model.GetType())))
        {
            return null;
        }
        if (index < spawnedSlots.Count)
        {
            return spawnedSlots[index];
        }
        return null;
    }
    public override IEnumerable<Slot> GetAllSlots()
    {
        return spawnedSlots.FindAll((slot) => slot.gameObject.activeSelf);
    }
}
