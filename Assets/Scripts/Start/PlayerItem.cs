using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Start
{
    public class PlayerItem : MonoBehaviour
    {
        public TMP_Text playerNameText;
        public Image image;

        public Sprite character0;
        public Sprite character1;
        public Sprite character2;
        public Sprite character3;
        public Sprite character4;
        public Sprite character5;
        public Sprite character6;
        public Sprite character7;

        public PlayerItem LoadItem(string playerName, int character)
        {
            playerNameText.text = playerName;
            image.sprite = character switch
            {
                0 => character0,
                1 => character1,
                2 => character2,
                3 => character3,
                4 => character4,
                5 => character5,
                6 => character6,
                7 => character7,
                _ => throw new ArgumentOutOfRangeException(nameof(character), character, null)
            };
            return this;
        }
    }
}
