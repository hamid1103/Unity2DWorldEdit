using System;
using UnityEngine;

namespace Utils
{
    public class SaveIcon : MonoBehaviour
    {
        private Animator animator;

        public void Start()
        {
            animator = gameObject.GetComponent<Animator>();
        }

        public void BeginAnimation()
        {
            animator.SetBool("Saving", true);
        }

        public void StopAnimation()
        {
            animator.SetBool("Saving", false);
        }
    }
}