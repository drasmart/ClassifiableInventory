using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

#nullable enable

public class SlotDisplay : MonoBehaviour
{
    public Image? backgroundImage;
    public GameObject? nonAcceptingOverlay;

    public BackgroundColors backgroundColors = new BackgroundColors();

    private void Awake()
    {
        Assert.IsNotNull(backgroundImage);
        Assert.IsNotNull(nonAcceptingOverlay);
        backgroundImage!.color = backgroundColors.passive;
        nonAcceptingOverlay!.SetActive(false);
    }

    [System.Serializable]
    public class BackgroundColors
    {
        public Color passive = Color.white;
        public Color normal = Color.white;
        public Color source = Color.cyan;
        public Color swapSource = Color.magenta;
        public Color dropDestination = Color.green;
        public Color swapDestination = Color.yellow;
        public Color swapFallback = Color.red;
        public Color invalidDestination = Color.red;
        public Color invalidSource = Color.red;
    }
}
