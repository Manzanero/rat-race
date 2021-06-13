using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // UI
    public RectTransform freeDaysPanel;
    public RectTransform favoursPanel;
    public RectTransform messesPanel;
    public RectTransform messesEmptyPanel;

    public Button objectsButton;
    public RectTransform treasuresHolder;
    public GameObject treasurePrefab;

    public Button throwButton;
    public Dice dice;
    public TMP_Text bonusText;
    public Button unUseFavourButton;
    public Button useFavourButton;
    public TMP_Text favoursText;

    public TMP_Text resultText;
    public GameObject firstMedalGo;
    public GameObject passMedalGo;
    public GameObject failMedalGo;
    public GameObject lastMedalGo;

    public Button readyButton;
    public Toggle readyToggle;

    public EventsDeck eventsDeck;
    public RectTransform eventCardsHolder;
    public CharacterCard characterCard;
    public RectTransform opponentsHolder;
    public GameObject opponentPrefab;

    public GameObject resolutionGo;
    public RectTransform firstHolder;
    public RectTransform passHolder;
    public RectTransform failHolder;
    public RectTransform lastHolder;
    public GameObject addFreeDayButton;
    public GameObject addFavourButton;
    public GameObject addMessButton;
    public GameObject addTreasureButton;
    public GameObject subtractFavourButton;
    public RectTransform respiteHolder;
    public GameObject subtract3FavoursAdd1TreasureButton;
    public GameObject subtract1TreasureAdd3FavoursButton;
    public GameObject add1MessAdd4FavoursButton;
    public GameObject subtract5FavoursSubtract1MessButton;
    public GameObject add1MessAdd2FreeDayButton;
    public GameObject add1MessAdd2TreasuresButton;
    public GameObject subtract2FavoursAdd1FreeDayButton;
    public GameObject subtract4FreeDaysSubtract1MessButton;
    public Button pasoButton;

    // Game Variables
    public static bool Debug = true;
    public static Player LocalPlayer;
    public static List<OpponentCard> OpponentsCards = new List<OpponentCard>();
    public static List<TreasureCard> TreasureCards = new List<TreasureCard>();

    public static OpponentCard GetOpponentCardByPlayerName(string playerName)
    {
        return OpponentsCards.FirstOrDefault(x => x.remotePlayer.name == playerName);
    }

    public int FreeDays
    {
        get => freeDays;
        set
        {
            freeDaysPanel.sizeDelta = new Vector2(value != 0 ? value * 30 + 5 : 0, 35);
            freeDays = value;
        }
    }

    private int freeDays;

    public int Favours
    {
        get => favours;
        set
        {
            favoursPanel.sizeDelta = new Vector2(value != 0 ? value * 30 + 5 : 0, 35);
            favours = value;
        }
    }

    private int favours;

    public int Messes
    {
        get => messes;
        set
        {
            messesPanel.sizeDelta = new Vector2(value != 0 ? value * 30 + 5 : 0, 35);
            messes = value;
        }
    }

    private int messes;

    public int MaxMesses
    {
        get => maxMesses;
        set
        {
            messesEmptyPanel.sizeDelta = new Vector2(value != 0 ? value * 30 + 5 : 0, 35);
            maxMesses = value;
        }
    }

    private int maxMesses;

    public void AddTreasure()
    {
        Instantiate(treasurePrefab, treasuresHolder);
    }

    public void SubtractTreasure()
    {
        Destroy(treasuresHolder.GetChild(0));
    }

    public int Bonus
    {
        get => bonus;
        set
        {
            bonusText.text = value >= 0 ? "+" + value : value.ToString();
            bonus = value;
        }
    }

    private int bonus;

    public int FavoursToUse
    {
        get => favoursToUse;
        set
        {
            if (value > Favours) return;
            if (value < 0) return;
            favoursText.text = "+" + value;
            favoursToUse = value;
        }
    }

    private int favoursToUse;

    public int Result
    {
        get => result;
        set
        {
            resultText.text = value.ToString();
            result = value;
        }
    }

    private int result;

    public void ActivateMedals(bool activeFirstMedal, bool activePassMedal, bool activeFailMedal, bool activeLastMedal)
    {
        firstMedalGo.SetActive(activeFirstMedal);
        passMedalGo.SetActive(activePassMedal);
        failMedalGo.SetActive(activeFailMedal);
        lastMedalGo.SetActive(activeLastMedal);
    }

    public static Player TestPlayer()
    {
        var testCharacterBlueprint = FindResource<CharacterBlueprint>("Chars/Char0ElInformatico");
        return new Player
        {
            name = "Default",
            characterBlueprint = testCharacterBlueprint,
            maxMesses = testCharacterBlueprint.maxMesses,
            tech = testCharacterBlueprint.tech,
            cheek = testCharacterBlueprint.cheek,
            tedium = testCharacterBlueprint.tedium,
            abilityUsed = false
        };
    }

    private List<OpponentCard> TestOpponents()
    {
        var testCharacterBlueprint1 = FindResource<CharacterBlueprint>("Chars/Char1ElHijoDelJefe");
        var testCharacterBlueprint2 = FindResource<CharacterBlueprint>("Chars/Char2LaWorkaholica");
        return new List<OpponentCard>
        {
            NewOpponent(testCharacterBlueprint1, "Opponent Default 1"),
            NewOpponent(testCharacterBlueprint2, "Opponent Default 2")
        };
    }

    public OpponentCard NewOpponent(CharacterBlueprint blueprint, string opponentName)
    {
        var player = new Player
        {
            name = opponentName,
            characterBlueprint = blueprint,
            maxMesses = blueprint.maxMesses,
            tech = blueprint.tech,
            cheek = blueprint.cheek,
            tedium = blueprint.tedium,
            abilityUsed = false
        };
        return Instantiate(opponentPrefab, opponentsHolder).GetComponent<OpponentCard>().LoadPlayer(player);
    }

    private Animator resolutionAnimator;

    private bool isLoading = true;
    private bool isNewTurn;
    private bool isThinkingPhase;
    private bool isWaitingOpponentPhase;

    private bool isDiceThrown;
    private readonly List<int> opponentsResult = new List<int>();
    private static readonly int AnimTriggerFadeIn = Animator.StringToHash("FadeIn");
    private static readonly int AnimTriggerRespiteFadeIn = Animator.StringToHash("RespiteFadeIn");
    private static readonly int AnimTriggerFadeOut = Animator.StringToHash("FadeOut");
    private static readonly int AnimTriggerRespiteFadeOut = Animator.StringToHash("RespiteFadeOut");

    private void Awake()
    {
        Instance = this;
        LocalPlayer ??= TestPlayer();
        FreeDays = 0;
        Favours = 0;
        Messes = 0;
        MaxMesses = LocalPlayer.maxMesses;
        objectsButton.onClick.AddListener(delegate { UnityEngine.Debug.Log("Object selection"); });
        objectsButton.interactable = false;
        foreach (Transform child in treasuresHolder) Destroy(child.gameObject);
        throwButton.onClick.AddListener(delegate
        {
            dice.Throw();
            isDiceThrown = true;
        });
        throwButton.interactable = false;
        Bonus = 0;
        useFavourButton.interactable = false;
        unUseFavourButton.interactable = false;
        useFavourButton.onClick.AddListener(delegate { FavoursToUse += 1; });
        unUseFavourButton.onClick.AddListener(delegate { FavoursToUse -= 1; });
        FavoursToUse = 0;
        Result = 0;
        ActivateMedals(false, false, false, false);
        readyButton.onClick.RemoveAllListeners();
        readyButton.onClick.AddListener(delegate
        {
            readyToggle.isOn = true;
            isWaitingOpponentPhase = true;
        });
        readyButton.interactable = false;
        readyToggle.isOn = false;

        foreach (Transform child in eventCardsHolder) Destroy(child.gameObject);

        characterCard.LoadPlayer(LocalPlayer);

        foreach (Transform child in opponentsHolder) Destroy(child.gameObject);
        if (!OpponentsCards.Any()) OpponentsCards = TestOpponents();

        resolutionAnimator = resolutionGo.GetComponent<Animator>();
    }

    private void Start()
    {
        StartCoroutine(WaitStartingTime());
    }

    private IEnumerator WaitStartingTime()
    {
        yield return new WaitForSeconds(2);
        isLoading = false;
        isNewTurn = true;
    }

    private void Update()
    {
        if (isLoading)
        {
            return;
        }

        if (isNewTurn)
        {
            eventsDeck.NewCard();
            if (eventsDeck.currentCard.EventTypeCode != EventCard.EventTypes.Respite)
            {
                objectsButton.interactable = true;
                throwButton.interactable = true;
            }

            dice.PickUp();
            firstMedalGo.SetActive(false);
            passMedalGo.SetActive(false);
            failMedalGo.SetActive(false);
            lastMedalGo.SetActive(false);
            readyButton.interactable = false;
            readyToggle.isOn = false;

            foreach (var opponent in OpponentsCards)
            {
                opponent.readyToggle.isOn = false;
                opponent.dice.PickUp();
            }

            isNewTurn = false;
            isDiceThrown = false;
            isThinkingPhase = true;

            if (Debug)
            {
                foreach (var opponent in OpponentsCards)
                {
                    opponent.dice.Throw();
                    opponent.readyToggle.isOn = true;
                }
            }
        }

        if (isThinkingPhase)
        {
            opponentsResult.Clear();
            
            Bonus = eventsDeck.currentCard.EventTypeCode switch
            {
                EventCard.EventTypes.Tech => LocalPlayer.tech,
                EventCard.EventTypes.Cheek => LocalPlayer.cheek,
                EventCard.EventTypes.Tedium => LocalPlayer.tedium,
                EventCard.EventTypes.Respite => 0,
                _ => throw new ArgumentOutOfRangeException()
            };
            Result = dice.result + Bonus + FavoursToUse;
            opponentsResult.Add(Result);

            foreach (var opponent in OpponentsCards)
            {
                opponent.Bonus = eventsDeck.currentCard.EventTypeCode switch
                {
                    EventCard.EventTypes.Tech => opponent.remotePlayer.tech,
                    EventCard.EventTypes.Cheek => opponent.remotePlayer.cheek,
                    EventCard.EventTypes.Tedium => opponent.remotePlayer.tedium,
                    EventCard.EventTypes.Respite => 0,
                    _ => throw new ArgumentOutOfRangeException()
                };
                opponent.Result = opponent.dice.result + opponent.Bonus;
                opponentsResult.Add(opponent.Result);
            }

            var isMax = Result >= opponentsResult.Max();
            var isPassed = Result >= eventsDeck.currentCard.Difficulty;
            var isMin = Result <= opponentsResult.Min();
            firstMedalGo.SetActive(isMax);
            passMedalGo.SetActive(isPassed);
            failMedalGo.SetActive(!isPassed);
            lastMedalGo.SetActive(isMin);
            
            if (eventsDeck.currentCard.EventTypeCode != EventCard.EventTypes.Respite)
            {
                useFavourButton.interactable = FavoursToUse < Favours;
                unUseFavourButton.interactable = FavoursToUse > 0;
            }
            
            if (Result >= opponentsResult.Max()) firstMedalGo.SetActive(true);

            if (OpponentsCards.All(x => x.dice.result != 0) && dice.result != 0)
                readyButton.interactable = true;

            if (eventsDeck.currentCard.EventTypeCode == EventCard.EventTypes.Respite)
                readyButton.interactable = true;

            if (isDiceThrown) throwButton.interactable = false;
            if (isWaitingOpponentPhase) isThinkingPhase = false;
        }

        if (isWaitingOpponentPhase)
        {
            objectsButton.interactable = false;
            throwButton.interactable = false;
            useFavourButton.interactable = false;
            unUseFavourButton.interactable = false;
            readyButton.interactable = false;

            if (OpponentsCards.All(x => x.readyToggle.isOn))
            {
                StartCoroutine(eventsDeck.currentCard.EventTypeCode == EventCard.EventTypes.Respite
                    ? RespiteResolution()
                    : TrialResolution());
            }
        }
    }

    private IEnumerator RespiteResolution()
    {
        isWaitingOpponentPhase = false;
        isLoading = true;
        resolutionAnimator.SetTrigger(AnimTriggerRespiteFadeIn);

        foreach (Transform child in respiteHolder) Destroy(child.gameObject);
        pasoButton.interactable = true;
        pasoButton.onClick.RemoveAllListeners();

        var respiteButtons = new List<Button> {pasoButton};
        Button respiteButton;
        Button respiteButton2;
        switch (eventsDeck.currentCard.RespiteCode)
        {
            case EventCard.RespiteTypes.Subtract3FavoursAdd1TreasureOrInverse:
                respiteButton = Instantiate(subtract3FavoursAdd1TreasureButton, respiteHolder).GetComponent<Button>();
                respiteButton2 = Instantiate(subtract1TreasureAdd3FavoursButton, respiteHolder).GetComponent<Button>();
                respiteButton.onClick.AddListener(Subtract3FavoursAdd1Treasure);
                respiteButton2.onClick.AddListener(Subtract1TreasureAdd3Favours);
                respiteButton.onClick.AddListener(delegate
                {
                    respiteButton2.interactable = false;
                    pasoButton.interactable = false;
                });
                respiteButton2.onClick.AddListener(delegate
                {
                    respiteButton.interactable = false;
                    pasoButton.interactable = false;
                });
                pasoButton.onClick.AddListener(delegate { respiteButton2.interactable = false; });
                respiteButton.interactable = Favours >= 3;
                respiteButton2.interactable = TreasureCards.Count >= 1;
                respiteButtons.Add(respiteButton);
                respiteButtons.Add(respiteButton2);
                break;
            case EventCard.RespiteTypes.Add1MessAdd4Favours:
                respiteButton = Instantiate(add1MessAdd4FavoursButton, respiteHolder).GetComponent<Button>();
                respiteButton.onClick.AddListener(Add1MessAdd4Favours);
                respiteButton.onClick.AddListener(delegate { pasoButton.interactable = false; });
                respiteButtons.Add(respiteButton);
                break;
            case EventCard.RespiteTypes.Subtract5FavoursSubtract1Mess:
                respiteButton = Instantiate(subtract5FavoursSubtract1MessButton, respiteHolder).GetComponent<Button>();
                respiteButton.onClick.AddListener(Subtract5FavoursSubtract1Mess);
                respiteButton.onClick.AddListener(delegate { pasoButton.interactable = false; });
                respiteButton.interactable = Favours >= 5 && Messes >= 1;
                respiteButtons.Add(respiteButton);
                break;
            case EventCard.RespiteTypes.Add1MessAdd2FreeDay:
                respiteButton = Instantiate(add1MessAdd2FreeDayButton, respiteHolder).GetComponent<Button>();
                respiteButton.onClick.AddListener(Add1MessAdd2FreeDay);
                respiteButton.onClick.AddListener(delegate { pasoButton.interactable = false; });
                respiteButtons.Add(respiteButton);
                break;
            case EventCard.RespiteTypes.Add1MessAdd2Treasures:
                respiteButton = Instantiate(add1MessAdd2TreasuresButton, respiteHolder).GetComponent<Button>();
                respiteButton.onClick.AddListener(Add1MessAdd2Treasures);
                respiteButton.onClick.AddListener(delegate { pasoButton.interactable = false; });
                respiteButtons.Add(respiteButton);
                break;
            case EventCard.RespiteTypes.Subtract2FavoursAdd1FreeDay:
                respiteButton = Instantiate(subtract2FavoursAdd1FreeDayButton, respiteHolder).GetComponent<Button>();
                respiteButton.onClick.AddListener(Subtract2FavoursAdd1FreeDay);
                respiteButton.onClick.AddListener(delegate { pasoButton.interactable = false; });
                respiteButton.interactable = Favours >= 2;
                respiteButtons.Add(respiteButton);
                break;
            case EventCard.RespiteTypes.Subtract4FreeDaysSubtract1Mess:
                respiteButton = Instantiate(subtract4FreeDaysSubtract1MessButton, respiteHolder).GetComponent<Button>();
                respiteButton.onClick.AddListener(Subtract4FreeDaysSubtract1Mess);
                respiteButton.onClick.AddListener(delegate { pasoButton.interactable = false; });
                respiteButton.interactable = FreeDays >= 4 && Messes >= 1;
                respiteButtons.Add(respiteButton);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (respiteButton != null) pasoButton.onClick.AddListener(delegate { respiteButton.interactable = false; });

        while (!respiteButtons.TrueForAll(x => !x.interactable))
            yield return null;

        yield return new WaitForSeconds(2);
        resolutionAnimator.SetTrigger(AnimTriggerRespiteFadeOut);
        yield return new WaitForSeconds(1);
        eventsDeck.currentCard.Discard();
        yield return new WaitForSeconds(1);
        isLoading = false;
        isNewTurn = true;
    }

    private void Subtract3FavoursAdd1Treasure()
    {
        Favours -= 3;
        AddTreasure();
    }

    private void Subtract1TreasureAdd3Favours()
    {
        Favours += 3;
        SubtractTreasure();
    }

    private void Add1MessAdd4Favours()
    {
        Messes += 1;
        Favours += 4;
    }

    private void Subtract5FavoursSubtract1Mess()
    {
        Favours -= 5;
        Messes -= 1;
    }

    private void Add1MessAdd2FreeDay()
    {
        FreeDays += 2;
        Messes += 1;
    }

    private void Add1MessAdd2Treasures()
    {
        AddTreasure();
        AddTreasure();
        Messes += 1;
    }

    private void Subtract2FavoursAdd1FreeDay()
    {
        Favours -= 2;
        FreeDays += 1;
    }

    private void Subtract4FreeDaysSubtract1Mess()
    {
        Messes += 1;
        FreeDays -= 4;
    }
    
    private IEnumerator TrialResolution()
    {
        isWaitingOpponentPhase = false;
        isLoading = true;
        Favours -= FavoursToUse;
        FavoursToUse = 0;
        resolutionAnimator.SetTrigger(AnimTriggerFadeIn);
        yield return new WaitForSeconds(3);

        foreach (Transform child in passHolder) Destroy(child.gameObject);
        foreach (Transform child in firstHolder) Destroy(child.gameObject);
        foreach (Transform child in failHolder) Destroy(child.gameObject);
        foreach (Transform child in lastHolder) Destroy(child.gameObject);

        var isMax = Result >= opponentsResult.Max();
        var isPassed = Result >= eventsDeck.currentCard.Difficulty;
        var isMin = Result <= opponentsResult.Min();

        var rewardButtons = new List<Button>();
        Button firstRewardButton;
        switch (eventsDeck.currentCard.firstCode)
        {
            case EventCard.RewardTypes.FreeDay:
                firstRewardButton = Instantiate(addFreeDayButton, firstHolder).GetComponent<Button>();
                firstRewardButton.interactable = isMax;
                firstRewardButton.onClick.AddListener(AddFFreeDay);
                rewardButtons.Add(firstRewardButton);
                break;
            case EventCard.RewardTypes.Favour:
                firstRewardButton = Instantiate(addFavourButton, firstHolder).GetComponent<Button>();
                firstRewardButton.interactable = isMax;
                firstRewardButton.onClick.AddListener(AddFavour);
                rewardButtons.Add(firstRewardButton);
                break;
            case EventCard.RewardTypes.Treasure:
                firstRewardButton = Instantiate(addTreasureButton, firstHolder).GetComponent<Button>();
                firstRewardButton.interactable = isMax;
                firstRewardButton.onClick.AddListener(AddTreasure);
                rewardButtons.Add(firstRewardButton);
                break;
        }

        Button passRewardButton;
        switch (eventsDeck.currentCard.passCode)
        {
            case EventCard.RewardTypes.FreeDay:
                passRewardButton = Instantiate(addFreeDayButton, passHolder).GetComponent<Button>();
                passRewardButton.interactable = isPassed;
                passRewardButton.onClick.AddListener(AddFFreeDay);
                rewardButtons.Add(passRewardButton);
                break;
            case EventCard.RewardTypes.Favour:
                passRewardButton = Instantiate(addFavourButton, passHolder).GetComponent<Button>();
                passRewardButton.interactable = isPassed;
                passRewardButton.onClick.AddListener(AddFavour);
                rewardButtons.Add(passRewardButton);
                break;
        }

        Button failRewardButton;
        switch (eventsDeck.currentCard.failCode)
        {
            case EventCard.RewardTypes.Mess:
                failRewardButton = Instantiate(addMessButton, failHolder).GetComponent<Button>();
                failRewardButton.interactable = !isPassed;
                failRewardButton.onClick.AddListener(AddMess);
                rewardButtons.Add(failRewardButton);
                break;
        }

        Button lastRewardButton;
        Button lastRewardButton2;
        switch (eventsDeck.currentCard.lastCode)
        {
            case EventCard.RewardTypes.Favour:
                lastRewardButton = Instantiate(addFavourButton, lastHolder).GetComponent<Button>();
                lastRewardButton.interactable = isMin;
                lastRewardButton.onClick.AddListener(AddFavour);
                rewardButtons.Add(lastRewardButton);
                break;
            case EventCard.RewardTypes.Mess:
                lastRewardButton = Instantiate(addMessButton, lastHolder).GetComponent<Button>();
                lastRewardButton.interactable = isMin;
                lastRewardButton.onClick.AddListener(AddMess);
                rewardButtons.Add(lastRewardButton);
                break;
            case EventCard.RewardTypes.Option:
                lastRewardButton = Instantiate(subtractFavourButton, lastHolder).GetComponent<Button>();
                lastRewardButton2 = Instantiate(addMessButton, lastHolder).GetComponent<Button>();
                lastRewardButton.interactable = isMin;
                lastRewardButton2.interactable = isMin;
                lastRewardButton.onClick.AddListener(SubtractFavour);
                lastRewardButton2.onClick.AddListener(AddMess);
                lastRewardButton.onClick.AddListener(delegate { lastRewardButton2.interactable = false; });
                lastRewardButton2.onClick.AddListener(delegate { lastRewardButton.interactable = false; });
                lastRewardButton.interactable = Favours >= 1;
                rewardButtons.Add(lastRewardButton);
                rewardButtons.Add(lastRewardButton2);
                break;
        }

        while (!rewardButtons.TrueForAll(x => !x.interactable))
            yield return null;

        yield return new WaitForSeconds(2);
        resolutionAnimator.SetTrigger(AnimTriggerFadeOut);
        yield return new WaitForSeconds(1);
        eventsDeck.currentCard.Discard();
        yield return new WaitForSeconds(1);
        isLoading = false;
        isNewTurn = true;
    }

    private void AddFFreeDay() => FreeDays += 1;
    private void AddFavour() => Favours += 1;
    private void AddMess() => Messes += 1;
    private void SubtractFavour() => Favours = Favours > 0 ? Favours - 1 : 0;


    #region Utils

    public static T FindResource<T>(string path)
    {
        var res = FindResources<T>(path);
        if (!res.Any()) throw new Exception($"Resource not found: {path}");
        return res[0];
    }

    public static List<T> FindResources<T>(string path) => Resources.LoadAll(path, typeof(T)).Cast<T>().ToList();

    public static string NewId() => Guid.NewGuid().ToString().Substring(0, 8);

    public static string ToBase64(string data) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(data));

    public static string FromBase64(string data) =>
        Encoding.UTF8.GetString(Convert.FromBase64String(data));

    public static string NowIsoDate()
    {
        var localTime = DateTime.Now;
        var localTimeAndOffset = new DateTimeOffset(localTime, TimeZoneInfo.Local.GetUtcOffset(localTime));
        var str = localTimeAndOffset.ToString("O");
        return str.Substring(0, 26) + str.Substring(27);
    }

    #endregion
}