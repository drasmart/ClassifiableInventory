using UnityEditor;
using UnityEngine;

namespace drasmart.Classification.Colors.Tests
{
    [CustomPropertyDrawer(typeof(ColorsTestCases.TestCaseData.ColorExpectation))]
    [CustomPropertyDrawer(typeof(ColorsTestCases.TestCaseData.FilterExpectation))]
    public class TestCaseFilterDrawer : PropertyDrawer
    {
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // // Draw label
            // position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            //
            // // Don't make child fields be indented
            // var indent = EditorGUI.indentLevel;
            // EditorGUI.indentLevel = 0;

            // Calculate rects
            var checkBoxRect = new Rect(position.x, position.y, 15, position.height);
            var assetRect = new Rect(position.x + 17, position.y, position.width - 17, position.height);

            // Draw fields - pass GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(
                checkBoxRect,
                property.FindPropertyRelative("expected"),
                GUIContent.none);
            EditorGUI.PropertyField(
                assetRect,
                property.FindPropertyRelative("color") ?? property.FindPropertyRelative("filter"),
                GUIContent.none);

            // // Set indent back to what it was
            // EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}
