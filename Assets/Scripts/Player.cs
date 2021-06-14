using System;
using UnityEngine.Serialization;

[Serializable]
public class Player
{
    public string name;
    public CharacterBlueprint characterBlueprint;
    public int maxMesses;
    public int tech;
    public int cheek;
    public int tedium;
    public int freeDays;
    public int favours;
    public int messes;
    public int treasures;
    public bool abilityUsed;
    public bool inNewTurn = true;
}