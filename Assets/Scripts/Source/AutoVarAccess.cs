using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
//using Mirror;

[Serializable]
public class AutoVarAccess
{
    Component target;
    string varName;
    bool ignoreCase = true;
    Type varType = null;//타입제한

    public delegate object ChangeDataDelegate(object oldData, object newData);

    private ChangeDataDelegate changeData;
    public ChangeDataDelegate ChangeData //델리게이트 연결은 직접
    {   get
        {
            Debug.Log("Get ChangeData : " + (changeData != null));
            return changeData;
        }
        set
        {
            changeData = value;
            Debug.Log("Set ChangData : " + value.ToString());
        }
    }

    public Type VarType
    {
        get => varType;
    }
    public AutoVarAccess(Component Target, string VarName, ChangeDataDelegate changeEvent, Type VarType = null, bool IgnoreCase = true)
    {
        target = Target;
        varName = VarName;
        varType = VarType;
        ignoreCase = IgnoreCase;

        changeData = changeEvent;
        Debug.Log("Reset");
    }

    public object Data
    {
        get
        {
            return AutoVarAccessScript.GetVar(target, varName, ignoreCase);
        }
        set
        {
            if (value != null)
            {
                if (value.GetType() == varType || varType == null)
                    AutoVarAccessScript.SetVar(target, varName, value, ignoreCase);
                else
                    Debug.LogWarning("Data.Set is Fail");
            }else
            {
                Debug.LogWarning("Vaule is null");
            }
        }
    }//오브젝트 타입으로 간단하게

    public T Get<T>()
    {
        if (varType == typeof(T) || varType == null)
            return (T)AutoVarAccessScript.GetVar(target, varName, ignoreCase);
        else
            return default;
    }
    //[Command(requiresAuthority = false)]
    public bool Set<T>(T data)
    {
        if (varType == typeof(T))
        {
            ChangeDataEvent(data); //서버에게 데이터 전달
            //AutoVarAccessScript.SetVar(target, varName, data, ignoreCase);

            return true;
        }else
        if (varType == null)
        {
            if (Data.GetType() == data.GetType())
            {
                ChangeDataEvent(data); //서버에게 데이터 전달
                return true;
            }
        }

        Debug.LogWarning("AutoAccess.Set<> - fail : Not Equal Type");
        return false;
    }
    //입출력 함수로 + 타입지정 + 동기화

    public string GetPath()
    {
        if (target != null)
            return target.GetType().FullName + "/" + varName;
        else
            return null;
    }

    //[Command(requiresAuthority = false)]
    void ChangeDataEvent<T>(T data)
    {
        //값 유효한지 검사// Set<> 으로 보낼때 비정상값 필터링

        Debug.Log("send event - Server : " + data.ToString());

        if (ChangeData != null)
        {
            //ChangeData.Invoke(Data, data);
            var result = ChangeData(Data, data);
            if (result != null)
            {
                Data = result;
            }else
                Debug.LogWarning("Vaule is null");
            //Data = ChangeData(Data, data);
        }
        else
            Debug.Log("send event - Not Setting Delegate | " + ChangeData );
        //changeData?.Invoke(Data, data);//데이터 변경 알림 (이전값 , 현제값)

        if (! typeof(T).IsPrimitive)
        {
            Debug.LogWarning("Maybe Notwork When Use Not BasicType");
        }

        //유효 검사는 델리게이트 ChangeData에서
    }//서버에 저장만

}
public static class AutoVarAccessScript
{

    public static object GetVar(Component target, string varName, bool ignoreCase = true)
    {
        if (target != null)
        {
            FieldInfo[] flds = target.GetType().GetFields();

            for (int i = 0; i < flds.Length; i++)
            {
                if (ignoreCase)
                {
                    if (flds[i].Name.ToLower() == varName.ToLower())
                    {
                        return flds[i].GetValue(target);
                    }
                }
                else
                {
                    if (flds[i].Name == varName)
                    {
                        return flds[i].GetValue(target);
                    }
                }
            }
        }

