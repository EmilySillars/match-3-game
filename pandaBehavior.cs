using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * HEY!
 * Remember to give credit for photos and imagery!
 * https://4.bp.blogspot.com/-_o3uc6YLn8c/VUSjf9jtqWI/AAAAAAABh-o/qj8L6SaZsFc/s1920/Celestial_Fireworks_Hubble_25th_Anniversary_uhd.jpg
 * 
 */
/* Class Name: Panda Behavior
 * Description: This class defines the characteristics of a panda object. 
 * Instance Fields: Contains variables such as int type (which refers to the color of the panda), 
 * row, and collumn (which refer to the position of the panda on the gameboard.
 * Methods: Mostly getter and setter methods; also an onMouseDown() call which responds by telling the mouse its information.
 * **/
public class pandaBehavior : MonoBehaviour {

    /*Color[] colors = {new Color(255/255f, 153/255f,153/255f,1), new Color(153/255f, 153/255f, 255/255f, 1),
        new Color(255/255f, 255/255f, 102/255f, 1), new Color(153/255f, 255/255f, 102/255f, 1), new Color(102/255f, 255/255f, 255/255f, 1),
        new Color(255/255f, 153/255f, 0/255f, 1), new Color(204/255f, 255/255f, 255/255f, 1) };*/ //an array of seven possible colors for a panda.
    const int numTypes = 7;//constant
    public Sprite[] types = new Sprite[numTypes];
    public int type = -1; //the index of a panda color in the array colors. type refers to the color or 'type' of panda, i.e. 'a Blue Panda'.
    public int row = -1; //the row in which this panda resides on the gameboard.
    public int collumn = -1; //the collumn in which this panda resides on the gameboard.
    public GameObject mouse; //a reference to the mouse object.
    public Sprite square; //a placeholder square sprite. 
    //(Later, instead of a square sprite and an array of colors, there will simply be an array of different colored panda sprites.)
 
	// Use this for initialization
	void Start () {
        mouse = GameObject.Find("mouseTrigger"); //initialize the reference to the mouse object.
        /* assign panda a type/color */
        type = (int) Random.Range(0.0f, numTypes-2); //set type to a random integer between 0 and number of types(exclusive)
        //GetComponent<SpriteRenderer>().color = colors[type]; //set the color according to the value of type.
        GetComponent<SpriteRenderer>().sprite = types[type]; //set the color/type according to the value of type.

    }

    /* Method Name: onMouseDown()
*  Description: This built-in function fires whenever the mouse clicks on this object's collider.
*  In this case, it notifies the mouse which panda it clicked.
*  Parameters: none
*  Return: none
* **/
    private void OnMouseDown()
    {
        mouse.GetComponent<mouseTriggerBehavior>().clickedPanda(row, collumn); //tell the mouse object at which row and collumn it clicked a panda
    }

    /* Method Name: getType()
    *  Description: returns this panda's type
    *  Parameters: none
    *  Return: int; the type of panda
    * **/
    public int getType()
    {
        return type; //return this panda object's type
    }

    /* Method Name: setRC()
    *  Description: Sets the values of the panda's row and collumn on the gameboard. (used for updating position)
    *  Parameters: 
    *  int row - the row in which this panda resides on the gameboard.
    *  int collumn - the collumn in which this panda resides on the gameboard.
    *  Return: none
    * **/
    public void setRC(int row, int collumn)
    {
        this.row = row;
        this.collumn = collumn;
    }

  /* Method Name: clearSprite()
  *  Description: Clears the panda's sprite so it appears invisible.
  *  Parameters: none
  *  Return: none
  * **/
    public void clearSprite()
    {
        GetComponent<SpriteRenderer>().sprite = null; //set the sprite to null
    }

  /* Method Name: resetSprite()
  *  Description: Reassigns the panda's sprite so it is visible.
  *  Parameters: none
  *  Return: none
  * **/
    public void resetSprite()
    {
        GetComponent<SpriteRenderer>().sprite = types[type]; //set the color/type according to the value of type.
        //GetComponent<SpriteRenderer>().sprite = square; //set the sprite to the square
        //GetComponent<SpriteRenderer>().color = colors[type]; //set the color according to the value of type as an index value.
    }

  /* Method Name: setType()
  *  Description: Changes panda's type value, so the next time its sprite is assigned, it will be this new type.
  *  Parameters: 
  *  int type - an integer from 0 to 6 inclusive, which corresponds to a color in the colors array.
  *  Return: none
  * **/
    public void setType(int type)
    {
        if (type >= 0 && type < numTypes) //ensure that type is between 0 and 4 inclusive.
        {
            this.type = type; //change the type  to the given parameter value
        }
        else
        {
            Debug.Log("Tried to set the sprite to a new type, but type value is invalid. Must be between 0 and 6 inclusive!");
        }
    }

}//end of class
