using UnityEngine;

namespace SpaceSimFramework
{
public class ScreenOutput : MonoBehaviour
{
    private bool _isShown = true;
    private string _logString;
    private int _numberOfMessages = 0;
    private int _maxNumberOfMessages = 10;
    private string[] _textLines;

    void OnEnable()
    {
        _textLines = new string[_maxNumberOfMessages];
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        _logString = logString;
        string newString = "\n [" + type + "] : " + _logString;
        /*if (type == LogType.Exception)
        {
            newString = "\n" + stackTrace;
            myLogQueue.Enqueue(newString);
        }*/
        _logString = string.Empty;
        if (_numberOfMessages < _maxNumberOfMessages)
        {
            _numberOfMessages++;
        }
        for (int i = _numberOfMessages - 1; i > 0; i--)
        {
            _textLines[i] = _textLines[i - 1];
            _logString += "\n" + _textLines[i];
        }
        _textLines[0] = newString;
        _logString += "\n" + _textLines[0];
    }

    public void ToggleDebug()
    {
        _isShown = !_isShown;
    }

    void OnGUI()
    {
        if (_isShown)
            GUILayout.Label(_logString);
    }
}
}