using UnityEngine;

public class FpsDisplay : MonoBehaviour
{
    [SerializeField] private float updateInterval = 0.5f; // How often to update FPS
    private float timeSinceUpdate = 0f;
    private int frames = 0;
    private float fps = 0f;

    void Update()
    {
        frames++;
        timeSinceUpdate += Time.unscaledDeltaTime; // Use unscaled time so FPS not affected by Time.timeScale

        if (timeSinceUpdate >= updateInterval)
        {
            fps = frames / timeSinceUpdate;
            frames = 0;
            timeSinceUpdate = 0f;
        }
    }

    void OnGUI()
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = 24;
        style.normal.textColor = Color.white;
        GUI.Label(new Rect(10, 10, 200, 50), $"FPS: {fps:F1}", style);
    }
}
