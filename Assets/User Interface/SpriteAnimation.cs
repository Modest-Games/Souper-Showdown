using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSpriteAnimation", menuName = "ScriptableObjects/SpriteAnimation")]
public class SpriteAnimation : ScriptableObject
{
    public Sprite[] frames;
    public float duration;
}
