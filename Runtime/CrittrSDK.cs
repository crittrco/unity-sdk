using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Crittr;
using Crittr.Editor;

public class CrittrSDK : MonoBehaviour
{
    private CrittrConfig _config;
    [SerializeField]
    public Dictionary<string, string> tags = new Dictionary<string, string>();
    [SerializeField]
    public Dictionary<string, object> extras = new Dictionary<string, object>();

    // Start is called before the first frame update
    void Awake()
    {
        _config = CrittrConfig.Instance;
        CrittrRuntime.Instance.onSendReportRequest += _handleSendReportRequest;
        CrittrRuntime.Instance.onSendReportSuccess += _handleSendReportSuccess;
        CrittrRuntime.Instance.onSendReportFailure += _handleSendReportFailure;
        
    }

    // Update is called once per frame
    private void _sendReport() { 
        StartCoroutine(CrittrRuntime.Instance.CaptureScreenshot());
        var report = CrittrRuntime.Instance.NewReport();
        report.SetTags(tags);
        report.SetExtras(extras);

        CrittrRuntime.Instance.SendReport(report);
    }

    private void _handleSendReportRequest() {
        if (_config.Verbose) {
            Debug.Log("Sending the report");
        }
    }

    private void _handleSendReportSuccess(SuccessResponse response) {
        if (_config.Verbose) {
            Debug.Log("The report was sent successfully: " + response.location);
        }
        Application.OpenURL(response.location);
    }

    private void _handleSendReportFailure(ErrorResponse error) { 
        if (_config.Verbose) {
            Debug.Log("The report was not sent: " + error.status);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(_config.defaultKey)) {
            _sendReport();
        }
        
    }
}
