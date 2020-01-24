using System.Collections.Generic;
using UnityEngine;

public static class DialogsSettings 
{
    public static bool UseCharacterColors = true; 
    public static Dictionary<string, Color> CharactersColor { get; set; } = new Dictionary<string, Color>(); 
}
