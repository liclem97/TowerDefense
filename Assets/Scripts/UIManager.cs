using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI")]
    public GameObject gameStartUI;
    public GameObject cctvPanelUI;
    public GameObject gameOverUI;

    public Text waveText;
    public Text enemyCountText;
    public Text towerHPText;
    public Text coinText;

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
}