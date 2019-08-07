using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New String List")]
public class StringList : ScriptableObject
{
    [SerializeField] List<string> _stringList = new List<string>();

    public string this [int i]
    {
        get
        {
            if( i < _stringList.Count)
            {
                return _stringList[i];
            }
            return null;
        }
    }

    public int Count
    {
        get
        {
            return _stringList.Count;
        }
    }
}
