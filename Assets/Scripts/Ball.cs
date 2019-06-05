using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Ball : MonoBehaviour
{

    [Header("General")]
    [SerializeField] float targetSpeed;
    [SerializeField] float currentSpeed;
    [SerializeField]
    [Range(0.1f, 0.99f)] float maxDelta = 0.95f;
    [SerializeField]
    [Range(0.1f, 0.99f)] float correctionValue = 0.2f;
    [SerializeField] Text boostField;
    [SerializeField] float breakBonus = 0.05f;
    [SerializeField] Text magnetField;
    [SerializeField] float boostChargeDelay = 0.5f;
    [SerializeField] float magnetChargeDelay = 0.1f;
    [Header("Sounds")]
    [SerializeField] AudioClip hitSound;
    [SerializeField] AudioClip boostSound;
    [SerializeField] AudioClip magnetSound;
    [SerializeField] AudioClip ballLoseSound;

    public delegate bool BallEvent();
    public static BallEvent onBallLose;

    Paddle paddle;
    GameController gameController;
    BlocksController blocksController;
    Rigidbody2D rigidBody;
    SpriteRenderer spriteRenderer;

    float xSpeed;
    float ySpeed;

    bool hasStarted = false;
    float boostCharge;
    float magnetCharge;
    float additionalSpeed;
    Vector2 paddleToBallVector;
    Vector3 prevVelocity;

    #region Behaviour

    private void Start()
    {
        gameController = FindObjectOfType<GameController>();
        blocksController = FindObjectOfType<BlocksController>();
        paddle = FindObjectOfType<Paddle>();
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        paddleToBallVector = transform.position - paddle.transform.position;
        boostCharge = 1f;
        additionalSpeed = -1;

        StartCoroutine(BoostRecharge());
        StartCoroutine(MagnetRecharge());
    }

    private void Update()
    {
        if (!hasStarted)
        {
            LockBallToPaddle();
        }
        if (gameController.paused && rigidBody.velocity != Vector2.zero)
        {
            Time.timeScale = 0f;
        }
        if (!gameController.paused)
        {
            Control();
            xSpeed = rigidBody.velocity.x;
            ySpeed = rigidBody.velocity.y;
            currentSpeed = Mathf.Sqrt(Mathf.Pow(xSpeed, 2) + Mathf.Pow(ySpeed, 2));
        }
        if (transform.position.y < -1)
        {
            AudioSource.PlayClipAtPoint(ballLoseSound, Camera.main.transform.position, PlayerPrefs.GetFloat(GameConstants.SFXVolume));
            DestroyBall(onBallLose());
        }
    }

    public void DestroyBall(bool spawnNew = true)
    {
        if (spawnNew)
        {
            GameObject newBall = Instantiate(gameObject, paddle.transform.position + new Vector3(0, paddle.GetComponent<Collider2D>().bounds.size.y), Quaternion.identity);
            newBall.GetComponent<SpriteRenderer>().color = Color.white;
            newBall.GetComponent<Ball>().SetTargetSpeed(targetSpeed);
            newBall.name = "Ball";
        }
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        AdjustDirection();
        AdjustSpeed();
        if (hasStarted) {
            AudioSource.PlayClipAtPoint(hitSound, Camera.main.transform.position, 0.75f * PlayerPrefs.GetFloat(GameConstants.SFXVolume));
            //AudioClip clip = ballSounds[UnityEngine.Random.Range(0, ballSounds.Length)];
            //myAudioSource.PlayOneShot(clip);
            if (collision.gameObject.GetComponent<Paddle>())
            {
                Color newColor = paddle.gameObject.GetComponent<SpriteRenderer>().color;
                paddle.GetComponent<SpriteRenderer>().color = GetComponent<SpriteRenderer>().color;
                GetComponent<SpriteRenderer>().color = newColor;
            }
        }
    }
    #endregion

    #region General
    private void Control()
    {
        if (Input.GetMouseButtonDown(0) && blocksController.isReady)
        {
            hasStarted = true;
            SmartBoost();
        }
        if (Input.GetMouseButton(1) && blocksController.isReady)
        {
            Magnet();
        }
    }

    public void SetTargetSpeed(float speed)
    {
        targetSpeed = speed;
    }

    private void LockBallToPaddle()
    {
        Vector2 paddlePosition = new Vector2(paddle.transform.position.x, paddle.transform.position.y);
        transform.position = paddlePosition + paddleToBallVector;
    }

    private void AdjustSpeed()
    {
        if (Mathf.Abs(currentSpeed - (targetSpeed + additionalSpeed)) > Mathf.Epsilon)
        {
            Vector3 normal = rigidBody.velocity.normalized;
            rigidBody.velocity = new Vector2((targetSpeed + additionalSpeed) * normal.x, (targetSpeed + additionalSpeed) * normal.y);
        }
    }

    private void AdjustDirection()
    {
        Vector3 normal = rigidBody.velocity.normalized;
        if (Mathf.Abs(Mathf.Abs(rigidBody.velocity.normalized.x) - Mathf.Abs(rigidBody.velocity.normalized.y)) >= maxDelta)
        {
            if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y))
            {
                normal.x = Mathf.Sign(normal.x) * (Mathf.Abs(normal.x) - correctionValue);
                normal.y = Mathf.Sign(normal.y) * (Mathf.Abs(normal.y) + correctionValue);
            }
            else
            {
                normal.y = Mathf.Sign(normal.y) * (Mathf.Abs(normal.y) - correctionValue);
                normal.x = Mathf.Sign(normal.x) * (Mathf.Abs(normal.x) + correctionValue);
            }
            rigidBody.velocity = new Vector2((targetSpeed + additionalSpeed) * normal.x, (targetSpeed + additionalSpeed) * normal.y);
        }
    }
    
    #endregion

    #region SmartBoost
    private void SmartBoost()
    {
        if (boostCharge >= 1f)
        {
            PlayerPrefs.SetInt(GameConstants.SPUsed, PlayerPrefs.GetInt(GameConstants.SPUsed) + 1);
            spriteRenderer.color = Color.white;
            Block targetBlock = GetCloseBlock(0.1f);
            if (targetBlock == null)
            {
                return;
            }
            additionalSpeed++;
            Vector3 direction = (targetBlock.transform.position - transform.position).normalized;
            rigidBody.velocity = new Vector2((targetSpeed + additionalSpeed) * direction.x, (targetSpeed + additionalSpeed) * direction.y);
            AudioSource.PlayClipAtPoint(boostSound, Camera.main.transform.position, PlayerPrefs.GetFloat(GameConstants.SFXVolume));
            boostCharge = 0f;
        }
    }

    public void IncreaseBoostCharge()
    {
        boostCharge += breakBonus;
        BoostChargeUpdate();
    }

    private IEnumerator BoostRecharge()
    {
        while (true)
        {
            BoostChargeUpdate();
            yield return (boostCharge >= 1f) ? null : new WaitForSeconds(boostChargeDelay);
        }
    }

    private void BoostChargeUpdate()
    {
        if (boostCharge >= 1f)
        {
            boostField.text = "READY";
        }
        else
        {
            boostCharge += 0.01f;
            boostField.text = ((int)(boostCharge * 100)).ToString() + "%";
        }
    }

    private Block GetCloseBlock(float startingRadius = 0.1f)
    {
        Block[] blocks = FindObjectsOfType<Block>();
        if (blocks.Length == 0)
        {
            return null;
        }
        foreach (Block block in blocks)
        {
            if (Vector3.Distance(transform.position, block.transform.position) <= startingRadius)
            {
                return block;
            }
        }
        return GetCloseBlock(startingRadius + 0.1f);
    }
    #endregion

    #region Magnet
    private void Magnet()
    {
        if (magnetCharge >= 1f)
        {
            PlayerPrefs.SetInt(GameConstants.MUsed, PlayerPrefs.GetInt(GameConstants.MUsed) + 1);
            Vector3 direction = (paddle.transform.position - transform.position).normalized;
            rigidBody.velocity = new Vector2((targetSpeed + additionalSpeed) * direction.x, (targetSpeed + additionalSpeed) * direction.y);
            AudioSource.PlayClipAtPoint(magnetSound, Camera.main.transform.position, PlayerPrefs.GetFloat(GameConstants.SFXVolume));
            magnetCharge = 0f;
        }
    }

    private IEnumerator MagnetRecharge()
    {
        while (true)
        {
            if (magnetCharge >= 1f)
            {
                magnetField.text = "READY";
                yield return null;
            }
            else
            {
                if (hasStarted && blocksController.isReady)
                {
                    magnetCharge += 0.01f;
                }
                magnetField.text = ((int)(magnetCharge * 100)).ToString() + "%";
                yield return new WaitForSeconds(magnetChargeDelay);
            }
        }
    }
    #endregion

}
