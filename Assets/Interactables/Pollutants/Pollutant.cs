using UnityEngine;

[CreateAssetMenu(fileName = "NewPollutant", menuName = "ScriptableObjects/Pollutant")]
public class Pollutant : ScriptableObject
{
    public float effectAmount;
    public GameObject mesh;
}
