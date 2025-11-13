using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;



//손바닥 뻗으면 위에 새가 앉는 스크립트 
public class FollowPalmObject : MonoBehaviour
{
    public Transform palmTransform;
    public Vector3 localOffset = new Vector3(0, -0.1f, 0);
    public float followSpeed = 10f;

    private bool isPalmGestureActive = false;
    private BirdDistanceManager distanceManager;
    private BirdFollower birdFollower;

    

    void Start()
    {
        distanceManager = FindObjectOfType<BirdDistanceManager>();
        birdFollower = GetComponent<BirdFollower>(); // 같은 오브젝트에 붙어있다고 가정
    }

    void Update()
    {
        if (!isPalmGestureActive || palmTransform == null) return;

        Vector3 targetPosition = palmTransform.TransformPoint(localOffset);
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
    }

    public void OnPalmGestureDetected()
    {
        if (distanceManager != null && distanceManager.IsAnyBirdClose())
        {
            isPalmGestureActive = true;
            // 기본 따라오기 멈춤
            birdFollower?.SetExternalControl(true);
            //StateManager.instance.SetInteraction(InteractionState.Palm);
        }
    }

    public void OnPalmGestureEnded()
    {
        isPalmGestureActive = false;
        birdFollower?.SetExternalControl(false);
        //GetComponent<BirdFollower>()?.SetFollowing(true);
    }


}
