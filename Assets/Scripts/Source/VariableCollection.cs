using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static VariableCollection;


public enum TypeEnum
{
    Generic, Integer, Boolean, Float, String, Color, ObjectReference, LayerMask, Enum, Vector2, Vector3, Vector4,
    Rect, ArraySize, Character, AnimationCurve, Bounds, Gradient, Quaternion, ExposedReference, FixedBufferSize,
    Vector2Int, Vector3Int, RectInt, BoundsInt, ManagedReference
}

[System.Serializable]
public class VariableCollection
{
    public static Type ConvertType(string TypeFullName)
    {
        if (string.IsNullOrEmpty(TypeFullName))
        {
            return null;
        }
        else
        {
            // null 반환 없이 Type이 얻어진다면 얻어진 그대로 반환.
            var type = Type.GetType(TypeFullName);
            if (type != null)
                return type;

            // 프로젝트에 분명히 포함된 클래스임에도 불구하고 Type이 찾아지지 않는다면,
            // 실행중인 어셈블리를 모두 탐색 하면서 그 안에 찾고자 하는 Type이 있는지 검사.
            var currentAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
            foreach (var assemblyName in referencedAssemblies)
            {
                var assembly = System.Reflection.Assembly.Load(assemblyName);
                if (assembly != null)
                {
                    // 찾았다 요놈!!!
                    type = assembly.GetType(TypeFullName);
                    if (type != null)
                        return type;
                }
            }
        }

        // 못 찾았음;;; 클래스 이름이 틀렸던가, 아니면 알 수 없는 문제 때문이겠지...
        return null;
    }//if Convert Error Find Assmbly

    public static TypeEnum ConvertTypeEnum(string TypeName)
    {
        TypeEnum TypeEnum = TypeEnum.Generic;
        if (string.IsNullOrEmpty(TypeName))
        {
            return TypeEnum.Generic;
        }

        if (typeof(int).Name.Equals(TypeName))
            TypeEnum = TypeEnum.Integer;
        if (typeof(bool).Name.Equals(TypeName))
            TypeEnum = TypeEnum.Boolean;
        if (typeof(float).Name.Equals(TypeName))
            TypeEnum = TypeEnum.Float;
        if (typeof(string).Name.Equals(TypeName))
            TypeEnum = TypeEnum.String;
        if (typeof(Color).Name.Equals(TypeName))
            TypeEnum = TypeEnum.Color;
        if (typeof(GameObject).Name.Equals(TypeName))
            TypeEnum = TypeEnum.ObjectReference;//아닐지도?
        if (typeof(LayerMask).Name.Equals(TypeName))
            TypeEnum = TypeEnum.LayerMask;
        if (typeof(Enum).Name.Equals(TypeName))
            TypeEnum = TypeEnum.Enum;
        if (typeof(Vector2).Name.Equals(TypeName))
            TypeEnum = TypeEnum.Vector2;
        if (typeof(Vector3).Name.Equals(TypeName))
            TypeEnum = TypeEnum.Vector3;
        if (typeof(Vector4).Name.Equals(TypeName))
            TypeEnum = TypeEnum.Vector4;//10
        if (typeof(Rect).Name.Equals(TypeName))
            TypeEnum = TypeEnum.Rect;
        if (typeof(Array).Name.Equals(TypeName))
            TypeEnum = TypeEnum.ArraySize;//될려나??
        //if (typeof(Character).Name.Equals(TypeName))
        //    TypeEnum = TypeEnum.Character;//???
        if (typeof(AnimationCurve).Name.Equals(TypeName))
            TypeEnum = TypeEnum.AnimationCurve;
        if (typeof(Bounds).Name.Equals(TypeName))
            TypeEnum = TypeEnum.Bounds;
        if (typeof(Gradient).Name.Equals(TypeName))
            TypeEnum = TypeEnum.Gradient;
        if (typeof(Quaternion).Name.Equals(TypeName))
            TypeEnum = TypeEnum.Quaternion;
        //if (typeof(ExposedReference).Name.Equals(TypeName))
        //    TypeEnum = TypeEnum.ExposedReference;
        //if (typeof(Buffer).Name.Equals(TypeName))
        //    TypeEnum = TypeEnum.FixedBufferSize;//???
        if (typeof(Vector2Int).Name.Equals(TypeName))
            TypeEnum = TypeEnum.Vector2Int;//20
        if (typeof(Vector3Int).Name.Equals(TypeName))
            TypeEnum = TypeEnum.Vector3Int;
        if (typeof(RectInt).Name.Equals(TypeName))
            TypeEnum = TypeEnum.RectInt;
        if (typeof(BoundsInt).Name.Equals(TypeName))
            TypeEnum = TypeEnum.BoundsInt;
        //if (typeof().Name.Equals(TypeName))
        //    TypeEnum = TypeEnum.ManagedReference;

        return TypeEnum;
    }
    public static string ConvertTypeName(TypeEnum TypeEnum)
    {
        switch (TypeEnum)
        {
            case TypeEnum.Generic:
                return null;
            case TypeEnum.Integer:
                return typeof(int).FullName;
            case TypeEnum.Boolean:
                return typeof(bool).FullName;
            case TypeEnum.Float:
                return typeof(float).FullName;
            case TypeEnum.String:
                return typeof(string).FullName;
            case TypeEnum.Color:
                return typeof(Color).FullName;
            case TypeEnum.ObjectReference:
                return typeof(GameObject).FullName;
            case TypeEnum.LayerMask:
                return typeof(LayerMask).FullName;
            case TypeEnum.Enum:
                return typeof(Enum).FullName;
            case TypeEnum.Vector2:
                return typeof(Vector2).FullName;
            case TypeEnum.Vector3:
                return typeof(Vector3).FullName;
            case TypeEnum.Vector4:
                return typeof(Vector4).FullName;
            case TypeEnum.Rect:
                return typeof(Rect).FullName;
            case TypeEnum.ArraySize:
                //return typeof(Array).FullName;
                return null;
            case TypeEnum.Character:
                return null;
            case TypeEnum.AnimationCurve:
                return typeof(AnimationCurve).FullName;
            case TypeEnum.Bounds:
                return typeof(Bounds).FullName;
            case TypeEnum.Gradient:
                return typeof(Gradient).FullName;
            case TypeEnum.Quaternion:
                return typeof(Quaternion).FullName;
            case TypeEnum.ExposedReference:
            case TypeEnum.FixedBufferSize:
                return null;
            case TypeEnum.Vector2Int:
                return typeof(Vector2Int).FullName;
            case TypeEnum.Vector3Int:
                return typeof(Vector3Int).FullName;
            case TypeEnum.RectInt:
                return typeof(RectInt).FullName;
            case TypeEnum.BoundsInt:
                return typeof(BoundsInt).FullName;
            case TypeEnum.ManagedReference:
            default:
                return null;
        }
    }

