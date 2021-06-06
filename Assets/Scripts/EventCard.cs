using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventCard : MonoBehaviour
{
    // UI
    public TMP_Text titleText;
    public GameObject trialObjects;
    public Image typeImage;
    public TMP_Text typeText;
    public TMP_Text difficultyText;
    public TMP_Text flavourText;
    public Image eventImage;
    public Image firstImage;
    public Image passImage;
    public Image failImage;
    public Image lastImage;
    public TMP_Text respiteText;
    public GameObject reverse;

    public Sprite typeImageTech;
    public Sprite typeImageCheek;
    public Sprite typeImageTedium;
    public Sprite reward0;
    public Sprite reward1;
    public Sprite reward2;
    public Sprite reward3;
    public Sprite reward4;
    public Sprite reward5;
    public Sprite trialBackground;
    public Sprite respiteBackground;

    // Game
    public enum EventTypes
    {
        Respite = 0,
        Tech = 1,
        Cheek = 2,
        Tedium = 3
    }

    public EventTypes EventTypeCode
    {
        get => eventTypeCode;
        set
        {
            switch (value)
            {
                case EventTypes.Respite:
                    typeText.text = "Respiro";
                    image.sprite = respiteBackground;
                    break;
                case EventTypes.Tech:
                    typeImage.sprite = typeImageTech;
                    typeText.text = "Prueba de Tecnología";
                    image.sprite = trialBackground;
                    break;
                case EventTypes.Cheek:
                    typeImage.sprite = typeImageCheek;
                    typeText.text = "Prueba de Morro";
                    image.sprite = trialBackground;
                    break;
                case EventTypes.Tedium:
                    typeImage.sprite = typeImageTedium;
                    typeText.text = "Prueba de Tedio";
                    image.sprite = trialBackground;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            trialObjects.SetActive(trialTypes.Contains(value));
            eventTypeCode = value;
        }
    }

    private EventTypes eventTypeCode;

    public enum RewardTypes
    {
        Nothing = 0,
        FreeDay = 1,
        Favour = 2,
        Mess = 3,
        Treasure = 4,
        Option = 5
    }

    public RewardTypes firstCode;
    public RewardTypes passCode;
    public RewardTypes failCode;
    public RewardTypes lastCode;

    private int difficulty;

    public int Difficulty
    {
        get => difficulty;
        set
        {
            difficultyText.text = value.ToString();
            difficulty = value;
        }
    }

    public enum RespiteTypes
    {
        Nothing,
        Subtract3FavoursAdd1TreasureOrInverse,
        Add1MessAdd4Favours,
        Subtract5FavoursSubtract1Mess,
        Add1MessAdd2FreeDay,
        Add1MessAdd2Treasures,
        Subtract2FavoursAdd1FreeDay,
        Subtract4FreeDaysSubtract1Mess
    }

    public RespiteTypes RespiteCode
    {
        get => respiteCode;
        set
        {
            respiteText.text = value switch
            {
                RespiteTypes.Nothing => "",
                RespiteTypes.Subtract3FavoursAdd1TreasureOrInverse =>
                    "Cada jugador puede descartar 3 Favores para obtener un Tesoro, " +
                    "Cada jugador puede descartar un Tesoro para obtener 3 Favores",
                RespiteTypes.Add1MessAdd4Favours =>
                    "Cada jugador puede comerse un Marrón para conseguir 4 Favores",
                RespiteTypes.Subtract5FavoursSubtract1Mess =>
                    "Cada jugador puede descartar 5 Favores para eliminar un Marrón",
                RespiteTypes.Add1MessAdd2FreeDay =>
                    "Cada jugador puede comerse un Marrón para conseguir 2 Días Libres",
                RespiteTypes.Add1MessAdd2Treasures =>
                    "Cada jugador puede comerse un Marrón para conseguir 2 Tesoros",
                RespiteTypes.Subtract2FavoursAdd1FreeDay =>
                    "Cada jugador puede descartar 2 Favores para conseguir un Día Libre",
                RespiteTypes.Subtract4FreeDaysSubtract1Mess =>
                    "Cada jugador puede descartar 4 Favores para eliminar un Marrón",
                _ => throw new ArgumentOutOfRangeException()
            };

            respiteCode = value;
        }
    }

    private RespiteTypes respiteCode;

    private Animator animator;
    private Image image;

    private readonly List<EventTypes> trialTypes = new List<EventTypes>
    {
        EventTypes.Tech, EventTypes.Cheek, EventTypes.Tedium
    };

    private static readonly int AnimTriggerDiscard = Animator.StringToHash("Discard");
    private static readonly int AnimTriggerFlip = Animator.StringToHash("Flip");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        image = GetComponent<Image>();
        reverse.SetActive(true);
    }

    public EventCard LoadBlueprint(EventBlueprint eventBlueprint)
    {
        titleText.text = eventBlueprint.title;
        EventTypeCode = eventBlueprint.typeCode;
        Difficulty = eventBlueprint.difficulty;
        flavourText.text = eventBlueprint.flavour;
        eventImage.sprite = eventBlueprint.eventImage;
        firstCode = eventBlueprint.firstCode;
        firstImage.sprite = RewardSprite(eventBlueprint.firstCode);
        passCode = eventBlueprint.passCode;
        passImage.sprite = RewardSprite(eventBlueprint.passCode);
        failCode = eventBlueprint.failCode;
        failImage.sprite = RewardSprite(eventBlueprint.failCode);
        lastCode = eventBlueprint.lastCode;
        lastImage.sprite = RewardSprite(eventBlueprint.lastCode);
        firstCode = eventBlueprint.firstCode;
        passCode = eventBlueprint.passCode;
        failCode = eventBlueprint.failCode;
        lastCode = eventBlueprint.lastCode;
        RespiteCode = eventBlueprint.respiteCode;
        return this;
    }

    public void Flip()
    {
        animator.SetTrigger(AnimTriggerFlip);
    }

    private Sprite RewardSprite(RewardTypes rewardCode)
    {
        return rewardCode switch
        {
            RewardTypes.Nothing => reward0,
            RewardTypes.FreeDay => reward1,
            RewardTypes.Favour => reward2,
            RewardTypes.Mess => reward3,
            RewardTypes.Treasure => reward4,
            RewardTypes.Option => reward5,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public void Discard()
    {
        animator.SetTrigger(AnimTriggerDiscard);
        Destroy(gameObject, 2);
    }
}