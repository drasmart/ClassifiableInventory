using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDisplay : MonoBehaviour
{
    public Image iconImage;
    public Text countText;

    public void UpdateUI(DraggableModel draggableModel)
    {
        var item = draggableModel as Item;
        if (iconImage)
        {
            iconImage.sprite = item?.itemType?.sprite;
        }
        if (countText)
        {
            var count = item?.count ?? 0;
            countText.text = count.ToString();
            countText.gameObject.SetActive(count > 1);
        }
    }
}
