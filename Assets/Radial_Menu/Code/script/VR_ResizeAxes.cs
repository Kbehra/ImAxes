using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VR_ResizeAxes : MonoBehaviour
{
    #region Variables 
    
    Vector3 initialDistance;
    Vector3 currentDistance;
    Vector3 initialScaleAxes;
    Vector3 initialScaleVisu;
    float initialPointSize = 0.4f;
    Vector3 newScaleAxes;
    Vector3 newScaleVisu;
    float newPointSize;

    float MinScaleY = 0.266f;
    float MaxScaleY = 7.0f;

    bool firstTimeClick = true;

    public bool allowResizing = false;
   
    private List<GameObject> listAxis = new List<GameObject>();     //list of axis selected by left controllers

    #endregion

    #region MainMethods
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float istouch = OVRInput.Get(OVRInput.RawAxis1D.RHandTrigger);

        if (istouch == 0)
        {
            allowResizing = false;
            firstTimeClick = true;
        }
        else
        {
            allowResizing = true;
            if (firstTimeClick)     //when it's the first time we clicked on RHandTrigger
            {
                initialDistance = GameObject.Find("Controller (left)").transform.position - this.transform.position;    //distance between the two controllers
                firstTimeClick = false;
            }
            
        }
        
        ResizingAxes();
    }

    #endregion

    #region CustomMethods

    void ResizingAxes()
    {
        if (allowResizing)
        {
            //Calculating current distance between controllers
            currentDistance = GameObject.Find("Controller (left)").transform.position - this.transform.position;

            // For resizing only axes selected by the left controller
            GameObject controller = GameObject.Find("Controller (left)");              //get left controller
            
            foreach (Transform child in controller.transform)
            {
                // get current axis on left controller
                if (child.tag == "Axis")
                {
                    listAxis.Add(child.gameObject);
                }
            }

            if (listAxis != null)
            {
                if(listAxis.Count == 1) //histogram
                {
                    //Searching visualisation (values) of this axes
                    string visualisationName = listAxis[0].name + " " + "visualisation";

                    //Saving initial scale to set it if the scaling goes wrong
                    initialScaleAxes = listAxis[0].transform.localScale;
                    initialScaleVisu = GameObject.Find(visualisationName).transform.localScale;

                    //Calculate new scale
                    newScaleAxes = (listAxis[0].transform.localScale * currentDistance.magnitude) / initialDistance.magnitude;
                    newScaleVisu = (GameObject.Find(visualisationName).transform.localScale * currentDistance.magnitude) / initialDistance.magnitude;

                    //Set new scale to the axis and its visualisation
                    listAxis[0].transform.localScale = newScaleAxes;
                    GameObject.Find(visualisationName).transform.localScale = newScaleVisu;

                    // make sure axes's scale doesn't go below the initial scale (initial = when created), we also fixed the maximum scale
                    if ((listAxis[0].transform.localScale.y < MinScaleY) || (listAxis[0].transform.localScale.y > MaxScaleY))
                    {
                        listAxis[0].transform.localScale = initialScaleAxes;
                        GameObject.Find(visualisationName).transform.localScale = initialScaleVisu;
                    }
                }
                else if (listAxis.Count == 2)   //Scatterplot2D
                {
                    //Searching scatterplot visualisation of the 2 axes
                    string visualisationName = listAxis[0].name + " " + listAxis[1].name + " " + "visualisation";

                    if (GameObject.Find(visualisationName) == null)
                    {
                        visualisationName = listAxis[1].name + " " + listAxis[0].name + " " + "visualisation";
                    }
                    
                    //Saving initial scale to set it if the scaling goes wrong
                    initialScaleAxes = listAxis[0].transform.localScale;
                    initialScaleVisu = GameObject.Find(visualisationName).transform.localScale;

                    //Calculate new scale
                    newScaleAxes = (listAxis[0].transform.localScale * currentDistance.magnitude) / initialDistance.magnitude;
                    newScaleVisu = (GameObject.Find(visualisationName).transform.localScale * currentDistance.magnitude) / initialDistance.magnitude;

                    //Calculate new point size, point size get smaller when axis get bigger
                    newPointSize = (initialPointSize * initialDistance.magnitude) / currentDistance.magnitude;

                    //Set new scale to the axis and its visualisation
                    listAxis[0].transform.localScale = newScaleAxes;
                    listAxis[1].transform.localScale = newScaleAxes;
                    GameObject.Find(visualisationName).transform.localScale = newScaleVisu;

                    //Rescaling point size to see better
                    GameObject.Find(visualisationName).GetComponent<Visualization>().OnChangePointSize(newPointSize);

                    // make sure axes's scale doesn't go below the initial scale (initial = when created), we also fixed the maximum scale
                    if ((listAxis[0].transform.localScale.y < MinScaleY) || (listAxis[0].transform.localScale.y > MaxScaleY))
                    {
                        listAxis[0].transform.localScale = initialScaleAxes;
                        listAxis[1].transform.localScale = initialScaleAxes;
                        GameObject.Find(visualisationName).transform.localScale = initialScaleVisu;
                        GameObject.Find(visualisationName).GetComponent<Visualization>().OnChangePointSize(initialPointSize);
                    }
                }
                else if (listAxis.Count == 3)   //Scatterplot3D
                {

                }
            }
            
            // clear axes selection
            listAxis.Clear();
        }
    }

    #endregion

}
