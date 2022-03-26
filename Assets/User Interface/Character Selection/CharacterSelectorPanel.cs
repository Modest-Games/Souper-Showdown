using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectorPanel : MonoBehaviour
{
    public Character character;
    public GameObject selectionIcon;

    private int numSelected;
    private SpriteAnimator spriteAnimator;
    private Image image;
    private Image background;
    private Transform selectionsPanel;

    public void Setup()
    {
        // setup variables
        spriteAnimator = transform.Find("Character_Image").GetComponent<SpriteAnimator>();
        image = spriteAnimator.GetComponent<Image>();
        background = transform.Find("Background").GetComponent<Image>();
        selectionsPanel = transform.Find("Selections_Panel");

        if (character.animatedAvatar != null)
        {
            image.color = Color.white;
            spriteAnimator.spriteAnimation = character.animatedAvatar;
        }

        if (character.primaryColour != null)
            background.color = character.primaryColour;

        RenderSelections();
    }

    private void RenderSelections()
    {
        // clear all selections
        foreach (Transform selection in selectionsPanel.transform)
        {
            Destroy(selection.gameObject);
        }

        // add the needed selections
        for (int i = 0; i < numSelected; i++)
        {
            Instantiate(selectionIcon, selectionsPanel);
        }

        // update enabled
        SetEnabled(numSelected > 0);
    }

    private void SetEnabled(bool enabled)
    {
        Sprite newSprite = image.sprite;

        // set the state of the sprite animatior
        spriteAnimator.isEnabled = enabled;
    }

    public void Select()
    {
        numSelected++;
        RenderSelections();
    }

    public void Deselect()
    {
        numSelected--;
        RenderSelections();
    }

    private void OnCharacterChanged(string oldCharName, string newCharName)
    {
        //Debug.LogFormat("This char: {0}, oldChar: {1}, newChar: {2}", character.characterName, oldCharName, newCharName);

        if (oldCharName == newCharName)
            return;

        if (character.characterName == newCharName)
            Select();

        if (character.characterName == oldCharName)
            Deselect();
    }

    private void OnEnable()
    {
        // bind events listeners
        PlayerController.CharacterChanged += OnCharacterChanged;
    }

    private void OnDisable()
    {
        // clear event listeners
        PlayerController.CharacterChanged -= OnCharacterChanged;
    }
}
