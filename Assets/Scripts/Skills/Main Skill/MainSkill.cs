using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*using UnityEngine.VFX;                                                                /////////////VFX\\\\\\\\\\\\\\
using UnityEditor.Experimental.Rendering.HDPipeline;
using UnityEditor.VFX; */

namespace DoubleMMPrjc
{
    public class MainSkill : MonoBehaviour
    {

        //Class for trasisting from one world to another

        private float size = 0; //Size of the circle
        private bool changingCircleAtTheMoment = false; //Checks if a circle is changing its size
        private bool isInRealLife = false; //checks in what world are we now
        private float timeStartedLerping;   //time that starts when we start lerping the circle
        public float timeOfLerp;    //time of lerping


        /*private bool changingVFXAtTheMoment = false;                                      /////////////VFX\\\\\\\\\\\\\\
        public GameObject vfx;
        public UnityEngine.Experimental.VFX.VisualEffect visualEffect;
        private float minorRadius;
        private float majorRadius = 0f; */

        void Awake()
        {
            transform.localScale = new Vector3( 0, 0, 0 );  // circle is 0 size at the start
                                                            /* vfx.SetActive(false);                                                        /////////////VFX\\\\\\\\\\\\\\
                                                            visualEffect.SetFloat("minor radius", 1f); */
        }



        public void ChangeWorld()
        {
            if (isInRealLife == false) {
                isInRealLife = true;
            } else
                isInRealLife = false;

            changingCircleAtTheMoment = true;
            //changingVFXAtTheMoment = true;                                            /////////////VFX\\\\\\\\\\\\\\
            StartLerping();
        }

        void Update()
        {

            if (changingCircleAtTheMoment == true) {
                if (isInRealLife == true) {
                    size = Lerp( size, Screen.width / 128, timeStartedLerping, 0.7f );
                    transform.localScale = new Vector3( size, size, 0 );


                    if (size >= Screen.width / 128) {
                        changingCircleAtTheMoment = false;
                    }


                } else {
                    size = Lerp( size, 0, timeStartedLerping, 0.7f );
                    transform.localScale = new Vector3( size, size, 0 );


                    if (size <= 0) {
                        changingCircleAtTheMoment = false;
                    }
                }
            }
            /*if (changingVFXAtTheMoment == true)                                           /////////////VFX\\\\\\\\\\\\\\
            {
                visualEffect.SetInt("particles", 10000);
                if (isInRealLife == true)
                {
                    vfx.SetActive(true);

                    majorRadius = Lerp(majorRadius, 8, timeStartedLerping, 0.7f);
                    visualEffect.SetFloat("major radius", majorRadius);


                    if (majorRadius >= 6)
                    {
                        changingVFXAtTheMoment = false;

                    }

                }
                else
                {
                    Debug.Log("Down");
                    majorRadius = Lerp(majorRadius, 0, timeStartedLerping, 0.7f);
                    visualEffect.SetFloat("major radius", majorRadius);
                    if (majorRadius <= 0)
                    {
                        changingVFXAtTheMoment = false;

                    }
                }
            }
            else
            {
                visualEffect.SetInt("particles", 0);
            }*/


        }


        private void StartLerping() //Function for taking the starting time of lerping
        {
            timeStartedLerping = Time.time;
        }

        private float Lerp(float start, float end, float timeStartedLerping, float lerpTime = 1) //Easier usage of lerp
        {
            float timeSinceStarted = Time.time - timeStartedLerping;
            float percentageComplete = timeSinceStarted / lerpTime;
            float result = Mathf.Lerp( start, end, percentageComplete );
            return result;
        }


    }
}