using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectorPanel : MonoBehaviour
{
    public Character character;
    public GameObject selectionIcon;

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

        OnPlayerListChanged();
    }

    private void SetEnabled(bool enabled)
    {
        Sprite newSprite = image.sprite;

        // set the state of the sprite animatior
        spriteAnimator.isEnabled = enabled;
    }

    private void OnPlayerListChanged()
    {
        int selections = 0;

        // clear all selections
        foreach (Transform selection in selectionsPanel.transform)
        {
            Destroy(selection.gameObject);
        }

        // create new selections
        for (int i = 0; i < PlayersManager.Instance.players.Count; i ++)
        {
            if (PlayersManager.Instance.players[i].character == character.characterName)
            {
                selections++;
                GameObject newSelection = Instantiate(selectionIcon, selectionsPanel);
                newSelection.GetComponentInChildren<TMP_Text>().text = (i+1).ToString();
            }
        }

        SetEnabled(selections > 0);
    }

    private void OnEnable()
    {
        // bind events listeners
        PlayersManager.PlayerListChanged += OnPlayerListChanged;
    }

    private void OnDisable()
    {
        // clear event listeners
        PlayersManager.PlayerListChanged -= OnPlayerListChanged;
    }
}
