using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour, ITrackableObject
{
    public InputAction playerControl;
    public float moveSpeed = 5;
    public Rigidbody2D rb;

    //Condition Feature TEST//======================================================================================================
    private readonly TrackedProperty<int> _killCount = new TrackedProperty<int>(0);
    public TrackedProperty<int> KillCount => _killCount;
    private readonly TrackedProperty<int> _hp = new TrackedProperty<int>(0);
    public TrackedProperty<int> HP => _hp;
    private readonly TrackedProperty<float> _speed = new TrackedProperty<float>(0);
    public TrackedProperty<float> Speed => _speed;
    private readonly TrackedProperty<Vector3> _position = new TrackedProperty<Vector3>(Vector3.zero);
    public TrackedProperty<Vector3> Position => _position;
    public Dictionary<string, ITrackedProperty> GetProperties()
    {
        return new Dictionary<string, ITrackedProperty>
        {
            { "HP", HP },
            { "Speed", Speed },
            { "KillCount", KillCount },
            { "Position", Position }
        };
    }//링큐나 리플렉션 쓸까 고민해봤는데 굳이? 싶어서 각 클래스가 트래킹할 필드를 직접 넘기는 걸로 이렇게 남겨둡니다. -> 논의 필요
     //=============================================================================================================================

    Vector2 moveDirection = Vector2.zero;



    private void OnEnable()
    {
        playerControl.Enable();
    }
    private void OnDisable()
    {
        playerControl.Disable();
    }


    void Update()
    {
        moveDirection = playerControl.ReadValue<Vector2>();

        //Condition Feature TEST//======================================================================================================
        Position.Value = transform.position;
        //=============================================================================================================================

    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);
    }


}
