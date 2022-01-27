using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTrapBox", menuName = "ScriptableObjects/Trap Box")]
public class TrapBox : ScriptableObject
{
    public Color boxColor;
    public GameObject trapPrefab;
}
