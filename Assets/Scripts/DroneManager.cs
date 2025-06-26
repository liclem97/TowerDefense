using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneManager : MonoBehaviour
{
    public float minTime = 1;
    public float maxTime = 5;
    float createTime;
    float currentTime;
    public Transform[] spawnPoints;
    public GameObject droneFactory;
    
    void Start()
    {
        createTime = Random.Range(minTime, maxTime);
    }
    
    void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime > createTime)   // 1~5�� ���� �����ϰ� ��� ����
        {
            GameObject drone = Instantiate(droneFactory);
            int index = Random.Range(0, spawnPoints.Length);
            drone.transform.position = spawnPoints[index].position; // ������ ����� 4���� ���� ����Ʈ �� �ϳ��� ��ġ ����
            currentTime = 0f;   // ��� �ð� �ʱ�ȭ
            createTime = Random.Range(minTime, maxTime);    // �����ð� ���Ҵ�
        }
    }
}
