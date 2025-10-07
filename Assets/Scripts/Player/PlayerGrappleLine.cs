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
        if (stateHandler.state == MovementState.grappleing)
        {
            lineRenderer.SetPosition(0, startPoint);
            lineRenderer.SetPosition(1, endPoint);
            lineRenderer.enabled = true;

        }
        else
        {
            lineRenderer.enabled = false;
        }
    }

    public void SetEndPoint(Vector3 endPoint)
    {
        this.endPoint = endPoint;
    }
}