    public static T UnRapping<T>(string vaule)
    {
        if (typeof(Wrap<T>) != null && !string.IsNullOrEmpty(vaule))
        {
            return JsonUtility.FromJson<Wrap<T>>(vaule).Get;
        }
        else
        {
            return default;
        }
    }//Height Cost
    public static string Rapping<T>(T vaule)
    {
        Wrap<T> wrap = new Wrap<T>(vaule);
        return JsonUtility.ToJson(wrap);
    }//Height Cost
    public static string Rapping(TypeEnum typeEnum, string vaule = "")
    {
        switch (typeEnum)
        {
            case TypeEnum.Generic:
                return vaule;
            case TypeEnum.Integer:
                {
                    if (string.IsNullOrEmpty(vaule))
                    {
                        return JsonUtility.ToJson(new Wrap<int>(0));
                    }
                    else
                    {
                        return JsonUtility.ToJson(new Wrap<int>(JsonUtility.FromJson<int>(vaule)));
                    }
                }
            case TypeEnum.Boolean:
                {
                    if (string.IsNullOrEmpty(vaule))
                    {
                        return JsonUtility.ToJson(new Wrap<bool>(false));
                    }
                    else
                    {
                        return JsonUtility.ToJson(new Wrap<bool>(JsonUtility.FromJson<bool>(vaule)));
                    }
                }
            case TypeEnum.Float:
                {
                    if (string.IsNullOrEmpty(vaule))
                    {
                        return JsonUtility.ToJson(new Wrap<float>(0));
                    }
                    else
                    {
                        return JsonUtility.ToJson(new Wrap<float>(JsonUtility.FromJson<float>(vaule)));
                    }
                }
            case TypeEnum.String:
                return JsonUtility.ToJson(new Wrap<string>(vaule));
            case TypeEnum.Color:
                {
                    if (string.IsNullOrEmpty(vaule))
                    {
                        return JsonUtility.ToJson(new Wrap<Color>(Color.black));
                    }
                    else
                    {
                        return JsonUtility.ToJson(new Wrap<Color>(JsonUtility.FromJson<Color>(vaule)));
                    }
                }
            case TypeEnum.ObjectReference:
                {
                    if (string.IsNullOrEmpty(vaule))
                    {
                        return JsonUtility.ToJson(new Wrap<GameObject>(null));
                    }
                    else
                    {
                        return JsonUtility.ToJson(new Wrap<GameObject>(JsonUtility.FromJson<GameObject>(vaule)));
                    }
                }
            case TypeEnum.LayerMask:
                {
                    if (string.IsNullOrEmpty(vaule))
                    {
                        return JsonUtility.ToJson(new Wrap<LayerMask>(new LayerMask()));
                    }
                    else
                    {
                        return JsonUtility.ToJson(new Wrap<LayerMask>(JsonUtility.FromJson<LayerMask>(vaule)));
                    }
                }
            case TypeEnum.Enum:
                {
                    if (string.IsNullOrEmpty(vaule))
                    {
                        return JsonUtility.ToJson(new Wrap<Enum>(null));
                    }
                    else
                    {
                        return JsonUtility.ToJson(new Wrap<Enum>(JsonUtility.FromJson<Enum>(vaule)));
                    }
                }
            case TypeEnum.Vector2:
                {
                    if (string.IsNullOrEmpty(vaule))
                    {
                        return JsonUtility.ToJson(new Wrap<Vector2>(Vector2.zero));
                    }
                    else
                    {
                        return JsonUtility.ToJson(new Wrap<Vector2>(JsonUtility.FromJson<Vector2>(vaule)));
                    }
                }
            case TypeEnum.Vector3:
                {
                    if (string.IsNullOrEmpty(vaule))
                    {
                        return JsonUtility.ToJson(new Wrap<Vector3>(Vector3.zero));
                    }
                    else
                    {
                        return JsonUtility.ToJson(new Wrap<Vector3>(JsonUtility.FromJson<Vector3>(vaule)));
                    }
                }
            case TypeEnum.Vector4:
                {
                    if (string.IsNullOrEmpty(vaule))
                    {
                        return JsonUtility.ToJson(new Wrap<Vector4>(Vector4.zero));
                    }
                    else
                    {
                        return JsonUtility.ToJson(new Wrap<Vector4>(JsonUtility.FromJson<Vector4>(vaule)));
                    }
                }
            case TypeEnum.Rect:
                {
                    if (string.IsNullOrEmpty(vaule))
                    {
                        return JsonUtility.ToJson(new Wrap<Rect>(Rect.zero));
                    }
                    else
                    {
                        return JsonUtility.ToJson(new Wrap<Rect>(JsonUtility.FromJson<Rect>(vaule)));
                    }
                }
            case TypeEnum.ArraySize:
                {
                    if (string.IsNullOrEmpty(vaule))
                    {
                        return JsonUtility.ToJson(new Wrap<Array>(null));
                    }
                    else
                    {
                        return JsonUtility.ToJson(new Wrap<Array>(JsonUtility.FromJson<Array>(vaule)));
                    }
                }//...?
            case TypeEnum.Character:
                {
                    return null;
                }//null
            case TypeEnum.AnimationCurve:
                {
                    if (string.IsNullOrEmpty(vaule))
                    {
                        return JsonUtility.ToJson(new Wrap<AnimationCurve>(null));
                    }
                    else
                    {
                        return JsonUtility.ToJson(new Wrap<AnimationCurve>(JsonUtility.FromJson<AnimationCurve>(vaule)));
                    }
                }
            case TypeEnum.Bounds:
                {
                    if (string.IsNullOrEmpty(vaule))
                    {
                        return JsonUtility.ToJson(new Wrap<Bounds>(new Bounds()));
                    }
                    else
                    {
                        return JsonUtility.ToJson(new Wrap<Bounds>(JsonUtility.FromJson<Bounds>(vaule)));
                    }
                }
            case TypeEnum.Gradient:
                {
                    if (string.IsNullOrEmpty(vaule))
                    {
                        return JsonUtility.ToJson(new Wrap<Gradient>(null));
                    }
                    else
                    {
                        return JsonUtility.ToJson(new Wrap<Gradient>(JsonUtility.FromJson<Gradient>(vaule)));
                    }
                }
            case TypeEnum.Quaternion:
                {
                    if (string.IsNullOrEmpty(vaule))
                    {
                        return JsonUtility.ToJson(new Wrap<Quaternion>(Quaternion.identity));
                    }
                    else
                    {
                        return JsonUtility.ToJson(new Wrap<Quaternion>(JsonUtility.FromJson<Quaternion>(vaule)));
                    }
                }
            case TypeEnum.ExposedReference:
            case TypeEnum.FixedBufferSize:
                return null;
            case TypeEnum.Vector2Int:
                {
                    if (string.IsNullOrEmpty(vaule))
                    {
                        return JsonUtility.ToJson(new Wrap<Vector2Int>(Vector2Int.zero));
                    }
                    else
                    {
                        return JsonUtility.ToJson(new Wrap<Vector2Int>(JsonUtility.FromJson<Vector2Int>(vaule)));
                    }
                }
            case TypeEnum.Vector3Int:
                {
                    if (string.IsNullOrEmpty(vaule))
                    {
                        return JsonUtility.ToJson(new Wrap<Vector3Int>(Vector3Int.zero));
                    }
                    else
                    {
                        return JsonUtility.ToJson(new Wrap<Vector3Int>(JsonUtility.FromJson<Vector3Int>(vaule)));
                    }
                }
            case TypeEnum.RectInt:
                {
                    if (string.IsNullOrEmpty(vaule))
                    {
                        return JsonUtility.ToJson(new Wrap<RectInt>(new RectInt()));
                    }
                    else
                    {
                        return JsonUtility.ToJson(new Wrap<RectInt>(JsonUtility.FromJson<RectInt>(vaule)));
                    }
                }
            case TypeEnum.BoundsInt:
                {
                    if (string.IsNullOrEmpty(vaule))
                    {
                        return JsonUtility.ToJson(new Wrap<BoundsInt>(new BoundsInt()));
                    }
                    else
                    {
                        return JsonUtility.ToJson(new Wrap<BoundsInt>(JsonUtility.FromJson<BoundsInt>(vaule)));
                    }
                }
            case TypeEnum.ManagedReference:
            default:
                return null;
        }
    }//vaule = JsonUtility.ToJson(new wrap<T>(Vaule))

#if UNITY_EDITOR
    public static string DataField(string TypeFullName, string DataText, Rect Lrect, out bool NeedExpand, string LabelText = "", bool ErrorField = true)
    {
        Type LType = ConvertType(TypeFullName);
        TypeEnum LTypeEnum = TypeEnum.Generic;
        if (LType != null)
            LTypeEnum = ConvertTypeEnum(LType.Name);


        //layoutOption = new GUILayoutOption[] { GUILayout.Width(200) };
        NeedExpand = false;

        switch (LTypeEnum)
        {
            case TypeEnum.Generic:
                if (ErrorField)
                {
                    EditorGUI.LabelField(Lrect, "Not Support / Generic");
                }
                break;
            #region done
            case TypeEnum.Integer:
                {
                    //LProp.intValue = EditorGUILayout.IntField(LProp.intValue);
                    //collectionList.Set<int>(index, LProp.intValue);

                    //LProp.stringValue = VariableCollection.Rapping(EditorGUILayout.IntField(VariableCollection.UnRapping<int>(LProp.stringValue)));
                    DataText = Rapping(EditorGUI.IntField(Lrect, LabelText, UnRapping<int>(DataText)));
                    break;
                }
            case TypeEnum.Boolean:
                {
                    DataText = Rapping(EditorGUI.Toggle(Lrect, LabelText, UnRapping<bool>(DataText)));
                    break;
                }
            case TypeEnum.Float:
                {
                    DataText = Rapping(EditorGUI.FloatField(Lrect, LabelText, UnRapping<float>(DataText)));
                    break;
                }
            case TypeEnum.String:
                {
                    DataText = Rapping(EditorGUI.TextField(Lrect, LabelText, UnRapping<string>(DataText)));
                    break;
                }
            case TypeEnum.Color:
                {
                    DataText = Rapping(EditorGUI.ColorField(Lrect, LabelText, UnRapping<Color>(DataText)));
                    break;
                }
            case TypeEnum.ObjectReference:
                {
                    DataText = Rapping(EditorGUI.ObjectField(Lrect, LabelText, UnRapping<GameObject>(DataText), typeof(GameObject), true));
                    break;
                }//GameObject OR Object ?
            case TypeEnum.LayerMask:
                {
                    DataText = Rapping(EditorGUI.LayerField(Lrect, LabelText, ((LayerMask)UnRapping<int>(DataText)).value));//UnRapping<LayerMask>(DataText)
                    break;
                }
            case TypeEnum.Enum:
                {
                    if (ErrorField)
                        EditorGUI.LabelField(Lrect, "Add To Script");
                    break;
                }
            case TypeEnum.Vector2:
                {
                    DataText = Rapping(EditorGUI.Vector2Field(Lrect, LabelText, UnRapping<Vector2>(DataText)));
                    break;
                }
            case TypeEnum.Vector3:
                {
                    DataText = Rapping(EditorGUI.Vector3Field(Lrect, LabelText, UnRapping<Vector3>(DataText)));
                    break;
                }
            case TypeEnum.Vector4:
                {
                    DataText = Rapping(EditorGUI.Vector4Field(Lrect, LabelText, UnRapping<Vector4>(DataText)));
                    break;
                }
            case TypeEnum.Rect:
                {
                    DataText = Rapping(EditorGUI.RectField(Lrect, LabelText, UnRapping<Rect>(DataText)));
                    NeedExpand = true;
                    break;
                }
            #endregion
            case TypeEnum.ArraySize:
                {
                    //for (int i = 0; i < DataPropSlot.arraySize; i++)
                    {
                        //DataField(DataPropSlot, TypePropSlot, collectionList, i);
                    }
                    if (ErrorField)
                        EditorGUI.LabelField(Lrect, "Add To Script");
                    break;
                }//-------------아직 지원X // Add To Script
            case TypeEnum.Character:
                {
                    //??먼지 모르겠음
                    if (ErrorField)
                        EditorGUI.LabelField(Lrect, "Not Support");
                    break;
                }//Not Support
            case TypeEnum.AnimationCurve:
                {
                    DataText = Rapping(EditorGUI.CurveField(Lrect, LabelText, UnRapping<AnimationCurve>(DataText)));
                    break;
                }
            case TypeEnum.Bounds:
                {
                    DataText = Rapping(EditorGUI.BoundsField(Lrect, LabelText, UnRapping<Bounds>(DataText)));
                    NeedExpand = true;
                    break;
                }
            case TypeEnum.Gradient:
                {
                    DataText = Rapping(EditorGUI.GradientField(Lrect, LabelText, UnRapping<Gradient>(DataText)));
                    break;
                }
            case TypeEnum.Quaternion:
                {
                    DataText = Rapping(EditorGUI.Vector4Field(Lrect, LabelText, UnRapping<Vector4>(DataText)));
                    break;
                }
            case TypeEnum.ExposedReference:
            case TypeEnum.FixedBufferSize:
                {
                    if (ErrorField)
                        EditorGUI.LabelField(Lrect, "Not Support");
                    break;
                }
            case TypeEnum.Vector2Int:
                {
                    DataText = Rapping(EditorGUI.Vector2IntField(Lrect, LabelText, UnRapping<Vector2Int>(DataText)));
                    break;
                }
            case TypeEnum.Vector3Int:
                {
                    DataText = Rapping(EditorGUI.Vector3IntField(Lrect, LabelText, UnRapping<Vector3Int>(DataText)));
                    break;
                }
            case TypeEnum.RectInt:
                {
                    DataText = Rapping(EditorGUI.RectIntField(Lrect, LabelText, UnRapping<RectInt>(DataText)));
                    NeedExpand = true;
                    break;
                }
            case TypeEnum.BoundsInt:
                {
                    DataText = Rapping(EditorGUI.BoundsIntField(Lrect, LabelText, UnRapping<BoundsInt>(DataText)));
                    NeedExpand = true;
                    break;
                }
            case TypeEnum.ManagedReference:
                {
                    if (ErrorField)
                        EditorGUI.LabelField(Lrect, "Not Support");
                    break;
                }//Not Support
            default:
                {
                    if (ErrorField)
                        EditorGUI.TextArea(Lrect, "Unknown Type");
                    break;
                }
        }
        return DataText;
    }
    public static string DataFieldLayout(string TypeFullName, string DataText, string LabelText = "", bool ErrorField = true)
    {
        Type LType = ConvertType(TypeFullName);
        TypeEnum LTypeEnum = TypeEnum.Generic;
        if (LType != null)
            LTypeEnum = ConvertTypeEnum(LType.Name);


        //layoutOption = new GUILayoutOption[] { GUILayout.Width(200) };

        switch (LTypeEnum)
        {
            case TypeEnum.Generic:
                if (ErrorField)
                {
                    EditorGUILayout.LabelField("Not Support / Generic");
                }
                break;
            #region done
            case TypeEnum.Integer:
                {
                    //LProp.intValue = EditorGUILayout.IntField(LProp.intValue);
                    //collectionList.Set<int>(index, LProp.intValue);

                    //LProp.stringValue = VariableCollection.Rapping(EditorGUILayout.IntField(VariableCollection.UnRapping<int>(LProp.stringValue)));
                    DataText = Rapping(EditorGUILayout.IntField(LabelText, UnRapping<int>(DataText)));
                    break;
                }
            case TypeEnum.Boolean:
                {
                    DataText = Rapping(EditorGUILayout.Toggle(LabelText, UnRapping<bool>(DataText)));
                    break;
                }
            case TypeEnum.Float:
                {
                    DataText = Rapping(EditorGUILayout.FloatField(LabelText, UnRapping<float>(DataText)));
                    break;
                }
            case TypeEnum.String:
                {
                    DataText = Rapping(EditorGUILayout.TextField(LabelText, UnRapping<string>(DataText)));
                    break;
                }
            case TypeEnum.Color:
                {
                    DataText = Rapping(EditorGUILayout.ColorField(LabelText, UnRapping<Color>(DataText)));
                    break;
                }
            case TypeEnum.ObjectReference:
                {
                    DataText = Rapping(EditorGUILayout.ObjectField(LabelText, UnRapping<GameObject>(DataText), typeof(GameObject), true));
                    break;
                }//GameObject OR Object ?
            case TypeEnum.LayerMask:
                {
                    DataText = Rapping(EditorGUILayout.LayerField(LabelText, ((LayerMask)UnRapping<int>(DataText)).value));//UnRapping<LayerMask>(DataText)
                    break;
                }
            case TypeEnum.Enum:
                {
                    if (ErrorField)
                        EditorGUILayout.LabelField("Add To Script");
                    break;
                }
            case TypeEnum.Vector2:
                {
                    DataText = Rapping(EditorGUILayout.Vector2Field(LabelText, UnRapping<Vector2>(DataText)));
                    break;
                }
            case TypeEnum.Vector3:
                {
                    DataText = Rapping(EditorGUILayout.Vector3Field(LabelText, UnRapping<Vector3>(DataText)));
                    break;
                }
            case TypeEnum.Vector4:
                {
                    DataText = Rapping(EditorGUILayout.Vector4Field(LabelText, UnRapping<Vector4>(DataText)));
                    break;
                }
            case TypeEnum.Rect:
                {
                    DataText = Rapping(EditorGUILayout.RectField(LabelText, UnRapping<Rect>(DataText)));
                    break;
                }
            #endregion
            case TypeEnum.ArraySize:
                {
                    //for (int i = 0; i < DataPropSlot.arraySize; i++)
                    {
                        //DataField(DataPropSlot, TypePropSlot, collectionList, i);
                    }
                    if (ErrorField)
                        EditorGUILayout.LabelField("Add To Script");
                    break;
                }//-------------아직 지원X // Add To Script
            case TypeEnum.Character:
                {
                    //??먼지 모르겠음
                    if (ErrorField)
                        EditorGUILayout.LabelField("Not Support");
                    break;
                }//Not Support
            case TypeEnum.AnimationCurve:
                {
                    DataText = Rapping(EditorGUILayout.CurveField(LabelText, UnRapping<AnimationCurve>(DataText)));
                    break;
                }
            case TypeEnum.Bounds:
                {
                    DataText = Rapping(EditorGUILayout.BoundsField(LabelText, UnRapping<Bounds>(DataText)));
                    break;
                }
            case TypeEnum.Gradient:
                {
                    DataText = Rapping(EditorGUILayout.GradientField(LabelText, UnRapping<Gradient>(DataText)));
                    break;
                }
            case TypeEnum.Quaternion:
                {
                    DataText = Rapping(EditorGUILayout.Vector4Field(LabelText, UnRapping<Vector4>(DataText)));
                    break;
                }
            case TypeEnum.ExposedReference:
            case TypeEnum.FixedBufferSize:
                {
                    if (ErrorField)
                        EditorGUILayout.LabelField("Not Support");
                    break;
                }
            case TypeEnum.Vector2Int:
                {
                    DataText = Rapping(EditorGUILayout.Vector2IntField(LabelText, UnRapping<Vector2Int>(DataText)));
                    break;
                }
            case TypeEnum.Vector3Int:
                {
                    DataText = Rapping(EditorGUILayout.Vector3IntField(LabelText, UnRapping<Vector3Int>(DataText)));
                    break;
                }
            case TypeEnum.RectInt:
                {
                    DataText = Rapping(EditorGUILayout.RectIntField(LabelText, UnRapping<RectInt>(DataText)));
                    break;
                }
            case TypeEnum.BoundsInt:
                {
                    DataText = Rapping(EditorGUILayout.BoundsIntField(LabelText, UnRapping<BoundsInt>(DataText)));
                    break;
                }
            case TypeEnum.ManagedReference:
                {
                    if (ErrorField)
                        EditorGUILayout.LabelField("Not Support");
                    break;
                }//Not Support
            default:
                {
                    if (ErrorField)
                        EditorGUILayout.TextArea("Unknown Type");
                    break;
                }
        }
        return DataText;
    }
    /*
    public static string DataFieldSlot<T>(string LabelText, string DataText)
    {
        return Rapping((T)EditorGUILayout.EnumPopup(LabelText, UnRapping<T>(DataText)));
    }//Use For CustomEnum
    */

