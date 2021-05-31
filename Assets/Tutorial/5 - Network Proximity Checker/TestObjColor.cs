using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TestObjColor : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnChangeColor))]
    public Color color = Color.yellow;

    void OnChangeColor(Color oldColor, Color newColor)
    {
        gameObject.GetComponent<Renderer>().material.color = newColor;
    }

}
