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

    private void Start()
    {
        var classifiable = GetComponent<Classifiable>();
        var slotPrime = slotPrefab?.GetComponent<Slot>();
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
        for(int i = 0; i < count; i++)
        {
            var nextSlot = Instantiate(slotPrime);
            var nextClassifiable = nextSlot.GetComponent<Classifiable>();

            nextSlot.gameObject.SetActive(false);

            nextSlot.targetScript = targetScript;
            nextSlot.property = property;
            nextSlot.propertyType = propertyType;
            nextSlot.index = i;
            nextClassifiable.AddAllFrom(classifiable);
            nextSlot.transform.SetParent(transform, false);

            nextSlot.gameObject.SetActive(true);

            spawnedSlots.Add(nextSlot);
        }
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
}
