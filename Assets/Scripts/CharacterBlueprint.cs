using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Character", menuName = "ScriptableObjects/Character", order = 1)]
public class CharacterBlueprint : ScriptableObject
{
    public string title;
    public string flavour;
    public Sprite image;
    public int maxMesses;
    public int tech;
    public int cheek;
    public int tedium;
    public string ability;
}