    public static void SetVarCollectionProperty<T>(SerializedProperty ParameterSlot, T vaule = default)
    {
        ParameterSlot.FindPropertyRelative("Data").stringValue = Rapping<T>(vaule);
        ParameterSlot.FindPropertyRelative("DataType").stringValue = typeof(T).FullName;
    }
#endif
}//계산만

[System.Serializable]
public class Wrap<T>
{
    [SerializeField]
    T data;

    public Wrap(T data)
    {
        this.data = data;
    }

    public T Get { get => data; }
}//이렇게 감싸주면 배열,리스트 등을 직렬화가능 //https://birthbefore.tistory.com/11 이걸로 커스텀 직렬화

[System.Serializable]
public class VarCollection
{
    public string Data;
    public string DataType;

    public T Get<T>()
    {
        if (typeof(T) == ConvertType(DataType))
        {
            return JsonUtility.FromJson<Wrap<T>>(Data).Get;
        }
        else
        {
            Debug.Log("Not Equal GetType , DataType");
            return default;
        }
    }
    public bool Set<T>(T vaule, bool Override = false)
    {
        Wrap<T> wrap = new Wrap<T>(vaule);
        string data = JsonUtility.ToJson(wrap);

        if (string.IsNullOrEmpty(DataType) || Override)
        {
            Data = data;
            DataType = typeof(T).FullName;
            return true;
        }
        else
        {
            if (typeof(T) == ConvertType(DataType))
            {
                Data = data;
                DataType = typeof(T).FullName;
                return true;
            }
            else
            {
                Debug.Log("Not Equal SetType , DataType");
                return false;
            }
        }
    }
    public void ForceSet(string TypeName, string VauleText)
    {
        Data = VauleText;
        DataType = TypeName;
    }
    public string[] ForceGet()
    {
        return new string[] { DataType, Data };
    }

