using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;  


namespace RadialMenu
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Image))]       // to be sure to have an Image

    public class VR_MenuButton : MonoBehaviour
    {
        #region Variables
        [Header("Button Properties")]
        public int buttonID;
        public string buttonText;
        public Image buttonIcon;
        public Sprite normalImage;
        public Sprite hoverImage;

        [Header("Events")]
        public UnityEvent OnClick = new UnityEvent(); 

        private Animator animator;
        private Image currentImage; 

        #endregion

        #region Main Methods
        // Start is called before the first frame update
        void Start()
        {
            animator = GetComponent<Animator>();
            currentImage = GetComponent<Image>();
            if (currentImage && normalImage)
            {
                currentImage.sprite = normalImage; 
            }
        }

        // Update is called once per frame
        #endregion

        #region Custom Methods
        public void OnHover(int anID)
        {
            if(currentImage)
            {
                if(anID == buttonID && hoverImage)
                {
                    currentImage.sprite = hoverImage;
                    HandleAnimator(true);
                }
                else if (normalImage)
                {
                    currentImage.sprite = normalImage;
                    HandleAnimator(false);
                }
            }

        }
        public void Click(int anID)
        {
            if (buttonID == anID)
            {
                if (OnClick != null)
                {
                    OnClick.Invoke();
                   
                }
            }
        }
       void HandleAnimator(bool aToggle)
        {
            if (animator)
            {
                animator.SetBool("hover", aToggle);
            }
        }
         

        #endregion
    }
}
