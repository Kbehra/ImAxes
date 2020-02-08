using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events; 

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
        private List<GameObject> listAxis = new List<GameObject>();
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
                        case 0:
                            // mean
                            WandController controller = (GameObject.FindWithTag("Controller")).GetComponent<WandController>() ;
                            //listAxis = controller.draggingObjects; //mettre la liste des axes attachés aux controlleurs
                            if (controller.IsDragging() == true)
                            {   //faire un for pour prendre en compte tout les axes attachés aux controlleurs
                                //valeur moyenne des valeurs contenues dans chaques axes
                                //System.Nullable<Decimal> averageValues = (from  in ).Average();

                                //Console.WriteLine(averageValues);

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
                        case 5:
                            // screenshot
                                         
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
                            // standard deviation
                            break;
                        case 7:
                            // percentage
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