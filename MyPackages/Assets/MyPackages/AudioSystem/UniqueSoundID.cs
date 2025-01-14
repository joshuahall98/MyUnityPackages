using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniqueSoundID
{
    public string soundID { get; }

    public UniqueSoundID()
    {
        string newGuid = Guid.NewGuid().ToString();
        string newDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");

        soundID = newGuid + newDateTime;
    }
}
