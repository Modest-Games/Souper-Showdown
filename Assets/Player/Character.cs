using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "ScriptableObjects/Character")]
public class Character : ScriptableObject
{
    public string characterName;
    public GameObject characterPrefab;
    public Pollutant deadCharacter;
    public Color primaryColour;
    public SpriteAnimation animatedAvatar;
}
