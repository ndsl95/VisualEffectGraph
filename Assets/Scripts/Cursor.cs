using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    [SerializeField]
    Texture2D cursor;
    public int width;
    public int height;
    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Cursor.visible = false;
    }

    // Update is called once per frame
   void OnGUI()
    {
        GUI.DrawTexture(new Rect(Event.current.mousePosition.x - width / 2 , Event.current.mousePosition.y - height / 2 , width , height) , cursor);
    }
}
