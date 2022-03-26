using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectorPanel : MonoBehaviour
{
    public Character character;

    private SpriteAnimator spriteAnimator;
    private Image image;
    private Image background;

    public void Setup()
    {
        // setup variables
        spriteAnimator = transform.Find("Character_Image").GetComponent<SpriteAnimator>();
        image = spriteAnimator.GetComponent<Image>();
        background = transform.Find("Background").GetComponent<Image>();

        if (character.animatedAvatar != null)
        {
            image.color = Color.white;
            spriteAnimator.spriteAnimation = character.animatedAvatar;
        }

        if (character.primaryColour != null)
            background.color = character.primaryColour;
    }
}
