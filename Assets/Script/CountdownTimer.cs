using UnityEngine;
using UnityEngine.UI;  // Required for UI

public class CountdownTimer : MonoBehaviour
{
    public Text timerText;  // UI Text element to show the timer
    private float countdownTime;  // The random time for the countdown (between 30 and 50 seconds)
    private bool isTimerRunning = true;  // Whether the timer is running or not

    void Start()
    {
        // Randomly set the countdown time between 30 and 50 seconds
        countdownTime = Random.Range(60f, 90f);

        // Update the UI with the starting countdown time
        UpdateTimerText();
    }

    void Update()
    {
        if (isTimerRunning)
        {
            // Decrease the timer by the time passed this frame
            countdownTime -= Time.deltaTime;

            // Update the UI text to show the remaining time
            UpdateTimerText();

            // If the countdown reaches 0, stop the game
            if (countdownTime <= 0)
            {
                countdownTime = 0;  // Ensure timer doesn't go negative
                StopGame();
            }
        }
    }

    // Update the UI text with the current timer value
    private void UpdateTimerText()
    {
        // Format the timer to display minutes:seconds
        timerText.text = "Time Left: " + Mathf.Ceil(countdownTime).ToString() + "s";
    }

    // Stop the game when the timer reaches 0
    private void StopGame()
    {
        isTimerRunning = false;
        timerText.text = "Time's Up!";  // Display message when time's up

        // Optionally stop all game activities (e.g., freeze time, disable player, etc.)
        Time.timeScale = 0f;  // Stop all game actions by freezing time
        Debug.Log("Game Over! Everything is stopped.");
    }
}
