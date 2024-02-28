using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(CenteredSlider)), CanEditMultipleObjects]
public class CenteredSliderEditor : SliderEditor
{
    CenteredSlider component;

    SerializedProperty valueTMP;
    SerializedProperty numDecimalDigits;
    SerializedProperty snapToDecimal;
    SerializedProperty color;

    protected override void OnEnable()
    {
        base.OnEnable();

        component = (CenteredSlider)target;

        valueTMP = serializedObject.FindProperty("valueTMP");
        numDecimalDigits = serializedObject.FindProperty("numDecimalDigits");
        snapToDecimal = serializedObject.FindProperty("snapToDecimal");
        color = serializedObject.FindProperty("color");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();
        EditorGUILayout.PropertyField(valueTMP);
        EditorGUILayout.PropertyField(snapToDecimal);
        EditorGUILayout.PropertyField(numDecimalDigits);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(color);
        EditorGUILayout.Space();
        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
        {
            component.ApplyColor();
        }
    }
}
