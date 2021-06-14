using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    public static readonly List<Msg> MessagesToRead = new List<Msg>();
    public static readonly List<Msg> MessagesToSend = new List<Msg>();
    
    [Serializable]
    public class Msg
    {
        public string topic;
        public List<string> payload;
        public bool read;
    }

    [Serializable]
    private class MsgData
    {
        public List<Msg> messages = new List<Msg>();
    }

    [Serializable]
    private class MsgResponse : Server.Response
    {
        public string date;
        public List<Msg> messages = new List<Msg>();
    }
    
    public static string Land = "rat-race";
    public static string Realm = "test";
    public static string RealmUrl;
    
    private string fromDate;
    private readonly object receiveMessagesFrequency = new WaitForSecondsRealtime(1f);
    private readonly object sendMessagesFrequency = new WaitForSecondsRealtime(1f);
    
    private IEnumerator ReceiveMessages()
    {
        while (Server.ServerReady)
        {
            yield return receiveMessagesFrequency;

            var url = $"{RealmUrl}/receive?topic=actions&from={fromDate}&persistence=0";
            var request = Server.GetRequest(url);
            while (!request.isDone)
                yield return null;
            
            var response = Server.GetResponse<MsgResponse>(request);
            if (!response)
            {
                Debug.LogError(response.message);
                continue;
            }
            
            MessagesToRead.AddRange(response.messages);
            fromDate = response.date;
        }
    }
    
    private IEnumerator SendMessages()
    {
        while (Server.ServerReady)
        {
            yield return sendMessagesFrequency;
            
            if (!MessagesToSend.Any()) continue;

            var messages = MessagesToSend.ToList();
            var data = new MsgData {messages = messages};
            MessagesToSend.Clear();
            
            var url = $"{RealmUrl}/publish";
            var request = Server.PutRequest(url, data);
            while (!request.isDone)
                yield return null;
            
            Server.GetResponse<MsgResponse>(request, false);
        }
    }
    
    private void ReadMessages()
    {
        var newMessages = MessagesToRead.Where(a => !a.read).ToList();
        foreach (var message in newMessages)
            ResolveMessage(message);

        var actionsToDelete = MessagesToRead.Where(a => a.read).ToList();
        if (actionsToDelete.Any())
            MessagesToRead.Remove(actionsToDelete[0]);
    }

    public static class MessageTypes
    {
        public const string Roll = "Roll";
        public const string Ready = "Ready";
        public const string Status = "Status";
        public const string Object = "Object";
        public const string Ability = "Ability";
    }

    private void ResolveMessage(Msg message)
    {
        var action = message.payload[0];
        var playerName = message.payload[1];
        var opponent = GameManager.GetOpponentCardByPlayerName(playerName);
            
        try
        {
            switch (action)
            {
                case MessageTypes.Roll:
                    var result = int.Parse(message.payload[2]);
                    opponent.dice.Throw(result);
                    break;
                case MessageTypes.Ready:
                    var totalResult = int.Parse(message.payload[2]);
                    opponent.secretResult = totalResult;
                    opponent.secretBonus = totalResult - opponent.dice.result;
                    opponent.readyToggle.isOn = true;
                    opponent.remotePlayer.inNewTurn = false;
                    break;
                case MessageTypes.Status:
                    var freeDays = int.Parse(message.payload[2]);
                    var favours = int.Parse(message.payload[3]);
                    var messes = int.Parse(message.payload[4]);
                    var treasures = int.Parse(message.payload[5]);
                    opponent.FreeDays = freeDays;
                    opponent.Favours = favours;
                    opponent.Messes = messes;
                    opponent.Treasures = treasures;
                    opponent.remotePlayer.inNewTurn = true;
                    break;
                case MessageTypes.Object:
                    var treasureName = message.payload[2];
                    var treasureTargetName = message.payload[3];
                    break;
                case MessageTypes.Ability:
                    var abilityName = message.payload[2];
                    var abilityTargetName = message.payload[3];
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Message (action={action}) throws error: {e}");
        }
        finally
        {
            message.read = true;
        }
    }

    private void Start()
    {
        Server.ServerReady = true;
        
        RealmUrl = $"{Server.BaseUrl}/lands/{Land}/realms/{Realm}";
        fromDate = GameManager.NowIsoDate();
        StartCoroutine(ReceiveMessages());
        StartCoroutine(SendMessages());
    }

    private void Update()
    {
        ReadMessages();
    }
}