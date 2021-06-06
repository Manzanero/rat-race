using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCard : MonoBehaviour
{
    public TMP_Text title;
    public Image image;
    public TMP_Text messText;
    public TMP_Text techText;
    public TMP_Text cheekText;
    public TMP_Text tediumText;
    public TMP_Text abilityText;
    public Button abilityButton;
    
    public int Tech
    {
        get => tech;
        set
        {
            GameManager.LocalPlayer.tech = value;
            techText.text = value >= 0 ? "+" + value : value.ToString(); 
            tech = value;
        }
    }
    
    private int tech;
    
    public int Cheek
    {
        get => cheek;
        set
        {
            GameManager.LocalPlayer.cheek = value;
            cheekText.text = value >= 0 ? "+" + value : value.ToString(); 
            cheek = value;
        }
    }
    
    private int cheek;
    
    public int Tedium
    {
        get => tedium;
        set
        {
            GameManager.LocalPlayer.tedium = value;
            tediumText.text = value >= 0 ? "+" + value : value.ToString(); 
            tedium = value;
        }
    }
    
    private int tedium;

    private void Awake()
    {
        abilityButton.onClick.AddListener(delegate { abilityButton.gameObject.SetActive(false); });
    }

    public void LoadPlayer(Player player)
    {
        var characterBlueprint = player.characterBlueprint;
        title.text = characterBlueprint.title;
        image.sprite = characterBlueprint.image;
        messText.text = characterBlueprint.maxMesses.ToString();
        Tech = characterBlueprint.tech;
        Cheek = characterBlueprint.cheek;
        Tedium = characterBlueprint.tedium;
        abilityText.text = characterBlueprint.ability;
    }
}