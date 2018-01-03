using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//due to debuggin purposes,
using System.IO;
//using UnityEditor;

/* Class Name: Mouse Trigger Behavior
     * Description: This class outlines the behavior for the player's mouse. It stores information on the panda 
     * the mouse clicks on and that panda's neighbors, and creates an illusion for the player of clicking and then "dragging pandas around". 
     * On clicking up, it can call the match manager's switchPandas() function.
     * Instance Fields: Contains references to this game object's four children(fake pandas), references to the panda clicked and its neighbors,
     * and other variables storing position information and flags for changes in states.
     * Methods: Contains methods for switching states, performing actions in specific states, and storing information on the panda clicked.
     * Here is a layout of this game's 4 x 4 game board for reference. The coordinates are anchor points.
 *           0             1            2            3      
 *     _____________ _____________ ____________ ____________
 *    |             |             |            |            |
 * 0  | (-1.5, 2.5) | (-0.5, 2.5) | (0.5, 2.5) | (1.5, 2.5) |
 *    |_____________|_____________|____________|____________|
 *    |             |             |            |            |
 * 1  | (-1.5, 1.5) | (-0.5, 1.5) | (0.5, 1.5) | (1.5, 1.5) |
 *    |_____________|_____________|____________|____________|
 *    |             |             |            |            |
 * 2  | (-1.5, 0.5) | (-0.5, 0.5) | (0.5, 0.5) | (1.5, 0.5) |
 *    |_____________|_____________|____________|____________|
 *    |             |             |            |            |
 * 3  | (-1.5,-0.5) | (-0.5,-0.5) | (0.5,-0.5) | (1.5,-0.5) |
 *    |_____________|_____________|____________|____________|
 *  
     * **/
public class mouseTriggerBehavior : MonoBehaviour {
    //float x = (float)-1.5 + collumn; //x = x-coord. of left side of game board + collumn * width. (Width = 1.)
    //float y = (float)2.5 - row; //y = y-coord. of top of game board + collumn * height. (Height = 1.)
    //contants
    public  double boardX = -1.5; //x-coord.of left side of game board
    public  double boardY = 6.5; //y-coord. of top of game board
    //flags
    public bool switchingMode; //flag denoting the mouse is switching around pandas. (creating an illusion for the player of clicking and dragging pandas aound))
    public bool directionPicked; //flag denoting the direction of movement during switching mode has been picked.
    public bool directionPickedHorizontal; //flag denoting the direction of movement picked during switching mode is HORIZONTAL.
    public bool inside; //flag denoting the mouse is inside a panda. this flag may be unnecessary. delete?
    //object references
    public GameObject pandas; //a reference to the match manager object, which holds references to all the pandas on the gameboard.
    GameObject left; //this object's child, located directly to the left of this object
    GameObject right; //this object's child, located directly to the right of this object
    GameObject top; //this object's child, located directly above this object
    GameObject bottom; //this object's child, located directly below this object
    GameObject middle; //this object's child, located directly on top(overlayed on top) of this object
    GameObject insidePanda; //the panda the mouse clicked on
    GameObject leftPanda; //the insidePanda's adjacent left neighbor
    GameObject rightPanda; //the insidePanda's adjacent right neighbor
    GameObject topPanda; //the insidePanda's adjacent top neighbor
    GameObject bottomPanda; //the insidePanda's adjacent bottom neighbor
    GameObject middlePanda; //also the panda the mouse clicked on; same as the insidePanda game object reference.
    //other variables
    public int[] pandaMap; //an array of integers that represents a map of insidePanda's neighbors. Key below. 
       /* Index 0: TOP NEIGHBOR
        * Index 1: BOTTOM NEIGHBOR
        * Index 2: LEFT NEIGHBOR
        * Index 3: RIGHT NEIGHBOR
        * Index 4: MIDDLE NEIGHBOR
        * *A value > 0 means the neighbor exists; a value < 0 means the neighbor does NOT exist.
        * ****/
    public int row; //the row in which the insidePanda/middlePanda resides.
    public int collumn; //the collumn in which the insidePanda/middlePanda resides.
    public int direction; //the direction in which the insidePanda is being dragged. Key below.
        /* Value of 0 means UP
         * Value of 1 means DOWN
         * Value of 2 means LEFT
         * Value of 3 means RIGHT
         * Value of -1 means no direction assigned yet.
         * ****/
    public Vector3 anchor; //the insidePanda's original position; all future positions are compared to this to determine direction of movement.
    public bool picking; //the mouse is still determining in which direction the insidePanda is being dragged.
    public bool horizontal; //the inside panda is being dragged horizontally.

