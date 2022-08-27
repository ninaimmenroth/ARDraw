using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragManager : MonoBehaviour
{
    public ModeManager modeManager;
    private LineRenderer selectedObject;
    private bool delMode;
    // Start is called before the first frame update
    void Start()
    {
        delMode = false;        
    }

    // Update is called once per frame
    void Update()
    {
        //delMode = modeManager.delMode;

        if(!delMode){
            return;
        } else {
            //selectedObject = SelectDragManager.getSelectedObject();
            //Destroy(selectedObject);
        }
    }
}
