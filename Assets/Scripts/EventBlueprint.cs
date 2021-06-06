using UnityEngine;

[CreateAssetMenu(fileName = "Event", menuName = "ScriptableObjects/Event", order = 3)]
public class EventBlueprint : ScriptableObject
{
    public string title;
    public EventCard.EventTypes typeCode;
    public int difficulty;
    public string flavour;
    public Sprite eventImage;
    public EventCard.RewardTypes firstCode;
    public EventCard.RewardTypes passCode;
    public EventCard.RewardTypes failCode;
    public EventCard.RewardTypes lastCode;
    public EventCard.RespiteTypes respiteCode;
}
