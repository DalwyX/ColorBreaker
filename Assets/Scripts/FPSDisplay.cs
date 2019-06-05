using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSDisplay : MonoBehaviour
{

    Text fpsField;

    void Start()
    {
        fpsField = GetComponent<Text>();
        StartCoroutine(FPSUpdate());
    }

    private IEnumerator FPSUpdate()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.1f);
            fpsField.text = (1 / Time.deltaTime).ToString();
        }
    }
}
