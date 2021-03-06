﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Alert : MonoBehaviour, IObserver
{
    [SerializeField] float alertWaitTime = 3f;

    private GameObject alertSign;
    private TextMeshPro alertText;

    IEnumerator showAlert;

    private void Awake()
    {
        alertSign = transform.GetChild(0).gameObject;
        alertText = alertSign.transform.GetChild(0).GetComponent<TextMeshPro>();
        alertSign.SetActive(false);
    }

    public void ReceiveText(string msg)
    {
        if (showAlert != null) { StopCoroutine(showAlert); }
        showAlert = DisplayAlert(msg);
        StartCoroutine(showAlert);
    }

    public void ReceiveUpdate(ISubject subject)
    {
        Upgrade up = subject as Upgrade;
        string message = up.GetMessage();
        if (showAlert != null) { StopCoroutine(showAlert); }
        showAlert = DisplayAlert(message);
        StartCoroutine(showAlert);
        //Debug.Log("You have picked up " + message + "!");
    }

    private IEnumerator DisplayAlert(string message)
    {
        alertText.text = "You have picked up " + message + "!";
        alertSign.SetActive(true);
        yield return new WaitForSeconds(alertWaitTime);
        alertSign.SetActive(false);
    }
}
