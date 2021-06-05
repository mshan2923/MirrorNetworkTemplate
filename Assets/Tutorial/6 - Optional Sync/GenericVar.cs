using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

[System.Serializable]
public class GenericVar
{
    public string data;
    System.Type type;

    public GenericVar(System.Type Type, object Data)
    {
        type = Type;
        data = JsonUtility.ToJson(new Wrap<object>(Data));
    }

    public void ForceSet<T>(T Data)
    {
        type = typeof(T);
        data = JsonUtility.ToJson(new Wrap<T>(Data));
    }
    public bool Set<T>(T Data)
    {
        if (typeof(T) == type || type == null)
        {
            type = typeof(T);
            data = JsonUtility.ToJson(new Wrap<T>(Data));

            return true;
        }
        else
            return false;
    }
    public T Get<T>()
    {
        if (typeof(T) == type)
            return JsonUtility.FromJson<Wrap<T>>(data).Get;
        else
            return default;
    }

    public System.Type GetDataType()
    {
        return type;
    }

}//Can Change Var Type

[System.Serializable]
public class GenericVar<T>
{
    string data;
    public T Data
    {
        get
        {
            return JsonUtility.FromJson<Wrap<T>>(data).Get;
        }
        set
        {
            data = JsonUtility.ToJson(new Wrap<T>(value));
        }
    }
}//Static Var Type , More Simple 


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(GenericVar))]
public class GenericVarEditor : PropertyDrawer
{
    GenericVar Onwer;
    bool expand = false;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 20 + (expand ? 20 : 0);
        //return base.GetPropertyHeight(property, label);
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //base.OnGUI(position, property, label);
        var paths = property.propertyPath.Split('.');
        //Debug.Log(property.serializedObject.targetObject.GetType().GetField(paths[0]).GetValue(property.serializedObject.targetObject));
        Onwer = (GenericVar)GetPropertyDrawerTarget<GenericVar>(fieldInfo, property);

        {
            /*        string Tdata = property.FindPropertyRelative("data").stringValue;

        if (Onwer.type == typeof(int))
        {
            //property.FindPropertyRelative("data").objectReferenceValue = EditorGUI.IntField(position, label.text, Onwer.Get<int>());
            Tdata = JsonUtility.ToJson(EditorGUI.IntField(position, label.text, Onwer.Get<int>()));
        }
        else if (Onwer.type == typeof(bool))
        {
            Tdata = JsonUtility.ToJson(EditorGUI.Toggle(position, label.text, Onwer.Get<bool>()));
        }
        else if (Onwer.type == typeof(float))
        {
            Tdata = JsonUtility.ToJson(EditorGUI.FloatField(position, label.text, Onwer.Get<float>()));
        }
        else if (Onwer.type == typeof(string))
        {
            //string Temp = EditorGUI.TextField(position, label.text, Onwer.Get<string>());
            string Ldata = JsonUtility.FromJson<string>(property.FindPropertyRelative("data").stringValue);
            string TField = EditorGUI.TextField(position, label.text, Ldata);
            Tdata = JsonUtility.ToJson(TField);
            Debug.Log(JsonUtility.ToJson(TField) + " < " + TField + " < " + Onwer.Get<string>() + " == " + Ldata);
        }
        else if (Onwer.type == typeof(Color))
        {
            Tdata = JsonUtility.ToJson(EditorGUI.ColorField(position, label.text, Onwer.Get<Color>()));
        }
        else if (Onwer.type == typeof(GameObject))
        {
            Tdata = JsonUtility.ToJson(EditorGUI.ObjectField(position, label.text, Onwer.Get<GameObject>(), typeof(GameObject), true));
        }
        else if (Onwer.type == typeof(Vector2))
        {
            Tdata = JsonUtility.ToJson(EditorGUI.Vector2Field(position, label.text, Onwer.Get<Vector2>()));
        }
        else if (Onwer.type == typeof(Vector3))
        {
            Tdata = JsonUtility.ToJson(EditorGUI.Vector3Field(position, label.text, Onwer.Get<Vector3>()));
        }
        else if (Onwer.type == typeof(Vector4))
        {
            Tdata = JsonUtility.ToJson(EditorGUI.Vector4Field(position, label.text, Onwer.Get<Vector4>()));
        }
        else if (Onwer.type == typeof(Gradient))
        {
            Tdata = JsonUtility.ToJson(EditorGUI.GradientField(position, label.text, Onwer.Get<Gradient>()));
        }
        else if (Onwer.type == typeof(Quaternion))
        {
            Tdata = JsonUtility.ToJson(Quaternion.Euler(EditorGUI.Vector3Field(position, label.text, Onwer.Get<Quaternion>().eulerAngles)));
        }
        else if (Onwer.type == typeof(Vector2Int))
        {
            Tdata = JsonUtility.ToJson(EditorGUI.Vector2Field(position, label.text, Onwer.Get<Vector2Int>()));
        }
        else if (Onwer.type == typeof(Vector3Int))
        {
            Tdata = JsonUtility.ToJson(EditorGUI.Vector3Field(position, label.text, Onwer.Get<Vector3Int>()));
        }
                    //property.FindPropertyRelative("data").stringValue = Tdata;
             */
        }//disable
        property.FindPropertyRelative("data").stringValue = 
            VariableCollection.DataField(Onwer.GetDataType().FullName, property.FindPropertyRelative("data").stringValue, position, out expand, label.text);
    }
    public static object GetPropertyDrawerTarget<T>(System.Reflection.FieldInfo fieldInfo, SerializedProperty property)
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
            System.Reflection.FieldInfo LChildField = property.serializedObject.targetObject.GetType().GetField(paths[0]);
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
                Lobj = paths.Length > 1 ? LChildField.GetValue(LChildObj) : LChildObj;
            }
        }
        return Lobj;
    }
}
#endif
