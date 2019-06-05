using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaveDisplay : MonoBehaviour
{

    Text waveField;

    private void Awake()
    {
        waveField = GetComponent<Text>();
    }

    public void UpdateWave(int wave)
    {
        waveField.text = "W" + wave.ToString();
    }

}
