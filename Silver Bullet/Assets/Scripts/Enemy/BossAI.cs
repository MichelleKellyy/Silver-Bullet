using UnityEngine;

public class BossAI : MonoBehaviour
{
    private Transform player;
    private Rigidbody rb;
    private Animator animator;
    private float fixedY;

    private enum BossState
    {
        Normal,
        ChargeUp,
        Charging,
        Swinging
    }

    private BossState currentState = BossState.Normal;

    [Header("Distance")]
    [SerializeField] private float meleeDistance = 2.5f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float chargeSpeed = 9f;
    [SerializeField] private float chargeDuration = 1.0f;
    [SerializeField] private float chargeUpDuration = 0.8f;

    [Header("Ranged Attack")]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private float arrowSpeed = 18f;
    [SerializeField] private float arrowLifeTime = 5f;
    [SerializeField] private int arrowDamage = 1;
    [SerializeField] private float minShootInterval = 1.0f;
    [SerializeField] private float maxShootInterval = 2.5f;

    [Header("Sword")]
    [SerializeField] private BossSword bossSword;
    [SerializeField] private float minChargeInterval = 3.0f;
    [SerializeField] private float maxChargeInterval = 6.0f;
    [SerializeField] private float minSwingInterval = 1.2f;
    [SerializeField] private float maxSwingInterval = 2.0f;
    [SerializeField] private LayerMask wallLayers;
    [SerializeField] private float collisionCheckRadius = 0.5f;
    [SerializeField] private float collisionSkin = 0.05f;
    [SerializeField] private AudioSource swingSound;
    [SerializeField] private AudioSource dashSound;

    [Header("Animation Parameters")]
    [SerializeField] private string chargeUpTrigger = "ChargeUp";
    [SerializeField] private string chargeHoldBool = "ChargeHold";
    [SerializeField] private string swingTrigger = "Swing";

    private float shootTimer;
    private float chargeTimer;
    private float swingTimer;
    private float swingStateTimerInitial = 1f;
    private float swingStateTimer;

    private float currentChargeMoveTimer;
    private Vector3 lockedChargeDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        fixedY = transform.position.y;
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;

        ResetShootTimer();
        ResetChargeTimer();
        ResetSwingTimer();
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float dt = Time.fixedDeltaTime;

        if (shootTimer > 0f) shootTimer -= dt;
        if (chargeTimer > 0f) chargeTimer -= dt;
        if (swingTimer > 0f) swingTimer -= dt;

        Vector3 dirToPlayer = player.position - transform.position;
        dirToPlayer.y = 0f;

        float dist = dirToPlayer.magnitude;

        switch (currentState)
        {
            case BossState.ChargeUp:
                StopXZ();
                return;

            case BossState.Charging:
                DoCharge(dt);
                return;

            case BossState.Swinging:
                StopXZ();
                swingStateTimer -= dt;
                if (swingStateTimer <= 0f)
                    EndSwing();
                return;
        }

        // Melee takes priority if close enough
        if (dist <= meleeDistance && swingTimer <= 0f)
        {
            StartSwing();
            return;
        }

        // Random dash can happen at any distance
        if (chargeTimer <= 0f)
        {
            StartChargeUp();
            return;
        }

        if (dist > meleeDistance)
        {
            MoveDir(dirToPlayer.normalized, moveSpeed);
        }
        else
        {
            StopXZ();
        }

