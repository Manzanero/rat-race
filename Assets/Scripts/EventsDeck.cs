using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class EventsDeck : MonoBehaviour
{
    private const int TotalCards = 16;
    
    public GameObject topCard;
    public RectTransform eventCardsHolder;
    public GameObject eventCardPrefab;
    public EventCard currentCard;
    public TMP_Text cardsLeftText; 

    public List<EventBlueprint> cardOrder;
    public int round;
    

    private void Awake()
    {
        cardOrder = GameManager.FindResources<EventBlueprint>("Events");
        cardOrder = cardOrder.OrderBy(i => Random.value).ToList();
    }

    public void NewCard()
    {
        if (round == cardOrder.Count)
        {
            cardsLeftText.text = "";
            return;
        }
        if (currentCard != null) currentCard.Discard();
        var currentCardGo = Instantiate(eventCardPrefab, eventCardsHolder.position, Quaternion.identity, eventCardsHolder);
        currentCard = currentCardGo.GetComponent<EventCard>().LoadBlueprint(cardOrder[round]);
        currentCard.Flip();
        round += 1;
        if (round == cardOrder.Count) topCard.SetActive(false);
        cardsLeftText.text = (TotalCards - round).ToString();
    }
}
