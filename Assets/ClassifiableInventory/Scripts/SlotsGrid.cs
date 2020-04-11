using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Classification;

[RequireComponent(typeof(GridLayoutGroup))]
public class SlotsGrid : BaseSlot
{
    public GameObject slotPrefab;

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
            (list) => count = list.Count,
            (array) => count = array.Length,
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
        }
    }
}
