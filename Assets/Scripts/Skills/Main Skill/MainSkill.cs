using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSkill : MonoBehaviour //This class is meant for features of the "change to other world skill"
{
    private GameObject deathWorld, realWorld; //Normally i would just make "Find("").activeSelf" but apparently if the object is not active, it's
    private GameObject enemies;               //Also not detectable for script so you have to make the object active on start, attach it on script
                                              //awake and in Start set it false (kto to wymyślił XD).
    
    public void Awake()
    {
        deathWorld = GameObject.Find("Death World");
        realWorld = GameObject.Find("Real World");
        enemies = GameObject.Find("Enemies");
    }

    public void Start()
    {
        deathWorld.SetActive(false);
        
    }

    public void ChangeToOtherWorld() //Here we switch the objects from "Real World" to "Death World" and opposite
    {
        
        
        if (realWorld.activeSelf == true)
        {
            realWorld.SetActive(false);
            deathWorld.SetActive(true);
            ChangeTheTransprencyOfEnemies(0.5f);
        }
        else if (deathWorld.activeSelf == true)
        {
            realWorld.SetActive(true);
            deathWorld.SetActive(false);
            ChangeTheTransprencyOfEnemies(1f);
        }
        
    }


    private void ChangeTheTransprencyOfEnemies(float transparency)  //Change the transparency of enemy (later we will use it for other people and objects)
    {

        foreach (Transform enemy in enemies.transform)
        {

            Color enemyColor = enemy.transform.Find("Sprite").gameObject.GetComponent<SpriteRenderer>().color;
            enemyColor.a = transparency;
            enemy.transform.Find("Sprite").gameObject.GetComponent<SpriteRenderer>().color = enemyColor;
        }
    }

    //Here we can change the transparency of other characters
    //We also need to change the speed of the game somehow (f.e. attach a constant to each object we want the speed to go slower)
    //If you're at position of an obstacle in another world and you suddenly change the world, you glitch in the obstacle (we need to come up with an idea to remove the zajebany glitch)


}
