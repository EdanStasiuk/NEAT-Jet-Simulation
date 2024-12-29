using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeController : MonoBehaviour
{
    public TMP_InputField timeInputField;
    public int minTimeScale = 0;
    public int maxTimeScale = 20;

    void Start()
    {
        if (timeInputField != null)
        {
            timeInputField.text = Time.timeScale.ToString("F0");
            timeInputField.onEndEdit.AddListener(OnTimeScaleInput);
        }
    }

    private void OnTimeScaleInput(string input)
    {
        int newTimeScale;
        if (int.TryParse(input, out newTimeScale))
        {
            Time.timeScale = Mathf.Clamp(newTimeScale, minTimeScale, maxTimeScale);
        }
        else
        {
            Debug.LogError("Invalid input for time scale. Please enter a valid number.");
        }
    }
}