        return null;
    }
    public static T GetVar<T>(Component target, string varName, bool ignoreCase = true)
    {
        object Temp = GetVar(target, varName, ignoreCase);

        if (Temp != null)
        {
            if (Temp.GetType() == typeof(T))
            {
                return (T)GetVar(target, varName, ignoreCase);
            }
        }

        return default;
    }

    public static object GetVar(GameObject Target ,string varName, bool ignoreCase = true)
    {
        Component[] components = Target.GetComponents(typeof(MonoBehaviour));

        object result = null;

        for (int i = 0; i < components.Length; i++)
        {
            result = GetVar(components[i], varName, ignoreCase);
            if (result != null)
            {
                return result;
            }
        }

        return result;
    }
    public static T GetVar<T>(GameObject Target, string varName, bool ignoreCase = true)
    {
        return (T)GetVar(Target, varName, ignoreCase);
    }

    public static bool SetVar(Component target, string varName, object Data, bool ignoreCase = true)
    {
        if (target != null)
        {
            FieldInfo[] flds = target.GetType().GetFields();

            for (int i = 0; i < flds.Length; i++)
            {
                if (flds[i].FieldType == Data.GetType())
                {
                    if (ignoreCase)
                    {
                        if (flds[i].Name.ToLower() == varName.ToLower())
                        {
                            flds[i].SetValue(target, Data);
                            return true;
                        }
                    }
                    else
                    {
                        if (flds[i].Name.ToString() == varName)
                        {
                            flds[i].SetValue(target, Data);
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }
    public static bool SetVar(GameObject Target, string varName, object Data, bool ignoreCase = true)
    {
        Component[] components = Target.GetComponents(typeof(MonoBehaviour));

        for (int i = 0; i < components.Length; i++)
        {
            if (SetVar(components[i], varName, Data, ignoreCase))
            {
                return true;
            }
        }

        return false;
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(AutoVarAccess))]
public class AutovarAccessEditor : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label);
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //base.OnGUI(position, property, label);

        {
            /*
var Owner = GetPropertyDrawerTarget(fieldInfo, property);
MemberInfo[] infos = Owner.GetType().FindMembers
    (MemberTypes.All,
    BindingFlags.Public | BindingFlags.Instance,
    new MemberFilter(DelegateToSearch),
    "ReferenceEquals");


if (infos.Length > 0)
{
    Debug.Log("first member : " + infos[0].Name);

}
else
    Debug.Log("info is null");
*/
        }//Test MemberInfos

        var Owner = (AutoVarAccess)GetPropertyDrawerTarget(fieldInfo, property);

        if (Owner != null)
        {
            string path = (Owner).GetPath();

            if (string.IsNullOrEmpty(path) || Owner.Data == null)
            {
                GUI.color = Color.red;
                EditorGUI.LabelField(position, (label.text + " : Not Connected"));
            }
            else
            {
                if (Owner.VarType == Owner.Data.GetType() || Owner.VarType == null)
                {
                    GUI.color = Color.green;
                    EditorGUI.LabelField(position, (label.text +
                        " : Connect Variable To  [ " + (Owner).GetPath()) + " ]");
                }
                else
                {
                    GUI.color = Color.yellow;
                    EditorGUI.LabelField(position, (label.text +
                        " : Connect Variable To  [ " + (Owner).GetPath()) + " ] But Different Type");
                }
            }

            GUI.color = Color.white;
        }
    }

    public static object GetPropertyDrawerTarget(FieldInfo fieldInfo, SerializedProperty property)
    {
        object Lobj = null;

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

        return Lobj;
    }
    public bool DelegateToSearch(MemberInfo memberInfo, object Search)
    {
        //return memberInfo.Name.ToString() == Search.ToString();
        return true;
    }
}

#endif