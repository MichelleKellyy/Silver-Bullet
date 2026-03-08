using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyLoopAI : MonoBehaviour
{
    private Transform player;

    [Header("Ranges")]
    [SerializeField] private float aggroRange = 12f;      // starts AI if player within this
    [SerializeField] private float disengageRange = 16f;  // goes home if player farther than this

    [Header("Combat spacing")]
    [SerializeField] private float attackDistance = 2.5f; // when it reaches this, it retreats
    [SerializeField] private float retreatToDistance = 4.5f; // retreats until at least this far

    [Header("Timing")]
    [SerializeField] private float holdBackTime = 1.2f;   // wait after retreat before re-approach

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float turnSpeed = 10f;

    private Rigidbody rb;
    private Vector3 homePos;
    private float fixedY;

    private enum State { IdleHome, Approach, Retreat, HoldBack, GoHome }
    private State state = State.IdleHome;

    private float timer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        homePos = transform.position;
        fixedY = transform.position.y;

        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else if (Camera.main != null) player = Camera.main.transform;
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector3 myFlat = Flat(rb.position);
        Vector3 playerFlat = Flat(player.position);
        Vector3 homeFlat = Flat(homePos);

        float distToPlayer = Vector3.Distance(myFlat, playerFlat);
        float distToHome = Vector3.Distance(myFlat, homeFlat);

        // Global transitions: decide whether AI should be active
        if (distToPlayer > disengageRange)
        {
            state = State.GoHome;
        }
        else if (distToPlayer <= aggroRange)
        {
            // If player comes back near, re-activate even if we were home
            if (state == State.IdleHome || state == State.GoHome)
                state = State.Approach;
        }

        switch (state)
        {
            case State.IdleHome:
                StopXZ();
                // If player comes close, start again
                if (distToPlayer <= aggroRange) state = State.Approach;
                break;

            case State.Approach:
                MoveDir((playerFlat - myFlat).normalized, moveSpeed);

                // When in "attack distance", retreat
                if (distToPlayer <= attackDistance)
                    state = State.Retreat;
                break;

            case State.Retreat:
                MoveDir((myFlat - playerFlat).normalized, moveSpeed);

                // Once far enough, hold back for a moment
                if (distToPlayer >= retreatToDistance)
                {
                    state = State.HoldBack;
                    timer = holdBackTime;
                    StopXZ();
                }
                break;

            case State.HoldBack:
                StopXZ();

                timer -= Time.fixedDeltaTime;

                // If player pushes in, retreat again immediately
                if (distToPlayer <= attackDistance)
                {
                    state = State.Retreat;
                    break;
                }

                // After waiting, approach again
                if (timer <= 0f)
                    state = State.Approach;
                break;

            case State.GoHome:
                // Go back to start position
                Vector3 toHome = (homeFlat - myFlat);
                if (toHome.sqrMagnitude > 0.05f)
                {
                    MoveDir(toHome.normalized, moveSpeed);
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

    void MoveDir(Vector3 dirFlat, float spd)
    {
        Vector3 dir = new Vector3(dirFlat.x, 0f, dirFlat.z);
        Vector3 next = rb.position + dir * spd * Time.fixedDeltaTime;
        next.y = fixedY;

        rb.MovePosition(next);
    }

    void StopXZ()
    {
        Vector3 v = rb.linearVelocity;
        v.x = 0f;
        v.z = 0f;
        rb.linearVelocity = v;
    }

    static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
}