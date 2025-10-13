using UnityEngine;

public class SpawnedPlatform : MonoBehaviour
{
    private float minScale = 0f;

    private float maxScale = 7f;

    private float increaseIncrement = 0.05f;

    private float decreaseIncrement = 0.002f;


    private bool reachedMaxScale = false;

    // Update is called once per frame
    void Update()
    {
        if (increaseIncrement == 0)
        {
            Debug.Log("increments not set");
            return;
        }

        if (!reachedMaxScale)
        {
            IncreaseScale();
        }
        else
        {
            DecreaseScale();
        }
    }

    public void SetVariables(float minScale, float maxScale, float increaseIncrement,float decreaseIncrement)
    {
        this.minScale = minScale;
        this.maxScale = maxScale;

        gameObject.transform.localScale = new Vector3(minScale, gameObject.transform.localScale.y, minScale);

        this.increaseIncrement = increaseIncrement;
        this.decreaseIncrement = decreaseIncrement;
    }

    void IncreaseScale()
    {
        gameObject.transform.localScale += new Vector3(increaseIncrement, 0, increaseIncrement);
        if (gameObject.transform.localScale.x >= maxScale)
        {
            reachedMaxScale = true; 
        }
    }

    void DecreaseScale()
    {
        gameObject.transform.localScale -= new Vector3(decreaseIncrement, 0, decreaseIncrement);
        if (gameObject.transform.localScale.x <= minScale)
        {
            Destroy(gameObject);
        }
    }
    

}
