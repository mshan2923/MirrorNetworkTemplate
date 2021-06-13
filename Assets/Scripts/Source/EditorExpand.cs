using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace Expand
{
    public static class EditorExpand
    {
        public static Rect NextLine(Rect Pos, Rect DrawRect, int LineHeight = 20)
        {
            return new Rect(Pos.x, (DrawRect.y + LineHeight), Pos.width, LineHeight);
        }
        public static Rect GetNextSpace(Rect Pos, Rect DrawRect, float width, float PreWidth = 0, bool LineFirst = false, int LineHeight = 20)
        {
            if (LineFirst)
            {
                return new Rect(Pos.x, DrawRect.y, width, LineHeight);
            }
            else
            {
                return new Rect(DrawRect.x + PreWidth, DrawRect.y, width, LineHeight);
            }
        }
        public static Rect RateRect(Rect Pos, Rect DrawRect, int index, int Amount, float Offset = 0, int LineHeight = 20)
        {
            float size = (Pos.width - Offset) / Amount;
            return new Rect((Pos.x + Offset + size * index), DrawRect.y, size, LineHeight);
        }
        public static Rect RateRect(Rect Pos, Rect DrawRect, int index, int Amount, float Height, float Offset = 0)
        {
            float size = (Pos.width - Offset) / Amount;
            return new Rect((Pos.x + Offset + size * index), DrawRect.y, size, Height);
        }

        public static Rect ResizedLabel(Rect Pos, Rect DrawRect, string Text)
        {
            EditorGUI.indentLevel = 0;
            float Size = GUI.skin.label.CalcSize(new GUIContent(Text)).x;

            Rect LdrawRect = new Rect(DrawRect.x, DrawRect.y, Size, DrawRect.height);

            EditorGUI.LabelField(LdrawRect, Text);
            return new Rect(LdrawRect.x + Size, LdrawRect.y, Size, LdrawRect.height);
        }

        public static Rect InputField(Rect pos, Rect DrawRect, SerializedProperty property, string Text, int LineAmount, float Space = 0, bool ResizedText = true)
        {
            EditorGUI.indentLevel = 0;
            Rect LRect = new Rect(DrawRect.x + Space, DrawRect.y, DrawRect.width - Space, DrawRect.height);

            float InputWidth = 0;
            if (ResizedText)
            {
                LRect = ResizedLabel(pos, LRect, Text);

                InputWidth = Mathf.Max(0, (((pos.width) / LineAmount) - LRect.width));
            }
            else
            {
                Rect TextRect = new Rect(LRect.x, LRect.y, ((pos.width) / LineAmount * 0.5f), LRect.height);
                EditorGUI.LabelField(TextRect, Text);

                InputWidth = TextRect.width;
                LRect = new Rect(TextRect.x + TextRect.width, LRect.y, TextRect.width, LRect.height);
            }

            LRect = GetNextSpace(pos, LRect, InputWidth);
            switch (VariableCollection.ConvertTypeEnum(PropertyTypeToType(property.type).Name))
            {
                case TypeEnum.Generic:
                    EditorGUI.LabelField(LRect, "Not Surpport");
                    break;
                case TypeEnum.Integer:
                    property.intValue = EditorGUI.IntField(LRect, property.intValue);
                    break;
                case TypeEnum.Boolean:
                    property.boolValue = EditorGUI.Toggle(LRect, property.boolValue);
                    break;
                case TypeEnum.Float:
                    property.floatValue = EditorGUI.FloatField(LRect, property.floatValue);
                    break;
                case TypeEnum.String:
                    property.stringValue = EditorGUI.TextField(LRect, property.stringValue);
                    break;
                case TypeEnum.Color:
                    property.colorValue = EditorGUI.ColorField(LRect, property.colorValue);
                    break;
                case TypeEnum.ObjectReference:
                    property.objectReferenceValue = EditorGUI.ObjectField(LRect, property.objectReferenceValue, typeof(GameObject), true);
                    break;
                case TypeEnum.LayerMask:
                    {
                        if (property.type == "int" || property.type == "LayerMask")
                        {
                            property.intValue = LayerMaskField(LRect, "", property.intValue);
                        }
                        else
                        {
                            EditorGUI.LabelField(LRect, "Surpport type is 'int' OR 'LayerMask'  - " + property.type);
                        }
                        break;
                    }
                case TypeEnum.Enum:
                    EditorGUI.LabelField(LRect, "Not Surpport");
                    break;
                case TypeEnum.Vector2:
                    property.vector2Value = EditorGUI.Vector2Field(LRect, "", property.vector2Value);
                    break;
                case TypeEnum.Vector3:
                    property.vector3Value = EditorGUI.Vector3Field(LRect, "", property.vector3Value);
                    break;
                case TypeEnum.Vector4:
                    property.vector4Value = EditorGUI.Vector4Field(LRect, "", property.vector4Value);
                    break;
                case TypeEnum.Rect:
                    {

                    }
                    break;
                case TypeEnum.ArraySize:
                    EditorGUI.LabelField(LRect, "Not Surpport");
                    break;
                case TypeEnum.Character:
                    EditorGUI.LabelField(LRect, "Not Surpport");
                    break;
                case TypeEnum.AnimationCurve:
                    property.animationCurveValue = EditorGUI.CurveField(LRect, property.animationCurveValue);
                    break;
                case TypeEnum.Bounds:
                    {

                    }
                    break;
                case TypeEnum.Gradient:
                    //property.animationCurveValue = EditorGUI.GradientField(,);
                    EditorGUI.LabelField(LRect, "Not Surpport");
                    break;
                case TypeEnum.Quaternion:
                    {
                        Rect LinputRec = new Rect(LRect.x + 50, LRect.y, LRect.width - 50, LRect.height);
                        Rect Lbutton = new Rect(LRect.x, LRect.y, 50, LRect.height);

                        if (GUI.RepeatButton(Lbutton, "Euler"))
                        {
                            EditorGUI.Vector3Field(LinputRec, "", property.quaternionValue.eulerAngles);
                        }
                        else
                        {
                            Quaternion Lquater = property.quaternionValue;
                            Vector4 LReceive = EditorGUI.Vector4Field(LinputRec, "", new Vector4(Lquater.x, Lquater.y, Lquater.z, Lquater.w));
                            property.quaternionValue = new Quaternion(LReceive.x, LReceive.y, LReceive.z, LReceive.w);
                        }
                    }
                    break;
                case TypeEnum.ExposedReference:
                    EditorGUI.LabelField(LRect, "Not Surpport");
                    break;
                case TypeEnum.FixedBufferSize:
                    EditorGUI.LabelField(LRect, "Not Surpport");
                    break;
                case TypeEnum.Vector2Int:
                    property.vector2IntValue = EditorGUI.Vector2IntField(LRect, "", property.vector2IntValue);
                    break;
                case TypeEnum.Vector3Int:
                    property.vector3IntValue = EditorGUI.Vector3IntField(LRect, "", property.vector3IntValue);
                    break;
                case TypeEnum.RectInt:
                    {

                        //EditorGUI.MaskField();
                    }
                    break;
                case TypeEnum.BoundsInt:
                    {

                    }
                    break;
                case TypeEnum.ManagedReference:
                    EditorGUI.LabelField(LRect, "Not Surpport");
                    break;
            }
            LRect = GetNextSpace(pos, LRect, 0, InputWidth);
            return LRect;
        }//Not Work Rect, Bound , Gradient
        public static int LayerMaskField(Rect pos, string label, int Layers, float Space = 0)
        {
            {/*
            List<string> layers = new List<string>();
            List<int> layerNumbers = new List<int>();

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (layerName != "")
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }
            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & Layers) > 0)
                    maskWithoutEmpty |= (1 << i);//  ========>  maskWithoutEmpty = maskWithoutEmpty <비트 OR 연산> (1 << i {2의 i 제곱} )
            }
            maskWithoutEmpty = EditorGUI.MaskField(pos, label, maskWithoutEmpty, layers.ToArray());
            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) > 0)
                    mask |= (1 << layerNumbers[i]);
            }
            return mask;*/
            }

            Rect LRect = new Rect(pos.x + Space, pos.y, pos.width - Space, pos.height);
            LRect = ResizedLabel(pos, LRect, label);

            float InputWidth = Mathf.Max(0, (pos.width - LRect.width));

            LRect = GetNextSpace(pos, LRect, InputWidth);

            return EditorGUI.MaskField(LRect, Layers, UnityEditorInternal.InternalEditorUtility.layers);
        }
        public static LayerMask LayerMaskField(Rect pos, string label, LayerMask Layers)
        {
            return LayerMaskField(pos, label, Layers.value);
        }

        public static Rect LabelRateField(Rect pos, Rect DrawRect, string Text, int LineAmount, float Space = 0, bool hightLight = false)
        {
            Rect LRect = new Rect(DrawRect.x + Space, DrawRect.y, ((pos.width) / LineAmount), DrawRect.height);

            EditorGUI.indentLevel = 0;
            EditorGUI.LabelField(LRect, Text);
            if (hightLight)
            {
                EditorGUI.HelpBox(LRect, "", MessageType.None);
            }

            return new Rect((LRect.x + ((pos.width) / LineAmount)), DrawRect.y, ((pos.width) / LineAmount), DrawRect.height);
        }
        public static Rect LabelRateField(Rect pos, Rect DrawRect, GUIContent Text, int LineAmount, float Space = 0, bool hightLight = false)
        {
            Rect LRect = new Rect(DrawRect.x + Space, DrawRect.y, ((pos.width) / LineAmount), DrawRect.height);

            EditorGUI.indentLevel = 0;
            EditorGUI.LabelField(LRect, Text);
            if (hightLight)
            {
                EditorGUI.HelpBox(LRect, "", MessageType.None);
            }

            return new Rect((LRect.x + ((pos.width) / LineAmount)), DrawRect.y, ((pos.width) / LineAmount), DrawRect.height);
        }

        public static object GetPropertyDrawerTarget<T>(FieldInfo fieldInfo, SerializedProperty property)
        {
            object Lobj = null;
            if (property.serializedObject.targetObject.GetType() == typeof(T))
            {
                Lobj = fieldInfo.GetValue(property.serializedObject.targetObject);
            }
            else
            {
                var paths = property.propertyPath.Split('.');
                object LChildObj = property.serializedObject.targetObject.GetType().GetField(paths[0])
                    .GetValue(property.serializedObject.targetObject);
                FieldInfo LChildField = property.serializedObject.targetObject.GetType().GetField(paths[0]);
                //Debug.Log(property.serializedObject.targetObject.GetType().GetField(paths[0]) + " | " + paths[0]);

                if (LChildField != null)
                {
                    for (int i = 1; i < paths.Length; i++)
                    {
                        LChildField = LChildField.FieldType.GetField(paths[i]);

                        if (i + 1 != paths.Length)
                        {
                            LChildObj = LChildObj.GetType().GetField(paths[i]).GetValue(LChildObj);
                        }//LChildField 보다 1단계 상위에 있어야함
                    }

                    //Debug.Log(LChildField.Name + " | " + LChildObj.ToString());
                    if (typeof(T) == LChildObj.GetType())
                    {
                        Lobj = LChildObj;
                    }
                    else
                    {
                        Lobj = LChildField.GetValue(LChildObj);
                    }
                }
            }
            return Lobj;
        }//리스트와 배열이 있는경우는 미구현

        public static System.Type PropertyTypeToType(string propertyType)
        {
            switch (propertyType)
            {
                case "int":
                    return typeof(int);
                case "bool":
                    return typeof(bool);
                case "float":
                    return typeof(float);
                case "string":
                    return typeof(string);
                case "Color":
                    return typeof(Color);
                case "PPtr<$GameObject>":
                    return typeof(GameObject);
                case "LayerMask":
                    return typeof(LayerMask);
                case "Enum":
                    return null;
                case "Vector2":
                    return typeof(Vector2);
                case "Vector3":
                    return typeof(Vector3);
                case "Vector4":
                    return typeof(Vector4);
                case "Rect":
                    return typeof(Rect);
                case "ArraySize":
                    return null;
                case "Character":
                    return null;
                case "AnimationCurve":
                    return typeof(AnimationCurve);
                case "Bounds":
                    return typeof(Bounds);
                case "Gradient":
                    return typeof(Gradient);
                case "Quaternion":
                    return typeof(Quaternion);
                case "ExposedReference":
                case "FixedBufferSize":
                case "ManagedReference":
                    return null;
                case "Vector2Int":
                    return typeof(Vector2Int);
                case "Vector3Int":
                    return typeof(Vector3Int);
                case "RectInt":
                    return typeof(RectInt);
                case "BoundsInt":
                    return typeof(BoundsInt);
                default:
                    return null;
            }
            //Debug.Log(EditorExpand.PropertyTypeToType(property.type).FullName);
        }
    }

    #region AttributeLabel
    public class AttributeLabel : PropertyAttribute
    {
        public bool HightLight = false;
        public bool Expand = false;
        public string Text = "";
        public AttributeLabel(string text, bool hightLight = false, bool expand = false)
        {
            Text = text;
            Expand = expand;
            HightLight = hightLight;
        }
    }
    [CustomPropertyDrawer(typeof(AttributeLabel))]
    public class AttributeLabelEditor : PropertyDrawer
    {
        bool HightLight = false;
        string Text = "";
        AttributeLabel attributeLabel;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return Mathf.Min(20, GUI.skin.label.CalcSize(new GUIContent(Text)).y) + 20;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            attributeLabel = (AttributeLabel)attribute;
            HightLight = attributeLabel.HightLight;
            Text = attributeLabel.Text;

            Vector2 TextArea = GUI.skin.label.CalcSize(new GUIContent(Text));
            Rect DrawRect = new Rect();

            if (attributeLabel.Expand)
            {
                DrawRect = new Rect(position.x, position.y, position.width, TextArea.y);
            }
            else
            {
                DrawRect = new Rect(position.x, position.y, TextArea.x + 15, TextArea.y);
            }

            if (HightLight)
            {
                EditorGUI.HelpBox(DrawRect, Text, MessageType.None);
                DrawRect = EditorExpand.NextLine(position, DrawRect);
                TypeEnum Ltype = VariableCollection.ConvertTypeEnum(EditorExpand.PropertyTypeToType(property.type).Name);
                EditorExpand.InputField(position, DrawRect, property, property.displayName, 1);
            }
            else
            {
                EditorGUI.LabelField(DrawRect, Text);
                DrawRect = EditorExpand.NextLine(position, DrawRect);
                TypeEnum Ltype = VariableCollection.ConvertTypeEnum(EditorExpand.PropertyTypeToType(property.type).Name);
                EditorExpand.InputField(position, DrawRect, property, property.displayName, 1);
            }
        }
    }//[AttributeLabel("Testing", true, true)]
    #endregion AttributeLabel

    #region AttributeLayer
    public class AttributeLayer : PropertyAttribute
    {

    }
    [CustomPropertyDrawer(typeof(AttributeLayer))]
    public class AttributeLayerEditor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 20;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.intValue = EditorGUI.LayerField(position, property.displayName, property.intValue);
        }
    }//[AttributeLayer("Team Layer")] //Editor는 저장X, PropertyDrawer는 전부 구현하거나(일부분 구현X)
    #endregion AttributeLayer

    #region AttributeLayerMask
    public class AttributeLayerMask : PropertyAttribute
    {

    }
    [CustomPropertyDrawer(typeof(AttributeLayerMask))]
    public class AttributeLayerMaskEditor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 20;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.intValue = EditorGUI.MaskField(position, label.text, property.intValue, UnityEditorInternal.InternalEditorUtility.layers);
        }
    }//[AttributeLayer("Team Layer")] //Editor는 저장X, PropertyDrawer는 전부 구현하거나(일부분 구현X)
    #endregion AttributeLayerMask

    #region AttributeField

    public class AttributeField : PropertyAttribute
    {
        public TypeEnum Type = TypeEnum.Generic;
        public float Space = 0;

        public AttributeField(TypeEnum type, float space = 0)
        {
            Type = type;
            Space = space;
        }
    }
    [CustomPropertyDrawer(typeof(AttributeField))]
    public class AttributeFieldEditor : PropertyDrawer
    {
        AttributeField attributeField;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 20;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            attributeField = (AttributeField)attribute;
            position = EditorExpand.InputField(position, position, property, property.displayName, 1, attributeField.Space);
        }
    }//[AttributeField("Testing",TypeEnum.LayerMask)]
    #endregion AttributeField

    #region AttributeHorizontal
    public class AttributeHorizontal : PropertyAttribute
    {
        public string[] BasePath;
        public string[] Paths;
        public AttributeHorizontal(string[] basePath, params string[] paths)
        {
            BasePath = basePath;
            Paths = paths;
        }
    }

    [CustomPropertyDrawer(typeof(AttributeHorizontal))]
    public class AttributeHorizontalEditor : PropertyDrawer
    {
        AttributeHorizontal attributeHorizontal;
        Rect DrawRect;
        SerializedProperty BaseProperty;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            attributeHorizontal = (AttributeHorizontal)attribute;
            string[] BasePath = attributeHorizontal.BasePath;
            string[] Paths = attributeHorizontal.Paths;
            DrawRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            DrawRect = EditorExpand.InputField(position, DrawRect, property, property.displayName, (Paths.Length + 1));

            if (BasePath.Length > 0)
            {
                for (int i = 0; i < BasePath.Length; i++)
                {
                    if (i == 0)
                    {
                        BaseProperty = property.serializedObject.FindProperty(BasePath[i]);
                    }
                    else
                    {
                        BaseProperty = BaseProperty.FindPropertyRelative(BasePath[i]);
                    }
                }

                for (int i = 0; i < Paths.Length; i++)
                {
                    var Lproperty = BaseProperty.FindPropertyRelative(Paths[i]);//먼저 BasePath에 접근
                    DrawRect = EditorExpand.InputField(position, DrawRect, Lproperty, Lproperty.displayName, (Paths.Length + 1));
                }
            }
        }
    }
    #endregion//Attribute으로 다른 변수 접근해서 가로배치//[AttributeHorizontal(new string[] { 해당 변수속한 상위클래스 경로 }, 가로 배치할 변수이름)]

    #region AttributeMask
    public class AttributeMask : PropertyAttribute
    {
        public string[] SelectList;

        public AttributeMask(params string[] selectList)
        {
            SelectList = selectList;
        }
    }
    [CustomPropertyDrawer(typeof(AttributeMask))]
    public class AttributeMaskEditor : PropertyDrawer
    {
        AttributeMask attributeField;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 20;
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            attributeField = (AttributeMask)attribute;
            property.intValue = EditorGUI.MaskField(position, label.text, property.intValue, attributeField.SelectList);
        }
    }
    #endregion AttributeLayerMask


}
#endif