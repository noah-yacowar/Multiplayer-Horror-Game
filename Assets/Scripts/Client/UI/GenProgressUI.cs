using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GenProgressUI : MonoBehaviour
{
    private GeneratorSpawner genSpawner;
    [SerializeField] private TextMeshProUGUI[] genProgressText = new TextMeshProUGUI[5];

    private void Start()
    {
        genSpawner = GameObject.Find("GeneratorSpawner").GetComponent<GeneratorSpawner>();
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < genSpawner.genVals.Count; i++)
        {
            genProgressText[i].text = $"Generator {i} Progress: " + genSpawner.genVals[i].ToString();
        }
        
    }
}
