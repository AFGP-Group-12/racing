using TMPro;
using UnityEngine;

public class WindowTimer : MonoBehaviour
{
    public bool startTimer = true;
    
    [SerializeField] float timerDuration = 10f;
    private float timer;

    [SerializeField] float goTimerDuration = 4f;
    private float goTimer;

    private GameObject windowObject; // This is found through find with tag so maybe switch it so that its faster

    private TextMeshProUGUI timerPlayerText;

    void Start()
    {
        timerPlayerText = GetComponent<TextMeshProUGUI>();
        timer = timerDuration;
        windowObject = GameObject.FindWithTag("Window");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(startTimer && timer > 0)
        {
            timer -= Time.deltaTime;
            timerPlayerText.text = "Time to Race Start: " + (Mathf.Round(timer * 10f) / 10f).ToString("0.0");
            if(timer <= 0)
            {
                windowObject.SetActive(false);
                timerPlayerText.text = "GO!!!";
                goTimer = goTimerDuration;
                timer = 0;
            }
        }
        if(goTimer > 0)
        {
            goTimer -= Time.deltaTime;
            if(goTimer <= 0)
            {
                timerPlayerText.text = "";
                goTimer = 0;
            }
        }
        
    }
    public void RestartTimer()
    {
        timer = timerDuration;
        startTimer = true;
    }
    public void SetObject(GameObject newWindow)
    {
        windowObject = newWindow;
    }
}
