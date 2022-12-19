 using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GeneticAlgorithm : MonoBehaviour
{
    // Genetic Algo by Kaushall
    // Unity Translation by Julia

    public GameObject titanBase;
    public GameObject ui;

    // Change number of agents
    public int numAgents = 10;
    // Change number of paramters
    public int numParameters = 3;

    // Changes the minimum and maximum for the three parameters we have
    [Header("SpeedSettings")]
    public float maxSpeed = 10;
    public float minSpeed = 2;

    [Header("SizeSettings")]
    public float maxSize = 15;
    public float minSize = 2;

    [Header("DefenseSettings")]
    public float maxDefense = 5;
    public float minDefense = 1;

    float[,] agents;
    float[,] bestAgents;

    // Changes the how strict the genetic algorithm is
    int numBestAgents;
    float[] successRates;
    float[] bestSuccessRates;

    // Keeps track of wave number / score
    int waveNum;
    int score;
    int missed;

    // GameObjects that keep track of our titans
    List<GameObject> agentObjects;

    void Start()
    {
        // Initializing the arrays and values
        waveNum = 1;
        score = 0;
        missed = 0;

        numBestAgents = (int) (0.2f * numAgents);
        agents = new float[numAgents, numParameters];
        bestAgents = new float[numAgents, numParameters];
        successRates = new float[numAgents];
        bestSuccessRates = new float[numBestAgents];
        agentObjects = new List<GameObject>();

        // Initializing the titans
        for (int i = 0; i < numAgents; i++)
        {
            // Initial generation picks random values in the range for our parameters
            float agentSpeed = Random.Range(minSpeed, maxSpeed);
            float agentSize = Random.Range(minSize, maxSize);
            float agentDefense = Random.Range(minDefense, minDefense);

            // initialize agents and success rates array
            agents[i, 0] = agentSpeed;
            agents[i, 1] = agentSize;
            agents[i, 2] = agentDefense;
            successRates[i] = 0.0f;

            // Initialize titan object based on the base prefab
            GameObject agent = Instantiate(titanBase);
            agent.name = "Agent #" + i;
            agentObjects.Add(agent);

            // add agent component that keeps track of its stats, sizes, etc
            agent.AddComponent<Agent>();
            agent.GetComponent<Agent>().speed = agentSpeed;
            agent.GetComponent<Agent>().health = agentSize;
            agent.GetComponent<Agent>().defense = agentDefense;

            // initialize positions
            float offsetX = -50f + (i * 10.0f);
            // float offsetZ = Random.Range(0, 0);
            agent.transform.localPosition = new Vector3(offsetX, (agentSpeed), 60f);
        }
    }

    // Function used to find the best performing agents to propagate into the next generation
    void FindBestAgents()
    {
        int bestValue;
        float[,] tempAgents = new float[numAgents, numParameters];
        float[] tempSuccessRates = new float[numAgents];
        float swapHolder;

        // Insertion Sort on temp agents
        for (int i = 0; i < numAgents; i++)
        {
            for (int j = 0; j < numParameters; j++)
            {
                tempAgents[i, j] = agents[i, j];
            }
            tempSuccessRates[i] = successRates[i];
        }

        for (int i = 0; i < numAgents-1; i++)
        {
            bestValue = i;
            for (int j = i + 1; j < numAgents; j++)
            {
                if (successRates[bestValue] < successRates[j])
                {
                    bestValue = j;
                }
            }
            for (int k = 0; k < numParameters; k++)
            {
                swapHolder = tempAgents[i, k];
                tempAgents[i, k] = tempAgents[bestValue, k];
                tempAgents[bestValue, k] = swapHolder;
            }
            swapHolder = tempSuccessRates[i];
            tempSuccessRates[i] = tempSuccessRates[bestValue];
            tempSuccessRates[bestValue] = swapHolder;
        }

        // Selecting the number of best agents from the pile
        for (int i = 0; i < numBestAgents; i++)
        {
            for (int j = 0; j < numParameters; j++)
            {
                bestAgents[i, j] = tempAgents[i, j];
            }
            bestSuccessRates[i] = tempSuccessRates[i];
        }

        waveNum++;
    }

    // Update is called once per frame
    void Update()
    {
        string scores = string.Format("Wave: {0}\tScore: {1}\tMissed: {2}", waveNum, score, missed);
        ui.GetComponent<TextMeshProUGUI>().text = scores;

        // https://docs.unity3d.com/ScriptReference/Physics.Raycast.html
        // Hit detection fromm the user - checks if there is a hit,
        // and if there is, what body part of the titan was hit
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hitInfo = new RaycastHit();
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 10f))
            {
                bool dead = false;
                if (hitInfo.transform.gameObject.tag == "TitanArm")
                {
                    dead = hitInfo.transform.gameObject.GetComponentInParent<Agent>().TakeDamage(0);
                }
                else if (hitInfo.transform.gameObject.tag == "TitanBody")
                {
                    dead = hitInfo.transform.gameObject.GetComponentInParent<Agent>().TakeDamage(1);
                }
                else if (hitInfo.transform.gameObject.tag == "TitanLegs")
                {
                    dead = hitInfo.transform.gameObject.GetComponentInParent<Agent>().TakeDamage(2);
                }
                if (dead)
                {
                    score++;
                }
            }
        }

        // If the agents have reached the goal
        for (int i = 0; i < numAgents; i++)
        {
            // Stop the agent at Vector3(x, y, 100)
            GameObject agent = agentObjects[i];
            Vector3 agentVel = new Vector3(0f, 0f, agent.GetComponent<Agent>().speed) * Time.deltaTime;
            agent.transform.Translate(agentVel);
            bool reached = agent.GetComponent<Agent>().goalReached;
            if (!reached && agent.transform.localPosition.z < -60)
            {
                agent.GetComponent<Agent>().speed = 0f;
                agent.GetComponent<Agent>().goalReached = true;
                missed++;
            }

        }

        // Determining agent success by calculating how close each agent is to the finish line
        float velSum = 0;
        for (int i = 0; i < numAgents; i++)
        {
            GameObject agent = agentObjects[i];
            velSum += agent.GetComponent<Agent>().speed;
            // agent_vel[i].length();
            // instead of 1280, our max in Z-axis is 100
            successRates[i] = Mathf.Abs(-60 + agent.transform.localPosition.z);
        }

        // When all the titans have been stopped
        if (velSum == 0)
        {
            FindBestAgents();

            float[] averageParameters = new float[numParameters];
            for (int i = 0; i < numParameters; i++)
            {
                averageParameters[i] = 0;
                for (int k = 0; k < numBestAgents; k++)
                {
                    averageParameters[i] += bestAgents[k, i];
                }
                averageParameters[i] = averageParameters[i] / numParameters;
            }




            for (int i = 0; i < numAgents; i++)
            {
                for (int j = 0; j < numParameters; j++)
                {
                    // mutation rate - random.range
                    // progating to the next step and adding a degree of mutation
                    agents[i, j] = averageParameters[j] + Random.Range(-5, 5);
                }

                // Setting MaxLimits and MinLimits
                if (agents[i, 0] > maxSpeed)
                {
                    agents[i, 0] = maxSpeed;
                }
                if (agents[i, 1] > maxSize)
                {
                    agents[i, 1] = maxSize;
                }
                if (agents[i, 0] < minSpeed)
                {
                    agents[i, 0] = maxSpeed;
                }
                if (agents[i, 1] < minSize)
                {
                    agents[i, 1] = minSize;
                }
                
                if (agents[i, 2] > maxDefense)
                {
                    agents[i, 2] = maxDefense;
                }
                if (agents[i, 2] < minDefense)
                {
                    agents[i, 2] = minDefense;
                }
                

                // Resseting the simulation
                GameObject agent = agentObjects[i];
                float offsetX = -50f + (i * 10.0f);
                // float offsetZ = Random.Range(0, 0);
                agent.transform.localPosition = new Vector3(offsetX, agents[i, 0], 60f);
                agent.GetComponent<Agent>().speed = agents[i, 0];
                agent.GetComponent<Agent>().health = agents[i, 1];
                agent.GetComponent<Agent>().defense = agents[i, 2];
                agent.GetComponent<Agent>().UpdateAgent();
                successRates[i] = 0.0f;
            }

        }
    }
}
