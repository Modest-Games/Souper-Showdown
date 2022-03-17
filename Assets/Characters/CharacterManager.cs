using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;

    public List<Character> characterList;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public Character GetRandomCharacter()
    {
        return GetCharacter(Random.Range(0, characterList.Count));
    }

    public Character GetCharacter(int index)
    {
        return characterList[index];
    }

    public Character GetNextCharacter(string characterName)
    {
        // get the old character's index
        int oldCharacterIndex = characterList.IndexOf(characterList.Find(x => x.characterName == characterName));

        // get the next index value
        int newIndex = oldCharacterIndex + 1;

        // clamp new index
        // who needs modulo anyways
        if (newIndex == characterList.Count) {
            newIndex = 0;
        }

        return GetCharacter(newIndex);
    }

    public Character GetPreviousCharacter(string characterName)
    {
        // get the old character's index
        int oldCharacterIndex = characterList.IndexOf(characterList.Find(x => x.characterName == characterName));

        // get the previous index value
        int newIndex = oldCharacterIndex - 1;

        // clamp new index
        // who needs modulo anyways
        if (newIndex < 0) {
            newIndex = characterList.Count - 1;
        }

        return GetCharacter(newIndex);
    }

    public Character GetCharacter(string name, bool doLog)
    {
        Character foundCharacter = characterList.Find(x => x.characterName == name);

        if (doLog)
            Debug.LogFormat("Name: {0}, Found character: {1}", name, foundCharacter);

        return foundCharacter;
    }

    public Character GetCharacter(string name)
    {
        return GetCharacter(name, false);
    }
}
