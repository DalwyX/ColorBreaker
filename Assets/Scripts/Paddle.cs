using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Paddle : MonoBehaviour
{

    [SerializeField] float screenWidthInUnits = 16f;
    [SerializeField] float minX;
    [SerializeField] float maxX;

    GameController gameController;
    SpriteRenderer spriteRenderer;

    private bool autoplay = false;

    void Start()
    {
        gameController = GameController.Instance;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = Color.white;
        autoplay = false;
        StartCoroutine(Painter());
    }

    void Update ()
    {
        if (!gameController.paused)
        {
            
            if (Input.GetKeyDown(KeyCode.Space))
            {
                autoplay = autoplay ? false : true;
            }
            
            if (autoplay == true) Autoplay();
            else Manual();
        }
	}

    private void Autoplay()
    {
        if (FindObjectOfType<Ball>())
        {
            Time.timeScale = 5f;
            float XPos = Mathf.Clamp(FindObjectOfType<Ball>().transform.position.x, minX, maxX);
            Vector2 paddlePosition = new Vector2(XPos, transform.position.y);
            transform.position = paddlePosition;
        }
    }

    private void Manual()
    {
        Time.timeScale = 1f;
        float mouseXPos = Mathf.Clamp(Input.mousePosition.x / Screen.width * screenWidthInUnits, minX, maxX);
        Vector2 paddlePosition = new Vector2(mouseXPos, transform.position.y);
        transform.position = paddlePosition;
    }

    private IEnumerator Painter()
    {
        float currentR;
        float currentG;
        float currentB;
        float newR = 1f;
        float newG = 1f;
        float newB = 1f;
        float whitenizerTime = 0.5f;
        float valuePerFrame = whitenizerTime * Time.deltaTime;
        while (true)
        {
            currentR = spriteRenderer.color.r;
            currentG = spriteRenderer.color.g;
            currentB = spriteRenderer.color.b;

            if (currentR < 1f)
            {
                newR = (1f - currentR >= valuePerFrame) ? currentR + valuePerFrame : 1f;
            }
            if (currentG < 1f)
            {
                newG = (1f - currentG >= valuePerFrame) ? currentG + valuePerFrame : 1f;
            }
            if (currentB < 1f)
            {
                newB = (1f - currentB >= valuePerFrame) ? currentB + valuePerFrame : 1f;
            }

            spriteRenderer.color = new Color(newR, newG, newB);
            yield return null;
        }
    }
}