    public Type GetDataType()
    {
        return VariableCollection.ConvertType(DataType);
    }
    public TypeEnum GetDataTypeEnum()
    {
        return ConvertTypeEnum(DataType);
    }
}


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(VarCollection))]
public class VarCollectionProperty : PropertyDrawer
{
    SerializedProperty DataProp;
    SerializedProperty TypeProp;

    TypeEnum TypeEnum;
    string DataVaule;

    Rect rect = new Rect();
    float DrawPos = 0;
    float fontSize = 7;
    float Space = 10;
    float EnumOffset = 10;
    float LIndented = 0;

    bool NeedExpand = false;

    public Rect AddRect(Rect Lrect, Vector2 pos, float width)
    {
        Vector2 Lpos = Lrect.position + pos;
        return new Rect(Lpos.x, Lpos.y, width, Lrect.height);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //base.OnGUI(position, property, label);

        DataProp = property.FindPropertyRelative("Data");
        TypeProp = property.FindPropertyRelative("DataType");

        {
            if (TypeProp != null)
            {
                Type LType = ConvertType(TypeProp.stringValue);
                if (LType != null)
                    TypeEnum = ConvertTypeEnum(LType.Name);
                else
                    TypeEnum = TypeEnum.Generic;
            }
            else
            {
                TypeEnum = TypeEnum.Generic;
            }

            if (DataProp != null)
            {
                DataVaule = DataProp.stringValue;
            }
            else
            {
                DataVaule = "";
            }
        }//Set TypeEnum, DataVaule

        //EditorGUILayout.BeginHorizontal();

        {
            LIndented = EditorGUI.IndentedRect(position).x;
            DrawPos = property.name.Length * fontSize;
            rect = AddRect(EditorGUI.IndentedRect(position), new Vector2(0, 0), (DrawPos + LIndented));
            EditorGUI.LabelField(rect, property.name);

            rect = AddRect(EditorGUI.IndentedRect(position), new Vector2(DrawPos + Space, 0), (TypeEnum.ToString().Length * fontSize + EnumOffset + LIndented));
            TypeEnum = (TypeEnum)EditorGUI.EnumPopup(rect, TypeEnum);
            DrawPos += TypeEnum.ToString().Length * fontSize + EnumOffset + LIndented;

            {
                if (ConvertType(TypeProp.stringValue) != null)
                {
                    if (TypeEnum == TypeEnum.Generic || ConvertTypeEnum(ConvertType(TypeProp.stringValue).Name) == TypeEnum.Generic)
                    {
                        TypeProp.stringValue = ConvertTypeName(TypeEnum);
                    }
                    else if (TypeEnum != ConvertTypeEnum(ConvertType(TypeProp.stringValue).Name))
                    {
                        Debug.LogWarning("if Change Type , Type >> Generic >> Type");
                    }
                }
                else
                {
                    TypeProp.stringValue = ConvertTypeName(TypeEnum);
                }
            }//TypeProp Update for Generic

            //rect = AddRect(EditorGUI.IndentedRect(position), new Vector2( Space, 0), (position.width ));
            rect = new Rect(EditorGUI.IndentedRect(position).x + DrawPos, position.y, EditorGUI.IndentedRect(position).width - DrawPos, position.height);
            DataVaule = DataField(TypeProp.stringValue, DataProp.stringValue, rect, out NeedExpand, "", true);//Label -> " " 이면 버그 생김
            DataProp.stringValue = DataVaule;

        }//Update TypeProp & DataProp

        //EditorGUILayout.EndHorizontal();

        if (NeedExpand)
        {
            GUILayout.Space(20);
        }
    }
    public string DataField(string TypeFullName, string DataText, string LabelText = " ", bool ErrorField = true, params GUILayoutOption[] layoutOption)
    {
        Type LType = ConvertType(TypeFullName);
        TypeEnum LTypeEnum = TypeEnum.Generic;
        if (LType != null)
            LTypeEnum = ConvertTypeEnum(LType.Name);


        string title = "";
        if (!string.IsNullOrEmpty(LabelText))
            title = LabelText;

        //layoutOption = new GUILayoutOption[] { GUILayout.Width(200) };

        switch (LTypeEnum)
        {
            case TypeEnum.Generic:
                if (ErrorField)
                {
                    EditorGUILayout.LabelField("Not Support / Generic");
                }
                break;
            #region done
            case TypeEnum.Integer:
                {
                    //LProp.intValue = EditorGUILayout.IntField(LProp.intValue);
                    //collectionList.Set<int>(index, LProp.intValue);

                    //LProp.stringValue = VariableCollection.Rapping(EditorGUILayout.IntField(VariableCollection.UnRapping<int>(LProp.stringValue)));
                    DataText = Rapping(EditorGUILayout.IntField(title, UnRapping<int>(DataText), layoutOption));
                    break;
                }
            case TypeEnum.Boolean:
                {
                    DataText = Rapping(EditorGUILayout.Toggle(title, UnRapping<bool>(DataText), layoutOption));
                    break;
                }
            case TypeEnum.Float:
                {
                    DataText = Rapping(EditorGUILayout.FloatField(title, UnRapping<float>(DataText), layoutOption));
                    break;
                }
            case TypeEnum.String:
                {
                    DataText = Rapping(EditorGUILayout.TextField(title, UnRapping<string>(DataText), layoutOption));
                    break;
                }
            case TypeEnum.Color:
                {
                    DataText = Rapping(EditorGUILayout.ColorField(title, UnRapping<Color>(DataText), layoutOption));
                    break;
                }
            case TypeEnum.ObjectReference:
                {
                    DataText = Rapping(EditorGUILayout.ObjectField(title, UnRapping<GameObject>(DataText), typeof(GameObject), true, layoutOption));
                    break;
                }//GameObject OR Object ?
            case TypeEnum.LayerMask:
                {
                    DataText = Rapping(EditorGUILayout.LayerField(title, UnRapping<LayerMask>(DataText), layoutOption));
                    break;
                }
            case TypeEnum.Enum:
                {/*
                    int LEnumIndex = UnRapping<int>(DataText);
                    var LEnum = (Enum)Enum.GetValues(LType).GetValue(LEnumIndex);//Enum

                    var Ldata = Enum.Parse(LType, EditorGUILayout.EnumPopup(title, LEnum, layoutOption).ToString());
                    LEnumIndex = (int)Convert.ChangeType(Ldata, typeof(int));//Enum - EnumType => int

                    if (LEnumIndex >= 0)
                    {
                        DataText = Rapping<int>(LEnumIndex);
                    }*/
                    if (ErrorField)
                        EditorGUILayout.LabelField("Add To Script");
                    break;
                }
            case TypeEnum.Vector2:
                {
                    DataText = Rapping(EditorGUILayout.Vector2Field(title, UnRapping<Vector2>(DataText), layoutOption));
                    break;
                }
            case TypeEnum.Vector3:
                {
                    DataText = Rapping(EditorGUILayout.Vector3Field(title, UnRapping<Vector3>(DataText), layoutOption));
                    break;
                }
            case TypeEnum.Vector4:
                {
                    DataText = Rapping(EditorGUILayout.Vector4Field(title, UnRapping<Vector4>(DataText), layoutOption));
                    break;
                }
            case TypeEnum.Rect:
                {
                    DataText = Rapping(EditorGUILayout.RectField(title, UnRapping<Rect>(DataText), layoutOption));
                    break;
                }
            #endregion
            case TypeEnum.ArraySize:
                {
                    //for (int i = 0; i < DataPropSlot.arraySize; i++)
                    {
                        //DataField(DataPropSlot, TypePropSlot, collectionList, i);
                    }
                    if (ErrorField)
                        EditorGUILayout.LabelField("Add To Script");
                    break;
                }//-------------아직 지원X // Add To Script
            case TypeEnum.Character:
                {
                    //??먼지 모르겠음
                    if (ErrorField)
                        EditorGUILayout.LabelField("Not Support");
                    break;
                }//Not Support
            case TypeEnum.AnimationCurve:
                {
                    DataText = Rapping(EditorGUILayout.CurveField(title, UnRapping<AnimationCurve>(DataText), layoutOption));
                    break;
                }
            case TypeEnum.Bounds:
                {
                    DataText = Rapping(EditorGUILayout.BoundsField(title, UnRapping<Bounds>(DataText), layoutOption));
                    break;
                }
            case TypeEnum.Gradient:
                {
                    DataText = Rapping(EditorGUILayout.GradientField(title, UnRapping<Gradient>(DataText), layoutOption));
                    break;
                }
            case TypeEnum.Quaternion:
                {
                    DataText = Rapping(EditorGUILayout.Vector4Field(title, UnRapping<Vector4>(DataText), layoutOption));
                    break;
                }
            case TypeEnum.ExposedReference:
            case TypeEnum.FixedBufferSize:
                {
                    if (ErrorField)
                        EditorGUILayout.LabelField("Not Support");
                    break;
                }
            case TypeEnum.Vector2Int:
                {
                    DataText = Rapping(EditorGUILayout.Vector2IntField(title, UnRapping<Vector2Int>(DataText), layoutOption));
                    break;
                }
            case TypeEnum.Vector3Int:
                {
                    DataText = Rapping(EditorGUILayout.Vector3IntField(title, UnRapping<Vector3Int>(DataText), layoutOption));
                    break;
                }
            case TypeEnum.RectInt:
                {
                    DataText = Rapping(EditorGUILayout.RectIntField(title, UnRapping<RectInt>(DataText), layoutOption));
                    break;
                }
            case TypeEnum.BoundsInt:
                {
                    DataText = Rapping(EditorGUILayout.BoundsIntField(title, UnRapping<BoundsInt>(DataText), layoutOption));
                    break;
                }
            case TypeEnum.ManagedReference:
                {
                    if (ErrorField)
                        EditorGUILayout.LabelField("Not Support");
                    break;
                }//Not Support
            default:
                {
                    if (ErrorField)
                        EditorGUILayout.TextArea("Unknown Type");
                    break;
                }
        }
        return DataText;
    }

