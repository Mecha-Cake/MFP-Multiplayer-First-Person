using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class bl_PlayerMovement : bl_PhotonHelper {


    public PlayerState m_PlayerState;
    [Separator("Movement")]
    public float crouchSpeed = 2;
    public float walkSpeed = 6.0f;
    public float runSpeed = 11.0f;
    [Space(5)]
    public float m_crounchDelay = 8f;
    public float jumpSpeed = 8.0f;

    public float gravity = 20.0f;
    public float speed;
    public float ladderJumpSpeed = 5f;

    [Separator("HeadBob")]
    [SerializeField]private bl_CurveControllerBob m_HeadBob = new bl_CurveControllerBob();
    [SerializeField]private Transform BobObject = null;
    [SerializeField,Range(0.1f,10)]private float m_IntervalStep = 5;
    [SerializeField,Range(0.5f,12)]private float m_BobLerp = 7;

    [Separator("Footstep")]
    public bl_FootstepInfo[] Footsteps;
    public float minInterval = 0.1f;
    public float bias = 1.1f;
    [SerializeField]private AudioSource StepSource = null;

    private string lastFloorTag = "Untagged";
    [Separator("Second Jump")]
    //Can the player take a super jump
    public bool CanSuperJump = true;
    public float SecondJumpDelay = 0.2f;
    public bool BoostMovement = true;
    public Vector2 AirControlFactor = new Vector2(0.75f, 1.23f);
    public float SuperJumpSpeed = 14.0f;
    public AudioSource BoostSource = null;
    public AudioClip SuperJumpSound;

    [Separator("Other Settings")]
    // If the player ends up on a slope which is at least the Slope Limit as set on the character controller, then he will slide down
    public bool slideWhenOverSlopeLimit = false;

    // If checked and the player is on an object tagged "Slide", he will slide down it regardless of the slope limit
    public bool slideOnTaggedObjects = false;

    public float slideSpeed = 12.0f;

    // If checked, then the player can change direction while in the air
    public bool airControl = false;

    // Small amounts of this results in bumping when walking down slopes, but large amounts results in falling too fast
    public float antiBumpFactor = .75f;

    // Player must be grounded for at least this many physics frames before being able to jump again; set to 0 to allow bunny hopping 
    public int antiBunnyHopFactor = 1;

    public float PushPower = 1.2f;


    private Vector3 moveDirection = Vector3.zero;
    private Vector3 lastPosition = Vector3.zero;

    [HideInInspector]
    public bool grounded = false;
    [HideInInspector]
    public bool m_OnLadder;
    [Space(5)]
    [Separator("Fall Settings")]
    // Units that player can fall before a falling damage function is run. To disable, type "infinity" in the inspector
    public float fallingDamageThreshold = 10.0f;
    public float FallDamageMultipler = 2.3f;

    private CharacterController controller;
    private Transform myTransform;
    private RaycastHit hit;
    private bool falling = false;
    private float slideLimit;
    private float rayDistance;
    private bool playerControl = false;
    private int jumpTimer;
    private bool canSecond = false;
    private float fallDistance;
    [HideInInspector]
    public bool run;
    [HideInInspector]
    public bool canRun = true;
    private float distanceToObstacle;
    public GameObject cameraHolder;
    private float normalHeight = 0.7f;
    private float crouchHeight = 0.0f;
    private float m_HPoint;
    private bool isSuperJump = false;
    [HideInInspector] public  Vector3 vel = Vector3.zero;
    [HideInInspector]
    public float velMagnitude;
    private bl_PlayerDamage PlayerDamage;
    private bool OnVehicle = false;
    private bool isDeath = false;

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        if (!isMine)
        {
            this.enabled = false;
        }
        controller = GetComponent<CharacterController>();
        PlayerDamage = GetComponent<bl_PlayerDamage>();
        myTransform = this.transform;
        speed = walkSpeed;
        rayDistance = controller.height * .5f + controller.radius;
        slideLimit = controller.slopeLimit - .1f;
        jumpTimer = antiBunnyHopFactor;
        m_HeadBob.Setup(BobObject, m_IntervalStep);
        if(StepSource != null) { StartCoroutine(FootStepUpdate()); }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        bl_EventHandler.LocalPlayerVehicleEvent += OnLocalVehicleEvent;
        bl_EventHandler.LocalPlayerDeathEvent += OnLocalDeath;
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        bl_EventHandler.LocalPlayerVehicleEvent -= OnLocalVehicleEvent;
        bl_EventHandler.LocalPlayerDeathEvent -= OnLocalDeath;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Update()
    {
        if (isDeath)
            return;

        vel = controller.velocity;
        velMagnitude = vel.magnitude;

        if (m_OnLadder)
        {
           m_HPoint = myTransform.position.y;
            fallDistance = 0;
            grounded = false;
            run = false;
        }
        else
        {
            float inputX = Input.GetAxis("Horizontal");
            float inputY = Input.GetAxis("Vertical");

            float inputModifyFactor = (inputX != 0.0f && inputY != 0.0f) ? .7071f : 1.0f;

            if (grounded)
            {
                
                bool sliding = false;
                canSecond = false;
                isSuperJump = false;
                if (m_PlayerState == PlayerState.Jump || m_PlayerState == PlayerState.SuperJump)
                {
                    m_PlayerState = PlayerState.Stand;
                }
                // See if surface immediately below should be slid down. We use this normally rather than a ControllerColliderHit point,
                // because that interferes with step climbing amongst other annoyances
                if (Physics.Raycast(myTransform.position, -Vector3.up, out hit, rayDistance))
                {
                    if (Vector3.Angle(hit.normal, Vector3.up) > slideLimit)
                        sliding = true;
                }
                // However, just raycasting straight down from the center can fail when on steep slopes
                // So if the above raycast didn't catch anything, raycast down from the stored ControllerColliderHit point instead
                else
                {

                    if (Vector3.Angle(hit.normal, Vector3.up) > slideLimit)
                        sliding = true;
                }
                // If we were falling, and we fell a vertical distance greater than the threshold, run a falling damage routine
                if (falling)
                {
                    falling = false;
                    this.fallDistance = this.m_HPoint - this.myTransform.position.y;
                    if (this.fallDistance > this.fallingDamageThreshold)
                    {
                        FallingDamageAlert(fallDistance);
                    }
                    if ((this.fallDistance < this.fallingDamageThreshold) && (this.fallDistance > 0.0065f))
                    {
                        bl_EventHandler.OnSmallImpactEvent();
                    }

                }
                if (canRun && cameraHolder.transform.localPosition.y > normalHeight - 0.1f)
                {
                    if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W) && !Input.GetMouseButton(1))
                    {
                        run = true;
                    }
                    else
                    {
                        run = false;
                    }
                }
                // If sliding (and it's allowed), get a vector pointing down the slope we're on
                if ((sliding && slideWhenOverSlopeLimit))
                {
                    Vector3 hitNormal = hit.normal;
                    moveDirection = new Vector3(hitNormal.x, -hitNormal.y, hitNormal.z);
                    Vector3.OrthoNormalize(ref hitNormal, ref moveDirection);
                    moveDirection *= slideSpeed;
                    playerControl = false;
                }
                else
                {
                    if (m_PlayerState == PlayerState.Stand)
                    {
                        if (run)
                        {
                            speed = runSpeed;
                        }
                        else
                        {
                            if (Input.GetButton("Fire2"))
                            {
                                speed = crouchSpeed;
                            }
                            else
                            {
                                speed = walkSpeed;
                            }
                        }
                    }
                    else if (m_PlayerState == PlayerState.Crouch)
                    {
                        speed = crouchSpeed;
                        run = false;
                        if ((Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W)) && !Input.GetMouseButton(1))
                        {
                            m_PlayerState = PlayerState.Stand;
                        }
                    }

                    if (bl_CoopUtils.GetCursorState)
                    {
                        moveDirection = new Vector3(inputX * inputModifyFactor, -antiBumpFactor, inputY * inputModifyFactor);
                    }
                    else
                    {
                        moveDirection = new Vector3(0, -antiBumpFactor, 0);
                    }

                    moveDirection = myTransform.TransformDirection(moveDirection);
                    moveDirection *= speed;

                    //Head bob 
                    if (velMagnitude > 0)
                    {
                        Vector3 nextBobPos = m_HeadBob.DoHeadBob(velMagnitude + (speed * ((m_PlayerState == PlayerState.Stand) ? 1f : 0.7f)));
                        BobObject.localPosition = Vector3.Lerp(BobObject.localPosition, nextBobPos, Time.deltaTime * m_BobLerp);
                    }
                    else
                    {
                        BobObject.localPosition = Vector3.Lerp(BobObject.localPosition, Vector3.zero, Time.deltaTime * m_BobLerp);
                    }

                    if (Input.GetKeyDown(KeyCode.Space) && jumpTimer >= antiBunnyHopFactor && !canSecond)
                    {
                        jumpTimer = 0;
                        moveDirection.y = jumpSpeed;
                        isSuperJump = false;
                        if (CanSuperJump)
                        {
                            StartCoroutine(secondJump());
                        }
                        if (m_PlayerState != PlayerState.Stand)
                        {
                            CheckDistance();
                            if (distanceToObstacle > 1.6f)
                            {
                                m_PlayerState = PlayerState.Stand;
                            }
                        }
                        m_PlayerState = PlayerState.Jump;
                    }
                    else
                    {
                        jumpTimer++;
                    }
                }

            }
            else
            {
                this.run = false;
                if (this.myTransform.position.y > this.lastPosition.y)
                {
                    this.m_HPoint = this.myTransform.position.y;
                    this.falling = true;
                }
                // If we stepped over a cliff or something, set the height at which we started falling
                if (!falling)
                {
                    falling = true;
                    m_HPoint = myTransform.position.y;
                }
                // If air control is allowed, check movement but don't touch the y component
                if (airControl && playerControl)
                {
                    moveDirection.x = inputX * speed * inputModifyFactor;
                    moveDirection.z = inputY * speed * inputModifyFactor;
                    moveDirection = myTransform.TransformDirection(moveDirection);
                }
                if (Input.GetKeyDown(KeyCode.Space) && canSecond && CanSuperJump)
                {
                    jumpTimer = 0;
                    moveDirection.y = SuperJumpSpeed;
                    canSecond = false;
                    isSuperJump = true;
                    
                    bl_EventHandler.OnSmallImpactEvent();
                    if (SuperJumpSound && GetComponent<AudioSource>() != null)
                    {
                        GetComponent<AudioSource>().clip = SuperJumpSound;
                        GetComponent<AudioSource>().Play();
                    }
                    if (m_PlayerState != PlayerState.Stand)
                    {
                        CheckDistance();
                        if (distanceToObstacle > 1.6f)
                        {
                            m_PlayerState = PlayerState.Stand;
                        }
                    }
                    m_PlayerState = PlayerState.SuperJump;
                }
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                CheckDistance();

                if (m_PlayerState == PlayerState.Stand)
                {
                    m_PlayerState = PlayerState.Crouch;
                }
                else if (m_PlayerState == PlayerState.Crouch && distanceToObstacle > 1.6f)
                {
                    m_PlayerState = PlayerState.Stand;
                }
            }
            if (m_PlayerState == PlayerState.Stand)
            { //Stand Position
                Stand();
            }
            else if (m_PlayerState == PlayerState.Crouch)
            { //Crouch Position
                Crouch();
            }
            if (isFactorOn)
            {
                Vector3 td = myTransform.TransformDirection(new Vector3(inputX * speed * inputModifyFactor, 0, inputY * speed * inputModifyFactor));
                moveDirection = (FactorDirection * FactorForce);
                moveDirection += td;
                FactorDirection.y -= (gravity * Time.deltaTime) * FactorGravity;
            }
            else
            {
                // Apply gravity
                moveDirection.y -= gravity * Time.deltaTime;
            }
            // Move the controller, and set grounded true or false depending on whether we're standing on something
            grounded = (controller.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;
        }
        //When is super jump / jet pack, we have more forced in moved
        if (BoostMovement && CanSuperJump && isSuperJump && !grounded)
        {
            AirControl();
        }
        else
        {
            if (BoostSource != null)
            {
                if (BoostSource.isPlaying)
                {
                    BoostSource.Stop();
                }
            }
        }
    }


    /// <summary>
    /// Player Control / Boost Movement when is super jump
    /// </summary>
    void AirControl()
    {

        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");
        float inputModifyFactor = (inputX != 0 && inputY != 0) ? AirControlFactor.y : AirControlFactor.x;
        moveDirection.x = inputX * speed * inputModifyFactor;
        moveDirection.z = inputY * speed * inputModifyFactor;
        moveDirection = myTransform.TransformDirection(moveDirection);

        if (BoostSource != null)
        {
            if (BoostSource.clip == null)
            {
                BoostSource.clip = SuperJumpSound;
            }
            float t = (inputX + inputY) / 2;
            if (t < 0)
            {
                t = t * -1;
            }
            float mVolumen = BoostSource.volume;
            mVolumen = Mathf.Lerp(mVolumen, t, Time.deltaTime * 3);
            BoostSource.volume = mVolumen;
            if (!BoostSource.isPlaying)
            {
                BoostSource.Play();
            }
        }
    }

    /// <summary>
    /// When player is Walk,Run or Idle
    /// </summary>
    void Stand()
    {
        if (controller.height != 2.0f)
        {
            controller.height = 2.0f;
        }
        if (controller.center != Vector3.zero)
        {
            controller.center = Vector3.zero;
        }
        Vector3 ch = cameraHolder.transform.localPosition;
        if (cameraHolder.transform.localPosition.y > normalHeight)
        {
            ch.y = normalHeight;
        }
        else if (cameraHolder.transform.localPosition.y < normalHeight)
        {
            ch.y = Mathf.SmoothStep(ch.y, normalHeight, Time.deltaTime * m_crounchDelay);

        }
        cameraHolder.transform.localPosition = ch;
    }

    /// <summary>
    /// When player collision with other collider this is call.
    /// </summary>
    /// <param name="hit"></param>
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        lastFloorTag = hit.gameObject.tag;
        //avoid to apply force to vehicles.
        if (hit.transform.tag == "Vehicle")
            return;

        Rigidbody mRig = hit.collider.attachedRigidbody;
        if (mRig == null || mRig.isKinematic || hit.moveDirection.y < -0.3f)
        {
            return;
        }
        mRig.AddForce(hit.moveDirection * PushPower, ForceMode.Impulse);
    }

    /// <summary>
    /// When player is in Crounch
    /// </summary>
    void Crouch()
    {
        if (controller.height != 1.4f)
        {
            controller.height = 1.4f;
        }
        controller.center = new Vector3(0, -0.3f, 0);
        Vector3 ch = cameraHolder.transform.localPosition;

        if (cameraHolder.transform.localPosition.y != crouchHeight)
        {
            ch.y = Mathf.SmoothStep(ch.y, crouchHeight, Time.deltaTime * m_crounchDelay);
            cameraHolder.transform.localPosition = ch;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    void LateUpdate()
    {
        this.lastPosition = this.myTransform.position;
    }

    void OnLocalVehicleEvent(bool enter, VehicleType vt)
    {
        OnVehicle = enter;
    }

    void OnLocalDeath()
    {
        isDeath = true;
        StopAllCoroutines();
    }


    /// <summary>
    /// 
    /// </summary>
    void CheckDistance()
    {
        Vector3 pos = transform.position + controller.center - new Vector3(0, controller.height / 2, 0);
        RaycastHit hit;
        if (Physics.SphereCast(pos, controller.radius, transform.up, out hit, 10))
        {
            distanceToObstacle = hit.distance;
            Debug.DrawLine(pos, hit.point, Color.red, 2.0f);
        }
        else
        {
            distanceToObstacle = 3;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator FootStepUpdate()
    {
        while (true)
        {
            float vel = controller.velocity.magnitude;
            if (controller.isGrounded && vel > 0.2f && !OnVehicle)
            {
                AudioClip a = GetFloorStepAudio;
                if (a != null)
                {
                    StepSource.clip = a;
                    StepSource.Play();
                }
                float interval = minInterval * (runSpeed) / (vel);
                interval = Mathf.Clamp(interval, minInterval, 1);
                yield return new WaitForSeconds(interval);
            }
            else
            {
                yield return 0;
            }
        }
    }

    private Vector3 FactorDirection;
    private bool isFactorOn = false;
    private float FactorForce = 1;
    private float FactorGravity = 1;
    public void SetFactor(bool enable, Vector3 direction, float Force = 1, float _gravity = 1)
    {
        FactorDirection = direction;
        isFactorOn = enable;
        FactorForce = Force;
        FactorGravity = _gravity;
    }

    /// <summary>
    /// 
    /// </summary>
    private AudioClip GetFloorStepAudio
    {
        get
        {
          for(int i = 0; i < Footsteps.Length; i++)
            {
                if(lastFloorTag == Footsteps[i].Tag)
                {
                    return Footsteps[i].StepsSound[Random.Range(0, Footsteps[i].StepsSound.Length)];
                }
            }
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void OnLadder()
    {
        this.m_OnLadder = true;
        this.moveDirection = Vector3.zero;
        this.grounded = false;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ladderMovement"></param>
    public void OffLadder(object ladderMovement)
    {
        this.m_OnLadder = false;
        Vector3 forward = this.gameObject.transform.forward;
        if (Input.GetAxis("Vertical") > 0)
        {
            this.moveDirection = forward.normalized * 5;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public void JumpOffLadder()
    {
        m_OnLadder = false;
        Vector3 vector = cameraHolder.transform.forward + new Vector3(0, 0.2f, 0);
        moveDirection = (vector * ladderJumpSpeed);
    }
    // If falling damage occured, this is the place to do something about it. You can make the player
    // have hitpoints and remove some of them based on the distance fallen, add sound effects, etc.
    void FallingDamageAlert(float fallDistance)
    {
        PlayerDamage.DoDamage((int)(fallDistance * FallDamageMultipler));
        bl_EventHandler.EventFall(fallDistance * FallDamageMultipler);
        bl_EventHandler.OnSmallImpactEvent();
    }
    /// <summary>
    /// 
    /// </summary>
    void OnRoundEnd()
    {
        this.enabled = false;
    }

    public bool IsLocalPlayer
    {
        get
        {
            return this.isMine;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator secondJump()
    {
        yield return new WaitForSeconds(SecondJumpDelay);
        canSecond = true;
    }
}