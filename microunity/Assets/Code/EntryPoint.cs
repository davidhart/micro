using UnityEngine;
using System.Collections;

public class EntryPoint : MonoBehaviour
{
    public Session Session;

    void Start()
    {
        GameObject.DontDestroyOnLoad(gameObject);

        Session = gameObject.AddComponent<Session>();

        Application.LoadLevel("Menu_Connect");
    }
}
