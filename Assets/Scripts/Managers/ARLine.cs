using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARLine 
{
    private int positionCount = 0;

    private Vector3 prevPointDistance = Vector3.zero;
    
    private LineRenderer LineRenderer { get; set; }
    private LineSettings settings;
    private GameObject spawnablePrefab;

    private Camera arCamera;
    private Color selectedColor;
    private float selectedWidth;

    public ARLine(LineSettings settings, Color selectedColor, float selectedWidth, GameObject spawnablePrefab, Camera arCam)
    {
        this.settings = settings;
        this.selectedColor = selectedColor;
        this.selectedWidth = selectedWidth;
        this.spawnablePrefab = spawnablePrefab;
        this.arCamera = arCam;
    }

    public void AddPoint(Vector3 position)
    {
        if(prevPointDistance == null)
            prevPointDistance = position;

        if(prevPointDistance != null && Mathf.Abs(Vector3.Distance(prevPointDistance, position)) >= settings.minDistanceBeforeNewPoint)
        {
            
            prevPointDistance = position;
            positionCount++;

            LineRenderer.positionCount = positionCount;

            //kleine Boxen an jedem Punkt der Linie anbringen
            GameObject go = LineRenderer.gameObject;

            GameObject spawnedObject = GameObject.Instantiate(spawnablePrefab, position, Quaternion.identity) as GameObject;
            spawnedObject.transform.parent = go.transform;
            spawnedObject.transform.localScale = new Vector3(selectedWidth, selectedWidth, selectedWidth);
            /*
            BoxCollider boxCol = go.AddComponent<BoxCollider>(); 
            boxCol.center = position;
            boxCol.size = new Vector3(settings.minDistanceBeforeNewPoint, selectedWidth, selectedWidth);
            */

            // index 0 positionCount must be - 1
            LineRenderer.SetPosition(positionCount - 1, position);

            // applies simplification if reminder is 0
            if(LineRenderer.positionCount % settings.applySimplifyAfterPoints == 0 && settings.allowSimplification)
            {
                LineRenderer.Simplify(settings.tolerance);
            }
        }   
    }

    public void AddNewLineRenderer(Transform parent, ARAnchor anchor, Vector3 position)
    {
        positionCount = 2;
        GameObject go = new GameObject($"LineRenderer");
        
        go.transform.parent = anchor?.transform ?? parent;
        go.transform.position = position;
        go.tag = settings.lineTagName;
        
        LineRenderer goLineRenderer = go.AddComponent<LineRenderer>();
        goLineRenderer.startWidth = selectedWidth;
        goLineRenderer.endWidth = selectedWidth;

        ARDebugManager.Instance.LogInfo($"{selectedColor} drawing!");
        goLineRenderer.startColor = selectedColor;
        goLineRenderer.endColor = selectedColor;

        goLineRenderer.material = settings.defaultMaterial;
        goLineRenderer.material.color = selectedColor;
        goLineRenderer.useWorldSpace = true;
        goLineRenderer.positionCount = positionCount;

        goLineRenderer.numCornerVertices = settings.cornerVertices;
        goLineRenderer.numCapVertices = settings.endCapVertices;

        goLineRenderer.SetPosition(0, position);
        goLineRenderer.SetPosition(1, position);
        GameObject spawnedObject = GameObject.Instantiate(spawnablePrefab, position, Quaternion.identity) as GameObject;
        spawnedObject.transform.parent = go.transform;
        spawnedObject.transform.localScale = new Vector3(selectedWidth, selectedWidth, selectedWidth);

        LineRenderer = goLineRenderer;

        ARDebugManager.Instance.LogInfo($"New line renderer created");
    } 

    public void UpdateBoxCollider(){
        GameObject go = LineRenderer.gameObject;
        MeshCollider meshCollider = go.AddComponent<MeshCollider>();
        Mesh mesh = new Mesh();
        LineRenderer.BakeMesh(mesh, arCamera, true);
        meshCollider.sharedMesh = mesh;
        /*
        BoxCollider boxCol = go.AddComponent<BoxCollider>(); 
        boxCol.center = LineRenderer.bounds.center;
        boxCol.size = new Vector3(LineRenderer.bounds.size.x, LineRenderer.bounds.size.y, 0.1f);
        ARDebugManager.Instance.LogInfo($"Box Collider Size {boxCol.size}");
        ARDebugManager.Instance.LogInfo($"Box Collider Center {boxCol.center}");
        */
        //GameObject spawnedObject = GameObject.Instantiate(spawnablePrefab, LineRenderer.bounds.center, Quaternion.identity) as GameObject;
        //spawnedObject.transform.parent = go.transform;
        
    }

}