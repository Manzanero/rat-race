using UnityEngine;

[CreateAssetMenu(fileName = "Treasure", menuName = "ScriptableObjects/Treasure", order = 2)]
public class TreasureBlueprint : ScriptableObject
{
    public string title;
    public string flavour;
    public Sprite image;
    public string ability;
}
