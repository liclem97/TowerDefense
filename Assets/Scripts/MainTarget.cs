using UnityEngine;

// ���� �� MainTarget Ŭ����
public class MainTarget : MonoBehaviour
{
    public float targetHP;

    private void Start()
    {
        UIManager.Instance.towerHPText.text = $"TOWER HP\r\n{targetHP}/100";    // ���� �� UI ����
    }


    /*********************************************************************************************
    �Լ�: OnTargetDamaged
    ���: ���� Ÿ���� ü���� �����ϰ� 0 ������ �� GameOver�� ȣ��
    �Ű�����
        - damage: ���� ������� ��
    *********************************************************************************************/
    public void OnTargetDamaged(float damage)
    {
        targetHP -= damage;
        if (targetHP <= 0)
        {
            targetHP = 0;
            if (GameManager.Instance.isGameStarted == true)
            {
                GameManager.Instance.GameOver();
            }
        }
        UIManager.Instance.towerHPText.text = $"TOWER HP\r\n{targetHP}/100";    // UI ������Ʈ
    }
}
