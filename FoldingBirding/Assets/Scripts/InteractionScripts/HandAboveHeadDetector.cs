using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandAboveHeadDetector : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform headTransform;  // HMD, 즉 머리 위치
    public Transform handTransform;  // 손 위치 (왼손 or 오른손)
    public float yOffset = 0.15f;    // 머리보다 얼마나 위로 올라가야 하는지 기준

    private GameObject bird;
    private bool isMovingBird = false;

    public float birdMoveSpeed = 2f;


    void Start()
    {
        bird = GameObject.FindWithTag("MyBird");
    }


    void Update()
    {
        float headY = headTransform.position.y;
        float handY = handTransform.position.y;
        
        // 매 프레임 손과 머리의 Y 좌표 로그 출력

        if (handY > headY + yOffset)
        {
            if (!isMovingBird)
            {
                Debug.Log("[HandAboveHeadDetector] 손이 머리 위로 올라갔습니다!");
                isMovingBird = true;
                //StateManager.instance.SetInteraction(InteractionState.Call);

            }
        }
        else
        {
            isMovingBird = false;
        }

        if (isMovingBird && bird != null)
        {
            //따라오던 새 정지
            //bird.GetComponent<BirdFollower>()?.SetExternalControl(true);
            Vector3 target = headTransform.position + headTransform.forward * 0.5f;

            bird.transform.position = Vector3.Lerp(bird.transform.position, headTransform.position + headTransform.forward * 0.5f, Time.deltaTime * birdMoveSpeed);

            float distance = Vector3.Distance(bird.transform.position, target);
            if (distance < 0.05f)
            {
                Debug.Log("새가 플레이어 앞에 도착함: Follow");
                //StateManager.instance.SetInteraction(InteractionState.Follow); // 2단계: 따라다니기 시작
                isMovingBird = false;
            }
        }
    }
}
