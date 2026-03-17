using UnityEngine;

public class EnemyNavAI : MonoBehaviour
{
    private Transform player;

    [Header("Ranges")]
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private float aggroRange = 12f;      // starts AI if player within this
    [SerializeField] private float sprintRange = 15f;
    [SerializeField] private float disengageRange = 16f;  // goes home if player farther than this

    [Header("Combat spacing")]
    [SerializeField] private float attackDistance = 2.5f; // when it reaches this, it retreats
    [SerializeField] private float retreatToDistance = 4.5f; // retreats until at least this far

    [Header("Timing")]
    [SerializeField] private float holdBackTime = 1.2f;   // wait after retreat before re-approach

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    private Rigidbody rb;
    private Vector3 homePos;
    private float fixedY;

    private enum State { IdleHome, Approach, Retreat, HoldBack, Search, GoHome }
    private State state = State.IdleHome;

    private float timer;

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
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void FixedUpdate()
    {
        Vector3 dirToPlayer = Flat(player.position) - Flat(rb.position);
        Vector3 dirToHome = Flat(homePos) - Flat(rb.position);

        float distToPlayer = Mathf.Infinity;
        bool canSeePlayer = false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, dirToPlayer.normalized, out hit, dirToPlayer.magnitude, hitMask))
        {
            if (hit.collider.CompareTag("Player"))
            {
                canSeePlayer = true;
                distToPlayer = hit.distance;

                lastSeenPlayerPos = Flat(player.position);
                hasLastSeenPlayer = true;
            }
        }

        // Global transitions: decide whether AI should be active
        if (canSeePlayer && distToPlayer > disengageRange)
        {
            state = State.GoHome;
        }
        else if (canSeePlayer && distToPlayer <= aggroRange)
        {
            // If player comes back near, re-activate even if we were home
            if (state == State.IdleHome || state == State.GoHome)
            {
                state = State.Approach;
            }
        }

        switch (state)
        {
            case State.IdleHome:
                StopXZ();

                // If player comes close, start again
                if (canSeePlayer && distToPlayer <= aggroRange)
                {
                    state = State.Approach;
                }
                break;

            case State.Approach:
                if (canSeePlayer)
                {
                    if (distToPlayer > sprintRange)
                    {
                        MoveDir(dirToPlayer.normalized, moveSpeed * 2);
                    }
                    else
                    {
                        MoveDir(dirToPlayer.normalized, moveSpeed);
                    }

                    // When in "attack distance", attack then retreat
                    if (distToPlayer <= attackDistance)
                    {
                        state = State.Retreat;
                        player.gameObject.GetComponentInParent<PlayerStats>().attack(1);
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
                break;

            case State.Retreat:
                if (canSeePlayer)
                {
                    MoveDir(-dirToPlayer.normalized, moveSpeed);

                    // Once far enough, hold back for a moment
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
                break;

            case State.HoldBack:
                StopXZ();
                timer -= Time.fixedDeltaTime;

                // If player pushes in, retreat again immediately
                if (canSeePlayer && distToPlayer <= attackDistance)
                {
                    state = State.Retreat;
                    break;
                }

                if (!canSeePlayer && hasLastSeenPlayer)
                {
                    state = State.Search;
                    break;
                }

                // After waiting, approach again
                if (timer <= 0f)
                {
                    if (canSeePlayer)
                    {
                        state = State.Approach;
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
                break;

            case State.Search:
                Vector3 dirToLastSeen = Flat(lastSeenPlayerPos) - Flat(rb.position);

                if (canSeePlayer)
                {
                    state = State.Approach;
                    break;
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
                break;

            case State.GoHome:
                // Go back to start position
                if (dirToHome.sqrMagnitude > 0.05f)
                {
                    MoveDir(dirToHome.normalized, moveSpeed);
                }
                else
                {
                    // At home: wait, but if player comes near, restart
                    state = State.IdleHome;
                    StopXZ();
                }
                break;
        }
    }

    void MoveDir(Vector3 dir, float spd)
    {
        Vector3 next = rb.position + dir * spd * Time.fixedDeltaTime;
        next.y = fixedY;

        rb.MovePosition(next);
    }

    void StopXZ()
    {
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
    }

    static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
}