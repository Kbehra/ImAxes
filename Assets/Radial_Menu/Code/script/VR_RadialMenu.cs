using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Linq;

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

        //private VR_Screenshot screenshot = new VR_Screenshot();
        private List<GameObject> listAxis = new List<GameObject>();     //list of axis selected by controllers
        #endregion

        #region Statistic Methods
        private double CalcMean(List<float> dataList)       // Calcule la moyenne d'une liste de float
        {
            //valeur moyenne des valeurs contenues dans chaques axes
            return dataList.Average();
        }

        private float CalcStdDeviation(List<float> dataList)    // Calcul de l'ecart type d'une liste de float
        {
            double sum = 0;

            foreach (float idata in dataList)
            {
                sum += ((idata - CalcMean(dataList)) * (idata - CalcMean(dataList)));   //sum( (x - averageValue)² )
            }
            
            return Mathf.Sqrt((float)(sum / dataList.Count));       //s = sqrt(sum((x - averageValue)² ) / nb_x)
        }

        private double[] CalcPercent(List<float> dataList)
        {
            double[] quartile = new double[3];          //renvoi un tableau quartile[0] = premier quartile = 25%
            List<float> dataListSorted = dataList;      //                  quartile[1] = deuxieme quartile = mediane = 50%
            dataListSorted.Sort();                      //                  quartile[2] = troisieme quartile = 75%

            int size = dataListSorted.Count;                        // exemple : size = 8, size = 7
            int q1 = Mathf.FloorToInt((float)(size / 4));          // q1 = 2, q1 = 1.75 (= 1)
            int mid = Mathf.FloorToInt((float)(size / 2));          //mid = 4, mid = 3.5 (= 3)
            int q3 = Mathf.FloorToInt((float)((size * 3) / 4));     //q3 = 6, q3 = 5.25 (= 5)

            // si le nombre d'élément de la liste est impair, on prend la valeur a 1/4 de la liste, sinon on fais la moyenne des valeurs autour
            quartile[0] = (size % 2 != 0) ? (double)dataListSorted[q1] : ((double)dataListSorted[q1] + (double)dataListSorted[q1 - 1]) / 2;

            // si le nombre d'élément de la liste est impair, on prend la valeur du milieu, sinon on fais la moyenne des valeurs autour du milieu
            quartile[1] = (size % 2 != 0) ? (double)dataListSorted[mid] : ((double)dataListSorted[mid] + (double)dataListSorted[mid - 1]) / 2;

            // si le nombre d'élément de la liste est impair, on prend la valeur a 3/4 de la liste sinon on fais la moyenne des valeurs autour
            quartile[2] = (size % 2 != 0) ? (double)dataListSorted[q3] : ((double)dataListSorted[q3] + (double)dataListSorted[q3 - 1]) / 2;

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
                        case 0: //Mean
                            {
                                // Statistics
                                float[,] data;
                                GameObject controller1 = GameObject.Find("Controller (right)"); //get right controller
                                GameObject controller2 = GameObject.Find("Controller (left)");  //get left controller

                                foreach (Transform child in controller1.transform)  // liste des axes attachés au controlleur droit
                                {
                                    if (child.tag == "Axis")
                                    {
                                        listAxis.Add(child.gameObject);
                                    }
                                }
                                foreach (Transform child in controller2.transform)  // liste des axes attachés au controlleur gauche
                                {
                                    if (child.tag == "Axis")
                                    {
                                        listAxis.Add(child.gameObject);
                                    }
                                }

                                if (listAxis != null)
                                {
                                    foreach (GameObject axis in listAxis)  // pour tout les axes sélectionnés
                                    {
                                        Axis newAxis = axis.GetComponent<Axis>();
                                        data = newAxis.DataArraytest;               //on récupère les données de chaque axe
                                        List<float> dataList = new List<float>(); //liste temporaire de données de l'axe

                                        foreach (float data1 in data)
                                        {
                                            dataList.Add(data1);                  //on met les données pour chaque axes dans la liste
                                        }

                                        // Calcul de la moyenne
                                        //valeur moyenne des valeurs contenues dans chaques axes
                                        double averageValue = dataList.Average();

                                        System.Console.WriteLine(" Average value of " + axis.name + " : ");     //debug average value
                                        System.Console.WriteLine(averageValue);

                                        // Calcul de l'ecart type s= sqrt( sum( (x - averageValue)² ) / nb_x )
                                        double sum = 0;

                                        foreach (float idata in dataList)
                                        {
                                            sum += ((idata - averageValue) * (idata - averageValue));   //sum( (x - averageValue)² )
                                        }

                                        float stdDeviation = Mathf.Sqrt((float)(sum / dataList.Count));

                                        System.Console.WriteLine(" Standart Deviation of " + axis.name + " : ");     //debug standart deviation
                                        System.Console.WriteLine(stdDeviation);

                                        //calcul de la médiane
                                        List<float> dataListSorted = dataList;
                                        dataListSorted.Sort();

                                        int size = dataListSorted.Count;                        // exemple : size = 8, size = 7
                                        int q1 = Mathf.FloorToInt((float) (size / 4));          // q1 = 2, q1 = 1.75 (= 1)
                                        int mid = Mathf.FloorToInt((float)(size / 2));          //mid = 4, mid = 3.5 (= 3)
                                        int q3 = Mathf.FloorToInt((float)((size * 3) / 4));     //q3 = 6, q3 = 5.25 (= 5)

                                        // si le nombre d'élément de la liste est impair, on prend la valeur a 1/4 de la liste, sinon on fais la moyenne des valeurs autour
                                        double quartile1 = (size % 2 != 0) ? (double)dataListSorted[q1] : ((double)dataListSorted[q1] + (double)dataListSorted[q1 - 1]) / 2;

                                        System.Console.WriteLine(" First Quartile of " + axis.name + " : ");     //debug Quartile 1
                                        System.Console.WriteLine(quartile1);

                                        // si le nombre d'élément de la liste est impair, on prend la valeur du milieu, sinon on fais la moyenne des valeurs autour du milieu
                                        double median = (size % 2 != 0) ? (double)dataListSorted[mid] : ((double)dataListSorted[mid] + (double)dataListSorted[mid - 1]) / 2;

                                        System.Console.WriteLine(" Median of " + axis.name + " : ");     //debug median
                                        System.Console.WriteLine(median);

                                        // si le nombre d'élément de la liste est impair, on prend la valeur a 3/4 de la liste sinon on fais la moyenne des valeurs autour
                                        double quartile2 = (size % 2 != 0) ? (double)dataListSorted[q3] : ((double)dataListSorted[q3] + (double)dataListSorted[q3 - 1]) / 2;

                                        System.Console.WriteLine(" Third Quartile of " + axis.name + " : ");     //debug Quartile 2
                                        System.Console.WriteLine(quartile2);

                                    }
                                    //TODO : Créer un objet moyenne, pouvant être déplacé et associé à des axes

                                    //TODO : Créer un objet ecart-type, pouvant être déplacé et associé à des axes

                                    //TODO : Créer un objet quartile1, quartile2 et mediane, pouvant être déplacé et associé à des axes


                                }

                                // on vide la liste d'axe afin de pouvoir refaire le calcul en sélectionnant d'autres axes
                                listAxis.Clear();
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
                            // standard deviation (écart-type)
                            // Fait au dessus, fct ? ou calcul direct puis accés aux données?
                            break;
                        case 7:
                            // percentage
                            // Fait au dessus, fct ? ou calcul direct puis accés aux données?
                            break;
                    }
              
                }
                else
                {
                    HandleDebugText(updateMenuID.ToString());
                }


                //Update current Id
                if(updateMenuID != currentMenuID)
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