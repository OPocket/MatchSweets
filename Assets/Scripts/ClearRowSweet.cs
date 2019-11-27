using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearRowSweet : ClearSweet {

    public override void Clear()
    {
        base.Clear();
        Sweet.GameManager.ClearRowSweets(Sweet.Y);
    }
}
