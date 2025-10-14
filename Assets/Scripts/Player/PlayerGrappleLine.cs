using UnityEngine;

public class PlayerGrappleLine : MonoBehaviour
{
    private PlayerContext contextScript;

    private PlayerStateHandler stateHandler;

    bool isGrappling;

    public LineRenderer lineRenderer;

    public Transform grappleStartPoint;

    Vector3 startPoint;

    Vector3 endPoint;

    Vector3 currentEndPoint;

    public bool isForceingLine;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        contextScript = GetComponent<PlayerContext>();
        if (contextScript != null) stateHandler = contextScript.stateHandler;
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        startPoint = transform.position;
        if (isForceingLine || (stateHandler != null && stateHandler.state == MovementState.grappling))
        {
            currentEndPoint = Vector3.Lerp(currentEndPoint, endPoint, Time.deltaTime * 8f);

            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, currentEndPoint);
            lineRenderer.enabled = true;
        }
        else
        {
            currentEndPoint = startPoint;
            if (currentEndPoint != null) lineRenderer.SetPosition(1, currentEndPoint);
            isForceingLine = false;
            lineRenderer.enabled = false;
        }
    }

    public void SetEndPoint(Vector3 endPoint)
    {
        this.endPoint = endPoint;
    }

    public void ForceSetEndPoint(Transform playerTransform, Vector3 endPoint)
    {
        grappleStartPoint = playerTransform;
        isForceingLine = true;

        SetEndPoint(endPoint);
    }
    
    public void DisableSetPoint()
    {
        isForceingLine = false;
    }


}