using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARRaycastManager))]
public class SelectDragManager : MonoBehaviour
{

    [SerializeField]
    private Camera arCamera;

    [SerializeField]
    private Gradient highlightColor;

    [SerializeField]
    private ARAnchorManager anchorManager = null;

    [SerializeField]
    private AudioSource source;

    private Gradient savedGrad;
    private GameObject parent;

    private Vector2 touchPosition = default;
    private Vector2 mousePosition = default;

    private ARRaycastManager arRaycastManager;

    private bool onTouchHold = false;
    private bool canSelect;
    private bool dragMode;

    public ModeManager modeManager;
    private LineRenderer selectedObject;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Awake() 
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
        canSelect = false;
        dragMode = false;
    }
    
    public void AllowSelect(bool isAllow, bool drag)
    {
        canSelect = isAllow;
        dragMode = drag;
    }

    void Update()
    {
        AllowSelect(modeManager.selectMode, modeManager.dragMode);

        
        #if !UNITY_EDITOR    
        DragOnTouch();
        #else
        DragOnMouse();
        #endif
    }

    void updatePositions(Vector3 newPos){
        //move Line
        int length = selectedObject.positionCount;
        Vector3[] oldPos = new Vector3[length];
        selectedObject.GetPositions(oldPos);
        Vector3 dist = newPos - oldPos[0];
        for(int i = 0; i < oldPos.Length; i++){
            oldPos[i] = oldPos[i] + dist;
        }
        selectedObject.SetPositions(oldPos);

        //move Boxes
        int children = parent.transform.childCount;
        Transform child;
        for(int i = 0; i < children; i++){
            child = parent.transform.GetChild(i);
            child.position = child.position + dist;
        }
        source.Play();
    }

    void DragOnTouch(){
        if(!(canSelect|| dragMode))
            return;

        if(canSelect){
            if(Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                
                touchPosition = touch.position;
                if(touchPosition.y < 260){
                    return;
                }
                if(touch.phase == TouchPhase.Began){
                    
                    if(selectedObject == null){
                        Ray ray = arCamera.ScreenPointToRay(touchPosition);
                        RaycastHit hitObject;
                        
                        if(Physics.Raycast(ray, out hitObject))
                        {
                            ARDebugManager.Instance.LogInfo($"Hit Object {hitObject}");

                            parent = hitObject.transform.parent.gameObject;
                            selectedObject = parent.GetComponent<LineRenderer>();
                            onTouchHold = true;
                            if(selectedObject != null){
                                source.Play();
                                
                                Color oldCol = selectedObject.startColor;
                                savedGrad = new Gradient();
                                savedGrad.SetKeys(
                                    new GradientColorKey[] { new GradientColorKey(oldCol, 0.0f), new GradientColorKey(oldCol, 1.0f) },
                                    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
                                );
                                selectedObject.colorGradient = highlightColor;
                            }
                        }
                    } else {
                        Ray ray = arCamera.ScreenPointToRay(touchPosition);
                        RaycastHit hitObject;
                        
                        if(Physics.Raycast(ray, out hitObject))
                        {
                            ARDebugManager.Instance.LogInfo($"Hit Object {hitObject}");

                            parent = hitObject.transform.parent.gameObject;
                            LineRenderer selectedObject2 = parent.GetComponent<LineRenderer>();
                            onTouchHold = true;
                            if(selectedObject2 != null){
                                source.Play();
                                ARDebugManager.Instance.LogInfo($"Got LineRenderer");
                                selectedObject.colorGradient = savedGrad;
                                selectedObject = selectedObject2;
                                Color oldCol = selectedObject.startColor;
                                savedGrad = new Gradient();
                                savedGrad.SetKeys(
                                    new GradientColorKey[] { new GradientColorKey(oldCol, 0.0f), new GradientColorKey(oldCol, 1.0f) },
                                    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
                                );
                                selectedObject.colorGradient = highlightColor;
                            }
                        }
                    }
                
                }
                
                /*
                Ray ray = arCamera.ScreenPointToRay(touch.position);
                RaycastHit hitObject;
                
                if(Physics.Raycast(ray, out hitObject))
                {
                    ARDebugManager.Instance.LogInfo("Hit Object");
                    selectedObject = hitObject.transform.GetComponent<LineRenderer>();
                    onTouchHold = true;
                    if(selectedObject != null){
                        Color oldCol = selectedObject.startColor;
                        savedGrad = new Gradient();
                        savedGrad.SetKeys(
                            new GradientColorKey[] { new GradientColorKey(oldCol, 0.0f), new GradientColorKey(oldCol, 1.0f) },
                            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
                        );
                        selectedObject.colorGradient = highlightColor;
                    }
                }*/
            }
            
        }

        else if(dragMode && (selectedObject != null))
        {
            if(Input.touchCount > 0)
            {
                
                Touch touch = Input.GetTouch(0);
                touchPosition = touch.position;
                if(touchPosition.y < 260){
                    return;
                }
                if(touch.phase == TouchPhase.Began){
                    Vector3 arposition = arCamera.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, 0.3f));
                    updatePositions(arposition);
                    ARDebugManager.Instance.LogInfo($"updated positions of line");
                    /*
                    ARAnchor newAnc = anchorManager.AddAnchor(new Pose(arposition, Quaternion.identity));
                    GameObject oldAnc = parent.transform.parent.gameObject;
                    parent.transform.parent = newAnc.transform;
                    ARDebugManager.Instance.LogInfo($"new parent object {newAnc}");
                    Destroy(oldAnc);
                    */
                    //selectedObject.transform.position = arposition;
                    //ARDebugManager.Instance.LogInfo($"Moved Object to {arposition}");
                }
                
            }
        }
    }

    void DragOnMouse(){
        if(!(canSelect || dragMode))
            return;
        
        if(canSelect){
            if(Input.GetMouseButton(0)){
                mousePosition = Input.mousePosition;
                if(mousePosition.y < 350){
                    return;
                }
                if(selectedObject == null){
                    Ray ray = arCamera.ScreenPointToRay(mousePosition);
                    RaycastHit hitObject;
                    
                    if(Physics.Raycast(ray, out hitObject))
                    {
                        ARDebugManager.Instance.LogInfo($"Hit Object {hitObject}");

                        parent = hitObject.transform.parent.gameObject;
                        selectedObject = parent.GetComponent<LineRenderer>();
                        onTouchHold = true;
                        if(selectedObject != null){
                            source.Play();
                            ARDebugManager.Instance.LogInfo($"Got LineRenderer");
                            Color oldCol = selectedObject.startColor;
                            savedGrad = new Gradient();
                            savedGrad.SetKeys(
                                new GradientColorKey[] { new GradientColorKey(oldCol, 0.0f), new GradientColorKey(oldCol, 1.0f) },
                                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
                            );
                            selectedObject.colorGradient = highlightColor;
                        }
                    }
                } else {
                    Ray ray = arCamera.ScreenPointToRay(mousePosition);
                    RaycastHit hitObject;
                    
                    if(Physics.Raycast(ray, out hitObject))
                    {
                        ARDebugManager.Instance.LogInfo($"Hit Object {hitObject}");

                        parent = hitObject.transform.parent.gameObject;
                        LineRenderer selectedObject2 = parent.GetComponent<LineRenderer>();
                        onTouchHold = true;
                        if(selectedObject2 != null){
                            source.Play();
                            ARDebugManager.Instance.LogInfo($"Got LineRenderer");
                            selectedObject.colorGradient = savedGrad;
                            selectedObject = selectedObject2;
                            Color oldCol = selectedObject.startColor;
                            savedGrad = new Gradient();
                            savedGrad.SetKeys(
                                new GradientColorKey[] { new GradientColorKey(oldCol, 0.0f), new GradientColorKey(oldCol, 1.0f) },
                                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
                            );
                            selectedObject.colorGradient = highlightColor;
                        }
                    }
                }
            }

        } else if(dragMode && (selectedObject != null)) {
            
            if(Input.GetMouseButton(0))
            {             
                mousePosition = Input.mousePosition;
                if(mousePosition.y < 350){
                    return;
                }

                Vector3 arposition = arCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0.3f));
                updatePositions(arposition);
                ARDebugManager.Instance.LogInfo($"updated positions of line");
                
            }
        }
        

