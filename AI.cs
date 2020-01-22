using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using System;

public class AI : MonoBehaviour
{

    public Matrix<float> inputLayer = Matrix<float>.Build.Dense(1, 5);

    public List<Matrix<float>> hiddenLayers = new List<Matrix<float>>();

    public Matrix<float> outputLayer = Matrix<float>.Build.Dense(1, 2);

    public List<Matrix<float>> weights = new List<Matrix<float>>();

    public List<float> biases = new List<float>();

    public float fitness;
    private GenericManager genericManager;

    public void Initialize (int hiddenLayerCount, int hiddenNeuronCount)
    {
        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();
        weights.Clear();
        biases.Clear();

        for(int i = 0; i< hiddenLayerCount+1; i++)
        {
            Matrix<float> f = Matrix<float>.Build.Dense(1, hiddenNeuronCount);

            hiddenLayers.Add(f);

            biases.Add(UnityEngine.Random.Range(-1f, 1f));

            if(i==0)
            {
                Matrix<float>inputToH1 = Matrix<float>.Build.Dense(5, hiddenNeuronCount);
                weights.Add(inputToH1);
            }

            Matrix<float> HiddenToHidden = Matrix<float>.Build.Dense(hiddenNeuronCount, hiddenNeuronCount);
            weights.Add(HiddenToHidden);
             
        }

        Matrix<float> OutputWeight = Matrix<float>.Build.Dense(hiddenNeuronCount, 2);
        weights.Add(OutputWeight);
        biases.Add(UnityEngine.Random.Range(-1f, 1f));

        RandomiseWeights();

    }

    public AI InitializeCopy(int hiddenLayerCount, int hiddenNeuronCount)
    {
        AI ai = new AI();
        List<Matrix<float>> newWeights = new List<Matrix<float>>();

        for(int i = 0; i < this.weights.Count; i++)
        {
            Matrix<float> currentWeight = Matrix<float>.Build.Dense(weights[i].RowCount, weights[i].ColumnCount);

            for(int x = 0; x < currentWeight.RowCount; x++)
            {
                for(int y=0; y < currentWeight.ColumnCount; y++)
                {
                    currentWeight[x, y] = weights[i][x, y];
                }
            }
            newWeights.Add(currentWeight);
        }
        List<float> newBiases = new List<float>();

        newBiases.AddRange(biases);

        ai.weights = newWeights;
        ai.biases = newBiases;

        ai.InitializeHidden(hiddenLayerCount, hiddenNeuronCount);

        return ai;
    }
    public void InitializeHidden( int hiddenLayerCount,int hiddenNeuronCount)
    {
        inputLayer.Clear();
        hiddenLayers.Clear();
        outputLayer.Clear();

        for(int i = 0; i < hiddenLayerCount + 1; i++)
        {
            Matrix<float> newHiddenLayer = Matrix<float>.Build.Dense(1, hiddenNeuronCount);
            hiddenLayers.Add(newHiddenLayer);
        }
    }

    public void RandomiseWeights()
    {
        for (int i =0; i< weights.Count;i++)
        {
            for(int x=0; x <weights[i].RowCount;x++)
            {
                for(int y=0;y<weights[i].ColumnCount;y++)
                {

                    weights[i][x, y] = UnityEngine.Random.Range(-1f, 1f);

                }
            }
        }
    }

    public (float,float) RunNetwork (float a, float b, float c, float angularVelocity, float speed)
    {
        inputLayer[0, 0] = a;
        inputLayer[0, 1] = b;
        inputLayer[0, 2] = c;
        inputLayer[0, 3] = angularVelocity;
        inputLayer[0, 4] = speed;
        inputLayer = inputLayer.PointwiseTanh();


        hiddenLayers[0] = ((inputLayer * weights[0] + biases[0])).PointwiseTanh();

        for(int i=1;i<hiddenLayers.Count;i++)
        {
            hiddenLayers[i] = ((hiddenLayers[i - 1]) * weights[i] + biases[i]).PointwiseTanh();
        }

        outputLayer = ((hiddenLayers[hiddenLayers.Count - 1] * weights[weights.Count - 1]) * biases[biases.Count - 1]).PointwiseTanh();

        return ((float)Math.Tanh(outputLayer[0, 0]), (float)Math.Tanh(outputLayer[0, 1]));
    }
    public (float, float) RunBestNetwork(float a, float b, float c, float angularVelocity, float speed, int count)
    {
        inputLayer[0, 0] = a;
        inputLayer[0, 1] = b;
        inputLayer[0, 2] = c;
        inputLayer[0, 3] = angularVelocity;
        inputLayer[0, 4] = speed;
        inputLayer = inputLayer.PointwiseTanh();


        hiddenLayers[0] = ((inputLayer * (genericManager.bestPopulation[count].weights[0]) + genericManager.bestPopulation[count].biases[0])).PointwiseTanh(); 

        for (int i = 1; i < hiddenLayers.Count; i++)
        {
            hiddenLayers[i] = ((hiddenLayers[i - 1]) * genericManager.bestPopulation[count].weights[i] + genericManager.bestPopulation[count].biases[i]).PointwiseTanh();
        }

        outputLayer = ((hiddenLayers[hiddenLayers.Count - 1] * genericManager.bestPopulation[count].weights[genericManager.bestPopulation[count].weights.Count-1]) * genericManager.bestPopulation[count].biases[genericManager.bestPopulation[count].biases.Count - 1]).PointwiseTanh();

        return ((float)Math.Tanh(outputLayer[0, 0]), (float)Math.Tanh(outputLayer[0, 1]));
    }

    private bool floatToBool(float x)
    {
        if(x>0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
