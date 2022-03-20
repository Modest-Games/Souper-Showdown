using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapManager : MonoBehaviour
{
    public List<TrapBox> traps;

    public TrapBox GetTrap(string trapName, bool doLog = false)
    {
        TrapBox foundTrapBox = traps.Find(x => x.name == trapName);

        if (doLog)
            Debug.LogFormat("Name: {0}, Found TrapBox: {1}", trapName, foundTrapBox);

        return foundTrapBox;
    }

    public TrapBox GetTrap(int index)
    {
        return traps[index];
    }

    public TrapBox GetNextTrap(string trapName)
    {
        // get the old trap's index
        int oldTrapIndex = traps.IndexOf(traps.Find(x => x.name == trapName));

        // get the next trap's index value
        int newIndex = oldTrapIndex + 1;

        // clamp new index
        if (newIndex >= traps.Count)
            newIndex = 0;

        return GetTrap(newIndex);
    }

    public TrapBox GetPreviousTrap(string trapName)
    {
        // get the old trap's index
        int oldTrapIndex = traps.IndexOf(traps.Find(x => x.name == trapName));

        // get the previous trap's index value
        int newIndex = oldTrapIndex - 1;

        // clamp new index
        if (newIndex < 0)
            newIndex = traps.Count - 1;

        return GetTrap(newIndex);
    }
}