/*
        if(Input.GetMouseButton(0))
        {
            mousePosition = Input.mousePosition;

            if(selectedObject != null) {
                mousePosition = Input.mousePosition;
            } else {
                Ray ray = arCamera.ScreenPointToRay(mousePosition);
                RaycastHit hitObject;
                
                if(Physics.Raycast(ray, out hitObject))
                {
                    ARDebugManager.Instance.LogInfo($"Hit Object {hitObject}");

                    GameObject parent = hitObject.transform.parent.gameObject;
                    selectedObject = parent.GetComponent<LineRenderer>();
                    onTouchHold = true;
                    if(selectedObject != null){
                        ARDebugManager.Instance.LogInfo($"Got LineRenderer");
                        selectedObject.colorGradient = highlightColor;
                        //selectedObject.endColor = highlightColor;
                    }
                }
            }
                
        }
        else if(Input.GetMouseButtonUp(0))
        {
            onTouchHold = false;
            selectedObject = null;
        }*/

        /*
        //if dragMode
        if(dragMode && (selectedObject != null)) 
        {
            if(onTouchHold)
            {                
                Vector3 arposition = arCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, 0.3f));
                selectedObject.transform.position = arposition;
                ARDebugManager.Instance.LogInfo($"Changing Position to {arposition}");
            }
        }*/
        
    }

    public void unselect(){
        selectedObject.colorGradient = savedGrad;
        selectedObject = null;
    }

    public LineRenderer getSelectedObject(){
        return selectedObject;
    }

    public void deleteSelected(){
        Destroy(parent);
        parent = null;
        selectedObject = null;
    }

}
