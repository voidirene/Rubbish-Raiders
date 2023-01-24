using UnityEngine;

public class Manager : MonoBehaviour
{
    [Tooltip("How much the workers are sped up by this manager")]
    public float speedMultiplier;

    public Lane lane;
    
    [SerializeField]
    private Animator anim;

    [SerializeField]
    private float orderingTime;
    private float orderingTimer;

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        orderingTimer = orderingTime;
    }

    private void Update()
    {
        orderingTimer -= Time.deltaTime;
        if (orderingTimer <= 0.0f)
        {
            anim.Play("Ordering");
            orderingTimer = orderingTime + Random.Range(-3.0f, +3.0f);
        }

        foreach (Worker w in lane.workerList)
		{
            w.GetComponent<Movement>().speedMultiplier = speedMultiplier;
		}
    }
}
