using UnityEngine;
using System.Collections;

public class Movement : MonoBehaviour
{
    public Rigidbody rb;
    [Header("Movement")]
    public float offset=1.5f;
    public float maxTorque = 450f;
    private AI network;
    public Transform centerOfMass;
    private GenericManager genetics;

    public WheelCollider[] wheelColiders = new WheelCollider[4];
    public Transform[] tireMeshes = new Transform[4];


    private Vector3 startPosition, startRotation, startVelocity, startAngularVelocity, startInertiaTensor;
    private Quaternion startInertiaTensorRotation;
    [Range(0f, 1f)]
    public float a = 0;
    [Range(-1f,1f)]
    public float t=0;
    [Range(0f,1f)]

    [Header("Fitness")]

    public float timeSinceStart = 0f;
    public float overallFitness;
    public float distanceMultiplier = 1.4f;
    public float avgSpeedMultiplier = 0.2f;
    public float sensorMultiplier = 0.1f;

    [Header("Network options")]
    public bool SHOWBEST=false;
    public int LAYERS = 1;
    public int NEURONS = 10;

    public Vector3 lastPosition;
    public float totalDistanceTravelled;
    private float speed;
    private float avgSpeed;

    private float aSensor, bSensor, cSensor;
    private bool collision=false;

    public int lap=0;

    void OnCollisionEnter(Collision collisionInfo)
    {
        if (collisionInfo.collider.tag == "concretetag")
        {
            collision = true;
        }
        
    }
    void LateUpdate()
    {
        if (collision == true)
        {
            Death();
            collision = false;
        }
    }
    public void Reset()
    {
        timeSinceStart = 0f;
        totalDistanceTravelled = 0f;
        speed = 0f;
        overallFitness = 0f;
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
        rb.velocity = startVelocity;
        lastPosition = startPosition;
        rb.angularVelocity = startAngularVelocity;
       
    }
    void Start()
    {
        rb = GetComponent <Rigidbody>();
        rb.centerOfMass = centerOfMass.localPosition;
        startPosition = rb.position;
        startRotation = rb.rotation.eulerAngles;
        startVelocity = rb.velocity;
        startAngularVelocity=rb.angularVelocity;
        network = GetComponent<AI>();
        genetics = FindObjectOfType<GenericManager>();
        KillFirstGenome();

        
    }
    void FixedUpdate()
    {
        
        InputSensors();

        CalculateFitness();

        lastPosition = transform.position;

        //Debug.Log("A: " + aSensor + " B: " + bSensor+" C: "+cSensor+" Vel: "+ rb.angularVelocity.magnitude / 10+" Speed: "+speed/100);

        if (SHOWBEST == false)
        {
            (a, t) = network.RunNetwork(aSensor, bSensor, cSensor, rb.angularVelocity.magnitude / 10, speed / 100);

        }
        else
        {
            for (int count = 0; count < genetics.bestPopulationCount; count++)
            {
                (a, t) = network.RunBestNetwork(aSensor, bSensor, cSensor, rb.angularVelocity.magnitude / 10, speed / 100, count);
            }

        }
        MoveCar(a, t);

        //ManualMovement();

        timeSinceStart += Time.deltaTime;

    }
    private void ManualMovement()
    {
        float steer = Input.GetAxis("Horizontal");
        float accelerate = Input.GetAxis("Vertical");
        Quaternion quat;
        Vector3 pos;
        wheelColiders[3].GetWorldPose(out pos, out quat);
        float finalAngle = steer * 45f;
        for (int i = 0; i < 2; i++)
        {
            wheelColiders[i].steerAngle = finalAngle;
        }

        for (int i = 0; i < 4; i++)
        {
            wheelColiders[i].motorTorque = -accelerate * maxTorque;
        }
        t = wheelColiders[0].steerAngle / 45;
        a = wheelColiders[0].motorTorque / -400;
        UpdateMeshesPositions();
    }

    private void MoveCar(float v, float h)
    {
        float accelerate = v;
        float steer = h;

        float finalAngle = steer * 45f;
        for (int i = 0; i < 2; i++)
        {
            wheelColiders[i].steerAngle = finalAngle;
        }

        for (int i = 0; i < 4; i++)
        {
            wheelColiders[i].motorTorque = -accelerate * maxTorque;
        }

        UpdateMeshesPositions();
    }
    private void InputSensors()
    {
        Vector3 a = rb.transform.forward - transform.right;
        Vector3 b = rb.transform.forward;
        Vector3 c = rb.transform.forward + rb.transform.right;
        Vector3 position =new  Vector3(transform.position.x, transform.position.y+offset, transform.position.z);
        Ray r = new Ray(position, -a);
        RaycastHit hit;
        if (Physics.Raycast(r, out  hit))
        {
            if (hit.collider.name != "road")
            {
                aSensor = Sigmoid(hit.distance/10);
                Debug.DrawLine(r.origin, hit.point, Color.red);
            }
        }
        else
        {
            Debug.DrawRay(position, -20*a, Color.green);
            aSensor = 1;
        }

        r.direction = -b;

        if (Physics.Raycast(r, out hit) )
        {
            if (hit.collider.name != "road")
            {
                bSensor = Sigmoid(hit.distance/10);
                Debug.DrawLine(r.origin, hit.point, Color.red);
            }
        }
        else
        {
            Debug.DrawRay(position, -20*b, Color.green);
            bSensor = 1;
        }

        r.direction = -c;

        if (Physics.Raycast(r, out hit))
        {
            if (hit.collider.name != "road")
            {
                cSensor = Sigmoid(hit.distance/10);
                Debug.DrawLine(r.origin, hit.point, Color.red);
            }
        }
        else
        {
            Debug.DrawRay(position, -20*c, Color.green);
            cSensor = 1;
        }
    }

    void UpdateMeshesPositions()
    {
        for(int i=0; i<4; i++)
        {
            Quaternion quat;
            Vector3 pos;
            wheelColiders[i].GetWorldPose(out pos, out quat);

            tireMeshes[i].position = pos;
            tireMeshes[i].rotation = quat;

        }
    }

    private void CalculateFitness()
    {
        var direction=transform.InverseTransformPoint(lastPosition);
        if (direction.z > 0)
        {
            totalDistanceTravelled += Vector3.Distance(transform.position, lastPosition);
        }
        else
        {
            totalDistanceTravelled -= Vector3.Distance(transform.position, lastPosition);
        }
        if (direction.z>0)
        {
            speed = rb.velocity.magnitude;

        }
        else
        {
            speed = -rb.velocity.magnitude;
        }

        avgSpeed = totalDistanceTravelled / timeSinceStart;

        //Debug.Log(totalDistanceTravelled * distanceMultiplier+"+++"+ avgSpeed * avgSpeedMultiplier);

        overallFitness = (totalDistanceTravelled * distanceMultiplier) + (avgSpeed * avgSpeedMultiplier);
        if (timeSinceStart>overallFitness+10)
        {
            Death();
        }
        else if(totalDistanceTravelled>3500)
        {
            lap++;
            //Death();
            //Debug.Log("Death() called since f>7000");
        }
    }
    public void ResetWithNetwork(AI ai)
    {
        network = ai;
        Reset();
    }
    private void Death()
    {
        GameObject.FindObjectOfType<GenericManager>().Death(overallFitness, network);
    }
    private void KillFirstGenome()
    {
        GameObject.FindObjectOfType<GenericManager>().KillFirstGenome(overallFitness, network);
    }
    private float Sigmoid(float s)
    {
        return (1 / (1 + Mathf.Exp(-s)));
    }

}