    public bool switchBack;

    public Vector3 middlePos;
    public  Vector3 otherPos;
    public Vector3 middleAnchor;
    public Vector3 otherAnchor;


    // Use this for initialization
    void Start() {
        inside = false; //no panda has been clicked, so mouse inside panda is false. 
        switchBack = false;
        row = -1; //no panda has been clicked, so no insidePanda row value.
        collumn = -1; //no panda has been clicked, so no insidePanda collumn value.
        pandaMap = new int[5]; //set every neighbor in panda map to -1 (nonexistent) since no panda is clicked yet.
        for (int i = 0; i < pandaMap.Length; i++)
        {
            pandaMap[i] = -1;
        }
        switchingMode = false; //no panda clicked, so definitely not in switching mode.
        horizontal = false; //since no panda is being dragged about, horizontal movement is false.
       // picking = false; //since no panda is being dragged about, picking direction of movement is false.
        direction = -1;
        left = transform.Find("left").gameObject; //initialize reference to this object's child.
        right = transform.Find("right").gameObject; //initialize reference to this object's child.
        top = transform.Find("top").gameObject; //initialize reference to this object's child.
        bottom = transform.Find("bottom").gameObject; //initialize reference to this object's child.
        middle = transform.Find("middle").gameObject; //initialize reference to this object's child.
    }
	
