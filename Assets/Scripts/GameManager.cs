using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    enum WaveState { BeforeWave, Process, EndWave }             // 웨이브 상태 열거형

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject enemyPrefab_Drone;      // 스폰할 드론(원거리) 적 프리팹
    [SerializeField] private GameObject enemyPrefab_Melee;      // 스폰할 근거리 적 프리팹

    [Header("Enemy SpawnPoint")]
    [SerializeField] private Transform meleeEnemySpawnPoint;    // 근거리 적의 스폰 위치
    [SerializeField] private Transform droneEnemySpawnPoint;    // 원거리 적의 스폰 위치

    [Header("Target")]
    public Transform mainTarget;                                // 메인 타겟(Tower)

    //게임 오버
    [Header("Game Over")]
    [SerializeField] private GameObject player;                 // 플레이어 오브젝트
    [SerializeField] private Transform gameOverPosition;        // 게임 오버 시 플레이어가 이동할 위치
    [SerializeField] private Canvas redScreenEffect;            // 게임 오버 시 화면을 붉게 하는 이펙트

    [Header("Explosion Effect")]
    [SerializeField] private GameObject explosionEffect;        // 폭발 이펙트

    [Header("Sounds")]
    [SerializeField] private AudioClip backgroundSound;         // 배경음

    public bool isGameStarted = false;                          // 게임 실행 판별 Boolean
    private WaveState waveState = WaveState.BeforeWave;         // 웨이브 상태 저장
    private int waveCount;                                      // 실행 중인 웨이브의 수
    private int meleeEnemyToSpawn;                              // 스폰해야 하는 근거리 적의 수
    private int droneEnemyToSpawn;                              // 스폰해야 하는 원거리 적의 수
    private float currentTime = 0f;                             // 경과 시간 저장
    private float waitingTime = 3f;                             // 다음 웨이브까지 기다리는 시간
    private float spawnDelayTime = 2f;                          // 적 스폰 후 다음 적을 스폰하는 딜레이 시간

    public int playerCoin;                                      // 플레이어가 소지한 코인

    private readonly WaitForSeconds enemyCheckInterval = new WaitForSeconds(0.5f);  // 씬의 적의 수를 판별하는 간격
    private Coroutine enemyCheckRoutine;       // 코루틴 핸들 보관용

    private void Awake()
    {
        // 이미 인스턴스가 존재하면 새로 생성된 객체 파괴
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // 최초 인스턴스 초기화
        Instance = this;
        //DontDestroyOnLoad(gameObject);   // 씬 전환 시에도 유지
        // 활성 시 ReStart에 버그가 일어남
    }

    /*********************************************************************************************
    함수: SpawnExplosionParticle
    기능: 지정한 위치에서 폭발 이펙트를 스폰함
    매개변수:
        - transform: 폭발 이펙트를 스폰할 위치 값
    *********************************************************************************************/
    public void SpawnExplosionParticle(Transform transform)
    {
        if (explosionEffect)
        {
            GameObject spawnedEffect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            if (spawnedEffect)
            {
                spawnedEffect.GetComponent<ParticleSystem>().Play();
                Destroy(spawnedEffect, 2f);
            }
        }
    }

    /*********************************************************************************************
    함수: GameStart
    기능: 게임을 시작시키고 CCTV 패널의 UI를 갱신함
    *********************************************************************************************/
    public void GameStart()
    {
        isGameStarted = true;
        waveCount = 0;
        playerCoin = 0;

        // UI 갱신
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

    /*********************************************************************************************
    함수: BeforeWave
    기능: 웨이브 시작 전 함수, 경과 시간을 넘으면 Process로 상태를 변경하고 적 소환 코루틴 시작
    *********************************************************************************************/
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

    /*********************************************************************************************
    함수: EnemySpwanProcess
    기능: 적을 지정된 위치에서 스폰하는 함수
    *********************************************************************************************/
    private IEnumerator EnemySpwanProcess()
    {
        // 1. 근접 적 스폰
        for (int i = 0; i < meleeEnemyToSpawn; i++)
        {
            Instantiate(enemyPrefab_Melee, meleeEnemySpawnPoint.position, meleeEnemySpawnPoint.rotation);
            yield return new WaitForSeconds(spawnDelayTime);
        }

        // 2. 드론 적 스폰
        for (int i = 0; i < droneEnemyToSpawn; i++)
        {
            Instantiate(enemyPrefab_Drone, droneEnemySpawnPoint.position, droneEnemySpawnPoint.rotation);
            yield return new WaitForSeconds(spawnDelayTime);
        }

        // 적을 모두 스폰한 후 체크 플래그 활성화
        enemyCheckRoutine = StartCoroutine(CheckEnemiesRoutine());
    }

    /*********************************************************************************************
    함수: CheckEnemiesRoutine
    기능: 씬에 있는 적의 수를 판별하고, 적이 다 죽으면 EndWave로 넘어감
    *********************************************************************************************/
    private IEnumerator CheckEnemiesRoutine()
    {
        while (true)
        {
            // 태그 기반 검색 - 필요하면 풀링 매니저·카운터로 교체 가능
            if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0)
            {
                // 웨이브 종료 처리
                waveState = WaveState.EndWave;
                EndWave();
                yield break;                 // 루프 종료
            }

            yield return enemyCheckInterval; // 0.5 초 대기
            UIManager.Instance.enemyCountText.text = $"ENEMY : {GameObject.FindGameObjectsWithTag("Enemy").Length}";
        }
    }

    /*********************************************************************************************
    함수: EndWave
    기능: 
        1. 웨이브 카운트 증가
        2. 원거리 적은 웨이브 마다 1마리씩, 근거리 적은 3웨이브마다 1마리씩 증가
        3. 경과시간을 초기화하고 상태를 BeforWave로 넘김
    *********************************************************************************************/
    private void EndWave()
    {
        waveCount++;
        droneEnemyToSpawn = waveCount;
        meleeEnemyToSpawn = waveCount / 3;
        currentTime = 0f;
        waveState = WaveState.BeforeWave;

        // UI 업데이트
        UIManager.Instance.waveText.text = $"Wave : {waveCount}";

        // 검사 코루틴 정지(안전장치)
        if (enemyCheckRoutine != null)
        {
            StopCoroutine(enemyCheckRoutine);
            enemyCheckRoutine = null;
        }
    }

    /*********************************************************************************************
    함수: GameOver
    기능: 
        1. 게임 종료 상태로 바꿈
        2. 플레이어를 gameOverPosition으로 이동 및 회전
        3. 게임 오버 UI를 활성화하고 플레이어의 화면을 빨갛게 함
    *********************************************************************************************/
    public void GameOver()
    {
        isGameStarted = false; // 게임 종료 상태로 설정 (원하시는 상태로 조정)
        // 시점 변경 (게임 오버 카메라 활성화)
        player.transform.position = gameOverPosition.position;
        player.transform.rotation = gameOverPosition.rotation;

        // Game Over UI 표시
        UIManager.Instance.gameOverUI.SetActive(true);

        // 빨개지는 이펙트 
        redScreenEffect.gameObject.SetActive(true);
    }

    /*********************************************************************************************
    함수: AddPlayerCoin
    기능: 플레이어의 코인을 증가시키고 UI를 갱신함
    매개변수
        - coin: 플레이어에게 추가할 코인의 양
*********************************************************************************************/
    public void AddPlayerCoin(int coin)
    {
        playerCoin += coin;
        UIManager.Instance.coinText.text = $"COIN: {playerCoin}$";
    }
}