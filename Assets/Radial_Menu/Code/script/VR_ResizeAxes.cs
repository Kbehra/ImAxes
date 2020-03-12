using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VR_ResizeAxes : MonoBehaviour
{
    #region Variables 
    
    Vector3 initialDistance;
    Vector3 currentDistance;
    Vector3 newScale;

    float MinScale = 0.02f;

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
            if (firstTimeClick)
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
                foreach (GameObject axis in listAxis)
                {
                    //Calculating new scale of axes -> newScale = (initialScale / initialDistance) * currentDistance;
                    newScale = new Vector3((axis.transform.localScale.x / initialDistance.x) * currentDistance.x,
                                           (axis.transform.localScale.y / initialDistance.y) * currentDistance.y,
                                           (axis.transform.localScale.z / initialDistance.z) * currentDistance.z);
                    axis.transform.localScale = newScale;

                    // make sure object's scale doesn't go negative, colliders don't like that
                    if (transform.localScale.x < MinScale)
                    {
                        transform.localScale = new Vector3(MinScale, MinScale + 0.22f, MinScale);
                    }
                }
            }
            
            // clear axes selection
            listAxis.Clear();
        }
    }

    #endregion

}
