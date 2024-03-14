using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NameUI : MonoBehaviour
{
   TextMeshProUGUI textMeshPro;
   [SerializeField] Name name;
   
   //After network spawn, game object generation runs Start() functions.
   //It checks against the Name script what the playerName value is. Unless anything has gone wrong, that code has run before this one
   //It takes the value of that variable and sets the text in the UI element to be that.
   void Start()
   {
       textMeshPro = GetComponent<TextMeshProUGUI>();
       textMeshPro.text = name.playerName.Value.ToString();
       name.playerName.OnValueChanged += UpdateUI;
   }

   //This is added as a further redundancy check in case name would somehow be changed mid-game.
   private void UpdateUI(FixedString32Bytes previousValue, FixedString32Bytes newValue)
   {
       textMeshPro.text = newValue.Value.ToString();
   }
}