using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class Score : MonoBehaviour
{

    [SerializeField] UpdateMode updateMode = UpdateMode.Instant;
    [SerializeField] float updateCoef = 1;
    [SerializeField] int numberOfDigits = 6;
    [SerializeField] AudioClip SFX;
    
    Text scoreField;
    Coroutine updateRoutine;
    public static int currentPoints { get; private set; }
    int addedPoints;
    string format;

    // Режим обновления счета при добавлении новых очков
    //  Instant - мгновенный
    //  Progressive - постепенное набегание очков с постоянной скоростью
    //  SMARTAcceleration - набегание очков с динамическим изменением скорости
    enum UpdateMode { Instant, Progressive, SMARTAcceleration }

    private void Start()
    {
        // Подписываемся на эвент обновления счета
        BlocksController.updateScore += AddPoints;
        updateRoutine = null;

        scoreField = GetComponent<Text>();
        currentPoints = 0;
        format = GetFormat();
        scoreField.text = currentPoints.ToString(format);

    }

    private void AddPoints(int points)
    {
        addedPoints += points;
        if (updateRoutine == null)
        {
            updateRoutine = StartCoroutine(ScoreUpdateRoutine());
        }
    }

    private IEnumerator ScoreUpdateRoutine()
    {
        int pointsPerSecond = 1;
        float PPSA = 1;
        while (true)
        {
            if (addedPoints <= 0)
            {
                StopCoroutine(updateRoutine);
                updateRoutine = null;
                break;
            }
            switch (updateMode)
            {
                case UpdateMode.Progressive:
                    currentPoints++;
                    addedPoints--;
                    UpdateScore();
                    yield return new WaitForSeconds(1 / updateCoef);
                    break;

                case UpdateMode.SMARTAcceleration:
                    pointsPerSecond = GetAcceleration();
                    PPSA = pointsPerSecond * updateCoef;
                    if (1/PPSA > Time.deltaTime)
                    {
                        currentPoints++;
                        addedPoints--;
                        UpdateScore();
                        yield return new WaitForSeconds(1 / PPSA);
                    }
                    else
                    {
                        currentPoints += Mathf.CeilToInt(Time.deltaTime / (1 / PPSA));
                        addedPoints -= Mathf.CeilToInt(Time.deltaTime / (1 / PPSA));
                        UpdateScore();
                        yield return null;
                    }
                    break;

                default:
                    currentPoints += addedPoints;
                    addedPoints = 0;
                    UpdateScore();
                    break;
            }
        }
    }

    private void UpdateScore()
    {
        scoreField.text = currentPoints.ToString(format);
        AudioSource.PlayClipAtPoint(SFX, Camera.main.transform.position, 0.8f * PlayerPrefs.GetFloat(GameConstants.SFXVolume));
    }

    // Возвращает формат вывода счета
    private string GetFormat()
    {
        string format = "";
        for (int i = 0; i < numberOfDigits; i++)
        {
            format += "0";
        }
        return format;
    }

    // Возвращает ближайшее меньшее число-степень двойки
    private int GetAcceleration()
    {
        int pow = 0;
        int x = addedPoints;
        while (x > 0)
        {
            x >>= 1;
            pow++;
        }
        pow = (pow > 2) ? pow : 3;
        return (int)Mathf.Pow(2, pow);
    }

}