using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class PlayerMovement : MonoBehaviour, IPunObservable
{
    public static int playerIndex;

    public PhotonView PV;
    public RaycastHit[] hit;

    public bool playerNumber;

    public Transform _transform;
    public Rigidbody _rigidbody;

    public float speed;
    public float minSpeed;
    public float speedInertia;
    public bool canSpeedBoost;
    public float boostInertia;
    public float speedCooldown;

    public float gravity;
    public Vector3 direction;
    public float xOffset;
    public float xOffsetInertia;
    public float xOffsetLeftInertia, xOffsetRightInertia;
    public float xKnockForceIncrement;
    public float xKnockForce;
    public float xEdge;
    public bool hasFallen;

    public int pointsCount;
    public int pointsIndex0, pointsIndex1, pointsIndex2;
    public float increment;

    public bool leftKeyPressed, rightKeyPressed, boostKeyPressed;
    public int boostState;

    public Quaternion startRotation, endRotation;

    public string playerTag;
    public string obstacleTag;
    public MeshRenderer _renderer;

    private Vector3 smoothMove;
    public int otherIndex;
    public float otherOffset;
    public float otherLeft;
    public float otherRight;
    public float otherSpeed;

    bool flip;

    [Header("Touch Input")]
    public Vector2 m_StartingTouch;
	public bool m_IsSwiping = false;
	public bool m_IsTap = false;

    void Awake()
    {
        hit = new RaycastHit[1];

        // _transform = transform;
        // _rigidbody = rigidbody;
        // _renderer = meshRenderer;

        playerTag = "Player";
        obstacleTag = "Obstacle";

        SetValues(playerIndex);
        playerIndex++;

        // flip = false;

        _transform.SetParent(GameState.instance.transform);
    }

    void Start()
    {
        PV.RPC("RPC_Init", RpcTarget.All, null);
    }

    void SetValues(int number)
    {
        SetPlayer(number);

        SetPoints(
            RoadCreator.instance.evenPoints.Length, 
            RoadCreator.instance.roadWidth * 0.5f
        );

        // Init();
    }

    public void SetPlayer(int number)
    {
        playerNumber = number == 0 ? true : false;

        if(playerNumber)
        {
            _renderer.material.SetColor("_Color", Color.green);
            flip = false;
        }
        else
        {
            _renderer.material.SetColor("_Color", Color.red);
            flip = true;
        }

        // Init();
    }

    public void Init()
    {
        speed = minSpeed;
        minSpeed = 17.5f;
        speedInertia = 0;

        canSpeedBoost = true;
        boostInertia = 0;
        speedCooldown = 1;

        xOffset = playerNumber ? xEdge * 0.5f : xEdge * -0.5f;
        xOffsetInertia = 0;
        xOffsetLeftInertia = 0;
        xOffsetRightInertia = 0;

        xKnockForceIncrement = 0;
        xKnockForce = 0;

        pointsIndex0 = pointsCount - 1;
        pointsIndex1 = 0;
        pointsIndex2 = 1;

        increment = 0;

        gravity = 0;

        startRotation = Quaternion.LookRotation(BezierMovement.instance.GetDirection(pointsIndex0), Vector3.up);
        endRotation = Quaternion.LookRotation(BezierMovement.instance.GetDirection(pointsIndex1), Vector3.up);

        hasFallen = false;

        Vector3 startPos = BezierMovement.instance.points[pointsIndex0];
        startPos += _transform.TransformDirection(xOffset, 0, 0);
        startPos.y += 0.25f;

        _rigidbody.MovePosition(startPos);

        leftKeyPressed = false;
        rightKeyPressed = false;
        boostKeyPressed = false;

        boostState = 0;
    }

    public void SetPoints(int length, float edge)
    {
        pointsCount = length;
        xEdge = edge;

        Init();
    }

    void SetNextIndices()
    {
        if(pointsIndex0 == pointsCount - 3)
        {
            if(PV.IsMine)
                GameState.instance.SpawnNewObstacleFromPlayer();

            pointsIndex0++;
            pointsIndex1++;
            pointsIndex2 = 0;
        }
        else if(pointsIndex0 == pointsCount - 2)
        {
            pointsIndex0++;
            pointsIndex1 = 0;
            pointsIndex2 = 1;
        }
        else if(pointsIndex0 == pointsCount - 1)
        {
            // Debug.Break();
            pointsIndex0 = 0;
            pointsIndex1 = 1;
            pointsIndex2 = 2;
        }
        else if(pointsIndex0 >= 0)
        {
            pointsIndex0++;
            pointsIndex1++;
            pointsIndex2++;
        }

        direction = BezierMovement.instance.GetDirection(pointsIndex0);

        startRotation = endRotation;
        endRotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    public void Update()
    {
        if(PV.IsMine)
        {
            if(!hasFallen)
            {
                GetInput();
                MouseInput();
                AndroidInput();
            }

            if(_transform.localPosition.y < -10)
                Init();
        }
        else
        {
            SmoothMovement();
        }
    }

    void SmoothMovement()
    {
        _transform.localPosition = Vector3.Lerp(_transform.localPosition, smoothMove, Time.deltaTime * 10.0f);
    }

    public void FixedUpdate()
    {
        if(!PV.IsMine)
            return;
        
        ExecuteMovement();
        
        // PV.RPC("RPC_Knock", RpcTarget.All);
    }

    [PunRPC]
    void RPC_Knock()
    {
        int count = Physics.SphereCastNonAlloc(_transform.localPosition, 0.5f, Vector3.zero, hit, 0);
        Debug.Log("Count: " + count);

        if(count > 0)
        {
            Debug.Log(hit[0].transform.name, hit[0].transform.gameObject);
            if(hit[0].transform.CompareTag(playerTag))
            {
                Debug.Log(hit[0].transform.name, hit[0].transform.gameObject);
                hit[0].transform.GetComponent<PlayerMovement>().Init();
                // hit[0].transform.GetComponent<PlayerController>().movement.SetKnockValues(
                //     pointsIndex0, 
                //     xOffset, 
                //     xOffsetLeftInertia, 
                //     xOffsetRightInertia, 
                //     boostInertia
                // );
            }
        }
    }

    // private void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    // {
    //     if(stream.isWriting)
    //     {
    //         stream.SendNext(_rigidbody.position);
    //     }
    //     else
    //     {
    //         selfPos = (Vector3)stream.ReceiveNext();
    //     }
    // }

    void ExecuteMovement()
    {
        increment += Time.fixedDeltaTime * speed * speedInertia;
        if(increment >= 1)
        {
            float diff = increment - 1;
            increment = diff;
            SetNextIndices();
        }

        // Debug.Log("0: " + pointsIndex0);
        // Debug.Log("1: " + pointsIndex1);

        Vector3 movement = BezierMovement.instance.CalculateLerp(
            pointsIndex0, 
            pointsIndex1, 
            increment
        );

        Quaternion rotation = Quaternion.LerpUnclamped(
            startRotation, 
            endRotation, 
            increment
        );

        movement += _transform.TransformDirection(xOffset, 0, 0);
        movement.y += 0.25f;

        if(Mathf.Abs(xOffset) > xEdge)
        {
            hasFallen = true;
        }
        if(hasFallen)
        {
            if(gravity > -25.0f)
                gravity += Time.fixedDeltaTime * -10.0f;
            
            movement.y += gravity;
        }

        // Debug.Log("Force: " + xOffset);

        CalculateInertia();

        _rigidbody.MovePosition(movement);
        _rigidbody.MoveRotation(rotation);
    }

    bool IsGrounded()
    {
        int count = Physics.RaycastNonAlloc(_transform.localPosition, Vector3.down, hit, 0.3f);

        return count == 1 ? true : false;
    }

    void GetInput()
    {
        leftKeyPressed = false;
        rightKeyPressed = false;
        boostKeyPressed = false;

        if(Input.GetKey(KeyCode.Q))
            Left();

		if(Input.GetKey(KeyCode.E))
			Right();

        if(Input.GetKey(KeyCode.W))
        {
            Boost();
        }

        if(Input.GetKeyDown(KeyCode.S))
        {
            // Init();
            PV.RPC("RPC_ChangeColor", RpcTarget.All, null);
        }
    }

    public void Left()
    {
        leftKeyPressed = true;
    }

    public void Right()
    {
        rightKeyPressed = true;
    }

    public void Boost()
    {
        if(!PV.IsMine)
            return;
        
        boostKeyPressed = true;

        if(boostState == 0)
            boostState = 1;
    }

    public void SetKnockValues()
    {
        xKnockForceIncrement = 1;

        // xKnockForce = otherOffset - xOffset;
        // xKnockForce *= 0.01f;
        // xKnockForce += 0.5f * 0.05f;

        Debug.Log("X: " + xKnockForce);


        // float diff = (xOffset - otherOffset);
        float forceDiff = otherSpeed - boostInertia;
        float speedMulti = forceDiff > 0 ? 0.05f : 0;

        // From Which Side does the Other Player Come
        // From Right Side
        if(otherOffset > xOffset)
        {
            xKnockForce = otherOffset - xOffset;
            xKnockForce *= 0.01f;
            xKnockForce += otherRight * 0.05f;
        }
        else
        {
            xKnockForce = xOffset - otherOffset;
            xKnockForce *= 0.01f;
            xKnockForce += otherLeft * -0.05f;
        }

        Debug.Log("X: " + xKnockForce);

        // return;

        // // The Other Player is Behind Us
        // if(otherIndex < pointsIndex0)
        // {
        //     xKnockForce += speedMulti * forceDiff;
        // }

        // if(speedInertia < 1)
        // {
        //     xKnockForce += 0.05f * (otherSpeed + 1 - minSpeed);
        // }

        // Debug.Log("------------");
        // Debug.Log(otherIndex);
        // Debug.Log(otherOffset);
        // Debug.Log(otherLeft);
        // Debug.Log(otherRight);
        // Debug.Log(otherSpeed);
    }

    void CalculateInertia()
    {
        if(speedInertia < 1)
        {
            speedInertia += Time.fixedDeltaTime * 0.5f;

            if(speedInertia > 1)
                speedInertia = 1;
        }

        // 1 -> 0
        if(!leftKeyPressed)
        {
            if(xOffsetLeftInertia > 0)
                xOffsetLeftInertia -= Time.fixedDeltaTime * 0.5f;
            else
                xOffsetLeftInertia = 0;
        }
        else    // 0 -> 1
        {
            if(xOffsetLeftInertia < 1)
                xOffsetLeftInertia += 0.05f * Time.fixedDeltaTime;
            else
                xOffsetLeftInertia = -1;
        }

        if(!rightKeyPressed)
        {
            if(xOffsetRightInertia > 0)
                xOffsetRightInertia -= Time.fixedDeltaTime * 0.5f;
            else
                xOffsetRightInertia = 0;
        }
        else
        {
            if(xOffsetRightInertia < 1)
                xOffsetRightInertia += 0.05f * Time.fixedDeltaTime;
            else
                xOffsetRightInertia = 1;
        }

        // 1 -> 0
        if(xKnockForceIncrement > 0)
        {
            xKnockForceIncrement -= 2.0f * Time.fixedDeltaTime;

            if(xKnockForceIncrement < 0)
            {
                xKnockForceIncrement = 0;
                xKnockForce = 0;
            }
        }

        xOffset += ((xOffsetRightInertia - xOffsetLeftInertia) * 1.0f) + (xKnockForceIncrement * xKnockForce);
        
        if(boostState == 1)
        {
            boostInertia = 1.25f;
            boostState = 2;
        }
        if(boostState == 2)
        {
            boostInertia -= 1.0f * Time.deltaTime;

            if(boostInertia < 0)
            {
                boostState = 3;
                boostInertia = 0;
                speedCooldown = 0;
            }
        }
        if(boostState == 3)
        {
            speedCooldown += Time.fixedDeltaTime * 0.75f;
            if(speedCooldown > 1)
            {
                boostState = 0;
                speedCooldown = 0;
            }
        }

        speed = minSpeed + boostInertia * 30.0f;
    }

    float GetXOffset()
    {
        float offset;

        offset = Vector3.Dot(_transform.right, direction);// * xOffset;
        Debug.Log("Right: " + offset);

        return offset;
    }

    [PunRPC]
    void RPC_ChangeColor()
    {
        if(flip)
        {
            _renderer.material.SetColor("_Color", Color.green);
        }
        else
        {
            _renderer.material.SetColor("_Color", Color.red);
        }

        flip = !flip;
    }

    [PunRPC]
    void RPC_Init()
    {
        Init();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!PV.IsMine)
            return;

        if(other.tag == playerTag)
            SetKnockValues();
    }

    void OnTriggerStay(Collider other)
    {
        if(!PV.IsMine)
            return;
        
        if(other.tag == obstacleTag)
        {
            speedInertia = 0;
        }
    }

    public void MouseInput()
    {
        // Mouse
		Vector2 input = Input.mousePosition;

		if(Input.GetMouseButtonDown(0))
		{
			m_StartingTouch = input;
			m_IsSwiping = true;
			m_IsTap = false;
		}

		if(Input.GetMouseButton(0))
		{
			if(m_IsSwiping)
            {
                Vector2 diff = input - m_StartingTouch;

                // Put difference in Screen ratio, but using only width, so the ratio is the same on both
                // axes (otherwise we would have to swipe more vertically...)
                diff = new Vector2(diff.x/Screen.width, diff.y/Screen.width);

                if(diff.magnitude > 0.01f) //we set the swip distance to trigger movement to 1% of the screen width
                {
                    if(Mathf.Abs(diff.y) < Mathf.Abs(diff.x))
                    {
                        if(diff.x < 0)
                        {
                            Left();
                        }
                        else
                        {
                            Right();
                        }
                    }
                        
                    // m_IsSwiping = false;
					// m_IsTap = false;
                }
				else
				{
					m_IsTap = true;
				}
            }
		}

		if(Input.GetMouseButtonUp(0))
		{
			m_IsSwiping = false;

			// if(input == m_StartingTouch)
			if(m_IsTap)
			{
				// Boost();
				m_IsTap = false;
			}
		}
    }

    public void AndroidInput()
    {
        if(Input.touchCount == 1)
        {
            if(m_IsSwiping)
            {
                Vector2 diff = Input.GetTouch(0).position - m_StartingTouch;

                // Put difference in Screen ratio, but using only width, so the ratio is the same on both
                // axes (otherwise we would have to swipe more vertically...)
                diff = new Vector2(diff.x/Screen.width, diff.y/Screen.width);

                if(diff.magnitude > 0.01f) //we set the swip distance to trigger movement to 1% of the screen width
                {
                    if(Mathf.Abs(diff.y) < Mathf.Abs(diff.x))
                    {
                        if(diff.x < 0)
                        {
                            Left();
                        }
                        else
                        {
                            Right();
                        }
                    }

					// m_IsTap = false;
                    // m_IsSwiping = false;
                }
				else
				{
					// m_IsSwiping = false;
					m_IsTap = true;
				}
            }

			// Input check is AFTER the swip test, that way if TouchPhase.Ended happen a single frame after the Began Phase
			// a swipe can still be registered (otherwise, m_IsSwiping will be set to false and the test wouldn't happen for that began-Ended pair)
			if(Input.GetTouch(0).phase == TouchPhase.Began)
			{
				m_StartingTouch = Input.GetTouch(0).position;
				m_IsSwiping = true;
				m_IsTap = false;
			}

            if(Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                m_IsSwiping = false;

				// if(Input.GetTouch(0).position == m_StartingTouch)
				if(m_IsTap)
				{
					// Boost();
					m_IsTap = false;
				}
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(_transform.localPosition);

            stream.SendNext(pointsIndex0);
            stream.SendNext(xOffset);
            stream.SendNext(xOffsetLeftInertia);
            stream.SendNext(xOffsetRightInertia);
            stream.SendNext(speed);
        }
        else if(stream.IsReading)
        {
            smoothMove = (Vector3)stream.ReceiveNext();
            otherIndex = (int)stream.ReceiveNext();
            otherOffset = (float)stream.ReceiveNext();
            otherLeft = (float)stream.ReceiveNext();
            otherRight = (float)stream.ReceiveNext();
            otherSpeed = (float)stream.ReceiveNext();
        }
    }
}
