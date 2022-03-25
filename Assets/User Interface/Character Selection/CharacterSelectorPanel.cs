using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectorPanel : MonoBehaviour
{
    public Character character;

    private Image image;
    private Image background;

    public void Setup()
    {
        // setup variables
        image = transform.Find("Character_Image").GetComponent<Image>();
        background = transform.Find("Background").GetComponent<Image>();

        if (character.selectorSprite != null)
        {
            image.color = Color.white;
            image.sprite = character.selectorSprite;
        }

        if (character.primaryColour != null)
            background.color = character.primaryColour;
    }
}
