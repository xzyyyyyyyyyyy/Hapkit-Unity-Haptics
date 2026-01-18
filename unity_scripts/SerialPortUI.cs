using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SerialPortUI : MonoBehaviour
{
    public SerialHapkitReader reader;
    public TMP_InputField portInput;
    public Button applyButton;

    void Start()
    {
        if (applyButton != null)
        {
            applyButton.onClick.AddListener(() =>
            {
                if (reader != null && portInput != null)
                {
                    reader.SetPort(portInput.text);
                }
            });
        }
    }
}
