using System;
using System.Collections;
using UnityEngine;

public class AxesMeanWidget : MonoBehaviour     //This script was created in the case that we want to be able to grab the mean object 
{                                               //to put it on an other axis or something like this but it is not necessary for now
                                                
    Axis parentAxis;

    double meanValue;

    // Use this for initialization
    void Start()
    {
        parentAxis = GetComponentInParent<Axis>();

        meanValue = parentAxis.GetMean();
        
    }

    // Update is called once per frame
    void Update()
    {
        // TODO : refound the good meanValue when we calculate it
    }


}