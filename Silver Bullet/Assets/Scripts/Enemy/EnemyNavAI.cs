using UnityEngine;

public class EnemyNavAI : MonoBehaviour
{
    private Transform player;

    [SerializeField] private bool isArcher;
    [SerializeField] private AudioSource noise;

    [Header("Ranges")]
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private float aggroRange = 12f;
    [SerializeField] private float sprintRange = 15f;
    [SerializeField] private float disengageRange = 16f;

    [Header("Melee spacing")]
    [SerializeField] private float attackDistance = 2.5f;
    [SerializeField] private float retreatToDistance = 4.5f;

    [Header("Melee timing")]
    [SerializeField] private float holdBackTime = 1.2f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Archer AI")]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private float archerMinDistance = 5f;      // too close -> back away
    [SerializeField] private float archerMaxDistance = 9f;      // too far -> move closer
    [SerializeField] private float shootCooldown = 1.5f;
    [SerializeField] private float arrowSpeed = 18f;
    [SerializeField] private float arrowLifeTime = 5f;
    [SerializeField] private int arrowDamage = 1;

    private Rigidbody rb;
    private Vector3 homePos;
    private float fixedY;

    private enum State { IdleHome, Approach, Retreat, HoldBack, Search, GoHome }
    private State state = State.IdleHome;

    private float timer;
    private float shootTimer;

    private Vector3 lastSeenPlayerPos;
    private bool hasLastSeenPlayer = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        homePos = transform.position;
        fixedY = transform.position.y;
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        if (shootTimer > 0f)
            shootTimer -= Time.fixedDeltaTime;

        Vector3 dirToPlayer = Flat(player.position) - Flat(rb.position);
        Vector3 dirToHome = Flat(homePos) - Flat(rb.position);

        float distToPlayer = Mathf.Infinity;
        bool canSeePlayer = false;

