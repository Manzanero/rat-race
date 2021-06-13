using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class OpponentCard : MonoBehaviour
{
    public TMP_Text opponentName;
    public Image image;
    public Dice dice;
    public TMP_Text bonusText;
    public TMP_Text resultText;
    public TMP_Text freeDaysText;
    public TMP_Text favoursText;
    public TMP_Text messesText;
    public TMP_Text treasuresText;
    public Toggle readyToggle;

    public Player remotePlayer;
    
    public int Bonus
    {
        get => bonus;
        set { bonusText.text = value >= 0 ? "+" + value : value.ToString(); bonus = value; }
    }
    
    private int bonus;

    public int Result
    {
        get => result;
        set { resultText.text = value.ToString(); result = value; }
    }
    
    private int result;

    public int Tech
    {
        get => tech;
        set => remotePlayer.tech = tech = value;
    }
    
    private int tech;
    
    public int Cheek
    {
        get => cheek;
        set => remotePlayer.cheek = cheek = value;
    }
    
    private int cheek;
    
    public int Tedium
    {
        get => tedium;
        set => remotePlayer.tedium = tedium = value;
    }
    
    private int tedium;
    
    public int FreeDays
    {
        get => freeDays;
        set { remotePlayer.freeDays = freeDays = value; freeDaysText.text = value.ToString(); }
    }
    
    private int freeDays;
    
    public int Favours
    {
        get => favours;
        set { remotePlayer.favours = favours = value; favoursText.text = value.ToString(); }
    }
    
    private int favours;
    
    public int Messes
    {
        get => messes;
        set { remotePlayer.messes = messes = value; messesText.text = value.ToString(); }
    }
    
    private int messes;
    
    public int Treasures
    {
        get => treasures;
        set { remotePlayer.treasures = treasures = value; treasuresText.text = value.ToString(); }
    }

    public int secretBonus;
    public int secretResult;
    
    private int treasures;
    
    private void Awake()
    {
        dice.PickUp();
        bonusText.text = "+0";
        resultText.text = "0";
        readyToggle.isOn = false;
        FreeDays = 0;
        Favours = 0;
        Messes = 0;
        Treasures = 0;
    }

    public OpponentCard LoadPlayer(Player player)
    {
        opponentName.text = player.name;
        var characterBlueprint = player.characterBlueprint;
        image.sprite = characterBlueprint.image;
        remotePlayer = player;
        Tech = characterBlueprint.tech;
        Cheek = characterBlueprint.cheek;
        Tedium = characterBlueprint.tedium;
        return this;
    }
    
}
