using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;
public class ZombieController : MonoBehaviour
{
    public GameObject target;
    public float walkingSpeed;
    public float runningSpeed;
    public GameObject ragdoll;

    private NavMeshAgent agent;
    private Animator anim;

    private enum STATE
    {
        IDLE,
        WANDER,
        ATTACK,
        CHASE,
        DEAD
    };

    private STATE state = STATE.IDLE;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void TurnOffTriggers()
    {
        anim.SetBool("isWalking", false);
        anim.SetBool("isAttacking", false);
        anim.SetBool("isRunning", false);
        anim.SetBool("isDead", false);
    }

    float DistanceToPlayer()
    {
        return Vector3.Distance(target.transform.position, transform.position);
    }

    bool CanSeePlayer()
    {
        if (DistanceToPlayer() < 10)
        {
            return true;
        }

        return false;
    }

    bool ForgetPlayer()
    {
        if (DistanceToPlayer() > 20)
        {
            return true;
        }

        return false;
    }

    public void KillZombie()
    {
        TurnOffTriggers();
        anim.SetBool("isDead", true);
        state = STATE.DEAD;
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null)
        {
            target = GameObject.FindWithTag("Player");
            return;
        }
        switch (state)
        {
            case STATE.IDLE:
                if (CanSeePlayer())
                {
                    state = STATE.CHASE;
                } 
                else if (Random.Range(0, 5000) < 5)
                {
                    state = STATE.WANDER;
                }
                break;
            
            case STATE.WANDER:
                if (!agent.hasPath)
                {
                    float newX = transform.position.x + Random.Range(-5, 5);
                    float newZ = transform.position.z + Random.Range(-5, 5);
                    float newY = Terrain.activeTerrain.SampleHeight(new Vector3(newX, 0, newZ));
                    Vector3 dest = new Vector3(newX, newY, newZ);
                    agent.SetDestination(dest);
                    agent.stoppingDistance = 0;
                
                    TurnOffTriggers();
                    agent.speed = walkingSpeed;
                    anim.SetBool("isWalking", true);
                }

                if (CanSeePlayer())
                {
                    state = STATE.CHASE;
                } 
                else if (Random.Range(0, 5000) < 5)
                {
                    state = STATE.IDLE;
                    TurnOffTriggers();
                    agent.ResetPath();
                }
                break;
            
            case STATE.CHASE:
                agent.SetDestination(target.transform.position);
                agent.stoppingDistance = 5;
                TurnOffTriggers();
                agent.speed = runningSpeed;
                anim.SetBool("isRunning", true);
                
                // SetDestination is process in another thread, it's not instant computed,
                // so we need to check if the agent is still pending for a state
                if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
                {
                    state = STATE.ATTACK;
                }

                if (ForgetPlayer())
                {
                    state = STATE.WANDER;
                    agent.ResetPath();
                }
                break;
            
            case STATE.ATTACK:
                TurnOffTriggers();
                anim.SetBool("isAttacking", true);
                transform.LookAt(target.transform.position);
                if (DistanceToPlayer() > agent.stoppingDistance + 2)
                {
                    state = STATE.CHASE;
                }
                break;
            
            case STATE.DEAD:
                break;
        }
    }
}
