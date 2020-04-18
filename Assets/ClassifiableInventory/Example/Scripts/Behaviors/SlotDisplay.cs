using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotDisplay : MonoBehaviour
{
    public Image backgroundImage;
    public GameObject nonAcceptingOverlay;

    public BackgroundColors backgroundColors = new BackgroundColors();

    private void Awake()
    {
        backgroundImage.color = backgroundColors.passive;
        nonAcceptingOverlay.SetActive(false);
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
