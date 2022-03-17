using UnityEngine;
using System;

/// <summary>
/// EXample of a Custom Character Input Validator to only allow digits from 0 to 9.
/// </summary>
[Serializable]
[CreateAssetMenu(fileName = "InputValidator - IPAddress.asset", menuName = "TextMeshPro/Input Validators/IPAddress", order = 100)]
public class IPAddressInputValidator : TMPro.TMP_InputValidator
{
    public int characterLimit;

    // Custom text input validation function
    public override char Validate(ref string text, ref int pos, char ch)
    {
        if (((ch >= '0' && ch <= '9') || ch == '.' || ch == ':') && text.Length < characterLimit)
        {
            text += ch;
            pos += 1;
            return ch;
        }
        return (char)0;
    }
}
