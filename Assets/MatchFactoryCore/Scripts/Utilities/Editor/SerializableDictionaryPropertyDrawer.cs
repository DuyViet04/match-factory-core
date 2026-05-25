using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class SerializableDictionaryPropertyDrawerBase : PropertyDrawer
{
    const string KeysFieldName = "m_keys";
    const string ValuesFieldName = "m_values";
    protected const float IndentWidth = 15f;

    static GUIContent iconPlus = IconContent("Toolbar Plus", "Add entry");
    static GUIContent iconMinus = IconContent("Toolbar Minus", "Remove entry");

    static GUIContent warningIconConflict =
        IconContent("console.warnicon.sml", "Conflicting key, this entry will be lost");

    static GUIContent warningIconOther = IconContent("console.infoicon.sml", "Conflicting key");
    static GUIContent warningIconNull = IconContent("console.warnicon.sml", "Null key, this entry will be lost");
    static GUIStyle buttonStyle = GUIStyle.none;
    static GUIContent tempContent = new GUIContent();

    class ConflictState
    {
        public object conflictKey = null;
        public object conflictValue = null;
        public int conflictIndex = -1;
        public int conflictOtherIndex = -1;
        public bool conflictKeyPropertyExpanded = false;
        public bool conflictValuePropertyExpanded = false;
        public float conflictLineHeight = 0f;
    }

    struct PropertyIdentity
    {
        public PropertyIdentity(SerializedProperty property)
        {
            this.instance = property.serializedObject.targetObject;
            this.propertyPath = property.propertyPath;
        }

        public Object instance;
        public string propertyPath;
    }

    static Dictionary<PropertyIdentity, ConflictState> s_conflictStateDict =
        new Dictionary<PropertyIdentity, ConflictState>();

    enum Action
    {
        None,
        Add,
        Remove
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        label = EditorGUI.BeginProperty(position, label, property);

        Action buttonAction = Action.None;
        int buttonActionIndex = 0;

        var keyArrayProperty = property.FindPropertyRelative(KeysFieldName);
        var valueArrayProperty = property.FindPropertyRelative(ValuesFieldName);

        ConflictState conflictState = GetConflictState(property);

        if (conflictState.conflictIndex != -1)
        {
            keyArrayProperty.InsertArrayElementAtIndex(conflictState.conflictIndex);
            var keyProperty = keyArrayProperty.GetArrayElementAtIndex(conflictState.conflictIndex);
            SetPropertyValue(keyProperty, conflictState.conflictKey);
            keyProperty.isExpanded = conflictState.conflictKeyPropertyExpanded;

            valueArrayProperty.InsertArrayElementAtIndex(conflictState.conflictIndex);
            var valueProperty = valueArrayProperty.GetArrayElementAtIndex(conflictState.conflictIndex);
            SetPropertyValue(valueProperty, conflictState.conflictValue);
            valueProperty.isExpanded = conflictState.conflictValuePropertyExpanded;
        }

        var buttonWidth = buttonStyle.CalcSize(iconPlus).x;

        var labelPosition = position;
        labelPosition.height = EditorGUIUtility.singleLineHeight;
        if (property.isExpanded)
            labelPosition.xMax -= buttonStyle.CalcSize(iconPlus).x;

        EditorGUI.PropertyField(labelPosition, property, label, false);
        if (property.isExpanded)
        {
            var buttonPosition = position;
            buttonPosition.xMin = buttonPosition.xMax - buttonWidth;
            buttonPosition.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.BeginDisabledGroup(conflictState.conflictIndex != -1);
            if (GUI.Button(buttonPosition, iconPlus, buttonStyle))
            {
                buttonAction = Action.Add;
                buttonActionIndex = keyArrayProperty.arraySize;
            }

            EditorGUI.EndDisabledGroup();

            EditorGUI.indentLevel++;
            var linePosition = position;
            linePosition.y += EditorGUIUtility.singleLineHeight;
            linePosition.xMax -= buttonWidth;

            foreach (var entry in EnumerateEntries(keyArrayProperty, valueArrayProperty))
            {
                var keyProperty = entry.keyProperty;
                var valueProperty = entry.valueProperty;
                int i = entry.index;

                float lineHeight = DrawKeyValueLine(keyProperty, valueProperty, linePosition, i);

                var buttonRect = linePosition;
                buttonRect.x = linePosition.xMax;
                buttonRect.width = buttonWidth;
                buttonRect.height = EditorGUIUtility.singleLineHeight;

                if (GUI.Button(buttonRect, iconMinus, buttonStyle))
                {
                    buttonAction = Action.Remove;
                    buttonActionIndex = i;
                }

                if (i == conflictState.conflictIndex && conflictState.conflictOtherIndex == -1)
                {
                    var iconPosition = linePosition;
                    iconPosition.x = iconPosition.xMax;
                    iconPosition.width = buttonWidth;
                    GUI.Label(iconPosition, conflictState.conflictKey == null ? warningIconNull : warningIconConflict);
                }
                else if (i == conflictState.conflictOtherIndex)
                {
                    var iconPosition = linePosition;
                    iconPosition.x = iconPosition.xMax;
                    iconPosition.width = buttonWidth;
                    GUI.Label(iconPosition, warningIconOther);
                }

                linePosition.y += lineHeight;
            }

            EditorGUI.indentLevel--;
        }

        if (buttonAction == Action.Add)
        {
            keyArrayProperty.InsertArrayElementAtIndex(buttonActionIndex);
            valueArrayProperty.InsertArrayElementAtIndex(buttonActionIndex);
        }
        else if (buttonAction == Action.Remove)
        {
            DeleteArrayElementAtIndex(keyArrayProperty, buttonActionIndex);
            DeleteArrayElementAtIndex(valueArrayProperty, buttonActionIndex);
        }

        conflictState.conflictKey = null;
        conflictState.conflictValue = null;
        conflictState.conflictIndex = -1;
        conflictState.conflictOtherIndex = -1;
        conflictState.conflictLineHeight = 0f;
        conflictState.conflictKeyPropertyExpanded = false;
        conflictState.conflictValuePropertyExpanded = false;

        foreach (var entry1 in EnumerateEntries(keyArrayProperty, valueArrayProperty))
        {
            var keyProperty1 = entry1.keyProperty;
            int i = entry1.index;
            object keyProperty1Value = GetPropertyValue(keyProperty1);

            if (keyProperty1Value == null)
            {
                var valueProperty1 = entry1.valueProperty;
                SaveConflict(keyProperty1, valueProperty1, i, -1, conflictState);
                DeleteArrayElementAtIndex(keyArrayProperty, i);
                DeleteArrayElementAtIndex(valueArrayProperty, i);
                break;
            }


            foreach (var entry2 in EnumerateEntries(keyArrayProperty, valueArrayProperty, i + 1))
            {
                var keyProperty2 = entry2.keyProperty;
                int j = entry2.index;

                if (ComparePropertyValues(keyProperty1Value, GetPropertyValue(keyProperty2)))
                {
                    var valueProperty2 = entry2.valueProperty;
                    SaveConflict(keyProperty2, valueProperty2, j, i, conflictState);
                    DeleteArrayElementAtIndex(keyArrayProperty, j);
                    DeleteArrayElementAtIndex(valueArrayProperty, j);
                    break;
                }
            }

            if (conflictState.conflictIndex != -1)
                break;
        }

        EditorGUI.EndProperty();
    }

    static float DrawKeyValueLine(SerializedProperty keyProperty, SerializedProperty valueProperty, Rect linePosition,
        int index)
    {
        bool keyCanBeExpanded = CanPropertyBeExpanded(keyProperty);

        if (keyCanBeExpanded)
        {
            var keyLabel = "Key " + index.ToString();
            var valueLabel = "Value " + index.ToString();
            return DrawKeyValueLineExpand(keyProperty, valueProperty, linePosition, keyLabel, valueLabel);
        }
        else
        {
            var keyLabel = tempContent;
            keyLabel.text = "";
            return DrawKeyValueLineSimple(keyProperty, valueProperty, linePosition, keyLabel);
        }
    }

    static float DrawKeyValueLineSimple(SerializedProperty keyProperty, SerializedProperty valueProperty,
        Rect linePosition, GUIContent keyLabel)
    {
        float labelWidth = EditorGUIUtility.labelWidth;
        float labelWidthRelative = labelWidth / linePosition.width;

        float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
        var keyPosition = linePosition;
        keyPosition.height = keyPropertyHeight;
        keyPosition.width = labelWidth - IndentWidth;
        EditorGUIUtility.labelWidth = keyPosition.width / 2;
        EditorGUI.PropertyField(keyPosition, keyProperty, keyLabel, true);

        float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);
        var valuePosition = linePosition;
        valuePosition.height = valuePropertyHeight;
        valuePosition.xMin += labelWidth;
        EditorGUIUtility.labelWidth = valuePosition.width * labelWidthRelative;
        EditorGUI.PropertyField(valuePosition, valueProperty, GUIContent.none, true);

        EditorGUIUtility.labelWidth = labelWidth;

        return Mathf.Max(keyPropertyHeight, valuePropertyHeight);
    }

    static float DrawKeyValueLineExpand(SerializedProperty keyProperty, SerializedProperty valueProperty,
        Rect linePosition, string keyLabel, string valueLabel)
    {
        float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
        var keyPosition = linePosition;
        keyPosition.height = keyPropertyHeight;
        EditorGUI.PropertyField(keyPosition, keyProperty, new GUIContent(keyLabel), true);

        float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);
        var valuePosition = linePosition;
        valuePosition.y += keyPropertyHeight;
        valuePosition.height = valuePropertyHeight;
        EditorGUI.PropertyField(valuePosition, valueProperty, new GUIContent(valueLabel), true);

        return keyPropertyHeight + valuePropertyHeight;
    }

    static bool CanPropertyBeExpanded(SerializedProperty property)
    {
        switch (property.propertyType)
        {
            case SerializedPropertyType.Generic:
            case SerializedPropertyType.Vector4:
            case SerializedPropertyType.Quaternion:
                return true;
            default:
                return false;
        }
    }

    static void SaveConflict(SerializedProperty keyProperty, SerializedProperty valueProperty, int index,
        int otherIndex, ConflictState conflictState)
    {
        conflictState.conflictKey = GetPropertyValue(keyProperty);
        conflictState.conflictValue = GetPropertyValue(valueProperty);
        conflictState.conflictIndex = index;
        conflictState.conflictOtherIndex = otherIndex;
        float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
        float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);
        conflictState.conflictLineHeight = Mathf.Max(keyPropertyHeight, valuePropertyHeight);
        conflictState.conflictKeyPropertyExpanded = keyProperty.isExpanded;
        conflictState.conflictValuePropertyExpanded = valueProperty.isExpanded;
    }

    static ConflictState GetConflictState(SerializedProperty property)
    {
        ConflictState conflictState;
        var identity = new PropertyIdentity(property);
        if (!s_conflictStateDict.TryGetValue(identity, out conflictState))
        {
            conflictState = new ConflictState();
            s_conflictStateDict.Add(identity, conflictState);
        }

        return conflictState;
    }

    static Dictionary<SerializedPropertyType, System.Func<SerializedProperty, object>> s_propertyValueGetDict =
        new Dictionary<SerializedPropertyType, System.Func<SerializedProperty, object>>
        {
            { SerializedPropertyType.Integer, p => p.intValue },
            { SerializedPropertyType.Boolean, p => p.boolValue },
            { SerializedPropertyType.Float, p => p.floatValue },
            { SerializedPropertyType.String, p => p.stringValue },
            { SerializedPropertyType.Color, p => p.colorValue },
            { SerializedPropertyType.ObjectReference, p => p.objectReferenceValue },
            { SerializedPropertyType.LayerMask, p => (LayerMask)p.intValue },
            { SerializedPropertyType.Enum, p => p.enumValueIndex },
            { SerializedPropertyType.Vector2, p => p.vector2Value },
            { SerializedPropertyType.Vector3, p => p.vector3Value },
            { SerializedPropertyType.Vector4, p => p.vector4Value },
            { SerializedPropertyType.Rect, p => p.rectValue },
            { SerializedPropertyType.ArraySize, p => p.intValue },
            { SerializedPropertyType.Character, p => (char)p.intValue },
            { SerializedPropertyType.AnimationCurve, p => p.animationCurveValue },
            { SerializedPropertyType.Bounds, p => p.boundsValue },
            { SerializedPropertyType.Gradient, p => GetGradientValue(p) },
            { SerializedPropertyType.Quaternion, p => p.quaternionValue },
        };

    static Gradient GetGradientValue(SerializedProperty sp)
    {
        System.Reflection.PropertyInfo gradientValuePropertyInfo =
            typeof(SerializedProperty).GetProperty("gradientValue",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
        if (gradientValuePropertyInfo != null)
            return (Gradient)gradientValuePropertyInfo.GetValue(sp, null);
        return null;
    }

    static object GetPropertyValue(SerializedProperty p)
    {
        System.Func<SerializedProperty, object> getter;
        if (s_propertyValueGetDict.TryGetValue(p.propertyType, out getter))
        {
            return getter(p);
        }

        return null;
    }

    static Dictionary<SerializedPropertyType, System.Action<SerializedProperty, object>> s_propertyValueSetDict =
        new Dictionary<SerializedPropertyType, System.Action<SerializedProperty, object>>
        {
            { SerializedPropertyType.Integer, (p, v) => p.intValue = (int)v },
            { SerializedPropertyType.Boolean, (p, v) => p.boolValue = (bool)v },
            { SerializedPropertyType.Float, (p, v) => p.floatValue = (float)v },
            { SerializedPropertyType.String, (p, v) => p.stringValue = (string)v },
            { SerializedPropertyType.Color, (p, v) => p.colorValue = (Color)v },
            { SerializedPropertyType.ObjectReference, (p, v) => p.objectReferenceValue = (Object)v },
            { SerializedPropertyType.LayerMask, (p, v) => p.intValue = (int)v },
            { SerializedPropertyType.Enum, (p, v) => p.enumValueIndex = (int)v },
            { SerializedPropertyType.Vector2, (p, v) => p.vector2Value = (Vector2)v },
            { SerializedPropertyType.Vector3, (p, v) => p.vector3Value = (Vector3)v },
            { SerializedPropertyType.Vector4, (p, v) => p.vector4Value = (Vector4)v },
            { SerializedPropertyType.Rect, (p, v) => p.rectValue = (Rect)v },
            { SerializedPropertyType.Character, (p, v) => p.intValue = (char)v },
            { SerializedPropertyType.AnimationCurve, (p, v) => p.animationCurveValue = (AnimationCurve)v },
            { SerializedPropertyType.Bounds, (p, v) => p.boundsValue = (Bounds)v },
            { SerializedPropertyType.Quaternion, (p, v) => p.quaternionValue = (Quaternion)v },
        };

    static void SetPropertyValue(SerializedProperty p, object v)
    {
        System.Action<SerializedProperty, object> setter;
        if (s_propertyValueSetDict.TryGetValue(p.propertyType, out setter))
        {
            setter(p, v);
        }
    }

    static bool ComparePropertyValues(object value1, object value2)
    {
        if (value1 is IEqualityComparer<object>)
            return ((IEqualityComparer<object>)value1).Equals(value2);
        return object.Equals(value1, value2);
    }

    struct EnumerationEntry
    {
        public SerializedProperty keyProperty;
        public SerializedProperty valueProperty;
        public int index;

        public EnumerationEntry(SerializedProperty keyProperty, SerializedProperty valueProperty, int index)
        {
            this.keyProperty = keyProperty;
            this.valueProperty = valueProperty;
            this.index = index;
        }
    }

    static IEnumerable<EnumerationEntry> EnumerateEntries(SerializedProperty keyArrayProperty,
        SerializedProperty valueArrayProperty, int startIndex = 0)
    {
        if (keyArrayProperty.arraySize > startIndex)
        {
            int index = startIndex;
            var keyProperty = keyArrayProperty.GetArrayElementAtIndex(startIndex);
            var valueProperty = valueArrayProperty.GetArrayElementAtIndex(startIndex);
            var endProperty = keyArrayProperty.GetEndProperty();

            do
            {
                yield return new EnumerationEntry(keyProperty, valueProperty, index);
                index++;
            } while (keyProperty.Next(false) && valueProperty.Next(false) &&
                     !SerializedProperty.EqualContents(keyProperty, endProperty));
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float propertyHeight = EditorGUIUtility.singleLineHeight;

        if (property.isExpanded)
        {
            var keysProperty = property.FindPropertyRelative(KeysFieldName);
            var valuesProperty = property.FindPropertyRelative(ValuesFieldName);

            foreach (var entry in EnumerateEntries(keysProperty, valuesProperty))
            {
                var keyProperty = entry.keyProperty;
                var valueProperty = entry.valueProperty;
                float keyPropertyHeight = EditorGUI.GetPropertyHeight(keyProperty);
                float valuePropertyHeight = EditorGUI.GetPropertyHeight(valueProperty);
                float lineHeight = Mathf.Max(keyPropertyHeight, valuePropertyHeight);
                propertyHeight += lineHeight;
            }

            ConflictState conflictState = GetConflictState(property);

            if (conflictState.conflictIndex != -1)
            {
                propertyHeight += conflictState.conflictLineHeight;
            }
        }

        return propertyHeight;
    }

    static GUIContent IconContent(string name, string tooltip)
    {
        var builtinIcon = EditorGUIUtility.IconContent(name);
        return new GUIContent(builtinIcon.image, tooltip);
    }

    static void DeleteArrayElementAtIndex(SerializedProperty arrayProperty, int index)
    {
        var property = arrayProperty.GetArrayElementAtIndex(index);
        // if(arrayProperty.arrayElementType.StartsWith("PPtr<$"))
        if (property.propertyType == SerializedPropertyType.ObjectReference)
        {
            property.objectReferenceValue = null;
        }

        arrayProperty.DeleteArrayElementAtIndex(index);
    }
}

[CustomPropertyDrawer(typeof(SerializableDictionary<,>))]
[CustomPropertyDrawer(typeof(SerializableDictionary<,,>))]
public class SerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawerBase
{
}