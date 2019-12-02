using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionState { FIGHT, REST, INVESTIGATION }
//ATTACK State is when we are under attack
//REST State is everytime we are not under attack
//INVESTIGATION State - I haven't came up what does it look like yet but I thought that we can add some sort of clues UI ( TO DO )


/// <summary>
/// Class made for changing states. To use it - just pick a decent moement that you want the
/// state to change and then call one of the functions MakeTheStateFight() or MakeTheStateRest().
/// 
/// If you want to extend this class of new states then add a nes stete in enum, add a function
/// "makeTheState<yourName>()" adn add a new state in Animator.
/// 
/// For test Reasons - use Update to check if everything works. In case of probles - comment it out.
/// 
/// TO DO: States have to have it's cooldowns:
/// if we're under attack - then start attack
/// if we stop the attack - cooldown shows after 5 seconds 
/// </summary>
public class StateManager : MonoBehaviour
{

    public ActionState currentActionState = ActionState.REST;


    private bool firstStateChange = true;

    private Animator _animator;
    private Transform weaponExtensions, hpBar, clickEText;
    private GameObject mainCanvas;
    void Start()
    {


        mainCanvas = GameObject.Find("Main Canvas");
        _animator = mainCanvas.GetComponent<Animator>();


        weaponExtensions = mainCanvas.transform.Find("Weapon extensions");
        hpBar = mainCanvas.transform.Find("Hp");
        clickEText = mainCanvas.transform.Find("Click E");

        

        if (currentActionState == ActionState.REST) //If rest at start then everything can be invisible, and if figt then everything has to be shown so we don't have to call nothing because on the scene there is everything on SetActive(true)
        {

            SetUIFight(false);
        }

    }

    //TO DO: Delete it:
#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            if(currentActionState == ActionState.FIGHT)
            {
                MakeTheStateRest();
            }else
            {
                MakeTheStateFight();
            }
        }
    }
#endif

    /// <summary>
    /// ATTACK State is when we are under attack
    /// </summary>
    public void MakeTheStateFight()
    {
        
        if (currentActionState != ActionState.FIGHT)
        {
            if (firstStateChange == true)
            {
                firstStateChange = false;
                SetUIFight(true);
            }
            Debug.Log("Fight on");
            currentActionState = ActionState.FIGHT;
            _animator.SetTrigger("Fight");
        }
    }

    /// <summary>
    /// REST State is everytime we are not under attack
    /// </summary>
    public void MakeTheStateRest()
    {
        
        if (currentActionState != ActionState.REST)
        {
            if (firstStateChange == true)
            {
                firstStateChange = false;
                SetUIFight(true);
            }
            Debug.Log("Rest on");
            currentActionState = ActionState.REST;
            _animator.SetTrigger("Rest");

        }
    }


    /// <summary>
    /// I haven't came up what does it look like yet
    /// </summary>
    public void MakeTheStateInvestigation()
    {
        if (currentActionState != ActionState.INVESTIGATION)
        {
            currentActionState = ActionState.INVESTIGATION;
        }
    }

    void SetUIFight(bool toTrue) //It setting the ui object to false ( made for starting the game, when we define the start state) - if it's fight then nothing happens and if it's rest then ui dissapears
    {
        weaponExtensions.gameObject.SetActive(toTrue);
        hpBar.gameObject.SetActive(toTrue);
        clickEText.gameObject.SetActive(toTrue);
    }

}
