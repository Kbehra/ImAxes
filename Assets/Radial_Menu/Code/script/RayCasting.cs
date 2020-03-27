﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataBinding;
//using RadialMenu; 

public class RayCasting : MonoBehaviour
{
    [SerializeField]
    GameObject centerEyeAnchor;

    RaycastHit hit;
    Visualization visualization;

    public bool GetHit(){
        bool flag = false;
        Ray ray = new Ray(centerEyeAnchor.transform.position, centerEyeAnchor.transform.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        if(hits.Length != 0){
            for(int i = 0; i < hits.Length; i++){
                RaycastHit tempHit = hits[i];
                if(tempHit.collider.tag == "VisualisationMesh"){
                    hit = tempHit;
                    visualization = tempHit.transform.gameObject.GetComponent<Visualization>();
                    flag = true;
                }
            }
        }
        return flag;
    }

    public bool GetHitWithMouse()
    {
        bool flag = false;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        if (hits.Length != 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit tempHit = hits[i];
                if (tempHit.collider.tag == "VisualisationMesh")
                {
                    hit = tempHit;
                    visualization = tempHit.transform.gameObject.GetComponent<Visualization>();
                    flag = true;
                }
            }
        }
        return flag;
    }

    


    public float GetHistogramPointValue(DataObject dobjs, int Dimension, Vector3 hitPos){
        DiscreteBinner binner = new DiscreteBinner();
        float[] values = dobjs.GetCol(dobjs.DataArray, Dimension);

        binner.MakeIntervals(values, dobjs.Metadata[Dimension].binCount);
        foreach (float value in values)
        {
            binner.Bin(value);
        }

        float[] bins = binner.bins;

        float x0 = -0.5f;
        float distanceFromX0 = hitPos.x - x0;
        float step = 1.0f/bins.Length;
        int indexValue = Mathf.FloorToInt(distanceFromX0/step);
        Debug.Log("Index : " + indexValue);
        return values[indexValue];
    }
/*
    private void Update() {
      
        if(OVRInput.Get(OVRInput.Button.SecondaryThumbstick)){
            if (GetHit())
            {
                Vector3 hitPos = hit.collider.transform.InverseTransformPoint(hit.point); //Local hit point
                //Debug.Log("Hit position = " + hitPos);
                float value = GetHistogramPointValue(SceneManager.Instance.dataObject, visualization.axes[0].axisId, hit.point);

               
                //VR_RadialMenu.HandleDebugText(value.ToString()); 

                //Debug.Log("Value = " + );
            }
        }
     
    }
*/

    void OnDrawGizmos(){
        Gizmos.color = Color.red;
        Vector3 direction = centerEyeAnchor.transform.forward;
        Gizmos.DrawRay(centerEyeAnchor.transform.position,direction);
    }
}
