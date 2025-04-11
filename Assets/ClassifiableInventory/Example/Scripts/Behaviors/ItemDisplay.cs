using UnityEngine;
using UnityEngine.UI;

public class ItemDisplay : MonoBehaviour
{
    public Image iconImage;
    public Text countText;

    public Color shadowColor = Color.white;

    public void UpdateUI(IDraggableModel draggableModel, bool isShadow)
    {
        var item = draggableModel as Item;
        if (iconImage)
        {
            iconImage.sprite = item?.itemType?.sprite;
            iconImage.color = isShadow ? shadowColor : Color.white;
        }
        if (countText)
        {
            var count = item?.count ?? 0;
            countText.text = count.ToString();
            countText.gameObject.SetActive(count > 1);
        }
    }
}
