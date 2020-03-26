using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;
using MathNet.Numerics.Statistics;

namespace RadialMenu
{
    public class onHover : UnityEvent<int> { }
    public class onClick : UnityEvent<int> { }


    [RequireComponent(typeof(Animator))]                                    //requierement to handle the radialMenu 
    [RequireComponent(typeof(CanvasGroup))]                                 //fade out the RadialMenu

   
    public class VR_RadialMenu : MonoBehaviour
    {
        #region Variables 
   
        [Header("UI Properties")]
        public List<VR_MenuButton> menuButtons = new List<VR_MenuButton>();
        public RectTransform m_ArrowContainer;
        public Text m_DebugText;

        [Header("Events")]
        public UnityEvent OnMenuChanged = new UnityEvent();
   
        private Vector3 currentAxis;
        private Animator animator;

        private bool menuOpen = false;
        private bool allowNavigation = false;
        private bool isTouching = false;
        public bool isClick = false;
        private float currentAngle;                                         // wich menu is currently on 

        private int currentMenuID = -1;
        private int previousMenuID = -1;

        public GameObject screenshot; 

        private onHover OnHover = new onHover();
        private onClick OnClick = new onClick();

        bool firstTime = true;

        private List<GameObject> listAxis = new List<GameObject>();             //list of axis selected by controllers
        private List<List<float>> originalCSVValues = new List<List<float>>();  //all data in all axis in the scene

        #endregion

        #region Statistic Methods
        private double CalcMean(List<float> dataList)       
        {
            // compute mean of a list
            return dataList.Average();
        }

        private double CalcStdDeviation(List<float> dataList)
        {
            // compute std with MathNet.numerics (.NET 4.0)
            double std = dataList.StandardDeviation();

            return std;
        }

        private double[] CalcPercent(List<float> dataList)      
        {
            // compute quartile with MathNet.numerics (.NET 4.0)
            double[] quartile = new double[5];          //renvoi un tableau quartile[0] = zero quartile = minimum = 0%
                                                        //                  quartile[1] = premier quartile = 25%
                                                        //                  quartile[2] = deuxieme quartile = mediane = 50%
                                                        //                  quartile[3] = troisieme quartile = 75%
                                                        //                  quartile[4] = quatrieme quartile = maximum = 100%

            quartile[0] = dataList.Minimum();
            quartile[1] = dataList.LowerQuartile();
            quartile[0] = dataList.Median();
            quartile[1] = dataList.UpperQuartile();
            quartile[0] = dataList.Maximum();

            return quartile;
        }

        #endregion

        #region Main Methods
        // Start is called before the first frame update
        void Start()
        {
            animator = GetComponent<Animator>();

            if (menuButtons.Count > 0)
            {
                foreach(var button in menuButtons)
                {
                    OnHover.AddListener(button.OnHover);
                    OnClick.AddListener(button.Click); 
                }
            }

        }

        private void OnDisable()
        {
           // exact the same as start
           if(OnHover != null)
            {
                OnHover.RemoveAllListeners(); 
            }
           if (OnClick != null)
            {
                OnClick.RemoveAllListeners();
            }
        }

        // Update is called once per frame
        void Update()
        {
            //OVRInput.Update();
            //OVRInput.FixedUpdate();

            float istouch = OVRInput.Get(OVRInput.RawAxis1D.LHandTrigger);
            float isClicked = OVRInput.Get(OVRInput.RawAxis1D.LIndexTrigger);
            

            if (isClicked == 0)
            {
                isClick = false; 
            }
            else
            {
                isClick = true; 
            }


            if (istouch == 0)
            {
                isTouching = false; 
            }
            else
            {
                isTouching = true; 
            }

            HandleMenuActivation();
            UpdateMenu();
        }
          
    
        #endregion

        #region Custom Methods
        void HandlePrimaryIndexTriggered()
        {
            isTouching = true; 
            //HandleDebugText("Index Triggered");
        }

        void HandlePrimaryIndexUnTriggered()
        {
            isTouching = false; 
            //HandleDebugText("Index unTriggered");
        }

        void HandleMenuActivation()
        {
           if ((menuOpen == true && isTouching == false) || (menuOpen == false && isTouching == true))
            {
                menuOpen = !menuOpen;
                HandleAnimator();
            }
       
        }

        void HandleAnimator()
        {
            if (animator)
            {
                animator.SetBool("open", menuOpen); 
            }
        }
   
