using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Represents the value of the debug message.
/// </summary>
public enum TextState
{
    DEBUG,
    WARNING,
    ERROR,
    SUCCSESS
}

public class LoggerScript : MonoBehaviour
{
    //Inspector Variable
    [SerializeField]
    [Tooltip("The textfield to log the messages created by the backend.")]
    public TextMeshProUGUI debugLogger;

    //
    private int textLineCounter = 0;
    private static readonly int MAXLINECOUNT = 15;

    /// <summary>
    ///  This Method is getting called by any script having to tell the user debug informations. 
    /// </summary>
    /// <param name="message">the actual msg that gets displayed</param>
    /// <param name="textState">The value of the msg. Default is the debug mode.</param>
    internal void Log(string message, TextState textState=TextState.DEBUG)
    {
        int fontSizeDefault = 40;
        int fontSizeHuge = 55;
        switch (textState)
        {
            case TextState.DEBUG:
                debugLogger.color = Color.white;
                debugLogger.fontSize = fontSizeDefault;
                break;
            case TextState.WARNING:
                debugLogger.color = Color.yellow;
                textLineCounter += MAXLINECOUNT; //bypasses that the whole TMP gets this color
                debugLogger.fontSize = fontSizeHuge;
                break;
            case TextState.ERROR:
                debugLogger.color = Color.red;
                textLineCounter += MAXLINECOUNT; //bypasses that the whole TMP gets this color
                debugLogger.fontSize = fontSizeHuge;
                break;
            case TextState.SUCCSESS:
                debugLogger.color = Color.green;
                textLineCounter += MAXLINECOUNT; //bypasses that the whole TMP gets this color
                debugLogger.fontSize = fontSizeHuge;
                break;
        }

        textLineCounter++;
        if (textLineCounter > MAXLINECOUNT)
        {
            debugLogger.text = $"\n>> {message}\n";
            textLineCounter = 1;
        }
        else
        {
            debugLogger.text += $">> {message}\n";
        }
    }

}
