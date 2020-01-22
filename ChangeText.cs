using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeText : MonoBehaviour
{
    public Text changingText;
    public Movement movement;
    public GenericManager genetics;
    public float maxfitness = 0;
    public bool dynamicTime = true;
    public int maxTimeScale = 20;

    void Awake()
    {
        movement = FindObjectOfType<Movement>();
        genetics = FindObjectOfType<GenericManager>();
    }
    void FixedUpdate()
    {
        if (movement.overallFitness > maxfitness)
        {
            //Time.timeScale = 1f;
            maxfitness = movement.overallFitness - 1;
        }
        if (dynamicTime == true)
        {
            if (maxfitness / movement.overallFitness > maxTimeScale || movement.overallFitness < 0 || maxfitness == 0 || maxfitness < 0 || movement.overallFitness == 0)
            {
                Time.timeScale = maxTimeScale;
            }
            else if (maxfitness / movement.overallFitness < 1)
            {
                Time.timeScale = 1;
            }
            else
            {
                if (maxfitness / movement.overallFitness > 0)
                {
                    Time.timeScale = 5 + (maxfitness / movement.overallFitness);

                }
                else
                {
                    Time.timeScale = 30;
                }

            }
        }
        else
        {
            Time.timeScale = 1;

        }


    }
    void LateUpdate()
    {
        
        
        int fit = (int)movement.overallFitness;
        string gen = genetics.currentGeneration.ToString();
        string genm = genetics.currentGenome.ToString();
        string ft = fit.ToString();
        int speedf = (int)movement.rb.velocity.magnitude;
        string speed = speedf.ToString();
        string accel = movement.a.ToString();
        string steering = movement.t.ToString();
        if (Input.GetKeyDown(KeyCode.Plus)|| Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            if(dynamicTime==false)
            {
                Time.timeScale += 1;
            }
            else
            {
                maxTimeScale += 1;

            }
        }
        if (Input.GetKeyDown(KeyCode.Minus)||Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            if (dynamicTime == false)
            {
                Time.timeScale += 1;
            }
            else
            {
                maxTimeScale -= 1;

            }
        }
        int maxFitStr = (int)maxfitness + 1;
        string genetik = "\nGeneration: " + gen + "\nGenome: " + genm + "\nFitness: " + ft + "\nMax fitness: " + maxFitStr;
        int timescale = (int)Time.timeScale;
        changingText.text = "Time scale:" + timescale+"X" + genetik + "\nAcceleration: " + accel + "\nSteering: " + steering;
    }
}
