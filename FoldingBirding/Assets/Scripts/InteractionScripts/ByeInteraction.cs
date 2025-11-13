using Oculus.Interaction;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class ByeInteraction : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject bird;
    private bool isMovingBird = false;

    public float birdMoveSpeed = 2f;
    public Vector3 moveDirection = new Vector3(0, 1, 1);

    private BirdFollower birdFollower;

    void Start()
    {
        bird = GameObject.FindWithTag("MyBird");
        moveDirection = moveDirection.normalized;
        birdFollower = GetComponent<BirdFollower>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isMovingBird && bird != null)
        {
            bird.transform.position += moveDirection * birdMoveSpeed * Time.deltaTime;
        }
    }

    public void TriggerBirdFlyAway()
    {
        isMovingBird = true;
        // 새가 따라오지 않도록 설정
        birdFollower?.SetExternalControl(true);
        //StateManager.instance.SetInteraction(InteractionState.Bye);
    }

    public void CloseBirdFlyAway()
    {
        isMovingBird = false;
        // 새가 따라오도록 설정
        birdFollower?.SetExternalControl(false);
        //bird.GetComponent<BirdFollower>()?.SetFollowing(true);
    }
}
