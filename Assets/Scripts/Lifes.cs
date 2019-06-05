using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Lifes : MonoBehaviour
{

    [SerializeField] int startingAmountOfLifes = 3;
    [SerializeField] Sprite sprite1;
    [SerializeField] Sprite sprite2;

    Image[] lifes;
    int lifesCount;
    public int LifesCount { get { return lifesCount; } }
    int minLifes;
    int maxLifes;

    private void Start()
    {
        lifes = GetComponentsInChildren<Image>();
        minLifes = 1;
        maxLifes = lifes.Length;
        lifesCount = startingAmountOfLifes;
        UpdateLifesBar();

        Ball.onBallLose += TakeLife;
    }

    private void UpdateLifesBar()
    {
        LifesValueCheck();
        for (int i = 0; i < lifes.Length; i++)
        {
            lifes[i].sprite = (i <= lifesCount - 1) ? sprite1 : sprite2;
        }
    }

    private void LifesValueCheck()
    {
        if (lifesCount < minLifes)
        {
            FindObjectOfType<GameController>().GameOver();
        }
        if (lifesCount > maxLifes)
        {
            lifesCount = maxLifes;
        }
    }

    public void AddLife()
    {
        lifesCount++;
        UpdateLifesBar();
    }

    public bool TakeLife()
    {
        lifesCount--;
        UpdateLifesBar();
        if (lifesCount == 0)
            return false;
        return true;
    }

}
