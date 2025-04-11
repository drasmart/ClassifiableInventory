using System.Collections.Generic;
using UnityEngine;

#nullable enable

public class ColorTypeTest : MonoBehaviour
{
    public List<ColorTypeAsset> colors = new();
    public List<ColorTypeFilterAsset> filters = new();

    private void Start()
    {
        TestColors();
    }

    private void TestColors()
    {
        foreach(var c1 in colors)
        {
            string msg = "";
            foreach(var c2 in colors)
            {
                if (c1.IsSubtypeOf(c2))
                {
                    msg += c1.name + " is " + c2.name + "\n";
                }
            }
            foreach (var c2 in filters)
            {
                bool passed = c2.Filter(c1);
                msg += c1.name + (passed ? " is " : " is not ") + c2.name + "\n";
            }
            Debug.Log(msg);
        }
    }
}
