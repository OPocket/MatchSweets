using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearColumSweet : ClearSweet {

    public override void Clear()
    {
        base.Clear();
        Sweet.GameManager.ClearColumSweets(Sweet.X);
    }
}
