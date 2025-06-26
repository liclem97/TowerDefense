using UnityEngine;

public class ItemFloat : MonoBehaviour
{
    [Header("Float Settings")]
    [SerializeField] private float floatAmplitude = 0.5f; // �ٿ ����
    [SerializeField] private float floatFrequency = 1f;   // �ٿ �ӵ�

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        // y������ ���� ��� ���� ���� ����
        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}