        // Random arrow timing
        if (shootTimer <= 0f)
        {
            ShootArrow();
            ResetShootTimer();
        }
    }

    void StartChargeUp()
    {
        currentState = BossState.ChargeUp;
        StopXZ();

        if (bossSword != null)
            bossSword.SetCanDamage(false);

        if (animator != null)
        {
            animator.SetTrigger(chargeUpTrigger);
            animator.SetBool(chargeHoldBool, true);
        }

        Invoke(nameof(BeginCharge), chargeUpDuration);
        ResetChargeTimer();
    }

    void BeginCharge()
    {
        if (!GetComponent<BossStats>().getActivated())
        {
            currentState = BossState.Normal;

            if (bossSword != null)
                bossSword.SetCanDamage(false);
            if (animator != null)
                animator.SetBool(chargeHoldBool, false);

            return;
        }
        if (player == null) return;

        playDashSound();

        Vector3 dirToPlayer = player.position - transform.position;
        dirToPlayer.y = 0f;

        if (dirToPlayer.sqrMagnitude <= 0.001f)
            dirToPlayer = transform.forward;

        lockedChargeDirection = dirToPlayer.normalized;
        currentChargeMoveTimer = chargeDuration;
        currentState = BossState.Charging;

        if (bossSword != null)
            bossSword.SetCanDamage(true);
    }

    void DoCharge(float dt)
    {
        float moveDistance = chargeSpeed * dt;
        Vector3 origin = rb.position + Vector3.up * 0.5f;

        if (Physics.SphereCast(origin, collisionCheckRadius, lockedChargeDirection, out RaycastHit hit, moveDistance + collisionSkin, wallLayers))
        {
            EndCharge();
            return;
        }

        MoveDir(lockedChargeDirection, chargeSpeed);

        currentChargeMoveTimer -= dt;
        if (currentChargeMoveTimer <= 0f)
        {
            EndCharge();
        }
    }

    void EndCharge()
    {
        currentState = BossState.Normal;
        ResetSwingTimer();
        StopXZ();

        if (bossSword != null)
            bossSword.SetCanDamage(false);

        if (animator != null)
            animator.SetBool(chargeHoldBool, false);
    }

    void StartSwing()
    {
        currentState = BossState.Swinging;
        swingStateTimer = swingStateTimerInitial;
        StopXZ();
        ResetSwingTimer();

        if (animator != null)
            animator.SetTrigger(swingTrigger);
    }

    public void EndSwing()
    {
        currentState = BossState.Normal;

        if (bossSword != null)
            bossSword.SetCanDamage(false);
    }

    public void EnableSwordDamage()
    {
        if (bossSword != null)
            bossSword.SetCanDamage(true);
    }

    public void playSwingSound()
    {
        swingSound.Play();
    }

    public void playDashSound()
    {
        dashSound.Play();
    }

    void ShootArrow()
    {
        if (shootPoint == null || arrowPrefab == null || player == null) return;

        Vector3 aimPoint = player.position + Vector3.up * 1.0f;
        Vector3 dir = (aimPoint - shootPoint.position).normalized;
        GameObject arrowObj = Instantiate(arrowPrefab, shootPoint.position,Quaternion.LookRotation(dir));

        Rigidbody arrowRb = arrowObj.GetComponent<Rigidbody>();
        if (arrowRb != null)
        {
            arrowRb.linearVelocity = dir * arrowSpeed;
        }

        EnemyArrow arrow = arrowObj.GetComponent<EnemyArrow>();
        if (arrow != null)
        {
            arrow.SetDamage(arrowDamage);
        }

        Destroy(arrowObj, arrowLifeTime);
    }

    void MoveDir(Vector3 dir, float speed)
    {
        if (dir.sqrMagnitude <= 0.001f) return;

        Vector3 next = rb.position + dir * speed * Time.fixedDeltaTime;
        next.y = fixedY;
        rb.MovePosition(next);
    }

    void StopXZ()
    {
        rb.linearVelocity = Vector3.zero;

        Vector3 p = rb.position;
        p.y = fixedY;
        rb.position = p;
    }

    void ResetShootTimer()
    {
        shootTimer = Random.Range(minShootInterval, maxShootInterval);
    }

    void ResetChargeTimer()
    {
        chargeTimer = Random.Range(minChargeInterval, maxChargeInterval);
    }

    void ResetSwingTimer()
    {
        swingTimer = Random.Range(minSwingInterval, maxSwingInterval);
    }

    void OnDisable()
    {
        CancelInvoke();

        if (bossSword != null)
            bossSword.SetCanDamage(false);

        if (animator != null)
            animator.SetBool(chargeHoldBool, false);
    }
}