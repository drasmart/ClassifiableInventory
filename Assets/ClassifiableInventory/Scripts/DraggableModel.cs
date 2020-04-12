using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Classification;

public interface DraggableModel
{
    Classifiable.TypeAsset[] classes { get; }
    bool IsNull { get; }
}
