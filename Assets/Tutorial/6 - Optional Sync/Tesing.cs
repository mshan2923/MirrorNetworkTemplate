using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Tesing : MonoBehaviour
{
    public GenericVar genericVar = new GenericVar(typeof(string), "Testing");
    public VarCollection collection = new VarCollection();
    public string Test = "";

    public AutoVarAccess varAccess;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[CustomEditor(typeof(Tesing))]
public class TesingEditor : Editor
{
    Tesing onwer;
    private void OnEnable()
    {
        onwer = target as Tesing;   
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Reset AutoVarAccess"))
        {
            onwer.varAccess = new AutoVarAccess(onwer, "genericVar", null);
        }

        if (GUILayout.Button("AutoVarAccess"))
        {
            if (onwer.varAccess == null)
                onwer.varAccess = new AutoVarAccess(onwer, "genericVar", null);

            Debug.Log("AutoVarAccess : " + ((GenericVar)onwer.varAccess.Data).Get<string>());//GenericVar

            //onwer.varAccess.Data.data = JsonUtility.ToJson(new Wrap<string>(Random.value.ToString()));

            ((GenericVar)onwer.varAccess.Data).Set(Random.value.ToString());
        }
    }
}
