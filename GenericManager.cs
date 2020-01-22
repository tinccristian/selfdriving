using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using System.IO;
using UnityEditor;

public class GenericManager : MonoBehaviour
{
    [Header("References")]
    public Movement movement;

    [Header("Controls")]
    public int initialPopulation = 85;
    [Range(0.0f, 1.0f)]
    public float mutationRate = 0.055f;

    [Header("Crossover")]
    public int bestAgentSelection = 8;
    public int worstAgentSelection = 3;
    public int numberToCrossover;

    private List<int> genePool = new List<int>();

    private int naturallySelected;
    private AI[] population;
    public AI[] bestPopulation;
    [Header("Public View")]
    public int currentGeneration = 0;
    public int currentGenome = 0;
    public float fitness;
    public float acceleration;
    public float steering;
    private float average;
    public int bestPopulationCount=0;
    public float bestFitness=0;
    

    private void Start()
    {
        CreatePopulation();
        bestPopulationCount = 0;


    }

    private void Update()
    {
        fitness = movement.overallFitness;
        acceleration = movement.a;
        steering = movement.t;
    }
    private void CreatePopulation()
    {
        population = new AI[initialPopulation];
        bestPopulation = new AI[10];
        FillPopulationWithRandomValues(population, 0);
        ResetToCurrentGenome();
    }

    private void ResetToCurrentGenome()
    {
        movement.ResetWithNetwork(population[currentGenome]);
    }

    private void FillPopulationWithRandomValues(AI[] newPopulation, int startingIndex)
    {
        while (startingIndex < initialPopulation)
        {
            newPopulation[startingIndex] = new AI();
            newPopulation[startingIndex].Initialize(movement.LAYERS, movement.NEURONS);
            startingIndex++;
        }
    }

    public void Death(float fitness, AI ai)
    {

        if (currentGenome < population.Length - 1)
        {
            population[currentGenome].fitness = fitness;
            currentGenome++;
            ResetToCurrentGenome();
        }
        else
        {
            RePopulate();
        }
    }
    public void KillFirstGenome(float fitness, AI ai)
    {
        if (currentGenome == 0 && currentGeneration == 0)
        {
            currentGenome++;
            ResetToCurrentGenome();
        }
    }
    private void RePopulate()
    {
        genePool.Clear();
        currentGeneration++;
        naturallySelected = 0;
        SortPopulation();

        AI[] newPopulation = PickBestPopulation();

        Crossover(newPopulation);
        Mutate(newPopulation);

        FillPopulationWithRandomValues(newPopulation, naturallySelected);

        population = newPopulation;

        currentGenome = 0;

        ResetToCurrentGenome();
    }
    private void Crossover(AI[] newPopulation)
    {
        for (int i = 0; i < numberToCrossover; i += 2)
        {
            int AIndex = i;
            int BIndex = i + 1;

            if (genePool.Count >= 1)
            {
                for (int l = 0; l < 100; l++)
                {
                    AIndex = genePool[Random.Range(0, genePool.Count)];
                    BIndex = genePool[Random.Range(0, genePool.Count)];

                    if (AIndex != BIndex)
                        break;
                }
            }

            AI Child1 = new AI();
            AI Child2 = new AI();
            Child1.Initialize(movement.LAYERS, movement.NEURONS);
            Child2.Initialize(movement.LAYERS, movement.NEURONS);

            Child1.fitness = 0;
            Child2.fitness = 0;

            for (int w = 0; w < Child1.weights.Count; w++)
            {
                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    Child1.weights[w] = population[AIndex].weights[w];
                    Child2.weights[w] = population[BIndex].weights[w];
                }
                else
                {
                    Child2.weights[w] = population[AIndex].weights[w];
                    Child1.weights[w] = population[BIndex].weights[w];
                }
            }

            for (int w = 0; w < Child1.biases.Count; w++)
            {
                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    Child1.biases[w] = population[AIndex].biases[w];
                    Child2.biases[w] = population[BIndex].biases[w];
                }
                else
                {
                    Child2.biases[w] = population[AIndex].biases[w];
                    Child1.biases[w] = population[BIndex].biases[w];
                }
            }

