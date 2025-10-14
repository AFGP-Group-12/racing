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

    public void AddMovementReply(Vector3 pos, Vector3 velocity, double rotation, MovementState state)
    {
        if (lastRecievedMovementFrame == 0) { targetMovement = pos; }
        originMovement = targetMovement;
        targetMovement = pos;

        playerObj.transform.position = originMovement;

        Vector3 look_direction = new Vector3((float)Math.Cos(rotation), 0, (float)Math.Sin(rotation));
        playerObj.transform.rotation = Quaternion.LookRotation(look_direction);

        this.velocity = velocity;
        spriteController.HandleStateChanged(state);

        int currentFrame = Time.frameCount;

        averageMovementDelayInFrames *= (1 - frameAdjustmentWeight);
        averageMovementDelayInFrames += (currentFrame - lastRecievedMovementFrame) * frameAdjustmentWeight;
        lastRecievedMovementFrame = currentFrame;
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
        playerGrappleLine.ForceSetEndPoint(playerObj.transform, otherPlayer.playerObj.transform);
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