        if (dirToPlayer.sqrMagnitude > 0.001f)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, dirToPlayer.normalized, out hit, dirToPlayer.magnitude, hitMask))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    if (Random.Range(0f, 1f) > 0.8f)
                    {
                        noise.Play();
                    }
                    canSeePlayer = true;
                    distToPlayer = hit.distance;

                    lastSeenPlayerPos = Flat(player.position);
                    hasLastSeenPlayer = true;
                }
            }
        }

        // Global transitions
        if (canSeePlayer && distToPlayer > disengageRange)
        {
            state = State.GoHome;
        }
        else if (canSeePlayer && distToPlayer <= aggroRange)
        {
            if (state == State.IdleHome || state == State.GoHome)
            {
                state = State.Approach;
            }
        }

        switch (state)
        {
            case State.IdleHome:
                StopXZ();

                if (canSeePlayer && distToPlayer <= aggroRange)
                {
                    state = State.Approach;
                }
                break;

            case State.Approach:
                if (isArcher)
                {
                    HandleArcherApproach(canSeePlayer, distToPlayer, dirToPlayer);
                }
                else
                {
                    HandleMeleeApproach(canSeePlayer, distToPlayer, dirToPlayer);
                }
                break;

            case State.Retreat:
                if (isArcher)
                {
                    HandleArcherRetreat(canSeePlayer, distToPlayer, dirToPlayer);
                }
                else
                {
                    HandleMeleeRetreat(canSeePlayer, distToPlayer, dirToPlayer);
                }
                break;

            case State.HoldBack:
                if (isArcher)
                {
                    // Archers use HoldBack as "stand and shoot"
                    HandleArcherHold(canSeePlayer, distToPlayer, dirToPlayer);
                }
                else
                {
                    HandleMeleeHold(canSeePlayer, distToPlayer);
                }
                break;

            case State.Search:
                HandleSearch(canSeePlayer);
                break;

            case State.GoHome:
                if (dirToHome.sqrMagnitude > 0.05f)
                {
                    MoveDir(dirToHome.normalized, moveSpeed);
                }
                else
                {
                    state = State.IdleHome;
                    StopXZ();
                }
                break;
        }
    }

    void HandleMeleeApproach(bool canSeePlayer, float distToPlayer, Vector3 dirToPlayer)
    {
        if (canSeePlayer)
        {
            if (distToPlayer > sprintRange)
                MoveDir(dirToPlayer.normalized, moveSpeed * 2f);
            else
                MoveDir(dirToPlayer.normalized, moveSpeed);

            if (distToPlayer <= attackDistance)
            {
                state = State.Retreat;

                PlayerStats stats = player.GetComponentInParent<PlayerStats>();
                if (stats != null)
                    stats.attack(1);
            }
        }
        else if (hasLastSeenPlayer)
        {
            state = State.Search;
        }
        else
        {
            state = State.GoHome;
        }
    }

    void HandleMeleeRetreat(bool canSeePlayer, float distToPlayer, Vector3 dirToPlayer)
    {
        if (canSeePlayer)
        {
            MoveDir(-dirToPlayer.normalized, moveSpeed);

            if (distToPlayer >= retreatToDistance)
            {
                state = State.HoldBack;
                timer = holdBackTime;
                StopXZ();
            }
        }
        else if (hasLastSeenPlayer)
        {
            state = State.Search;
        }
        else
        {
            state = State.GoHome;
        }
    }

    void HandleMeleeHold(bool canSeePlayer, float distToPlayer)
    {
        StopXZ();
        timer -= Time.fixedDeltaTime;

        if (canSeePlayer && distToPlayer <= attackDistance)
        {
            state = State.Retreat;
            breakStateSafe();
            return;
        }

        if (!canSeePlayer && hasLastSeenPlayer)
        {
            state = State.Search;
            breakStateSafe();
            return;
        }

        if (timer <= 0f)
        {
            if (canSeePlayer)
                state = State.Approach;
            else if (hasLastSeenPlayer)
                state = State.Search;
            else
                state = State.GoHome;
        }
    }

    void HandleArcherApproach(bool canSeePlayer, float distToPlayer, Vector3 dirToPlayer)
    {
        if (canSeePlayer)
        {
            // Too far: move closer
            if (distToPlayer > archerMaxDistance)
            {
                MoveDir(dirToPlayer.normalized, moveSpeed);
            }
            // Too close: back away
            else if (distToPlayer < archerMinDistance)
            {
                state = State.Retreat;
            }
            // Good range: stop and shoot
            else
            {
                state = State.HoldBack;
                StopXZ();
            }
        }
        else if (hasLastSeenPlayer)
        {
            state = State.Search;
        }
        else
        {
            state = State.GoHome;
        }
    }

    void HandleArcherRetreat(bool canSeePlayer, float distToPlayer, Vector3 dirToPlayer)
    {
        if (canSeePlayer)
        {
            MoveDir(-dirToPlayer.normalized, moveSpeed);

            if (distToPlayer >= archerMinDistance + 0.75f)
            {
                state = State.HoldBack;
                StopXZ();
            }
        }
        else if (hasLastSeenPlayer)
        {
            state = State.Search;
        }
        else
        {
            state = State.GoHome;
        }
    }

    void HandleArcherHold(bool canSeePlayer, float distToPlayer, Vector3 dirToPlayer)
    {
        if (!canSeePlayer)
        {
            if (hasLastSeenPlayer)
                state = State.Search;
            else
                state = State.GoHome;

            return;
        }

        // Reposition if needed
        if (distToPlayer < archerMinDistance)
        {
            state = State.Retreat;
            return;
        }

        if (distToPlayer > archerMaxDistance)
        {
            state = State.Approach;
            return;
        }

        StopXZ();

        if (shootTimer <= 0f)
        {
            ShootArrow();
            shootTimer = shootCooldown;
        }
    }

    void HandleSearch(bool canSeePlayer)
    {
        Vector3 dirToLastSeen = Flat(lastSeenPlayerPos) - Flat(rb.position);

        if (canSeePlayer)
        {
            state = State.Approach;
            return;
        }

        if (dirToLastSeen.sqrMagnitude > 0.05f)
        {
            MoveDir(dirToLastSeen.normalized, moveSpeed);
        }
        else
        {
            hasLastSeenPlayer = false;
            state = State.GoHome;
            StopXZ();
        }
    }

    void ShootArrow()
    {
        if (arrowPrefab == null || shootPoint == null || player == null)
            return;

        Vector3 aimPoint = player.position + Vector3.up * 1.0f;
        Vector3 dir = (aimPoint - shootPoint.position).normalized;

        GameObject arrowObj = Instantiate(
            arrowPrefab,
            shootPoint.position,
            Quaternion.LookRotation(dir)
        );

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

    void MoveDir(Vector3 dir, float spd)
    {
        if (dir.sqrMagnitude <= 0.001f) return;

        Vector3 next = rb.position + dir * spd * Time.fixedDeltaTime;
        next.y = fixedY;

        rb.MovePosition(next);
    }

    void StopXZ()
    {
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
    }

    static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

    // Just avoids using "break" outside a switch case block
    void breakStateSafe() { }
}