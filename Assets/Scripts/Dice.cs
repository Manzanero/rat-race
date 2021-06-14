using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Dice : MonoBehaviour
{
    public Image topFace;

    public List<Sprite> faces;

    public int result;

    public void Awake()
    {
        PickUp();
    }

    public void Throw()
    {
        topFace.enabled = true;
        result = Random.Range(1, 6);
        topFace.sprite = faces[(result - 1) * 4 + Random.Range(0, 4)];

        ServerManager.MessagesToSend.Add(new ServerManager.Msg
            {topic = "actions", payload = new List<string> {"Roll", GameManager.LocalPlayer.name, result.ToString()}});
    }

    public void Throw(int forceResult)
    {
        topFace.enabled = true;
        result = forceResult;
        topFace.sprite = faces[(result - 1) * 4 + Random.Range(0, 4)];
    }

    public void PickUp()
    {
        topFace.enabled = false;
        result = 0;
    }
}