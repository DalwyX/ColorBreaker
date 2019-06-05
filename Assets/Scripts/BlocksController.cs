using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlocksController : MonoBehaviour
{

    [SerializeField] Block blockPrefab;
    [SerializeField] int minX;
    [SerializeField] int maxX;
    [SerializeField] int minY;
    [SerializeField] int maxY;
    [SerializeField] int maxBlockDurability = 10;
    [SerializeField] float blockSpawnDelay = 0.5f;

    public delegate void UpdateScore(int score);
    public static event UpdateScore updateScore;

    public static BlocksController Instance;

    GameController gameController;
    List<Block> blocks;
    ColorScheme colorScheme;
    float spawnConsistency;
    bool gameReady;
    public bool isReady { get { return gameReady; } }
    public int numberOfBlocks { get { return blocks.Count; } }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        gameController = FindObjectOfType<GameController>();
        FindObjectOfType<Ball>().GetComponent<SpriteRenderer>().color = Color.white;

        blocks = new List<Block>();
        gameReady = false;
    }

    private void Update()
    {
        if (blocks.Count < 1 && gameReady)
        {
            gameReady = false;
        }
    }

    public void SpawnNewBlocks(int numberOfBlocks = 12, float spawnConsistency = 0.5f)
    {
        gameReady = false;
        this.spawnConsistency = spawnConsistency;
        StartCoroutine(SpawnBlocks(numberOfBlocks));
    }

    public void SetColorScheme(ColorScheme cs)
    {
        colorScheme = cs;
    }

    public void DestroyBlock(GameObject block, int timesHitted)
    {
        if (updateScore != null)
            updateScore(64 + timesHitted + 2 * gameController.currentWave);
        PlayerPrefs.SetInt(GameConstants.BlocksTotal, PlayerPrefs.GetInt(GameConstants.BlocksTotal) + 1);
        if (FindObjectOfType<Ball>())
        {
            FindObjectOfType<Ball>().IncreaseBoostCharge();
        }
        blocks.Remove(block.GetComponent<Block>());
        Destroy(block);
    }

    private IEnumerator SpawnBlocks(int numberOfBlocks)
    {
        int xStart = Random.Range(8, 10);
        int yStart = Random.Range(7, 9);
        Vector3 currentPosition = new Vector3(xStart, yStart);

        List<Block> blocksSpawnedLastTime = new List<Block>();
        blocksSpawnedLastTime.Add(SpawnBlock(currentPosition));
        List<Vector3> nextPositions = new List<Vector3>();

        while (blocks.Count != numberOfBlocks)
        {
            yield return new WaitForSeconds(blockSpawnDelay);
            nextPositions = ChooseNextPositions(blocksSpawnedLastTime);
            blocksSpawnedLastTime.Clear();
            foreach (Vector3 pos in nextPositions)
            {
                if (!IsOccupied(pos))
                {
                    blocksSpawnedLastTime.Add(SpawnBlock(pos));
                }
                if (blocks.Count == numberOfBlocks)
                {
                    break;
                }
            }
            if (blocksSpawnedLastTime.Count <= 1 && blocks.Count != numberOfBlocks)
            {
                Vector3 newPosition = GetNewPosition();
                blocksSpawnedLastTime.Add(SpawnBlock(newPosition));
            }
        }
        gameReady = true;
    }

    private Block SpawnBlock(Vector3 position)
    {
        Block newBlock = Instantiate(blockPrefab, position, Quaternion.identity, transform) as Block;
        blocks.Add(newBlock);
        newBlock.name = "Block #" + blocks.Count;
        newBlock.SetBaseColor(colorScheme.GetRandomColor());
        newBlock.SetDurability(GetRandomDurability());
        return newBlock;
    }

    private List<Vector3> ChooseNextPositions(List<Block> blocks)
    {
        List<Vector3> possibleVariants = new List<Vector3>();
        Vector3 possiblePosition = new Vector3();
        foreach (Block block in blocks)
        {
            possiblePosition = block.transform.position + new Vector3(-1, 0);
            if (IsValidPosition(possiblePosition))
                possibleVariants.Add(possiblePosition);

            possiblePosition = block.transform.position + new Vector3(1, 0);
            if (IsValidPosition(possiblePosition))
                possibleVariants.Add(possiblePosition);

            possiblePosition = block.transform.position + new Vector3(0, -1);
            if (IsValidPosition(possiblePosition))
                possibleVariants.Add(possiblePosition);

            possiblePosition = block.transform.position + new Vector3(0, 1);
            if (IsValidPosition(possiblePosition))
                possibleVariants.Add(possiblePosition);
        }

        List<Vector3> nextPositions = new List<Vector3>();
        foreach (Vector3 position in possibleVariants)
        {
            if (GetRandomBool())
            {
                nextPositions.Add(position);
            }
        }
        return nextPositions;
    }

    private Vector3 GetNewPosition()
    {
        int xNew = Random.Range(minX, maxX + 1);
        int yNew = Random.Range(minY, maxY + 1);
        Vector3 newPosition = new Vector3(xNew, yNew);
        if (!IsValidPosition(newPosition))
        {
            return GetNewPosition();
        }
        return newPosition;
    }

    private bool IsValidPosition(Vector3 pos)
    {
        if (pos.x >= minX && pos.x <= maxX && pos.y >= minY && pos.y <= maxY && !IsOccupied(pos))
        {
            return true;
        }
        return false;
    }

    private bool IsOccupied(Vector3 pos)
    {
        foreach (Block block in blocks)
        {
            if (block.transform.position.x == pos.x && block.transform.position.y == pos.y)
            {
                return true;
            }
        }
        return false;
    }

    private bool GetRandomBool()
    {
        float guess = Random.Range(0f, 1f);
        return guess <= spawnConsistency;
    }

    private int GetRandomDurability()
    {
        int mod = Mathf.RoundToInt(gameController.currentWave / 10);
        float r = Random.Range(0f, 1f);
        if (r < 0.1f)
        {
            mod += 2;
        }
        else if (r < 0.25f)
        {
            mod++;
        }
        else if (r > 0.75f)
        {
            mod--;
        }
        int durability = Mathf.Clamp(mod, 1, maxBlockDurability);
        return durability;
    }

}
