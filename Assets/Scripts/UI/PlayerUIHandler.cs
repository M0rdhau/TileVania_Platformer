﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class PlayerUIHandler : MonoBehaviour
{
    float health;
    float charge;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI chargeText;
    [SerializeField] Slider healthSlider;
    [SerializeField] Slider chargeSlider;

    PlayerHealth pHealth;
    CombatCharge pCharge;



    private void Awake()
    {
        pHealth = FindObjectOfType<PlayerHealth>();
        pCharge = FindObjectOfType<CombatCharge>();
    }

    public void UpdateHealth(float health)
    {
        this.health = health;
        healthSlider.value = health / pHealth.GetMaxHealth();
        healthText.text = "HP: " + this.health;
    }

    public void UpdateCharge(float charge)
    {
        this.charge = charge;
        chargeSlider.value = pCharge.GetCharge();
        chargeText.text = "Charge: " + this.charge*100;
    }
}