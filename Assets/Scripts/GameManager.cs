using Meta.WitAi;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    enum WaveState { BeforeWave, Process, EndWave }

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject enemyPrefab_Drone;
    [SerializeField] private GameObject enemyPrefab_Melee;

    [Header("Enemy SpawnPoint")]
    [SerializeField] private Transform meleeEnemySpawnPoint;
    [SerializeField] private Transform droneEnemySpawnPoint;

    [Header("Target")]
    public Transform mainTarget;

    [Header("Explosion Effect")]
    [SerializeField] private GameObject explosionEffect;

    public bool isGameStarted = false;
    private WaveState waveState = WaveState.BeforeWave;
    private int waveCount;
    private int meleeEnemyToSpawn;
    private int droneEnemyToSpawn;
    private float currentTime = 0f;
    private float waitingTime = 5f;
    private float spawnDelayTime = 1.5f;
    private bool shouldCheckEnemy = false;

    public int playerCoin;

    private readonly WaitForSeconds enemyCheckInterval = new WaitForSeconds(0.5f);
    private Coroutine enemyCheckRoutine;       // �ڷ�ƾ �ڵ� ������

    private void Awake()
    {
        // �̹� �ν��Ͻ��� �����ϸ� ���� ������ ��ü �ı�
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // ���� �ν��Ͻ� �ʱ�ȭ
        Instance = this;
        DontDestroyOnLoad(gameObject);   // �� ��ȯ �ÿ��� ����
    }

    public void SpawnExplosionParticle(Transform transform)
    {
        if (explosionEffect)
        {
            GameObject spawnedEffect = Instantiate(explosionEffect, transform);
            if (spawnedEffect)
            {
                spawnedEffect.GetComponent<ParticleSystem>().Play();
                Debug.Log("effect spawned");
            }
            //spawnedEffect.GetComponent<ParticleSystem>().Play();
        }
    }
    public void GameStart()
    {
        isGameStarted = true;
        waveCount = 0;
        playerCoin = 0;

        UIManager.Instance.gameStartUI.SetActive(false);
        UIManager.Instance.cctvPanelUI.SetActive(true);
        UIManager.Instance.waveText.text = $"Wave : {waveCount}";
        UIManager.Instance.coinText.text = $"COIN: {playerCoin}$";
    }

    private void Update()
    {
        if (!isGameStarted) return;

        switch (waveState)
        {
            case WaveState.BeforeWave: BeforeWave(); break;
            case WaveState.Process: break;
            case WaveState.EndWave: break;
        }
    }

    private void BeforeWave()
    {
        currentTime += Time.time;
        if (currentTime > waitingTime)
        {
            currentTime = 0f;
            waveState = WaveState.Process;
            StartCoroutine(nameof(EnemySpwanProcess));
        }
    }

    private IEnumerator EnemySpwanProcess()
    {
        // 1. ���� �� ����
        for (int i = 0; i < meleeEnemyToSpawn; i++)
        {
            Instantiate(enemyPrefab_Melee, meleeEnemySpawnPoint.position, meleeEnemySpawnPoint.rotation);
            yield return new WaitForSeconds(spawnDelayTime);
        }

        // 2. ��� �� ����
        for (int i = 0; i < droneEnemyToSpawn; i++)
        {
            Instantiate(enemyPrefab_Drone, droneEnemySpawnPoint.position, droneEnemySpawnPoint.rotation);
            yield return new WaitForSeconds(spawnDelayTime);
        }

        // ���� ��� ������ �� üũ �÷��� Ȱ��ȭ
        enemyCheckRoutine = StartCoroutine(CheckEnemiesRoutine());
    }

    private IEnumerator CheckEnemiesRoutine()
    {
        while (true)
        {
            // �±� ��� �˻� - �ʿ��ϸ� Ǯ�� �Ŵ�����ī���ͷ� ��ü ����
            if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
            {
                // ���̺� ���� ó��
                waveState = WaveState.EndWave;
                EndWave();
                yield break;                 // ���� ����
            }

            yield return enemyCheckInterval; // 0.5 �� ���
            UIManager.Instance.enemyCountText.text = $"ENEMY : {GameObject.FindGameObjectsWithTag("Enemy").Length}";
        }
    }

    private void EndWave()
    {
        waveCount++;
        droneEnemyToSpawn = waveCount;
        meleeEnemyToSpawn = waveCount /*/3*/;
        currentTime = 0f;
        waveState = WaveState.BeforeWave;

        // UI ������Ʈ
        UIManager.Instance.waveText.text = $"Wave : {waveCount}";

        // �˻� �ڷ�ƾ ����(������ġ)
        if (enemyCheckRoutine != null)
        {
            StopCoroutine(enemyCheckRoutine);
            enemyCheckRoutine = null;
        }
    }

    public void GameOver()
    {
        // ���� ���� �� �� ��ü�� ���� ã��
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy); // ���� ã�Ƽ� ��� ����
        }

        isGameStarted = false; // ���� ���� ���·� ���� (���Ͻô� ���·� ����)
        Debug.Log("GameOVer");
    }

    public void AddPlayerCoin(int coin)
    {
        playerCoin += coin;
        UIManager.Instance.coinText.text = $"COIN: {playerCoin}$";
    }
}