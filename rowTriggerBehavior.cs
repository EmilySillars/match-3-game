using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Class: Row Trigger Behavior
 * Desciption: This class keeps track of the entrance and exits of pandas thru space. 
 * There are 16 of these rowTriggers, each in charge of one coordinate on the 4x4 panda gameboard.
 * Instance Fields: Contains flags such as bool entered and bool exited, and information on the panda 
 * touching it, such as int type, row, and collumn.
 * Methods: Mostly built in collision detection methods, like OnTriggerEnter2D() or OnMouseDown(). Also
 * a pandaExitedActions() method, made public for the match manager to call after deleting pandas.
 * **/


public class rowTriggerBehavior : MonoBehaviour {
    //instance fields
    bool exited; //a panda exited the trigger
    bool entered; //a panda entered the trigger
    public bool empty; //the trigger is empty
    public int collumn; //the collumn in which the trigger resides
    public int row; //the row in which the trigger resides
    public int type = -1; //the type of panda currently touching the trigger
    public GameObject mouse; //the mouse object
    public GameObject pandas; //the match manager, which manages all the panda instances in the game.
    //these flags  below were created to deal with the possibility of two pandas touching the same trigger at the same time while sliding downwards.
    //No more than two pandas can ever be touching a trigger at the same time, so the trigger keeps track of the first panda to enter,
    //the second panda to enter, and then resets to start looking for the 'first panda' to enter(since up to two can have both entered and not exited).
    public bool firstPanda; //the first panda to enter the trigger
    public bool secondPanda; //the second panda to enter the trigger

    // Use this for initialization
    void Start () {
        mouse = GameObject.Find("mouseTrigger"); // initialize the reference to the mouse object.
        pandas = GameObject.Find("matchManager"); // initialize the reference to the match manager(which holds all the pandas).
        exited = false;
        entered = false;
        empty = true;
        type = -1; //no panda is touching the trigger at the start of the game, so set it to an invalid value like -1.
        pandas.GetComponent<matchManagerBehavior>().updateType(row, collumn, type); //tell the match manager the type of panda touching this trigger. (In this case an invalid type of -1, since empty.)
        pandas.GetComponent<matchManagerBehavior>().updateFullness(row, collumn, empty); //tell the match manager whether this trigger is empty.
        firstPanda = false;
        secondPanda = false;

    }

    /* Method Name: OnTriggerEnter2D
    *  Description: This built-in function fires whenever an object enters this object's collider.
    *  In this case, it records information if a panda object enters the trigger.
    *  Parameters: 
    *  collision -  the object or object's collider that hit the trigger.
    *  Return: none
    * **/
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "childCollider") //if childCollider (a small collider in the center of every panda object) enters, do the following!
        {
            if (firstPanda == true && secondPanda == true) //if two pandas are touching the trigger,
            {
                //report an error, because it's not possible for three pandas to be touching the same trigger at the same time.
                Debug.Log("There's been an error! Apparently already two pandas have entered the trigger and have not left.");
            }
            else if (firstPanda == false && secondPanda == false) //if no pandas have been recorded entering,
            {
                firstPanda = true; //record a first panda has entered
            }
            else if (firstPanda == true && secondPanda == false) //if one panda has already entered,
            {
                secondPanda = true; //record a second panda has entered.
            }
            entered = true;
            empty = false;
            exited = false; //since a panda entered, exited is set to false to show there is a panda inside that has not left yet.
            collision.GetComponent<Transform>().GetComponentInParent<pandaBehavior>().setRC(row, collumn);//tell this panda it's position on the gameboard (the trigger's row and collumn)
            type = collision.GetComponent<Transform>().GetComponentInParent<pandaBehavior>().getType(); //update the type of panda currently touching the trigger
            pandas.GetComponent<matchManagerBehavior>().updateFullness(row, collumn, empty); //tell the match manager this trigger is full
            pandas.GetComponent<matchManagerBehavior>().updateType(row, collumn, type); //tell the match manager the type of panda at this position on the gameboard
            pandas.GetComponent<matchManagerBehavior>().updatePanda(row, collumn, collision.GetComponent<Transform>().parent.gameObject); //give the match manager a reference to the panda object touching this trigger
        }
     
    }

    /* Method Name: OnTriggerExit2D
   *  Description: This built-in function fires whenever an object exits this object's collider.
   *  In this case, it updates the necessary information if a panda object exits the trigger.
   *  Parameters: 
   *  collision -  the object or object's collider that exited the trigger.
   *  Return: none
   * **/
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.name == "childCollider") //if childCollider (a small collider in the center of every panda object) exits, do the following!
        {
            collision.GetComponent<Transform>().GetComponentInParent<pandaBehavior>().setRC(-1, -1);//clear the trigger's position on the gameboard from the exiting panda
            if (firstPanda == false && secondPanda == false) //if no pandas have been recorded entering,
            {
                //report an error because apparently a panda exited, but was not recorded as having ever entered.
                Debug.Log("There's been an error! A panda exited and yet it seems zero pandas have touched the trigger!");
            }
            else if (firstPanda == true && secondPanda == false) //if the first panda has entered,
            {
                firstPanda = false; //set first panda to false, because it has just exited.
                empty = true; //set empty to true because there are no longer any pandas touching the trigger.
                pandas.GetComponent<matchManagerBehavior>().updateFullness(row, collumn, empty); //tell the match manager the trigger is empty.
            }
            else if (firstPanda == true && secondPanda == true) //if a first and second panda have entered,
            {
                secondPanda = false; //set second panda to false because one panda exited.
                                     // leave the empty flag alone because there is still one panda touching the trigger
            }
            exited = true; //set exited to true because a panda exited.
        }
      
    }

/* Method Name: OnMouseDown()
*  Description: This built-in function fires whenever the mouse clicks down on this object's collider.
*  In this case, it's used to remedy a panda selection problem during runtime. Sometimes it would take a couple clicks 
*  for the mouseObject to register it had clicked a panda. I think this is because the trigger colliders were on top of the panda colliders somehow,
*  and the mouse object would select the trigger collider instead of the panda's collider. To try to remedy what I think is the problem, I have the
*  triggers' colliders also act as fake panda colliders/ an extension of the panda colliders. If weird problems arise, I will of course get rid of this.
*  Parameters: none
*  Return: none
* **/
    private void OnMouseDown() 
    {
        mouse.GetComponent<mouseTriggerBehavior>().clickedPanda(row, collumn); //tell the mouseTrigger it clicked a panda (heh.)
    }



}//end of class
