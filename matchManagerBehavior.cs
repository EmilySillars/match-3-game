using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
//using UnityEditor;

/******************************* some classes to create 2D arrays visible in unity's inspector ***********************************/
/* Class Name: Multi Dimensiona lInt
 * Description: A 2D array of ints that is visible in Unity's inspector.
 * Each item in this array is an array of ints, size 8.
 * **/
[System.Serializable]
public class MultiDimensionalInt
{
    public int[] intArray = new int[8];
}

/* Class Name: Multi Dimensional Bool
 * Description: A 2D array of booleans that is visible in Unity's inspector.
 * Each item in this array is an array of bools, size 8.
 * **/
[System.Serializable]
public class MultiDimensionalBool
{
    public bool[] boolArray = new bool[8];
}

/* Class Name: Multi Dimensional Game Object
 * Description: A 2D array of gameObjects that is visible in Unity's inspector.
 * Each item in this array is an array of gameObjects, size 8.
 * **/
[System.Serializable]
public class MultiDimensionalGameObject
{
    public GameObject[] GameObjectArray = new GameObject[8];
}

/******************************* The big important class this script was named after ***********************************/

/* Class Name: Match Manager Behavior
 * Description: This class manages all the pandas, the gameboard, and their up to date properties.
 * It checks for matches of three adjacent pandas of the same color/type and deletes them, as well as adds pandas as necessary.
 * *Note: Only vertical and horizontal matches, no diagonal!
 * Instance Fields: References to the mouse object and a panda object prefab, 
 * as well arrays of flags and information relating to the rowTriggers and their corresponding pandas.
 * Methods: Many methods, relating to game board set up, updating information, and matching/deleting/adding/switching pandas around.
 * Here is a layout of this game's 4 x 4 game board for reference:
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
public class matchManagerBehavior : MonoBehaviour
{
    //constants
    public const int boardWidth = 8;
    public const double boardX = -1.5;
    public const double boardY = 6.5;
    //variables
    public MultiDimensionalGameObject[] panda2DArray = new MultiDimensionalGameObject[boardWidth]; //a 2D aray of panda objects.
    public MultiDimensionalInt[] types = new MultiDimensionalInt[boardWidth]; //a 2D array of type/color values, corresponding to the array of panda objects
    public MultiDimensionalBool[] triggers = new MultiDimensionalBool[boardWidth]; //a 2D array of booleans, corresponding to the fullness of each rowTrigger on the gameboard.
    public MultiDimensionalBool[] toDelete = new MultiDimensionalBool[boardWidth]; //a 2D array of 'to-delete-flags', corresponding to the panda objects
    public GameObject panda; // a reference to a panda prefab, which allows this class to instantiate more panda objects during runtime.
    public GameObject mouse; //a reference to the mouse object
    public int totalNumMatches;
    //debugging variables below
    public TextAsset posAsset;
    public Text matchesDisplayText;
    public string content;
    string contentPath;
    public GameObject pandaFellow;



    // Use this for initialization
    void Start()
    {
        totalNumMatches = 0;
        content = posAsset.text;
        Debug.Log(content);
        contentPath = "Assets/Resources/positions.txt";

        /*//Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
        //writer.WriteLine("Test if this thing wrote anything");
        writer.Write("Test if this thing wrote anything");
        writer.Close();
        //content = txt.text;
        //Debug.Log(content);
        //Re-import the file to update the reference in the editor
        AssetDatabase.ImportAsset(path);
        TextAsset asset = Resources.Load("positions") as TextAsset;

        //Print the text from the file
        Debug.Log(asset.text); */

        //set every delete flag to false
        for (int i = 0; i < boardWidth; i++)
        {
            for (int j = 0; j < boardWidth; j++)
            {
                toDelete[i].boolArray[j] = false;
            }
        }

        //create 64 pandas above the 8x8 gameboard.
        for (int i = 0; i < boardWidth; i++) //for each row
        {
            for (int j = 0; j < boardWidth; j++)//for each collumn
            {
                double x = ((i * 1.0) + boardX);
                double y = ((j * 1.0) + boardY+1);
                Instantiate(panda, new Vector3((float)x, (float)y, 0), Quaternion.identity); //create a panda object at position (x,y).
            }
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //pandaFellow.GetComponent<Transform>().Translate(0f, 0.1f, 0, GameObject.Find("anchorOther").GetComponent<Transform>());
        /********** if in switching mode, perform switching mode actions ***************/
        if (mouse.GetComponent<mouseTriggerBehavior>().switchingMode == true)
        {
            Debug.Log("We're in switching mode!"); //debugging
        }
        /******** if NOT in switching mode AND all the triggers are full,
                            check if there is a match of three.           *************/
        else if (checkFull() && mouse.GetComponent<mouseTriggerBehavior>().switchBack == false)
        {
            int matches = checkForMatches(); //check for matches
            Debug.Log("Currently " + matches + " matches."); //debugging
            totalNumMatches += matches;
            //update UI text with new number for total matches
            matchesDisplayText.text = "Matches: " + totalNumMatches;
            addNecessaryPandas(); //add pandas if pandas are to be deleted
            deleteAndReset(); //delete pandas if matches were found and update match manager info.
        }
    }

    /* Method Name: checkFull()
    *  Description: This method checks if the game board is full (if all the rowTriggers have a panda object touching them).
    *  Parameters: none
    *  Return: bool; returns true if full, false if empty.
    * **/
    public bool checkFull()//if all the triggers are touching a panda, return true. Otherwise, return false!
    {
        for (int i = 0; i < boardWidth; i++)
        {
            for (int j = 0; j < boardWidth; j++)
            {
                if (triggers[i].boolArray[j] == true)//if the current trigger IS empty
                {
                    return false; //(since not every single trigger is full)
                }
            }
        }
        return true;
    }

    /* Method Name: updateFullness()
    *  Description: this method is called to update the match manager on whether the row trigger at (row, collumn) is full.
    *  Parameters:
    *  int row - the row in which the row trigger resides.
    *  int collumn - the collumn in which the row trigger resides
    *  bool empty - whether the row trigger is full
    *  Return: none
    * **/
    public void updateFullness(int row, int collumn, bool empty)
    {
        triggers[row].boolArray[collumn] = empty; //update the fullness boolean value for the trigger at (row, collumn). 
    }

    /* Method Name: updateType()
  *  Description: this method is called to update the match manager on the type of panda touching the row trigger at (row, collumn).
  *  Parameters:
  *  int row - the row in which the row trigger resides.
  *  int collumn - the collumn in which the row trigger resides
  *  int type - the type/color of panda touching the row trigger
  *  Return: none
  * **/
    public void updateType(int row, int collumn, int type)
    {
        types[row].intArray[collumn] = type; //update the type/color value for the panda object touching the trigger at (row, collumn). 
    }

    /* Method Name: updatePanda()
*  Description: this method is called to update the match manager on the panda object touching the row trigger at (row, collumn).
*  Parameters:
*  int row - the row in which the row trigger resides.
*  int collumn - the collumn in which the row trigger resides
*  GameObject panda - the panda object touching the row trigger
*  Return: none
* **/
    public void updatePanda(int row, int collumn, GameObject panda)
    {
        panda2DArray[row].GameObjectArray[collumn] = panda;
    }

    /* Method Name: checkRow()
    *  Description: This method checks for 3 adjacent pandas of the same type/color on a single row of 4 pandas on the 4x4 gameboard.
    *  Parameters:
    *  int row - the row on the gameboard to be checked for matches of 3.
    *  Return: int; the number of matches found in this row of pandas on the gameboard
    * **/
    int checkRow(int row)
    {
        int matches = 0; //currently 0 matches found
        int i = 0;
        int j = 1;
        for (int k = 2; k < boardWidth; i++, j++, k++)
        {
            //if the pandas at collumns i, j, and k all share the same type,
            if ((types[row].intArray[i] == types[row].intArray[j]) && (types[row].intArray[j] == types[row].intArray[k]))
            {
                matches++; //increase the number of matches by 1 (since there is a match of 3!)
                toDelete[row].boolArray[i] = true;
                toDelete[row].boolArray[j] = true;
                toDelete[row].boolArray[k] = true;
            }
        }

        return matches; //return the number of matches found
    }

    /* Method Name: checkCollumn()
    *  Description: This method checks for 3 adjacent pandas of the same type/color on a single collumn of 4 pandas on the 4x4 gameboard.
    *  Parameters:
    *  int row - the collumn on the gameboard to be checked for matches of 3.
    *  Return: int; the number of matches found in this collumn of pandas on the gameboard
    * **/
    int checkCollumn(int collumn)
    {
        int matches = 0; //currently 0 matches found
        int i = 0;
        int j = 1;
        for (int k = 2; k < boardWidth; i++, j++, k++)
        {
            if ((types[i].intArray[collumn] == types[j].intArray[collumn]) && (types[j].intArray[collumn] == types[k].intArray[collumn])) //if the pandas at rows i, j, and k all share the same type,
            {
                matches++; //increase the number of matches by 1 (since there is a match of 3!)
                           //record the positions of these matching pandas in the toDelete array so they can be deleted later.
                toDelete[i].boolArray[collumn] = true;
                toDelete[j].boolArray[collumn] = true;
                toDelete[k].boolArray[collumn] = true;
            }
        }   
        return matches; //return the number of matches found
    }

    /* Method Name: addNecessaryPandas()
    *  Description: This method creates a panda object at a position above the game board 
    *  that corresponds to the position of a panda that is going to be deleted from the game board.
    *  Parameters: none
    *  Return: none
    * **/
    void addNecessaryPandas()
    {
        //create a panda above the gameboard for each panda to be deleted, in a corresponding position.
        for (int i = 0; i < boardWidth; i++) //for each row
        {
            for (int j = 0; j < boardWidth; j++) //for each collumn
            {
                if (toDelete[i].boolArray[j]) //if the toDelete boolean at (row, collumn) is true, 
                {
                    //create a panda above the gameboard at a corresponding position
                    double x = ((j * 1.0) + boardX); // x = (collumn * widthOfPanda) + x-position of the left side of gameboard.
                    double y = ((i * 1.0) + boardY+1); // y = (row * heightOfPanda) + (y-position of the top of the gameboard + heightOfPanda).
                    Instantiate(panda, new Vector3((float)x, (float)y, 0), Quaternion.identity); //create a panda at position (x, y).
                }
            }
        }

    }

    /* Method Name: deleteAndReset()
    *  Description: This method deletes any pandas that make up a match of 3 according to the toDelete array.
    *  It also resets each toDelete flag from the toDelete array after destroying the corresponding panda.
    *  Parameters: none
    *  Return: none
    * **/
    void deleteAndReset()
    {
        //delete the matches and then reset that panda's corresponding value in the toDelete array to false.
        for (int i = 0; i < boardWidth; i++) //for each row,
        {
            for (int j = 0; j < boardWidth; j++) //for each collumn,
            {
                if (toDelete[i].boolArray[j]) //if the flag toDelete at (row, collumn) is true,
                {
                    string name = "rowTrigger" + i + "_" + j; //recreate the name of the panda's rowTrigger (debugging!!)
                    Destroy(panda2DArray[i].GameObjectArray[j]); //destroy the panda at (row, collumn) on the gameboard.
                    toDelete[i].boolArray[j] = false; //reset the flag toDelete at (row, collumn) to false.
                }
            }
        }

    }

    /*
     * Method Name: checkForMatches()
     * Description: This method checks every row and collumn on the gameboard for matches of 3 (3 adjacent pandas of the same type/color).
     * Parameters: none
     * Return: int; returns an integer representing the number of matches of 3 on the gameboard.
     * **/
    public int checkForMatches()
    {
        int matches = 0; //currently 0 matches
        for (int i=0; i < boardWidth; i++)
        {
            matches += checkRow(i);
            matches += checkCollumn(i);
        }
        return matches; //return the number of matches found.
    }

    /* Method Name: switchPandas()
    *  Description: This method switches two adjacent pandas on the gameboard according to the specified direction
    *  Parameters:
    *  int row - the row in which the panda to be switched resides.
    *  int collumn - the collumn in which the panda to be switched resides.
    *  int direction - the direction in which to switch the panda. Key below.
    *       0 - UP. (switch this panda with the panda on top of it.)
    *       1 - DOWN. (switch this panda with the panda directly below it.)
    *       1 - LEFT. (switch this panda with the panda directly to the left of it.)
    *       1 - RIGHT. (switch this panda with the panda directly to the right of it.)
    *  Return: none
    * **/
    public bool switchPandas(int row, int collumn, int direction)
    {
        int type = panda2DArray[row].GameObjectArray[collumn].GetComponent<pandaBehavior>().getType(); //save the type/color of the panda to be switched
        int tempType; //the other panda's type
        int tempRow; //the row in which the other panda resides
        int tempCollumn; //the collumn in which the other panda resides
        switch (direction) //given the value of integer direction, perform specific actions.
        {
            case 0: //up
                tempRow = row - 1; //set the other panda's row to the row directly above the original panda to be switched.
                tempCollumn = collumn; //set the other panda's collumn to the collumn of the panda to be switched.
                tempType = panda2DArray[tempRow].GameObjectArray[tempCollumn].GetComponent<pandaBehavior>().getType(); //save the other panda's type.
                //now switch the pandas
                panda2DArray[tempRow].GameObjectArray[tempCollumn].GetComponent<pandaBehavior>().setType(type); //set the other panda's type to the original panda's type.
                updateType(tempRow, tempCollumn, type); //update the match manager's info for type of panda at (tempRow, tempCollumn).
                panda2DArray[row].GameObjectArray[collumn].GetComponent<pandaBehavior>().setType(tempType); //set the original panda's type to the other panda's old type.
                updateType(row, collumn, tempType); //update the match manager's info for type of panda at (row, collumn).
                if (checkForMatches() <= 0)//if switching these pandas does NOT create a match,
                {
                    //switch the pandas back!!
                    panda2DArray[tempRow].GameObjectArray[tempCollumn].GetComponent<pandaBehavior>().setType(tempType); //set the other panda's type to its old type.
                    updateType(tempRow, tempCollumn, tempType); //update the match manager's info for type of panda at (tempRow, tempCollumn).
                    panda2DArray[row].GameObjectArray[collumn].GetComponent<pandaBehavior>().setType(type); //set the original panda's type to its old type.
                    updateType(row, collumn, type); //update the match manager's info for type of panda at (row, collumn).

                    Debug.Log("Tried to switch pandas, but no match would be created."); //debugging.
                    return false;
                }
                return true;
            case 1: //down
                tempRow = row + 1; //set the other panda's row to the row directly below the original panda to be switched
                tempCollumn = collumn; //set the other panda's collumn to the collumn of the panda to be switched.
                tempType = panda2DArray[tempRow].GameObjectArray[tempCollumn].GetComponent<pandaBehavior>().getType(); //save the other panda's type.
                //now switch the pandas
                panda2DArray[tempRow].GameObjectArray[tempCollumn].GetComponent<pandaBehavior>().setType(type);
                updateType(tempRow, tempCollumn, type);
                panda2DArray[row].GameObjectArray[collumn].GetComponent<pandaBehavior>().setType(tempType);
                updateType(row, collumn, tempType);
                if (checkForMatches() <= 0)//if switching these pandas does NOT create a match,
                {
                    //switch the pandas back!!
                    panda2DArray[tempRow].GameObjectArray[tempCollumn].GetComponent<pandaBehavior>().setType(tempType); //set the other panda's type to its old type.
                    updateType(tempRow, tempCollumn, tempType); //update the match manager's info for type of panda at (tempRow, tempCollumn).
                    panda2DArray[row].GameObjectArray[collumn].GetComponent<pandaBehavior>().setType(type); //set the original panda's type to its old type.
                    updateType(row, collumn, type); //update the match manager's info for type of panda at (row, collumn).

                    Debug.Log("Tried to switch pandas, but no match would be created."); //debugging.
                    return false;
                }
                return true;
            case 2: //left
                tempRow = row; //set the other panda's row to the row of the panda to be switched.
                tempCollumn = collumn - 1; //set the other panda's collumn to the collumn directly left of the original panda to be switched
                tempType = panda2DArray[tempRow].GameObjectArray[tempCollumn].GetComponent<pandaBehavior>().getType(); //save the other panda's type.
                //now switch the pandas
                panda2DArray[tempRow].GameObjectArray[tempCollumn].GetComponent<pandaBehavior>().setType(type);
                updateType(tempRow, tempCollumn, type);
                panda2DArray[row].GameObjectArray[collumn].GetComponent<pandaBehavior>().setType(tempType);
                updateType(row, collumn, tempType);
                if (checkForMatches() <= 0)//if switching these pandas does NOT create a match,
                {
                    //switch the pandas back!!
                    panda2DArray[tempRow].GameObjectArray[tempCollumn].GetComponent<pandaBehavior>().setType(tempType); //set the other panda's type to its old type.
                    updateType(tempRow, tempCollumn, tempType); //update the match manager's info for type of panda at (tempRow, tempCollumn).
                    panda2DArray[row].GameObjectArray[collumn].GetComponent<pandaBehavior>().setType(type); //set the original panda's type to its old type.
                    updateType(row, collumn, type); //update the match manager's info for type of panda at (row, collumn).

                    Debug.Log("Tried to switch pandas, but no match would be created."); //debugging.
                    return false;
                }
                return true;
            case 3: //right
                tempRow = row; //set the other panda's row to the row of the panda to be switched.
                tempCollumn = collumn + 1; //set the other panda's collumn to the collumn directly right of the original panda to be switched
                tempType = panda2DArray[tempRow].GameObjectArray[tempCollumn].GetComponent<pandaBehavior>().getType(); //save the other panda's type.
                //now switch the pandas
                panda2DArray[tempRow].GameObjectArray[tempCollumn].GetComponent<pandaBehavior>().setType(type);
                updateType(tempRow, tempCollumn, type);
                panda2DArray[row].GameObjectArray[collumn].GetComponent<pandaBehavior>().setType(tempType);
                updateType(row, collumn, tempType);
                if (checkForMatches() <= 0)//if switching these pandas does NOT create a match,
                {
                    //switch the pandas back!!
                    panda2DArray[tempRow].GameObjectArray[tempCollumn].GetComponent<pandaBehavior>().setType(tempType); //set the other panda's type to its old type.
                    updateType(tempRow, tempCollumn, tempType); //update the match manager's info for type of panda at (tempRow, tempCollumn).
                    panda2DArray[row].GameObjectArray[collumn].GetComponent<pandaBehavior>().setType(type); //set the original panda's type to its old type.
                    updateType(row, collumn, type); //update the match manager's info for type of panda at (row, collumn).

                    Debug.Log("Tried to switch pandas, but no match would be created."); //debugging.
                    return false;
                }
                return true;
            default:
                Debug.Log("Invalid direction value of" + direction); //the value of direction is not an integer from 0 to 3 inclusive. Do NOT switch any pandas.
                return true;
        }
    }

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

   

}//end of class