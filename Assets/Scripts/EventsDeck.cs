using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class EventsDeck : MonoBehaviour
{
    public GameObject topCard;
    public RectTransform eventCardsHolder;
    public GameObject eventCardPrefab;
    public EventCard currentCard;
    public TMP_Text cardsLeftText; 

    public static List<EventBlueprint> CardsBlueprints;
    public static List<EventBlueprint> Cards = new List<EventBlueprint>();
    public static List<int> Sequence;
    public static int Round;
    

    private void Awake()
    {
        CardsBlueprints = GameManager.FindResources<EventBlueprint>("Events");
        foreach (var position in Sequence) Cards.Add(CardsBlueprints[position]);
    }

    public void NewCard()
    {
        var totalCards = Cards.Count;
        if (Round == totalCards)
        {
            cardsLeftText.text = "";
            return;
        }
        if (currentCard != null) currentCard.Discard();
        var currentCardGo = Instantiate(eventCardPrefab, eventCardsHolder.position, Quaternion.identity, eventCardsHolder);
        currentCard = currentCardGo.GetComponent<EventCard>().LoadBlueprint(Cards[Round]);
        currentCard.Flip();
        Round += 1;
        if (Round == totalCards) topCard.SetActive(false);
        cardsLeftText.text = (totalCards - Round).ToString();
    }
}