            newPopulation[naturallySelected] = Child1;
            naturallySelected++;

            newPopulation[naturallySelected] = Child2;
            naturallySelected++;

        }
    }

    private void Mutate(AI[] newPopulation)
    {
        for (int i = 0; i < naturallySelected; i++)
        {
            for (int j = 0; j < newPopulation[i].weights.Count; j++)
            {
                if ((Random.Range(0f, 1f) < mutationRate / 10))
                {
                    newPopulation[i].weights[j] = MutateMatrix(newPopulation[i].weights[j]);
                }
            }
        }
    }
    Matrix<float> MutateMatrix(Matrix<float> A)
    {
        int randomPoints = Random.Range(1, (A.RowCount * A.ColumnCount) / 7);

        Matrix<float> C = A;

        for (int i = 0; i < randomPoints; i++)
        {
            int randomColumn = Random.Range(0, C.ColumnCount);
            int randomRow = Random.Range(0, C.RowCount);

            C[randomRow, randomColumn] = Mathf.Clamp(C[randomRow, randomColumn] + Random.Range(-1f, 1f), -1f, 1f);
        }
        return C;
    }
    private AI[] PickBestPopulation()
    {
        AI[] newPopulation = new AI[initialPopulation];
        for (int i = 0; i < bestAgentSelection; i++)
        {
            newPopulation[naturallySelected] = population[i].InitializeCopy(movement.LAYERS, movement.NEURONS);
            newPopulation[naturallySelected].fitness = 0;
            naturallySelected++;

            int f = Mathf.RoundToInt(population[i].fitness * 10);

            for (int c = 0; c < f; c++)
            {
                genePool.Add(i);
            }

        }

        for (int i = 0; i < worstAgentSelection; i++)
        {
            int last = population.Length - 1;
            last -= i;

            int f = Mathf.RoundToInt(population[last].fitness * 10);

            for (int c = 0; c < f; c++)
            {
                genePool.Add(last);
            }
        }
        return newPopulation;
    }
    private void SortPopulation()
    {
        average = 0;
        for (int i = 0; i < population.Length; i++)
        {
            for (int j = i; j < population.Length; j++)
            {
                if (population[i].fitness < population[j].fitness)
                {
                    AI temp = population[i];
                    population[i] = population[j];
                    population[j] = temp;
                }
            }
        }
        for (int i = 0; i < initialPopulation; i++)
        {
            average += population[i].fitness;
        }

        //AI[] bestPopulation = SaveBestPopulation();
        //bestPopulationCount++;

        // for (int i = 0; i < bestPopulationCount; i++)
        // {
        //Debug.Log("yes "+" i "+i+ "< "+bestPopulationCount);
        //Debug.Log("best population fitnesses: " + bestPopulation[i].fitness);

        //  }
        bestFitness = population[0].fitness;
        Debug.Log("Generation " + currentGeneration + " , " + movement.lap + " laps made, " + " average fitness: " + average / (initialPopulation) + " best fitnesses: " + population[0].fitness + " , " + population[1].fitness + " , " + population[2].fitness + " , " + population[3].fitness + " , " + population[4].fitness + " , " + population[5].fitness + " , " + population[6].fitness + " , " + population[7].fitness + " , " + population[8].fitness);
        movement.lap = 0;
    }

    private  AI[] SaveBestPopulation()
    {
       // Debug.Log(currentGeneration + "+" + bestPopulationCount);
        for (int i = currentGeneration-1; i < bestPopulationCount+1; i++)
        {
          //  Debug.Log(i+","+bestPopulationCount+1);
            bestPopulation[i] = population[0].InitializeCopy(movement.LAYERS, movement.NEURONS);
            bestPopulation[i].fitness = population[0].fitness;

        }
        return (bestPopulation);
    }

}