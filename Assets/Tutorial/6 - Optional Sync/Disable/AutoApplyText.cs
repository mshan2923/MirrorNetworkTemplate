using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AutoApplyText : MonoBehaviour
{
    public UnityEngine.UI.Text CText;
    public string text;
    public string TextVaule
    {
        get => text;
        set
        {
            text = value;
            change();
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CText.text = text;

    }

    void change()
    {
        CText.text = text;
    }
}
