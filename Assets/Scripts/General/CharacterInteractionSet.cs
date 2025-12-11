using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterInteractionSet
{
    public Transform character;                         // The character
    public List<Interactable> interactables = new();    // Interactables that belong to THAT character
}