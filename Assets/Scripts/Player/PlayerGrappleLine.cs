using UnityEngine;

public class PlayerGrappleLine : MonoBehaviour
{
    private PlayerContext contextScript;

    private PlayerStateHandler stateHandler;

    bool isGrappling;

    LineRenderer lineRenderer;

    public Transform grappleStartPoint;

    Vector3 startPoint;

    Vector3 endPoint;

    Vector3 currentEndPoint;

    bool isForceingLine;

    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        contextScript = GetComponent<PlayerContext>();
        stateHandler = contextScript.stateHandler;
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        startPoint = grappleStartPoint.position;
        if (isForceingLine || stateHandler.state == MovementState.grappling)
        {
            currentEndPoint = Vector3.Lerp(currentEndPoint, endPoint, Time.deltaTime * 8f);

            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, currentEndPoint);
            lineRenderer.enabled = true;

        }
        else
        {
            currentEndPoint = startPoint;
            lineRenderer.SetPosition(1, currentEndPoint);
            isForceingLine = false;
            lineRenderer.enabled = false;
        }
    }

    public void SetEndPoint(Vector3 endPoint)
    {
        this.endPoint = endPoint;
    }

    public void ForceSetEndPoint(Vector3 endPoint)
    {
        isForceingLine = true;

        SetEndPoint(endPoint);
    }
    
    public void DisableSetPoint()
    {
        isForceingLine = false;
    }


}