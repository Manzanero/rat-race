using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Start;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class StartManager : MonoBehaviour
{
    public TMP_InputField userInput;
    public InputField passInput;
    public TMP_InputField roomInput;
    public Button createButton;
    public Button joinButton;
    public Button leaveButton;
    public Button deleteButton;
    public Button beginButton;
    public RectTransform playersParent;
    public GameObject playerItemPrefab;
    public GameObject loadingImage;
    public RectTransform messageParent;
    public GameObject messagePrefab;

    // Selection
    public GameObject selectionPanel;
    public CharacterCard charSelected;
    public TMP_Text flavourText;
    public Button previousButton;
    public Button nextButton;
    public Button selectButton;

    private List<CharacterBlueprint> chars;
    private int charIndex;
    private bool InRoom;
    private List<string> players = new List<string>();
    private string host;
    private List<PlayerItem> playerItems = new List<PlayerItem>();
    private List<RealmProperty> characterProperties = new List<RealmProperty>();

    public void Create()
    {
        Server.SetCredentials(userInput.text, passInput.text);
        var roomName = roomInput.text;
        if (roomName == "")
        {
            NewAlert("Sala no válida");
            return;
        }

        loadingImage.SetActive(true);
        StartCoroutine(CreateRealmRequest(roomName));
    }

    [Serializable]
    public class CreateRealmData
    {
        public string name;
        public RealmInfo info;
    }

    [Serializable]
    public class RealmInfo
    {
        public string label;
    }

    [Serializable]
    public class CreateRealmResponse : Server.Response
    {
        public bool created;
        public bool exists;
    }

    public IEnumerator CreateRealmRequest(string roomName)
    {
        var data = new CreateRealmData {name = roomName};
        var url = $"{Server.BaseUrl}/lands/{ServerManager.Land}/realms/create";
        var request = Server.PutRequest(url, data);
        while (!request.isDone) yield return null;

        var response = Server.GetResponse<CreateRealmResponse>(request, false);
        if (response.status == 401)
        {
            NewAlert("Usuario no válido");
            loadingImage.SetActive(false);
            yield break;
        }

        if (response.status >= 400)
        {
            NewAlert("Error del servidor");
            loadingImage.SetActive(false);
            yield break;
        }

        if (!response.created && response.exists)
        {
            NewAlert("Ya existe la sala");
            loadingImage.SetActive(false);
            yield break;
        }

        if (!response.created)
        {
            NewAlert("Error al crear la sala");
            loadingImage.SetActive(false);
            yield break;
        }

        Debug.Log(response.message);

        StartCoroutine(JoinRealmRequest(roomName));
    }

    public void Join()
    {
        Server.SetCredentials(userInput.text, passInput.text);
        var roomName = roomInput.text;
        if (roomName == "")
        {
            NewAlert("Sala no válida");
            return;
        }

        loadingImage.SetActive(true);
        StartCoroutine(JoinRealmRequest(roomName));
    }

    [Serializable]
    public class JoinRealmResponse : Server.Response
    {
        public List<string> players;
        public string host;
    }

    [Serializable]
    public class RealmPropertiesData
    {
        public List<string> players;
        public List<string> values;
    }

    private bool ImHost => host == userInput.text;

    private IEnumerator JoinRealmRequest(string roomName)
    {
        var url = $"{Server.BaseUrl}/lands/{ServerManager.Land}/realms/{roomName}/join";
        var request = Server.GetRequest(url);
        while (!request.isDone) yield return null;
        var response = Server.GetResponse<JoinRealmResponse>(request, false);
        if (response.status == 401)
        {
            NewAlert("Usuario no válido");
            loadingImage.SetActive(false);
            yield break;
        }

        if (response.exception)
        {
            NewAlert("Error al entrar a la sala");
            loadingImage.SetActive(false);
            yield break;
        }

        userInput.interactable = false;
        passInput.interactable = false;
        roomInput.interactable = false;
        createButton.interactable = false;
        joinButton.interactable = false;

        players = response.players;
        host = response.host;
        leaveButton.interactable = true;
        deleteButton.interactable = ImHost;
        beginButton.interactable = ImHost;

        var urlWaitMe = $"{Server.BaseUrl}/lands/{ServerManager.Land}/realms/{roomInput.text}/properties/STARTED";
        var requestWaitMe =
            Server.PostRequest(urlWaitMe, new RealmPropertiesData {values = new List<string> {"false"}});
        while (!requestWaitMe.isDone) yield return null;
        Server.GetResponse<RealmPropertiesResponse>(requestWaitMe);

        selectionPanel.SetActive(true);
        while (selectionPanel.activeSelf) yield return null;

        var urlChars = $"{Server.BaseUrl}/lands/{ServerManager.Land}/realms/{roomInput.text}/properties/CHARACTER";
        var data = new RealmPropertiesData
            {players = new List<string> {userInput.text}, values = new List<string> {charIndex.ToString()}};
        var requestChars = Server.PostRequest(urlChars, data);
        while (!requestChars.isDone) yield return null;
        Server.GetResponse<RealmPropertiesResponse>(requestChars);

        var url2 = $"{Server.BaseUrl}/lands/{ServerManager.Land}/realms/{roomInput.text}/properties?name=CHARACTER";
        var request2 = Server.GetRequest(url2);
        while (!request2.isDone) yield return null;
        var response2 = Server.GetResponse<RealmPropertiesResponse>(request2);
        characterProperties = response2.properties;

        RefreshLobby();
        InRoom = true;
        loadingImage.SetActive(false);
    }

    public void Leave()
    {
        Server.SetCredentials(userInput.text, passInput.text);
        var roomName = roomInput.text;

        foreach (Transform child in playersParent) Destroy(child.gameObject);

        loadingImage.SetActive(true);
        StartCoroutine(LeaveRealmRequest(roomName));
    }

    public IEnumerator LeaveRealmRequest(string roomName)
    {
        var url = $"{Server.BaseUrl}/lands/{ServerManager.Land}/realms/{roomName}/leave";
        var request = Server.GetRequest(url);
        while (!request.isDone) yield return null;

        Server.GetResponse<CreateRealmResponse>(request, false);

        players.Clear();
        userInput.interactable = true;
        passInput.interactable = true;
        roomInput.interactable = true;
        createButton.interactable = true;
        joinButton.interactable = true;
        leaveButton.interactable = false;
        deleteButton.interactable = false;
        beginButton.interactable = false;

        var url2 = $"{Server.BaseUrl}/lands/{ServerManager.Land}/realms/{roomInput.text}/properties?name=CHARACTER";
        var request2 = Server.GetRequest(url2);
        while (!request2.isDone) yield return null;
        var response2 = Server.GetResponse<RealmPropertiesResponse>(request2);
        characterProperties = response2.properties;

        InRoom = false;
        loadingImage.SetActive(false);
    }

    public void Delete()
    {
        Server.SetCredentials(userInput.text, passInput.text);
        var roomName = roomInput.text;

        foreach (Transform child in playersParent) Destroy(child.gameObject);

        loadingImage.SetActive(true);
        StartCoroutine(DeleteRealmRequest(roomName));
    }

    public IEnumerator DeleteRealmRequest(string roomName)
    {
        var url = $"{Server.BaseUrl}/lands/{ServerManager.Land}/realms/{roomName}/delete";
        var request = Server.DeleteRequest(url);
        while (!request.isDone) yield return null;

        var response = Server.GetResponse<CreateRealmResponse>(request, false);
        if (response.exception)
        {
            NewAlert("Error al eliminar a la sala");
            loadingImage.SetActive(false);
            yield break;
        }

        userInput.interactable = true;
        passInput.interactable = true;
        roomInput.interactable = true;
        createButton.interactable = true;
        joinButton.interactable = true;
        leaveButton.interactable = false;
        deleteButton.interactable = false;
        beginButton.interactable = false;

        InRoom = false;
        loadingImage.SetActive(false);
    }

    public void NewAlert(string txt)
    {
        var message = Instantiate(messagePrefab, messageParent).GetComponent<TMP_Text>();
        message.text = txt;
        Destroy(message.gameObject, 3);
    }

    private void Awake()
    {
        GameManager.LocalPlayer ??= GameManager.TestPlayer();
        selectionPanel.SetActive(false);
        selectButton.onClick.AddListener(delegate { selectionPanel.SetActive(false); });
        previousButton.onClick.AddListener(delegate { charIndex = charIndex != 7 ? charIndex + 1 : 0; });
        nextButton.onClick.AddListener(delegate { charIndex = charIndex != 0 ? charIndex - 1 : 7; });
        chars = GameManager.FindResources<CharacterBlueprint>("Chars");
        loadingImage.SetActive(false);
        foreach (Transform child in messageParent) Destroy(child.gameObject);
        foreach (Transform child in playersParent) Destroy(child.gameObject);
        createButton.onClick.AddListener(Create);
        joinButton.onClick.AddListener(Join);
        leaveButton.onClick.AddListener(Leave);
        deleteButton.onClick.AddListener(Delete);
        leaveButton.interactable = false;
        deleteButton.interactable = false;
        beginButton.interactable = false;
        beginButton.onClick.AddListener(Begin);
    }

    private void Begin()
    {
        loadingImage.SetActive(true);
        StartCoroutine(BeginRequest());
    }

    private IEnumerator BeginRequest()
    {
        EventsDeck.Sequence = Enumerable.Range(0, 16).OrderBy(i => Random.value).ToList();
        var value = string.Join(",", EventsDeck.Sequence.Select(x => x.ToString()).ToArray());
        var urlSequence = $"{Server.BaseUrl}/lands/{ServerManager.Land}/realms/{roomInput.text}/properties/SEQUENCE";
        var requestSequence =
            Server.PostRequest(urlSequence, new RealmPropertiesData {values = new List<string> {value}});
        while (!requestSequence.isDone) yield return null;
        Server.GetResponse<RealmPropertiesResponse>(requestSequence);

        var url = $"{Server.BaseUrl}/lands/{ServerManager.Land}/realms/{roomInput.text}/properties/STARTED";
        var request = Server.PostRequest(url, new RealmPropertiesData {values = new List<string> {"true"}});
        while (!request.isDone) yield return null;
        Server.GetResponse<RealmPropertiesResponse>(request);
    }

    private void RefreshLobby()
    {
        playerItems.Clear();
        foreach (Transform child in playersParent) Destroy(child.gameObject);

        if (!players.Any()) return;

        foreach (var characterProperty in characterProperties)
        {
            var playerItem = Instantiate(playerItemPrefab, playersParent).GetComponent<PlayerItem>();
            playerItems.Add(playerItem.LoadItem(characterProperty.player, int.Parse(characterProperty.value)));
        }
    }

    [Serializable]
    public class RealmPropertiesResponse : Server.Response
    {
        public List<RealmProperty> properties;
    }

    [Serializable]
    public class RealmProperty
    {
        public string player;
        public string name;
        public string value;
    }

    private IEnumerator PollingLobby()
    {
        while (true)
        {
            yield return new WaitForSeconds(3);

            if (!InRoom)
            {
                yield return new WaitForSeconds(3);
                continue;
            }

            var url = $"{Server.BaseUrl}/lands/{ServerManager.Land}/realms/{roomInput.text}/properties?name=CHARACTER";
            var request = Server.GetRequest(url);
            while (!request.isDone) yield return null;
            var response = Server.GetResponse<RealmPropertiesResponse>(request);
            characterProperties = response.properties;

            RefreshLobby();

            var url2 = $"{Server.BaseUrl}/lands/{ServerManager.Land}/realms/{roomInput.text}/properties?name=STARTED";
            var request2 = Server.GetRequest(url2);
            while (!request2.isDone) yield return null;
            var response2 = Server.GetResponse<RealmPropertiesResponse>(request2);

            if (!response2.properties.Any()) continue;
            if (response2.properties[0].value != "true") continue;

            Debug.Log("Starting");
            loadingImage.SetActive(true);

            var urlSequence =
                $"{Server.BaseUrl}/lands/{ServerManager.Land}/realms/{roomInput.text}/properties?name=SEQUENCE";
            var requestSequence = Server.GetRequest(urlSequence);
            while (!requestSequence.isDone) yield return null;
            var responseSequence = Server.GetResponse<RealmPropertiesResponse>(requestSequence);
            EventsDeck.Sequence = responseSequence.properties[0].value.Split(',').Select(x => int.Parse(x)).ToList();

            var localCharacterBlueprint = chars[charIndex];
            GameManager.LocalPlayer = new Player
            {
                name = userInput.text,
                characterBlueprint = localCharacterBlueprint,
                maxMesses = localCharacterBlueprint.maxMesses,
                tech = localCharacterBlueprint.tech,
                cheek = localCharacterBlueprint.cheek,
                tedium = localCharacterBlueprint.tedium,
                abilityUsed = false
            };
            foreach (var item in playerItems)
            {
                if (item.PlayerName == userInput.text) continue;
                var remoteCharacterBlueprint = chars[item.Character];
                GameManager.RemotePlayers.Add(new Player
                {
                    name = item.PlayerName,
                    characterBlueprint = remoteCharacterBlueprint,
                    maxMesses = remoteCharacterBlueprint.maxMesses,
                    tech = remoteCharacterBlueprint.tech,
                    cheek = remoteCharacterBlueprint.cheek,
                    tedium = remoteCharacterBlueprint.tedium,
                    abilityUsed = false
                });
            }

            ServerManager.Realm = roomInput.text;

            SceneManager.LoadScene(1);
        }
    }

    private void Start()
    {
        StartCoroutine(PollingLobby());
    }

    private void Update()
    {
        if (selectionPanel.activeSelf)
        {
            var characterBlueprint = chars[charIndex];
            charSelected.title.text = characterBlueprint.title;
            charSelected.image.sprite = characterBlueprint.image;
            charSelected.messText.text = characterBlueprint.maxMesses.ToString();
            charSelected.Tech = characterBlueprint.tech;
            charSelected.Cheek = characterBlueprint.cheek;
            charSelected.Tedium = characterBlueprint.tedium;
            charSelected.abilityText.text = characterBlueprint.ability;

            flavourText.text = characterBlueprint.flavour;
        }
    }
}