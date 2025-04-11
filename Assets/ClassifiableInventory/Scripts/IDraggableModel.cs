using Classification;

#nullable enable

public interface IDraggableModel
{
    Classifiable.TypeAsset[] Classes { get; }
    bool IsNull { get; }
}
