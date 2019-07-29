using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedDestruct : MonoBehaviour
{
    [SerializeField] private float _time = 10.0f;


    void Awake()
    {
        Invoke("DestroyNow", _time);
    }

    void DestroyNow()
    {
        DestroyObject(gameObject);
    }
}
