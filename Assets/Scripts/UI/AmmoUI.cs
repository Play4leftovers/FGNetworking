using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;


public class AmmoUI : MonoBehaviour
{
    TextMeshProUGUI textMeshPro;
    [SerializeField] FiringAction firingAction;
    
    void Start()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        firingAction.ammo.OnValueChanged += UpdateUI;
        textMeshPro.text = firingAction.ammo.Value.ToString();
    }

    private void UpdateUI(int previousValue, int newValue)
    {
        textMeshPro.text = newValue.ToString();
    }
}