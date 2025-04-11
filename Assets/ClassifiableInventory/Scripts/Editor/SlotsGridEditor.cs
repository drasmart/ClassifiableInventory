using UnityEditor;

#nullable enable

[CustomEditor(typeof(SlotsGrid))]
[CanEditMultipleObjects]
public class SlotsGridEditor : BaseSlotEditor
{
    private SerializedProperty? slotPrefabProp;

    protected override void OnEnable()
    {
        base.OnEnable();
        slotPrefabProp = serializedObject.FindProperty("slotPrefab");
    }

    protected override void OnSlotInspection()
    {
        EditorGUILayout.PropertyField(slotPrefabProp);
        base.OnSlotInspection();
    }

    protected override PropertyPickHandler? PickPlainField(string fieldName, System.Reflection.FieldInfo field) => null;
}
