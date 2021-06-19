using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class ControllOptionalSync : MonoBehaviour
{
    public OptionalSync optionalSync;
    public TextMesh textMesh;

    public float Distance = 2f;
    public float Delay = 1f;

    void Start()
    {
        optionalSync.SetUp(ValidEvent, ConditionEvent, SyncUpdated);

        Loop();
        //������ �ֱ������� ����
    }

    private bool ValidEvent(OptionalSync.OptionalMessage Data)
    {
        return true;
    }
    private bool ConditionEvent(int ConnID)
    {
        var ConnObj = NetworkServer.connections[ConnID].identity.gameObject;

        return (ConnObj.transform.position - gameObject.transform.position).sqrMagnitude <= (Distance * Distance);
    }
    private void SyncUpdated(bool Updated)
    {
        gameObject.GetComponent<Renderer>().material.color = Updated ? Color.white : Color.red;
        if (Updated)
        {
            textMesh.text = optionalSync.Get<float>().ToString("0.#");//�̶� �� �������� ������
        }
    }

    [ServerCallback]
    void Loop()
    {
        StartCoroutine(SyncLoop());
    }
    IEnumerator SyncLoop()
    {
        yield return new WaitForSeconds(Delay);

        optionalSync.Set(optionalSync.Convert(UnityEngine.Random.value * 10));//�����͸� �������� ����

        StartCoroutine(SyncLoop());
    }
}
