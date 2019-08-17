using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveInfo : InteractiveItem
{
    [SerializeField] private string _infoText;

    public override string GetText()
    {
        return _infoText;
    }
}
