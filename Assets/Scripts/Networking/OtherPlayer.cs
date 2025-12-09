using TMPro;
using System;
using UnityEngine;


public class OtherPlayer
{
    int player_id;
    GameObject playerObj;
    Canvas canvas;
    SpriteController spriteController;
    PlayerGrappleLine playerGrappleLine;

    private Vector3 originMovement;
    private Vector3 targetMovement;
    private Vector3 velocity;

    private int lastRecievedMovementFrame = 0;
    private float averageMovementDelayInFrames = 15;

    private const float frameAdjustmentWeight = 0.3f;
    public bool isGhost = false;

    public OtherPlayer(GameObject prefab, string name, Camera camera, int id)
    {
        playerObj = UnityEngine.Object.Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
        canvas = playerObj.GetComponentInChildren<Canvas>();
        spriteController = playerObj.GetComponentInChildren<SpriteController>();
        canvas.worldCamera = camera;

        TextMeshProUGUI text = playerObj.GetComponentInChildren<TextMeshProUGUI>();
        playerGrappleLine = playerObj.GetComponent<PlayerGrappleLine>();
        text.SetText(name);

        spriteController.viewer = camera.transform;
        player_id = id;

        OtherPlayerIdHolder idHolder = playerObj.GetComponent<OtherPlayerIdHolder>();
        idHolder.Id = id;
    }

    public void resetBot(GameObject prefab, Camera camera)
    {
        if (playerObj != null)
        {
            UnityEngine.Object.Destroy(playerObj);
        }

        playerObj = UnityEngine.Object.Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
        canvas = playerObj.GetComponentInChildren<Canvas>();
        spriteController = playerObj.GetComponentInChildren<SpriteController>();
        canvas.worldCamera = camera;

        spriteController.viewer = camera.transform;

        OtherPlayerIdHolder idHolder = playerObj.GetComponent<OtherPlayerIdHolder>();
        idHolder.Id = player_id;

        if (isGhost)
        {
            turnGhost();
        }
    }

    public void turnGhost()
    {
        playerObj.GetComponentInChildren<GhostContext>().gameObject.SetActive(true);
        playerObj.GetComponentInChildren<SpriteController>().gameObject.SetActive(false);
    }

    public bool AddMovementReply(Vector3 pos, Vector3 velocity, double rotation, MovementState state)
    {
        originMovement = lastRecievedMovementFrame == 0 ? pos : targetMovement;
        targetMovement = pos;

        if (playerObj == null) { return false; }

        playerObj.transform.position = originMovement;

        Vector3 look_direction = new Vector3((float)Math.Cos(rotation), 0, (float)Math.Sin(rotation));
        playerObj.transform.rotation = Quaternion.LookRotation(look_direction);

        this.velocity = velocity;
        spriteController.HandleStateChanged(state);

        int currentFrame = Time.frameCount;
        Debug.Log("Movement delay frames: " + (currentFrame - lastRecievedMovementFrame));

        averageMovementDelayInFrames = averageMovementDelayInFrames * (1 - frameAdjustmentWeight) + (currentFrame - lastRecievedMovementFrame) * frameAdjustmentWeight;
        lastRecievedMovementFrame = currentFrame;
        return true;
    }

    public void Update(bool doPrediction)
    {
        if (playerObj == null) { return; }

        Vector3 origin;
        Vector3 target;

        if (doPrediction)
        {
            origin = targetMovement;
            target = CalculatePrediction();
        }
        else
        {
            target = targetMovement;
            origin = originMovement;
        }

        Vector3 interpolation = (target - origin) * CalculateInterpolation();

        playerObj.transform.position = origin + interpolation;
        canvas.transform.LookAt(canvas.worldCamera.transform);
    }

    public void Grapple(Vector3 targetPoint)
    {
        playerGrappleLine.ForceSetEndPoint(playerObj.transform, targetPoint);
    }
    public void Grapple(OtherPlayer otherPlayer)
    {
        Grapple(otherPlayer.playerObj.transform);
    }
    public void Grapple(Transform transform)
    {
        playerGrappleLine.ForceSetEndPoint(playerObj.transform, transform);
    }
    public void EndGrapple()
    {
        playerGrappleLine.DisableSetPoint();
    }

    private Vector3 CalculatePrediction()
    {
        return targetMovement + (Time.deltaTime * averageMovementDelayInFrames * velocity);
    }

    private float CalculateInterpolation()
    {
        return (Time.frameCount - lastRecievedMovementFrame) / averageMovementDelayInFrames;
    }

    public void Destroy()
    {
        if (playerObj != null)
        {
            UnityEngine.Object.Destroy(playerObj);
            playerObj = null;
            canvas = null;
        }
    }

};