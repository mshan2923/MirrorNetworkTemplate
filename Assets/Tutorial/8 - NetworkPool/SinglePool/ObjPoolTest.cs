using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Mirror;

public class ObjPoolTest : NetworkObjectPool
{
    public float delay = 1f;
    public List<GameObject> Spawned = new List<GameObject>();

    private void Start()
    {
        if (isServer)
        {
            StartLoop();
        }
    }
    [Server]
    public void StartLoop()
    {
        StartCoroutine(loop());
    }
    IEnumerator loop()
    {
        yield return new WaitForSeconds(delay);

        GameObject obj = Spawn();
        if (obj != null)
            Spawned.Add(obj);

        StartCoroutine(loop());
    }

    public override void EventGenrate(GameObject Obj)
    {
        Obj.transform.SetParent(gameObject.transform);//µ¿±âÈ­ ¾ÈµÊ

        if (isServer)
        {
            Obj.transform.position = Vector3.up * Spawned.Count * 0.5f;
        }

        base.EventGenrate(Obj);
    }

    [Command(requiresAuthority = false)]
    public void SpawnToServer()
    {
        Debug.Log("--");
        GameObject obj = Spawn();
        if (obj != null)
            Spawned.Add(obj);
    }
    [Command(requiresAuthority = false)]
    public void DespawnToServer()
    {
        if (Spawned.Count > 0)
        {
            Despawn(Spawned[Spawned.Count - 1]);
            Spawned.RemoveAt(Spawned.Count - 1);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ObjPoolTest))]
public class ObjPoolTestEditor : Editor
{
    ObjPoolTest Onwer;
    private void OnEnable()
    {
        Onwer = target as ObjPoolTest;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Spawn"))
        {
            Onwer.SpawnToServer();
        }

        if (GUILayout.Button("Despawn"))
        {
            Onwer.DespawnToServer();
        }
    }
}
#endif