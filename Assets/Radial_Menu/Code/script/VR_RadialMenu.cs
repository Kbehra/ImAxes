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
        private float currentAngle;                                         // wich menu is currently on 

        private int currentMenuID = -1;
        private int previousMenuID = -1;

        private onHover OnHover = new onHover();
        private onClick OnClick = new onClick(); 
       
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

            float istouch = OVRInput.Get(OVRInput.RawAxis1D.LHandTrigger); 
            
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

            //OVRInput.FixedUpdate();

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

                //HandleDebugText(currentAngle.ToString()); 
                float menuAngle = currentAngle; 
                if(menuAngle < 0)
                {
                    menuAngle += 360;
                }
                int updateMenuID = (int)(menuAngle / (360.0 / 8.0));

                switch (updateMenuID)
                {
                    case 0:
                        // mean
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
                        break;
                    case 6:
                        // standard deviation
                        break;
                    case 7:
                        // percentage
                        break;

                }
                HandleDebugText(updateMenuID.ToString());

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