        void HandleDebugText(string aString)
        {
            if (m_DebugText)
            {
                m_DebugText.text = aString; 
            }
        }
        void UpdateMenu()
        {
            if(isTouching)
            {
                //Get the current Axis from the joystick and turn it into an angle 
                currentAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick); 
                currentAngle = Vector2.SignedAngle(Vector2.up, currentAxis);

                // for statistics uses
                GameObject controller1 = GameObject.Find("Controller (right)");             //get right controller
                GameObject controller2 = GameObject.Find("Controller (left)");              //get left controller

                foreach (Transform child in controller1.transform)
                {
                    // get current axis on right controller
                    if (child.tag == "Axis")
                    {
                        listAxis.Add(child.gameObject);
                    }
                }
                foreach (Transform child in controller2.transform)
                {
                    // get current axis on left controller
                    if (child.tag == "Axis")
                    {
                        listAxis.Add(child.gameObject);
                    }
                }

                if (listAxis != null && firstTime && listAxis.Count() > 0)   
                //Getting all the data of all axes to be able to calculate mean, std...
                {
                    firstTime = false;             
                    //when we have all the data we don't need to get another time

                    // get one axis 
                    Axis newAxis = listAxis[0].GetComponent<Axis>();    
                    //all axis recieve all csv data but keep only a part of them
                    //so we can take any axis and it will work

                    DataBinding.DataObject data = newAxis.DataArray;
                    
                    if (data != null)
                    {
                         originalCSVValues = data.getOriginalValues();  
                        //original values of the csv file
                    }                                                        
                }                                                            
                                                                                

                float menuAngle = currentAngle; 
                if(menuAngle < 0)
                {
                    menuAngle += 360;
                }
                int updateMenuID = (int)(menuAngle / (360.0 / 8.0));

                if (isClick == true)
                {
                    switch (updateMenuID)
                    {
                        case 0: 
                            // compute mean
                            {
                                foreach (GameObject axis in listAxis)
                                {
                                    // id of current axis
                                    int axisID = axis.GetComponent<Axis>().GetSourceIndex();

                                    // axis data
                                    List<float> dataList = new List<float>();           
                                    //list of data attached on current axis

                                    for (int i = 0; i < originalCSVValues.Count(); i++) 
                                        //we put in dataList the value of this axis
                                    {
                                        dataList.Add(originalCSVValues[i][axisID]);     
                                        //using axisID to be sure its the good axis
                                    }

                                    if ((dataList != null))                               
                                        //if there is data
                                    {
                                        try
                                        { 
                                            double averageValue = CalcMean(dataList);
                                            //we calculate the mean with those data
                                            Debug.Log("averager");
                                            Debug.Log(averageValue);
                                            axis.GetComponent<Axis>().SetMean(averageValue);   
                                            //to show the mean value on the axis

                                            HandleDebugText(averageValue.ToString().Substring(0, 6));        //--debug mean
                                        }
                                        catch
                                        {
                                            //
                                        }
                                      
                                    }
                                }
                            }
                            break;
                        case 1:
                            // histogram
                            break;
                        case 2:
                            // export to HTML
                            break;
                        case 3:
                            // parameters
                            break;
                        case 4:
                            break;
                        case 5: // screenshot

                            // to prevent issue when the user press the controller too long   
                            if (GameObject.Find("UI_ScreenShot") == false && GameObject.Find("UI_ScreenShot(Clone)") == false)
                            {
                                GameObject UI_ScreenShot = Instantiate(Resources.Load("UI_ScreenShot", typeof(GameObject))) as GameObject;
                                UI_ScreenShot.SetActive(true);

                                GameObject rightcontroller = GameObject.Find("Controller (right)");
                               
                                // give the transform of the right controller to the Screenshot UI
                                UI_ScreenShot.transform.parent = rightcontroller.transform;
                                UI_ScreenShot.transform.position = rightcontroller.transform.position;
                                UI_ScreenShot.transform.rotation = rightcontroller.transform.rotation;
                            }
                            break;
                        case 6:
                            // compute std
                            foreach (GameObject axis in listAxis)
                            {
                                // id of current axis
                                int axisID = axis.GetComponent<Axis>().GetSourceIndex();

                                // axis data
                                List<float> dataList = new List<float>();           
                                //list of data attached on current axis

                                for (int i = 0; i < originalCSVValues.Count(); i++) 
                                    //we put in dataList the value of this axis
                                {
                                    dataList.Add(originalCSVValues[i][axisID]);     
                                    //using axisID to be sure its the good axis
                                }

                                if (dataList != null)                               
                                    //if there is data
                                {
                                    double std = CalcStdDeviation(dataList);      
                                    //we calculate the std with those data

                                    HandleDebugText(std.ToString().Substring(0, 6));                    //--debug std  
                                }
                            }
                            break;
                        case 7:
                            // percentage
                            foreach (GameObject axis in listAxis)
                            {
                                // id of current axis
                                int axisID = axis.GetComponent<Axis>().GetSourceIndex();

                                // axis data
                                List<float> dataList = new List<float>();           
                                //list of data attached on current axis

                                for (int i = 0; i < originalCSVValues.Count(); i++) 
                                    //we put in dataList the value of this axis
                                {
                                    dataList.Add(originalCSVValues[i][axisID]);     
                                    //using axisID to be sure its the good axis
                                }

                                if (dataList != null)
                                {
                                    double[] quartile = CalcPercent(dataList);

                                    HandleDebugText(quartile.ToString().Substring(0, 6));                    //--debug quartile 
                                }
                            }
                            break;
                    }
                }
                else
                {
                    HandleDebugText(updateMenuID.ToString());
                }

                // clear axis selection
                listAxis.Clear();


                //Update current Id
                if (updateMenuID != currentMenuID)
                {
                    if(OnHover != null)
                    {
                        OnHover.Invoke(updateMenuID);

                    }
                    if(OnMenuChanged != null)
                    {
                        OnMenuChanged.Invoke(); 
                    }
                    previousMenuID = currentMenuID;
                    currentMenuID = updateMenuID; 
                }

                //Rotate Arrow 
                if (m_ArrowContainer)
                {
                    m_ArrowContainer.localRotation = Quaternion.AngleAxis(currentAngle, Vector3.forward);
                }
            }

        }
        #endregion

    }
}