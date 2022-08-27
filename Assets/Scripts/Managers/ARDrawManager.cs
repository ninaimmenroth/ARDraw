using System.Collections.Generic;
using DilmerGames.Core.Singletons;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARAnchorManager))]
public class ARDrawManager : Singleton<ARDrawManager>
{
    [SerializeField]
    private LineSettings lineSettings = null;

    [SerializeField]
    private UnityEvent OnDraw = null;

    [SerializeField]
    private ARAnchorManager anchorManager = null;

    [SerializeField] 
    private Camera arCamera = null;

    [SerializeField]
    GameObject spawnablePrefab;

    public ModeManager modeManager;
    private AudioSource source;
    private List<ARAnchor> anchors = new List<ARAnchor>();

    private Dictionary<int, ARLine> Lines = new Dictionary<int, ARLine>();

    private bool CanDraw { get; set; }

    void Start() {
        source = GetComponent<AudioSource>();
    }

    void Update()
    {
        AllowDraw(modeManager.drawMode);
        
        #if !UNITY_EDITOR    
        DrawOnTouch();
        #else
        DrawOnMouse();
        #endif
	}

    public void AllowDraw(bool isAllow)
    {
        CanDraw = isAllow;
    }

    void DrawOnTouch()
    {
        if(!CanDraw) {
            source.Stop();
            return;
        }

        int tapCount = Input.touchCount > 1 && lineSettings.allowMultiTouch ? Input.touchCount : 1;

        for(int i = 0; i < tapCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if(Input.GetTouch(i).position.y < 260){
                return;
            }
            Vector3 touchPosition = arCamera.ScreenToWorldPoint(new Vector3(Input.GetTouch(i).position.x, Input.GetTouch(i).position.y, lineSettings.distanceFromCamera));
            //bool isOverUI = touchPosition.IsPointOverUIObject();
            //ARDebugManager.Instance.LogInfo($"{touch.fingerId}");

            //if(isOverUI)
            if(touch.phase == TouchPhase.Began)
            {
                OnDraw?.Invoke();
                source.Play();
                ARAnchor anchor = anchorManager.AddAnchor(new Pose(touchPosition, Quaternion.identity));
                if (anchor == null) 
                    Debug.LogError("Error creating reference point");
                else 
                {
                    anchors.Add(anchor);
                    ARDebugManager.Instance.LogInfo($"Anchor created & total of {anchors.Count} anchor(s)");
                }

                Color selectedColor = modeManager.getSelectedColor();
                float selectedWidth = modeManager.getSelectedWidth();
                ARLine line = new ARLine(lineSettings, selectedColor, selectedWidth, spawnablePrefab, arCamera);
                Lines.Add(touch.fingerId, line);
                line.AddNewLineRenderer(transform, anchor, touchPosition);
                ARDebugManager.Instance.LogInfo($"Added LineRenderer at {touchPosition}");
            }
            else if(touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                Lines[touch.fingerId].AddPoint(touchPosition);
            }
            else if(touch.phase == TouchPhase.Ended)
            {
                Lines[0].UpdateBoxCollider();
                source.Stop();
                Lines.Remove(touch.fingerId);
            }
        }
    }

    void DrawOnMouse()
    {
        if(!CanDraw) {
            source.Stop();
            return;
        }

        Vector3 mousePosition = arCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, lineSettings.distanceFromCamera));

        if(Input.GetMouseButton(0))
        {
            if(Input.mousePosition.y < 350){
                return;
            }
            OnDraw?.Invoke();
            source.Play();
            if(Lines.Keys.Count == 0)
            {
                Color selectedColor = modeManager.getSelectedColor();
                float selectedWidth = modeManager.getSelectedWidth();
                ARLine line = new ARLine(lineSettings, selectedColor, selectedWidth, spawnablePrefab, arCamera);            
                Lines.Add(0, line);
                line.AddNewLineRenderer(transform, null, mousePosition);
                ARDebugManager.Instance.LogInfo($"Added LineRenderer at {mousePosition}");
            }
            else 
            {
                Lines[0].AddPoint(mousePosition);
            }
        }
        else if(Input.GetMouseButtonUp(0))
        {
            //Lines[0].UpdateBoxCollider();
            source.Stop();
            Lines.Remove(0);   
        }
    }

    GameObject[] GetAllLinesInScene()
    {
        return GameObject.FindGameObjectsWithTag("Line");
    }

    public void ClearLines()
    {
        GameObject[] lines = GetAllLinesInScene();
        foreach (GameObject currentLine in lines)
        {
            LineRenderer line = currentLine.GetComponent<LineRenderer>();
            Destroy(currentLine);
        }
    }
}