    public string DataField(string TypeFullName, string DataText, Rect Lrect, out bool NeedExpand, string LabelText = "", bool ErrorField = true)
    {
        Type LType = ConvertType(TypeFullName);
        TypeEnum LTypeEnum = TypeEnum.Generic;
        if (LType != null)
            LTypeEnum = ConvertTypeEnum(LType.Name);


        //layoutOption = new GUILayoutOption[] { GUILayout.Width(200) };
        NeedExpand = false;

        switch (LTypeEnum)
        {
            case TypeEnum.Generic:
                if (ErrorField)
                {
                    EditorGUI.LabelField(Lrect, "Not Support / Generic");
                }
                break;
            #region done
            case TypeEnum.Integer:
                {
                    //LProp.intValue = EditorGUILayout.IntField(LProp.intValue);
                    //collectionList.Set<int>(index, LProp.intValue);

                    //LProp.stringValue = VariableCollection.Rapping(EditorGUILayout.IntField(VariableCollection.UnRapping<int>(LProp.stringValue)));
                    DataText = Rapping(EditorGUI.IntField(Lrect, LabelText, UnRapping<int>(DataText)));
                    break;
                }
            case TypeEnum.Boolean:
                {
                    DataText = Rapping(EditorGUI.Toggle(Lrect, LabelText, UnRapping<bool>(DataText)));
                    break;
                }
            case TypeEnum.Float:
                {
                    DataText = Rapping(EditorGUI.FloatField(Lrect, LabelText, UnRapping<float>(DataText)));
                    break;
                }
            case TypeEnum.String:
                {
                    DataText = Rapping(EditorGUI.TextField(Lrect, LabelText, UnRapping<string>(DataText)));
                    break;
                }
            case TypeEnum.Color:
                {
                    DataText = Rapping(EditorGUI.ColorField(Lrect, LabelText, UnRapping<Color>(DataText)));
                    break;
                }
            case TypeEnum.ObjectReference:
                {
                    DataText = Rapping(EditorGUI.ObjectField(Lrect, LabelText, UnRapping<GameObject>(DataText), typeof(GameObject), true));
                    break;
                }//GameObject OR Object ?
            case TypeEnum.LayerMask:
                {
                    DataText = Rapping(EditorGUI.LayerField(Lrect, LabelText, ((LayerMask)UnRapping<int>(DataText)).value));//UnRapping<LayerMask>(DataText)
                    break;
                }
            case TypeEnum.Enum:
                {/*
                    int LEnumIndex = UnRapping<int>(DataText);
                    var LEnum = (Enum)Enum.GetValues(LType).GetValue(LEnumIndex);//Enum

                    var Ldata = Enum.Parse(LType, EditorGUILayout.EnumPopup(title, LEnum, layoutOption).ToString());
                    LEnumIndex = (int)Convert.ChangeType(Ldata, typeof(int));//Enum - EnumType => int

                    if (LEnumIndex >= 0)
                    {
                        DataText = Rapping<int>(LEnumIndex);
                    }*/
                    if (ErrorField)
                        EditorGUI.LabelField(Lrect, "Add To Script");
                    break;
                }
            case TypeEnum.Vector2:
                {
                    DataText = Rapping(EditorGUI.Vector2Field(Lrect, LabelText, UnRapping<Vector2>(DataText)));
                    break;
                }
            case TypeEnum.Vector3:
                {
                    DataText = Rapping(EditorGUI.Vector3Field(Lrect, LabelText, UnRapping<Vector3>(DataText)));
                    break;
                }
            case TypeEnum.Vector4:
                {
                    DataText = Rapping(EditorGUI.Vector4Field(Lrect, LabelText, UnRapping<Vector4>(DataText)));
                    break;
                }
            case TypeEnum.Rect:
                {
                    DataText = Rapping(EditorGUI.RectField(Lrect, LabelText, UnRapping<Rect>(DataText)));
                    NeedExpand = true;
                    break;
                }
            #endregion
            case TypeEnum.ArraySize:
                {
                    //for (int i = 0; i < DataPropSlot.arraySize; i++)
                    {
                        //DataField(DataPropSlot, TypePropSlot, collectionList, i);
                    }
                    if (ErrorField)
                        EditorGUI.LabelField(Lrect, "Add To Script");
                    break;
                }//-------------아직 지원X // Add To Script
            case TypeEnum.Character:
                {
                    //??먼지 모르겠음
                    if (ErrorField)
                        EditorGUI.LabelField(Lrect, "Not Support");
                    break;
                }//Not Support
            case TypeEnum.AnimationCurve:
                {
                    DataText = Rapping(EditorGUI.CurveField(Lrect, LabelText, UnRapping<AnimationCurve>(DataText)));
                    break;
                }
            case TypeEnum.Bounds:
                {
                    DataText = Rapping(EditorGUI.BoundsField(Lrect, LabelText, UnRapping<Bounds>(DataText)));
                    NeedExpand = true;
                    break;
                }
            case TypeEnum.Gradient:
                {
                    DataText = Rapping(EditorGUI.GradientField(Lrect, LabelText, UnRapping<Gradient>(DataText)));
                    break;
                }
            case TypeEnum.Quaternion:
                {
                    DataText = Rapping(EditorGUI.Vector4Field(Lrect, LabelText, UnRapping<Vector4>(DataText)));
                    break;
                }
            case TypeEnum.ExposedReference:
            case TypeEnum.FixedBufferSize:
                {
                    if (ErrorField)
                        EditorGUI.LabelField(Lrect, "Not Support");
                    break;
                }
            case TypeEnum.Vector2Int:
                {
                    DataText = Rapping(EditorGUI.Vector2IntField(Lrect, LabelText, UnRapping<Vector2Int>(DataText)));
                    break;
                }
            case TypeEnum.Vector3Int:
                {
                    DataText = Rapping(EditorGUI.Vector3IntField(Lrect, LabelText, UnRapping<Vector3Int>(DataText)));
                    break;
                }
            case TypeEnum.RectInt:
                {
                    DataText = Rapping(EditorGUI.RectIntField(Lrect, LabelText, UnRapping<RectInt>(DataText)));
                    NeedExpand = true;
                    break;
                }
            case TypeEnum.BoundsInt:
                {
                    DataText = Rapping(EditorGUI.BoundsIntField(Lrect, LabelText, UnRapping<BoundsInt>(DataText)));
                    NeedExpand = true;
                    break;
                }
            case TypeEnum.ManagedReference:
                {
                    if (ErrorField)
                        EditorGUI.LabelField(Lrect, "Not Support");
                    break;
                }//Not Support
            default:
                {
                    if (ErrorField)
                        EditorGUI.TextArea(Lrect, "Unknown Type");
                    break;
                }
        }
        return DataText;
    }
}//if Change Type , Type >> Generic >> Type
#endif