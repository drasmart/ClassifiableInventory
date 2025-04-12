using Classification;

#nullable enable

public class ColorsTestCase
{
    public ColorTypeAsset Color;
    public Classifiable.TypeFilter Filter;
    public bool Expected;

    public ColorsTestCase(ColorTypeAsset color, Classifiable.TypeFilter filter, bool expected)
    {
        Color = color;
        Filter = filter;
        Expected = expected;
    }

    public override string ToString()
    {
        return $"{Color.name} {(Expected ? "is" : "is not")} {Filter.name}";
    }
}
