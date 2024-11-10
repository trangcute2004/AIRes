using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Table : MonoBehaviour
{
    public bool IsOccupied { get; private set; }
    public bool IsDirty { get; private set; }

    public void Occupy() => IsOccupied = true;
    public void Vacate() => IsOccupied = false;
    public void SetDirty() => IsDirty = true;
    public void Clean() => IsDirty = false;
}
