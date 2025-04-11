using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Classification;
using UnityEngine.Assertions;

#nullable enable

[RequireComponent(typeof(GridLayoutGroup))]
public class SlotsGrid : BaseSlot
{
    public GameObject? slotPrefab;

    private List<Slot> spawnedSlots = new();
    private Classifiable? classifiable;
    private Slot? slotPrime;

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
        Assert.IsNotNull(slotPrime);
        Assert.IsNotNull(classifiable);
        if (!slotPrime || !classifiable)
        {
            return;
        }
        int count = 0;
        GetAccess(targetScript, property, propertyType,
            null,
            (list, _) => count = list.Count,
            (array, _) => count = array.Length,
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
                var nextSlot = Instantiate(slotPrime, transform, false);
                var nextClassifiable = nextSlot.GetComponent<Classifiable>();

                nextSlot.gameObject.SetActive(false);

                SetupSlot(nextSlot, i);
                nextClassifiable.Clear();
                nextClassifiable.AddAllFrom(classifiable);

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
    public override Slot? FindFreeSlotFor(IDraggableModel model)
    {
        System.Type? storageType = null;
        IList? storage = null;
        GetAccess(targetScript, property, propertyType,
            null,
            (list, dataType) => { storageType = dataType; storage = list; },
            (array, dataType) => { storageType = dataType; storage = array; },
            null);
        int index = -1;
        if (storage != null)
        {
            for(int i = 0, n = storage.Count; i < n; i++)
            {
                var nextObj = storage[i];
                if ((nextObj == null) || ((nextObj as IDraggableModel)?.IsNull == true))
                {
                    index = i;
                    break;
                }
            }
        }
        if (!(index != -1 && storageType is not null && storageType.IsAssignableFrom(model.GetType())))
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
