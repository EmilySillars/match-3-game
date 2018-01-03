using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mouseChildBehavior : MonoBehaviour {
    /*Color[] colors = {new Color(255/255f, 153/255f,153/255f,1), new Color(153/255f, 153/255f, 255/255f, 1),
        new Color(255/255f, 255/255f, 102/255f, 1), new Color(153/255f, 255/255f, 102/255f, 1), new Color(102/255f, 255/255f, 255/255f, 1),
        new Color(255/255f, 153/255f, 0/255f, 1), new Color(204/255f, 255/255f, 255/255f, 1) };*/
    public int type = -1;
    //public Sprite square;
    public Sprite[] types = new Sprite[7];
    // Use this for initialization
    void Start () {
        clearSprite();
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public void setSprite(int type)
    {
        this.type = type;
        if (type > -1)
            {
            //GetComponent<SpriteRenderer>().sprite = square;
            //GetComponent<SpriteRenderer>().color = colors[type];
            GetComponent<SpriteRenderer>().sprite = types[type]; //set the sprite according to the value of type.
        }
        }

    public void clearSprite()
    {
        GetComponent<SpriteRenderer>().sprite = null;
    }
}
