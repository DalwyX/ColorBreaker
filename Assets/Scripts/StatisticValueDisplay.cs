using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatisticValueDisplay : MonoBehaviour
{

    [SerializeField] Stat statToShow;
    
    Text textField;

    enum Stat { MaxWave, WaveTotal, MaxScore, TotalScore, BlocksTotal, BlocksHitted, SPUsed, MUsed, TotalTime }

    private void Start()
    {
        textField = GetComponent<Text>();
        StartCoroutine(Updating());
    }

    private string GetStat()
    {
        switch (statToShow)
        {
            case Stat.WaveTotal:
                return PlayerPrefs.GetInt(GameConstants.WaveTotal).ToString();

            case Stat.BlocksHitted:
                return PlayerPrefs.GetInt(GameConstants.BlocksHitted).ToString();

            case Stat.BlocksTotal:
                return PlayerPrefs.GetInt(GameConstants.BlocksTotal).ToString();

            case Stat.SPUsed:
                return PlayerPrefs.GetInt(GameConstants.SPUsed).ToString();

            case Stat.MUsed:
                return PlayerPrefs.GetInt(GameConstants.MUsed).ToString();
                
            case Stat.MaxScore:
                return PlayerPrefs.GetInt(GameConstants.MaxScore).ToString();

            case Stat.TotalScore:
                return PlayerPrefs.GetInt(GameConstants.TotalScore).ToString();

            case Stat.TotalTime:
                return GetTotalTime();

            default:
                return PlayerPrefs.GetInt(GameConstants.MaxWave).ToString();
        }
    }

    private IEnumerator Updating()
    {
        while (true)
        {
            yield return new WaitUntil(() => GetStat() != textField.text);
            textField.text = GetStat();
        }
    }

    private string GetTotalTime()
    {
        float tt = PlayerPrefs.GetFloat(GameConstants.TotalTime, 0);
        int hours = (int) tt / 3600;
        int minutes = (int) tt / 60 - 60 * hours;
        int seconds = (int) tt - 3600 * hours - 60 * minutes;
        return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

}
