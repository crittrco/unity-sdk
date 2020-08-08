using System.Collections.Generic;
using UnityEngine;
using Crittr;
using System;
using UnityEngine.Events;

// Bug in several Unity distributions. Extending and serializing the UnityEvent
// class allows it to show up in the editor GUI.
[Serializable]
public class CrittrEventReport : UnityEvent<Report> { }
[Serializable]
public class CrittrEventReportSuccess : UnityEvent<Report, SuccessResponse> { }
[Serializable]
public class CrittrEventReportFailure : UnityEvent<Report, ErrorResponse> { }


public class CrittrSDK : MonoBehaviour
{
    [SerializeField]
    public Dictionary<string, string> tags = new Dictionary<string, string>();
    [SerializeField]
    public Dictionary<string, object> extras = new Dictionary<string, object>();

    [SerializeField]
    [Header("Connection URI with API Key")]
    public string ConnectionURI;
    public bool isVerbose = true;

    [Header("Automatically sends reports on exceptions and errors", order=1)]
    public bool sendAutomaticReports = false;

    public int maxLogs = 100;

    [Header("Keyboard key to trigger manual report", order=1)]
    public KeyCode keyboardInputTrigger = KeyCode.F8;

    [Header("Controller keys to trigger manual report", order=1)]
    public List<KeyCode> controllerInputTriggers = new List<KeyCode> { KeyCode.JoystickButton0, KeyCode.JoystickButton4, KeyCode.JoystickButton5 };

    [SerializeField]
    public CrittrEventReport OnReportSend;
    [SerializeField]
    public CrittrEventReportSuccess OnReportSuccess;
    [SerializeField]
    public CrittrEventReportFailure OnReportFailure;
    private bool isSending = false;


    void Awake()
    {
        CrittrRuntime.Instance.SetConnectionURI(ConnectionURI);
        CrittrRuntime.Instance.SetMaxLogs(maxLogs);
        CrittrRuntime.Instance.SetIsVerboseMode(isVerbose);

        CrittrRuntime.Instance.OnReportSend += HandleReportSend;
        CrittrRuntime.Instance.OnSendReportSuccess += HandleReportSuccess;
        CrittrRuntime.Instance.OnSendReportFailure += HandleReportFailure;
        CrittrRuntime.Instance.OnExceptionError += HandleExceptionError;
    }

    public virtual void PrepareReport()
    {
        isSending = true;
        var report = CrittrRuntime.Instance.NewReport();
        SendReport(report, true);
    }

    public void SendReport(Report report, bool withScreenshot) { 
        StartCoroutine(CrittrRuntime.Instance.SendReport(report, withScreenshot));
    }

    public virtual void HandleReportSend(Report report)
    {
        if (isVerbose)
        {
            Debug.Log("After constructing the report request");
        }
        // A good place to pause the game...
        OnReportSend?.Invoke(report);
    }

    public virtual void HandleReportSuccess(Report report, SuccessResponse response)
    {
        isSending = false;
        if (isVerbose)
        {
            Debug.Log("The report was sent successfully: " + response.location);
        }
        Application.OpenURL(response.location);
        OnReportSuccess?.Invoke(report, response);
    }

    public virtual void HandleReportFailure(Report report, ErrorResponse error)
    {
        isSending = false;
        if (isVerbose)
        {
            Debug.Log("The report was not sent: " + error.status);
        }
        OnReportFailure?.Invoke(report, error);
    }

    public virtual void HandleExceptionError(string message, string stackTrace)
    {
        if (!sendAutomaticReports) return;

        var report = CrittrRuntime.Instance.NewReport();
        report.category = "error";
        report.title = message;
        report.description = stackTrace;
        SendReport(report, true);

        if (isVerbose)
        {
            Debug.Log("Sending automatic exception");
        }
    }

    public virtual void Update()
    {
        if (isSending) {
            return;
        }

        if (Input.GetKeyDown(keyboardInputTrigger))
        {
            //PrepareReport();
            Debug.Log("Pressed keyboard key");
        }

        var hasPressedControllerKeys = true;
        foreach (KeyCode input in controllerInputTriggers)
        {
            var isKeyPressed = Input.GetKey(input);
            hasPressedControllerKeys = hasPressedControllerKeys && isKeyPressed;
        }
        if (hasPressedControllerKeys)
        {
            //PrepareReport();
            Debug.Log("Pressed controller key");
        }

    }
}
