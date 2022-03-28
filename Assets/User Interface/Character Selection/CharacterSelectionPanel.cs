using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectionPanel : MonoBehaviour
{
    public GameObject characterSelectionPanel_prefab;

    void Start()
    {
        // clear existing characters
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // create character selector panels for each character in the character manager
        foreach (Character character in CharacterManager.Instance.characterList)
        {
            GameObject newObj = Instantiate(characterSelectionPanel_prefab, transform);
            CharacterSelectorPanel newCharSelector = newObj.GetComponent<CharacterSelectorPanel>();
            newCharSelector.character = character;
            newCharSelector.Setup();
        }
    }
}
