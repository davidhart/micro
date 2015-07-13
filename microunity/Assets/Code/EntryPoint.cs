using UnityEngine;
using System.Collections;

public class EntryPoint : MonoBehaviour
{
    void Start()
    {
        GameObject.DontDestroyOnLoad(gameObject);

        gameObject.AddComponent<ServerSession>();
        gameObject.AddComponent<Session>();

        Application.LoadLevel("Menu_Connect");
    }
}
