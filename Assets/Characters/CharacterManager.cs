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

    public Character GetCharacter(int index)
    {
        return characterList[index];
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
