using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdFollower : MonoBehaviour
{
    public Transform hmdTransform;              // HMD 위치
    public float followDistance = 0.7f;         // 따라올 거리 (예: 1m)
    public float followHeightOffset = 0.3f;     // 약간 위에 있도록
    public float followSpeed = 3.0f;

    public float maxFollowStartDistance = 0.7f;  // 이 거리 이내면 따라오기 시작
    public float stopFollowDistance = 1.5f;      // 이 거리 넘으면 따라오기 멈춤


    public bool isFollowing = true;             // 내부에서 제어
    private bool externallyDisabled = false; //외부에서 제어 가능


    // Update is called once per frame
    void Update()
    {
        if (hmdTransform == null || externallyDisabled)
            return;

        float currentDistance = Vector3.Distance(transform.position, hmdTransform.position);

        // 새가 너무 멀어졌으면 멈춤
        if (isFollowing && currentDistance > stopFollowDistance)
        {
            Debug.Log("[BirdFollower] 새가 너무 멀어졌습니다. 따라오기 멈춤.");
            isFollowing = false;
        }

        // 새가 가까워졌으면 따라오기 시작
        if (!isFollowing && currentDistance <= maxFollowStartDistance)
        {
            Debug.Log("[BirdFollower] 새가 가까워졌습니다. 따라오기 시작합니다.");
            isFollowing = true;
            //StateManager.instance.SetInteraction(InteractionState.Follow);
        }

        // 따라오기 로직
        if (isFollowing)
        {
            //Vector3 targetPosition = hmdTransform.position +
            //                         hmdTransform.forward * followDistance +
            //                         Vector3.up * followHeightOffset;

            Vector3 localOffset = new Vector3(0.5f, -0.3f, 0.7f);  // 오른쪽 + 위
            Vector3 targetPosition = hmdTransform.TransformPoint(localOffset);

            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
        }
    }

    public void SetFollowing(bool shouldFollow)
    {
        isFollowing = shouldFollow;
    }

    // 외부에서 호출: 자동 제어 막기/해제
    public void SetExternalControl(bool disabled)
    {
        Debug.Log($"[BirdFollower] 외부 제어 설정: {(disabled ? "비활성화" : "활성화")}");
        externallyDisabled = disabled;
    }

}
