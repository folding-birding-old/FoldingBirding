using Oculus.Interaction.Input;
using Oculus.Platform;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerEvent : MonoBehaviour
{
    private HandRef handRef;

    private Pose pose;

    private GameObject bird;

    public float followSpeed = 3f;
    public float attachDistance = 0.1f;
    public Vector3 localOffset = new Vector3(0f, 0.0f, 0f);
    public bool isSelected;

    public bool isBirdAttached = false;

    //원래 새 회전값
    private Quaternion defaultRotation;

    void Start()
    {
        bird = GameObject.FindWithTag("MyBird"); //새 오브젝트찾기
        handRef = GetComponent<HandRef>(); //손 찾기 
        if (handRef == null)
        {
            Debug.LogError("HandRef component not found on the GameObject.");
            return;
        }

        if (bird != null)
            defaultRotation = bird.transform.rotation;
        // Subscribe to the OnLog event
        //OVRManager.OnLog += OnLog;
    }

    private void Update()
    {
        if (!isSelected || bird == null || handRef == null)
        {
            // 회전 복원 시도
            if (!isBirdAttached && bird != null)
            {
                bird.transform.rotation = Quaternion.Slerp(
                    bird.transform.rotation,
                    defaultRotation,
                    Time.deltaTime * 5f
                );
            }
            return;
        }

        if (handRef.Hand.GetJointPose(HandJointId.HandIndex2, out pose))
        {
            Vector3 targetPos = pose.position + pose.rotation * localOffset;
            float distance = Vector3.Distance(bird.transform.position, targetPos);

            if (!isBirdAttached && distance <= attachDistance)
            {
                // 새가 손가락 가까이 왔을 때 착지
                isBirdAttached = true;
                Debug.Log("새가 손가락에 착지!");
            }

            if (isBirdAttached)
            {
                // 손가락 위에 붙이기
                Vector3 offset = new Vector3(-0.14f, -0.016f, 0.08f);
                Vector3 finalPosition = pose.position + pose.rotation * offset;
                bird.transform.position = finalPosition;
                bird.transform.rotation = pose.rotation * Quaternion.Euler(180f, 180f, -90f); // 방향 고정
            }
            else
            {
                // 따라오기(Lerp)
                bird.transform.position = Vector3.MoveTowards(
                    bird.transform.position,
                    targetPos,
                    Time.deltaTime * followSpeed
                );
            }
        }


        //if (isSelected)
        //{
        //    handRef.Hand.GetJointPose(HandJointId.HandIndex2, out pose);
        //    bird.transform.position = pose.position;
        //}
    }



    public void OnFingerEvent (bool isFingerUp)
    {
        //Debug.Log("OnFingerEvent 호출됨: " + isFingerUp);
        isSelected = isFingerUp;
        //StateManager.instance.SetInteraction(InteractionState.Finger);
        if (!isFingerUp)
        {
            isBirdAttached = false; // 손가락 내리면 다시 떨어짐
        }
    }

    public void OnLog(string msg)
    { 
        Debug.Log(msg); 
    }


}
