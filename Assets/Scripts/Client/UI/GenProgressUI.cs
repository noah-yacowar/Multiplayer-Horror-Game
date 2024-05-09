using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GenProgressUI : MonoBehaviour
{
    private GeneratorSpawner genSpawner;
    [SerializeField] private Transform[] genProgressItems = new Transform[5];
    private float maximumGenProgress;

    private void Start()
    {
        genSpawner = GameObject.Find("GeneratorSpawner").GetComponent<GeneratorSpawner>();
        maximumGenProgress = GeneratorController.NEEDED_PROGRESS_FOR_COMPLETION;
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < genSpawner.genVals.Count; i++)
        {
            genProgressItems[i].Find("GenProgress").GetComponent<Image>().fillAmount = (genSpawner.genVals[i] / maximumGenProgress);
        }
        
    }
}