	// Update is called once per frame
	void Update () {
        Vector3 pz = Camera.main.ScreenToWorldPoint(Input.mousePosition); //set pz to current mouse position
        pz.z = 0; //make sure there is no z axis rotation
        if (switchingMode) //if in switching mode,
        {
            /***************** have this object and its children move in switchingMode fashion ***************/
            //OBJECT MOVEMENT:
            Debug.Log("moving in switching mode fashion!");
            Vector3 difference = anchor - pz;//calculate the difference between the anchor point and current mouse position.
            difference.z = 0; //make sure z coordinate is 0, because 2D game.
            //move children according to the recorded direction of movement
            if (direction == -1)
            {
                if (Mathf.Abs(difference.x) > Mathf.Abs(difference.y)) // if horiztonal difference > vertical difference, move HORIZONTALLY.
                {
                    moveHorizontally(pz, difference);
                }
                else //move VERTICALLY
                {
                    moveVertically(pz, difference);
                }
            }
            else if (direction <= 1)
            {
                moveVertically(pz, difference);
            }
            else //direction <= 3
            {
                moveHorizontally(pz, difference);
            }

            

            //if the mouse is pressed up (and in switching mode), turn switching mode OFF.
            if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1) || Input.GetMouseButton(2))
            {
                if(pandas.GetComponent<matchManagerBehavior>().switchPandas(row, collumn, direction) == false) //if no switch occurred, slide the pandas back!
                {
                    switchBack = true;
                    middleAnchor = anchor;
                    switch (direction)
                    {
                        case 0: //otherPos = top.transform.position;
                            otherAnchor = new Vector3(anchor.x, (anchor.y + 1), 0);
                            break;
                        case 1: //otherPos = bottom.transform.position;
                            otherAnchor = new Vector3(anchor.x, (anchor.y - 1), 0);
                            break;
                        case 2: //otherPos = left.transform.position;
                            otherAnchor = new Vector3(anchor.x - 1, anchor.y, 0);
                            break;
                        case 3: //otherPos = right.transform.position;
                            otherAnchor = new Vector3(anchor.x + 1, anchor.y, 0);
                            break;
                    }
                    switchingMode = false;
                    return;
                }           
                turnSwitchingModeOFF(); //disable switchingMode cosmetics
                anchor = new Vector3(0,0,0);
                picking = false;
                insidePanda = null; //get rid of the panda saved as the one the mouseTrigger is inside
                inside = false; //set the state of the mouseTrigger to NOT inside a panda.
                row = -1; //reset the row value to a null default value
                collumn = -1; //reset the saved collumn value to a null default value
            }
        }
        else if (switchBack)
        {
            if (middle.GetComponent<Transform>().position == middleAnchor)
            {
                Debug.Log("Supposedly, middle position "+ middle.GetComponent<Transform>().position.ToString("R")+" = "+middleAnchor+" middleAnchor\n current anchor value: "+anchor.ToString("R"));
                GetComponent<Transform>().position = new Vector3(anchor.x, anchor.y, 0);
                top.GetComponent<Transform>().position = new Vector3(anchor.x, (anchor.y + 1), 0);
                bottom.GetComponent<Transform>().position = new Vector3(anchor.x, (anchor.y - 1), 0);
                left.GetComponent<Transform>().position = new Vector3(anchor.x - 1, anchor.y, 0);
                right.GetComponent<Transform>().position = new Vector3(anchor.x + 1, anchor.y, 0);
                middle.GetComponent<Transform>().position = new Vector3(anchor.x , anchor.y, 0);
                turnSwitchingModeOFF(); //disable switchingMode cosmetics
                anchor = new Vector3(0, 0, 0);
                picking = false;
                insidePanda = null; //get rid of the panda saved as the one the mouseTrigger is inside
                inside = false; //set the state of the mouseTrigger to NOT inside a panda.
                row = -1; //reset the row value to a null default value
                collumn = -1; //reset the saved collumn value to a null default value
                //reset switchback values too!!!
                switchBack = false;

            }
            else
            {
               slideBack(row, collumn, direction);
            }
            //trandform.Translate(Time.deltaTime * - 0.1, 0, 0, Camera.main.transform);
        }
        else
        {
            //have this object follow the mouse
            GetComponent<Transform>().position = pz;
        }     
    } //end of update

    /* Method Name: moveHorizontally()
 *  Description: This method moves the middle child and the left or the right child
 *  according to the movmement of the mouse, creating an illusion for the player of swapping pandas.
 *  Parameters:
 *  Vector3 pz - the current mouse position
 *  Vector3 difference - the difference between the mouse and the anchor point's positions
 *  Return: none
 * **/
 private void moveHorizontally(Vector3 pz, Vector3 difference)
    {
        //horizontal movement
        if (pz.x > anchor.x + 1 && pandaMap[3] == 1) //if mouse is far right of the anchor AND right movement allowed,
        {
            //if(pandaMap[3] == 1)
            GetComponent<Transform>().position = new Vector3(anchor.x + 1, anchor.y, 0); //move to rightmost point
            //Debug.Log("rightest point.");
            right.GetComponent<Transform>().position = new Vector3(anchor.x, anchor.y, 0); //move rightChild to middle position.
            left.GetComponent<Transform>().position = new Vector3(anchor.x - 1, anchor.y, 0); //don't move leftChild.
            top.GetComponent<Transform>().position = new Vector3(anchor.x, (anchor.y + 1), 0); //don't move topChild
            bottom.GetComponent<Transform>().position = new Vector3(anchor.x, (anchor.y - 1), 0); //don't move bottomChild

        }
        else if (pz.x < (anchor.x - 1) && pandaMap[2] == 1)  //if mouse is far left of the anchor AND left movement allowed,
        {
            GetComponent<Transform>().position = new Vector3(anchor.x - 1, anchor.y, 0); //move to leftmost point
            //Debug.Log("leftest point.");
            right.GetComponent<Transform>().position = new Vector3(anchor.x + 1, anchor.y, 0); //don't move rightChild.
            left.GetComponent<Transform>().position = new Vector3(anchor.x, anchor.y, 0); //move leftChild to middle position.
            top.GetComponent<Transform>().position = new Vector3(anchor.x, (anchor.y + 1), 0); //don't move topChild
            bottom.GetComponent<Transform>().position = new Vector3(anchor.x, (anchor.y - 1), 0); //don't move bottomChild
        }
        else if (pz.x < anchor.x && pandaMap[2] == 1) //if mouse is left of the anchor AND left movement allowed,
        {
            GetComponent<Transform>().position = new Vector3(pz.x, anchor.y, 0);   //move left
            //Debug.Log("moving left.");
            left.GetComponent<Transform>().position = new Vector3(anchor.x - 1, anchor.y, 0) + new Vector3(Mathf.Abs(difference.x), 0, 0); //move leftChild right a corresponding amount.
            right.GetComponent<Transform>().position = new Vector3(anchor.x + 1, anchor.y, 0);
            top.GetComponent<Transform>().position = new Vector3(anchor.x, (anchor.y + 1), 0);
            bottom.GetComponent<Transform>().position = new Vector3(anchor.x, (anchor.y - 1), 0);
        }
        else if (pz.x > anchor.x && pandaMap[3] == 1) //if mouse is right of the anchor AND right movement allowed,
        {
            GetComponent<Transform>().position = new Vector3(pz.x, anchor.y, 0); //move right
            //Debug.Log("moving right.");
            right.GetComponent<Transform>().position = new Vector3(anchor.x + 1, anchor.y, 0) + new Vector3(Mathf.Abs(difference.x) * -1, 0, 0); //move rightChild left a corresponding amount.
            left.GetComponent<Transform>().position = new Vector3(anchor.x - 1, anchor.y, 0);
            top.GetComponent<Transform>().position = new Vector3(anchor.x, (anchor.y + 1), 0);
            bottom.GetComponent<Transform>().position = new Vector3(anchor.x, (anchor.y - 1), 0);

        }
        else //in all other cases, keep mouse and children in standard position.
        {
            GetComponent<Transform>().position = new Vector3(anchor.x, anchor.y, 0);
           // Debug.Log("no other option. anchor coords.");
            left.GetComponent<Transform>().position = new Vector3(anchor.x - 1, anchor.y, 0);
            right.GetComponent<Transform>().position = new Vector3(anchor.x + 1, anchor.y, 0);
            top.GetComponent<Transform>().position = new Vector3(anchor.x, (anchor.y + 1), 0);
            bottom.GetComponent<Transform>().position = new Vector3(anchor.x, (anchor.y - 1), 0);
        }
        /************* Record Direction of Movement **************/
        if (GetComponent<Transform>().position.x > anchor.x + 0.5)
        {
            direction = 3; //swapping direction is right
        }
        else if (GetComponent<Transform>().position.x < anchor.x - 0.5)
        {
            direction = 2; //swapping direction is left
        }
        else
        {
            direction = -1; //no swapping direction assigned
        }
    }

    /* Method Name: moveVertically()
 *  Description: This method moves the middle child and the top or the bottom child
 *  according to the movmement of the mouse, creating an illusion for the player of swapping pandas.
 *  Parameters:
 *  Vector3 pz - the current mouse position
 *  Vector3 difference - the difference between the mouse and the anchor point's positions
 *  Return: none
 * **/
    private void moveVertically(Vector3 pz, Vector3 difference)
    {
        //vertical movement
        if (pz.y > anchor.y + 1 && pandaMap[0] == 1) //if mouse is above anchor and upward movement allowed,
        {
            GetComponent<Transform>().position = new Vector3(anchor.x, anchor.y + 1, 0); //move to highest point
            top.GetComponent<Transform>().position = new Vector3(anchor.x, anchor.y, 0); //move topChild to middle position.
            bottom.GetComponent<Transform>().position = new Vector3(anchor.x, (anchor.y - 1), 0); //don't move bottomChild
            left.GetComponent<Transform>().position = new Vector3(anchor.x - 1, anchor.y, 0); // don't move leftChild
            right.GetComponent<Transform>().position = new Vector3(anchor.x + 1, anchor.y, 0); // don't move rightChild

            //  middlePos.Add(GetComponent<Transform>().position - anchor);

            // topPos.Add(top.GetComponent<Transform>().position + new Vector3(anchor.x, anchor.y+1, 0));
        }
        else if (pz.y < anchor.y - 1 && pandaMap[1] == 1)
        {
            GetComponent<Transform>().position = new Vector3(anchor.x, anchor.y - 1, 0); //move to lowest point
            bottom.GetComponent<Transform>().position = new Vector3(anchor.x, anchor.y, 0);
            top.GetComponent<Transform>().position = new Vector3(anchor.x, (anchor.y + 1), 0);
            left.GetComponent<Transform>().position = new Vector3(anchor.x - 1, anchor.y, 0);
            right.GetComponent<Transform>().position = new Vector3(anchor.x + 1, anchor.y, 0);
        }
        else if (pz.y < anchor.y && pandaMap[1] == 1)
        {
            GetComponent<Transform>().position = new Vector3(anchor.x, pz.y, 0); //move down
            bottom.GetComponent<Transform>().position = new Vector3(anchor.x, (anchor.y - 1), 0) + new Vector3(0, Mathf.Abs(difference.y), 0);
            top.GetComponent<Transform>().position = new Vector3(anchor.x, (anchor.y + 1), 0);
            left.GetComponent<Transform>().position = new Vector3(anchor.x - 1, anchor.y, 0);
            right.GetComponent<Transform>().position = new Vector3(anchor.x + 1, anchor.y, 0);
        }
        else if (pz.y > anchor.y && pandaMap[0] == 1)
        {
            GetComponent<Transform>().position = new Vector3(anchor.x, pz.y, 0); //move up
            top.GetComponent<Transform>().position = new Vector3(anchor.x, (anchor.y + 1), 0) + new Vector3(0, Mathf.Abs(difference.y) * -1, 0);
            bottom.GetComponent<Transform>().position = new Vector3(anchor.x, (anchor.y - 1), 0);
            left.GetComponent<Transform>().position = new Vector3(anchor.x - 1, anchor.y, 0);
            right.GetComponent<Transform>().position = new Vector3(anchor.x + 1, anchor.y, 0);

            //middlePos.Add(GetComponent<Transform>().position - anchor);
            //topPos.Add(top.GetComponent<Transform>().position + new Vector3(anchor.x, anchor.y + 1, 0));
        }
        else //in all other cases, keep mouse and children in standard position.
        {
            GetComponent<Transform>().position = new Vector3(anchor.x, anchor.y, 0);
            top.GetComponent<Transform>().position = new Vector3(anchor.x, (anchor.y + 1), 0);
            bottom.GetComponent<Transform>().position = new Vector3(anchor.x, (anchor.y - 1), 0);
            left.GetComponent<Transform>().position = new Vector3(anchor.x - 1, anchor.y, 0);
            right.GetComponent<Transform>().position = new Vector3(anchor.x + 1, anchor.y, 0);

            //  middlePos.Add(GetComponent<Transform>().position - anchor);
        }

        /************* Record Direction of Movement **************/
        if (GetComponent<Transform>().position.y > anchor.y + 0.5)
        {
            direction = 0; //swapping direction is up
        }
        else if (GetComponent<Transform>().position.y < anchor.y - 0.5)
        {
            direction = 1; //swapping direction is down
        }
        else
        {
            direction = -1; //no swapping direction assigned
        }
    }

    /* Method Name: clickedPanda()
   *  Description: This method notifies the mouse it has clicked a panda object. 
   *  Depending on the state of the gameboard, it may turn switchingMode ON.
   *  Parameters:
   *  int row - the row in which the panda the mouse clicked resides
   *  int collumn - the collumn in which the panda the mouse clicked resides
   *  Return: none
   * **/
    public void clickedPanda(int row, int collumn)
    {
        if (row != -1 && collumn != -1 && pandas.GetComponent<matchManagerBehavior>().checkFull()) //if the game board is full and the panda's position is valid,
        {
            insidePanda = pandas.GetComponent<matchManagerBehavior>().panda2DArray[row].GameObjectArray[collumn]; //save the panda the mouse is inside of
            inside = true; //set the state of the mouseTrigger to INSIDE a panda.
            this.row = insidePanda.GetComponent<pandaBehavior>().row; //set mouseTrigger's row to the insidePanda's row value.
            this.collumn = insidePanda.GetComponent<pandaBehavior>().collumn; //set mouseTrigger's collumn to the insidePanda's collumn value.
            //calculate the anchor point according to new row and collumn values
            float x = (float)boardX + collumn; //x = x-coord. of left side of game board + collumn * width. (Width = 1.)
            float y = (float)boardY - row; //y = y-coord. of top of game board + collumn * height. (Height = 1.)
            anchor = new Vector3(x, y, 0); //assign anchor pointposition to (x, y).
           // picking = true; //dragging the panda around is about to be enabled, so to start, the direction of movement still needs to be determined.
            turnSwitchingModeON(); //set switching mode flag to true and enable switching mode apperance
            /**** if there is a problem, I may need to re-enable predictPandas() call below. For now it seems unnecessary. *****/
            //tell pandas to calculate possible matches!
            /*int[] positionsToPrint = pandas.GetComponent<matchManagerBehavior>().predictSwaps(row, collumn);
            Debug.Log("Possible swaps for panda " + row + "," + collumn + ":\n"
                + positionsToPrint[0] + " (TOP)\n"
                + positionsToPrint[1] + " (BOTTOM)\n"
                + positionsToPrint[2] + " (LEFT)\n"
                + positionsToPrint[3] + " (RIGHT)\n");*/
            /*****************************************************************************************************************/
           // Debug.Log("clickedPanda set a value!!");
        }
        else
        {
            //the game board is not full, or the panda's position is invalid, so DO NOT enable switchingMode and do NOT drag about!
            Debug.Log("clicked on a panda with row -1 or collumn -1 OR gameboard is not full!");
        }
    }

    /* Method Name: turnSwitchingModeON()
    *  Description: This method enables the appearance of switching mode,
    *  as well as determines in which directions the clicked panda can be moved.
    *  Parameters: none
    *  Return: none
    * **/
    public void turnSwitchingModeON()
    {
        if (switchingMode == false) //if NOT in swiching mode,
        {
            switchingMode = true; //set the mouse's switching mode to true.
            
            /**************************** set up the appearance ****************************/
            //set up middle tile
            middle.GetComponent<mouseChildBehavior>().setSprite(pandas.GetComponent<matchManagerBehavior>().types[row].intArray[collumn]);
            middlePanda = pandas.GetComponent<matchManagerBehavior>().panda2DArray[row].GameObjectArray[collumn];
            //clear the actual middle panda's sprite
            middlePanda.GetComponent<pandaBehavior>().clearSprite();
            //update PandaMap
            pandaMap[4] = 1;
            //set up top and bottom tiles
            if (row == 0) //if row 0, there's no neighbor on top.
            {
                //set the bottom child's sprite to the bottom neighbor's sprite.
                bottom.GetComponent<mouseChildBehavior>().setSprite(pandas.GetComponent<matchManagerBehavior>().types[row+1].intArray[collumn]);
                //clear the actual bottom panda's sprite
                pandas.GetComponent<matchManagerBehavior>().panda2DArray[row+1].GameObjectArray[collumn].GetComponent<pandaBehavior>().clearSprite();
                bottomPanda = pandas.GetComponent<matchManagerBehavior>().panda2DArray[row + 1].GameObjectArray[collumn]; //save a reference to this bottom panda neighbor
                //update PandaMap
                pandaMap[1] = 1; //there's a bottom nighbor

            }
            else if ( row == 7) //if row 7, there's no neighbor below.
            {
                //set the top child's sprite to the top neighbor's sprite.
                top.GetComponent<mouseChildBehavior>().setSprite(pandas.GetComponent<matchManagerBehavior>().types[row-1].intArray[collumn]);
                //clear the actual top panda's sprite
                pandas.GetComponent<matchManagerBehavior>().panda2DArray[row - 1].GameObjectArray[collumn].GetComponent<pandaBehavior>().clearSprite();
                topPanda = pandas.GetComponent<matchManagerBehavior>().panda2DArray[row - 1].GameObjectArray[collumn]; //save a reference to this top panda neighbor
                //update PandaMap
                pandaMap[0] = 1; //there's a neighbor on top

            }
            else //if NOT row 0 or 7 (the top or bottom rows),then there is a top AND bottom neighbor!
            {
                //set the top child's sprite to the top neighbor's sprite.
                top.GetComponent<mouseChildBehavior>().setSprite(pandas.GetComponent<matchManagerBehavior>().types[row-1].intArray[collumn]);
                //set the bottom child's sprite to the bottom neighbor's sprite.
                bottom.GetComponent<mouseChildBehavior>().setSprite(pandas.GetComponent<matchManagerBehavior>().types[row+1].intArray[collumn]);
                //clear the actual top panda's sprite
                pandas.GetComponent<matchManagerBehavior>().panda2DArray[row - 1].GameObjectArray[collumn].GetComponent<pandaBehavior>().clearSprite();
                //clear the actual bottom panda's sprite
                pandas.GetComponent<matchManagerBehavior>().panda2DArray[row + 1].GameObjectArray[collumn].GetComponent<pandaBehavior>().clearSprite();
                //update PandaMap
                pandaMap[0] = 1; //there's a neighbor on top
                pandaMap[1] = 1; //there's a neighbor below
                topPanda = pandas.GetComponent<matchManagerBehavior>().panda2DArray[row - 1].GameObjectArray[collumn]; //save a reference to the top panda neighbor
                bottomPanda = pandas.GetComponent<matchManagerBehavior>().panda2DArray[row + 1].GameObjectArray[collumn]; //save a reference to the bottom panda neighbor
            }

            //set up left and right tiles
            if (collumn == 0) //if collumn 0, there's no left neighbor.
            {
                //set the right child's sprite to the right neighbor's sprite.
                right.GetComponent<mouseChildBehavior>().setSprite(pandas.GetComponent<matchManagerBehavior>().types[row].intArray[collumn+1]);
                //clear the actual right panda's sprite
                pandas.GetComponent<matchManagerBehavior>().panda2DArray[row].GameObjectArray[collumn + 1].GetComponent<pandaBehavior>().clearSprite();
                rightPanda = pandas.GetComponent<matchManagerBehavior>().panda2DArray[row].GameObjectArray[collumn + 1]; //save a reference to the right panda neighbor
                //update PandaMap
                pandaMap[3] = 1; //there's a right neighbor
            }
            else if (collumn == 7) //if collumn 7, there's no right neighbor.
            {
                //set the left child's sprite to the left neighbor's sprite.
                left.GetComponent<mouseChildBehavior>().setSprite(pandas.GetComponent<matchManagerBehavior>().types[row].intArray[collumn-1]);
                //clear the actual left panda's sprite
                pandas.GetComponent<matchManagerBehavior>().panda2DArray[row].GameObjectArray[collumn - 1].GetComponent<pandaBehavior>().clearSprite();
                leftPanda = pandas.GetComponent<matchManagerBehavior>().panda2DArray[row].GameObjectArray[collumn - 1]; //save a reference to the left panda neighbor
                //update PandaMap
                pandaMap[2] = 1; //there's a left neighbor
            }
            else //if NOT row 0 nor 3, there's a left AND right neighbor!
            {
                //set the left child's sprite to the left neighbor's sprite.
                left.GetComponent<mouseChildBehavior>().setSprite(pandas.GetComponent<matchManagerBehavior>().types[row].intArray[collumn-1]);
                //set the right child's sprite to the right neighbor's sprite.
                right.GetComponent<mouseChildBehavior>().setSprite(pandas.GetComponent<matchManagerBehavior>().types[row].intArray[collumn+1]);
                //clear the actual right panda's sprite
                pandas.GetComponent<matchManagerBehavior>().panda2DArray[row].GameObjectArray[collumn + 1].GetComponent<pandaBehavior>().clearSprite();
                //clear the actual left panda's sprite
                pandas.GetComponent<matchManagerBehavior>().panda2DArray[row].GameObjectArray[collumn - 1].GetComponent<pandaBehavior>().clearSprite();
                rightPanda = pandas.GetComponent<matchManagerBehavior>().panda2DArray[row].GameObjectArray[collumn + 1]; //save a reference to the right panda neighbor
                leftPanda = pandas.GetComponent<matchManagerBehavior>().panda2DArray[row].GameObjectArray[collumn - 1]; //save a reference to the left panda neighbor
                //update PandaMap
                pandaMap[2] = 1; //there's a left neighbor
                pandaMap[3] = 1; //there's a right neighbor
            }          
        }
        else
        {
            //switching mode was already on when this new panda was clicked, which should never ever happen. Report an error.
            Debug.Log("ERROR: tried to turn switchingModeON when switching mode was already true!");
        }
    }
    
    /* Method Name: turnSwitchingModeOFF()
    *  Description: This method disables the switchingMode appearance,
    *  sets the switchingMode flag to false and resets the pandaMap.
    *  Parameters: none
    *  Return: none
    * **/
    public void turnSwitchingModeOFF()
    {
        switchingMode = false;
        direction = -1;
        /******** Disable switchingMode appearance ************/
        top.GetComponent<mouseChildBehavior>().clearSprite();
        bottom.GetComponent<mouseChildBehavior>().clearSprite();
        left.GetComponent<mouseChildBehavior>().clearSprite();
        right.GetComponent<mouseChildBehavior>().clearSprite();
        middle.GetComponent<mouseChildBehavior>().clearSprite();
        /********* Make the actual panda objects visible again **********/
        if (pandaMap[0] > 0) //if top neighbor exists,
        {
            topPanda.GetComponent<pandaBehavior>().resetSprite(); //make the top neighbor's sprite visible.
            pandaMap[0] = -1; //mark topneighbor as nonexistent.
        }
        if (pandaMap[1] > 0) //if bottom neighbor exists
        {
            bottomPanda.GetComponent<pandaBehavior>().resetSprite(); //make the bottom neighbor's sprite visible.
            pandaMap[1] = -1; //mark bottom neighbor as nonexistent.
        }
        if (pandaMap[2] > 0) //if left neighbor exists,
        {
            leftPanda.GetComponent<pandaBehavior>().resetSprite(); //make the left neighbor's sprite visible.
            pandaMap[2] = -1; //mark left neighbor as nonexistent.
        }
        if (pandaMap[3] > 0) //if right neighbor exists,
        {
            rightPanda.GetComponent<pandaBehavior>().resetSprite(); //make the right neighbor's sprite visible.
            pandaMap[3] = -1; //mark right neighbor as nonexistent.
        }
        if (pandaMap[4] > 0) //if middle neighbor exists, (which it always does so why an if statement?)
        {
            middlePanda.GetComponent<pandaBehavior>().resetSprite(); //make the middlePanda/insidePanda's sprite visible.
            pandaMap[4] = -1; //mark middle neighbor as nonexistent.
        }

    }

    //DEbugging methods below
    /*
    * Method Name: writeText()
    * Description: this is a debugging method used to record the position changes of pandas during switching.
    * Parameters: 
    * - string path - the file path to the .txt asset to write to. For ex., "Assets/Resources/positions.txt"
    * - string writing - the words to be written to the .txt file asset.
    * Return: none
    */
    public void writeText(string path, string writing)
    {
        //Write some text to the positions.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(writing);
        writer.Close();
    }

    /*
    * Method Name: printText()
    * Description: this is a debuggin method used to read a .txt file asset.
    * Parameters: 
    * - string path - the file path to the .txt asset to read from. For ex., "Assets/Resources/positions.txt"
    * - string fileNameWithoutExtension - the name of the .txt file asset. For ex., "positions"
    * Return: none
    */
    public void printText(string path, string fileNameWithoutExtension)
    {
        //Re-import the file to update the reference in the editor
       // AssetDatabase.ImportAsset(path);
       // TextAsset asset = Resources.Load(fileNameWithoutExtension) as TextAsset;
        //Print the text from the file
        //Debug.Log(asset.text);

    }

    public void slideBack(int r, int c, int dir)
    {
        //Vector3 middlePos = middle.transform.position;
        //Vector3 otherPos = 
        //switch () { }
        Vector3 diff = new Vector3();
        switch (direction)
        {
            case 0: //up
                top.GetComponent<Transform>().position += new Vector3(0f, 0.1f, 0);
                middle.GetComponent<Transform>().position += new Vector3(0f, -0.1f, 0);
                //otherPos = top.transform.position;
                //otherAnchor = new Vector3(anchor.x, (anchor.y + 1), 0);
                diff = middle.GetComponent<Transform>().position - middleAnchor;
                Debug.Log("Difference is "+diff.ToString("R")+"\n middle position: "+middle.GetComponent<Transform>().position.ToString("R")
                    +"\n middle anchor: "+middleAnchor.ToString("R")+"\n absolute value difference: "+ Mathf.Abs(diff.y));
                if (Mathf.Abs(diff.y) >= 0 && Mathf.Abs(diff.y) <= 0.2)
                {
                    top.GetComponent<Transform>().position = otherAnchor;
                    middle.GetComponent<Transform>().position = middleAnchor;
                }
                Debug.Log("Hit the UP case on the slide back function");
                break;
            case 1: //down
                    // bottom.GetComponent<Transform>().Translate(0f, -0.1f, 0, otherAnchor.GetComponent<Transform>());
                    // middle.GetComponent<Transform>().Translate(0f, 0.1f, 0, middleAnchor.GetComponent<Transform>());
                bottom.GetComponent<Transform>().position += new Vector3(0f, -0.1f, 0f);
                middle.GetComponent<Transform>().position += new Vector3(0f, 0.1f, 0f);
                //otherAnchor = new Vector3(anchor.x, (anchor.y - 1), 0);
                diff = middle.GetComponent<Transform>().position - middleAnchor;
                if (Mathf.Abs(diff.y) >= 0 && Mathf.Abs(diff.y) <= 0.2)
                {
                    bottom.GetComponent<Transform>().position = otherAnchor;
                    middle.GetComponent<Transform>().position = middleAnchor;
                }
                break;
            case 2: //left
                    //left.GetComponent<Transform>().Translate(-0.1f, 0f, 0, otherAnchor.GetComponent<Transform>());
                    // middle.GetComponent<Transform>().Translate(0.1f, 0, 0, middleAnchor.GetComponent<Transform>());
                left.GetComponent<Transform>().position += new Vector3(-0.1f, 0f, 0f);
                middle.GetComponent<Transform>().position += new Vector3(0.1f, 0f, 0f);
                // otherAnchor = new Vector3(anchor.x - 1, anchor.y, 0);
                diff = middle.GetComponent<Transform>().position - middleAnchor;
                if (Mathf.Abs(diff.x) >= 0 && Mathf.Abs(diff.x) <= 0.2)
                {
                    left.GetComponent<Transform>().position = otherAnchor;
                    middle.GetComponent<Transform>().position = middleAnchor;
                }
                break;
            case 3: //right
                    // right.GetComponent<Transform>().Translate(0.1f, 0, 0, otherAnchor.GetComponent<Transform>());
                    // middle.GetComponent<Transform>().Translate(-0.1f, 0, 0, middleAnchor.GetComponent<Transform>());
                right.GetComponent<Transform>().position += new Vector3(0.1f, 0f, 0f);
                middle.GetComponent<Transform>().position += new Vector3(-0.1f, 0f, 0f);
                // otherAnchor = new Vector3(anchor.x + 1, anchor.y, 0);
                diff = middle.GetComponent<Transform>().position - middleAnchor;
                if (Mathf.Abs(diff.x) >= 0 && Mathf.Abs(diff.x) <= 0.2)
                {
                    right.GetComponent<Transform>().position = otherAnchor;
                    middle.GetComponent<Transform>().position = middleAnchor;
                }
                break;
            default: Debug.Log("hit the default case in the slideBack function.");
                break;
        }


    }

}//end of class
