using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public static MainMenuUI Instance { get; private set; }

    [SerializeField] private Button enterNameBtn;
    [SerializeField] private TMP_InputField nameField;

    public string defaultName = "Buttface";

    public event Action<string> EnteredName;

    void Awake()
    {
        Instance = this;

        nameField.onEndEdit.AddListener(OnInputEndEdit);
        enterNameBtn.onClick.AddListener(() => OnInputEndEdit(nameField.text));
    }

    void OnInputEndEdit(string inputText)
    {
        if(inputText == "")
        {
            EnteredName?.Invoke(defaultName);
            return;
        }

        EnteredName?.Invoke(inputText);
    }
}
