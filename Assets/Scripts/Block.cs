using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    [SerializeField] AudioClip spawnSound;
    [SerializeField] AudioClip breakSound;
    
    SpriteRenderer spriteRenderer;

    int durability = 1;
    int timesHitted;
    float newR;
    float newG;
    float newB;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        timesHitted = 0;
        AudioSource.PlayClipAtPoint(spawnSound, Camera.main.transform.position, PlayerPrefs.GetFloat(GameConstants.SFXVolume));
        newR = spriteRenderer.color.r;
        newG = spriteRenderer.color.g;
        newB = spriteRenderer.color.b;
        StartCoroutine(BallPainting());
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.GetComponent<Ball>())
            ManageHit(col.gameObject.GetComponent<SpriteRenderer>());
    }

    public void SetBaseColor(Color color)
    {
        spriteRenderer.color = color;
    }

    public void SetDurability(int durability)
    {
        this.durability = durability;
    }

    private void ManageHit(SpriteRenderer ballSprite)
    {
        GetComponent<Animator>().SetTrigger("Shake");
        PlayerPrefs.SetInt(GameConstants.BlocksHitted, PlayerPrefs.GetInt(GameConstants.BlocksHitted) + 1);

        float blockR = newR;
        float blockG = newG;
        float blockB = newB;

        float ballR = ballSprite.color.r;
        float ballG = ballSprite.color.g;
        float ballB = ballSprite.color.b;

        newR = blockR;
        newG = blockG;
        newB = blockB;
        
        newR = ((ballR / durability + blockR) >= 1f) ? 1f : blockR + ballR / durability;
        newG = ((ballG / durability + blockG) >= 1f) ? 1f : blockG + ballG / durability;
        newB = ((ballB / durability + blockB) >= 1f) ? 1f : blockB + ballB / durability;
        ballSprite.color = new Color(blockR, blockG, blockB);
        timesHitted++;
    }

    private IEnumerator BallPainting()
    {
        float curR = spriteRenderer.color.r;
        float curG = spriteRenderer.color.g;
        float curB = spriteRenderer.color.b;
        float curA = spriteRenderer.color.a;

        float deltaR = Mathf.Abs(newR - curR);
        float deltaG = Mathf.Abs(newG - curG);
        float deltaB = Mathf.Abs(newB - curB);

        float speed = 2.5f;
        float valuePerFrame = speed * Time.deltaTime;

        float setR = (deltaR > valuePerFrame) ? curR + valuePerFrame : newR;
        float setG = (deltaG > valuePerFrame) ? curG + valuePerFrame : newG;
        float setB = (deltaB > valuePerFrame) ? curB + valuePerFrame : newB;
        float setA = curA;
        spriteRenderer.color = new Color(setR, setG, setB, setA);
        yield return null;

        while (true)
        {
            curR = spriteRenderer.color.r;
            curG = spriteRenderer.color.g;
            curB = spriteRenderer.color.b;
            curA = spriteRenderer.color.a;

            if ((curR + curG + curB) >= 2.75)
            {
                setA -= 2.5f * Time.deltaTime;
            }
            if (curA <= 0)
            {
                BlocksController.Instance.DestroyBlock(gameObject, timesHitted);
            }

            deltaR = Mathf.Abs(newR - curR);
            deltaG = Mathf.Abs(newG - curG);
            deltaB = Mathf.Abs(newB - curB);

            valuePerFrame = speed * Time.deltaTime;

            setR = (deltaR > valuePerFrame) ? curR + valuePerFrame : newR;
            setG = (deltaG > valuePerFrame) ? curG + valuePerFrame : newG;
            setB = (deltaB > valuePerFrame) ? curB + valuePerFrame : newB;
            spriteRenderer.color = new Color(setR, setG, setB, setA);
            yield return null;
        }
    }

}