using UnityEngine;

[CreateAssetMenu(fileName = "NewPollutant", menuName = "ScriptableObjects/Pollutant")]
public class Pollutant : ScriptableObject
{
    public string type;
    public GameObject mesh;
    public float effectAmount;
}
