using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[CreateAssetMenu(fileName =  "Colors", menuName =  "ScriptableObjects/Colors", order =  1)]
public class Colors : ScriptableObject
{
    public Color32 textRed;
    public Color32 textBlue;
}
