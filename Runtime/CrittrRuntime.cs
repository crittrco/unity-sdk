using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.Build.Reporting;
using UnityEditor.MemoryProfiler;
using UnityEditor.VersionControl;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using Crittr.Editor;

namespace Crittr {
    [Serializable]
    public struct RefAndMethod {
        public string href;
        public string method;
    }
    [Serializable]
    public struct SuccessLinks {
        public RefAndMethod get;
        public RefAndMethod update;
        public RefAndMethod upload_attachment;
    }
    [Serializable]
    public struct SuccessResponse {
        public string location;
        public SuccessLinks links;
    }
    
    [Serializable]
    public struct ErrorMessage {
        public string message;
        public int status;
        public string type;
        public string field;

    }
    [Serializable]
    public struct ErrorResponse {
        public int status;
        public ErrorMessage[] errors;
    }
    public class APIProperties {
        public string scheme;
        public string host;
        public int port;
        public int projectId;
        public string apiKey;

        public UriBuilder BaseURI { get { return new UriBuilder(scheme, host, port); } }

        public APIProperties(string connectionURI) {
            Uri uri = new Uri(connectionURI);
            var splitPath = uri.LocalPath.Split('/');
            var _projectId = Int32.Parse(splitPath[splitPath.Length - 2]);

            scheme = uri.Scheme;
            host = uri.Host;
            port = uri.Port;
            projectId = _projectId;
            apiKey = uri.UserInfo;
        }
    }


    public class CrittrRuntime {
        private static readonly Lazy<CrittrRuntime> _instance = new Lazy<CrittrRuntime>(() => new CrittrRuntime());
        public static CrittrRuntime Instance { get { return _instance.Value; } }
        private bool _isInitialized;
        private List<string> _logs = new List<string>();
        private List<string> _attachments = new List<string>();
        private CrittrConfig _config;


        public event Action onSendReportRequest;
        public event Action<SuccessResponse> onSendReportSuccess;
        public event Action<ErrorResponse> onSendReportFailure;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnLoadMethod()
        {
            CrittrRuntime.Instance.Init();
            Application.quitting += () => CrittrRuntime.Instance.Destroy();
        }

        public void Init() {
            if (_isInitialized) {
                return;
            }

            _config = CrittrConfig.Instance;
            _isInitialized = true;
            _logs = new List<string>();
            _attachments = new List<string>();
            Application.logMessageReceived += _handleLogs;
        }

        public void Destroy() {
            if (!_isInitialized) {
                return;
            }

            _config = null;
            _isInitialized = false;
            Application.logMessageReceived -= _handleLogs;
            _logs = new List<string>();
        }

        public IEnumerator CaptureScreenshot() { 
            yield return new WaitForEndOfFrame();
            var timeNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string filename = $"Crittr_Screenshot_{timeNow}.png";
             #if UNITY_EDITOR
            var path = Application.dataPath + "/../" + filename;
#else
            var path = Application.dataPath + "/" + filename;
#endif
            ScreenCapture.CaptureScreenshot(filename);
            AddAttachment(path);
        }

        private void _handleLogs(string message, string stackTrace, LogType logType) {
            if (!_isInitialized) {
                // TODO: Add error logging.
                return;
            }
            _logs.Add(message);
        }

        public void AddAttachment(string path) {
            _attachments.Add(path);
        }

        public Report NewReport() {
            return new Crittr.Report();
        }

        public AsyncOperation SendReport(Report report) {
            // TODO: Set max length of logs.
            onSendReportRequest?.Invoke();

            report.SetLogs(_logs);
            APIProperties apiProperties = new APIProperties(_config.ConnectionURI);
            var builder = apiProperties.BaseURI;
            builder.Path = $"/projects/{apiProperties.projectId}/reports";
            var www = new UnityWebRequest(builder.ToString()) { method = "POST" };
            www.SetRequestHeader("X-Crittr-Client-Key", apiProperties.apiKey);

            byte[] rawReport = Encoding.UTF8.GetBytes(report.ToJson());
            var uploadHandler = new UploadHandlerRaw(rawReport);
            www.uploadHandler = uploadHandler;
            www.SetRequestHeader("Content-Type", "application/json");
            www.downloadHandler = new DownloadHandlerBuffer();
            UnityWebRequestAsyncOperation wwwOp = www.SendWebRequest();
            wwwOp.completed += _handleSendMessageCompleted;
            return wwwOp;
        }


        private void _handleSendMessageCompleted(AsyncOperation op) {
            UnityWebRequestAsyncOperation wwwOp = (UnityWebRequestAsyncOperation) op;
            var www = wwwOp.webRequest;
            if (www.isNetworkError || www.isHttpError || www.responseCode != 200)
            {
                if (_config.Verbose) {
                    UnityEngine.Debug.LogError("Error sending report: " + www.downloadHandler.text);
                }
                ErrorResponse errorResponse = JsonUtility.FromJson<ErrorResponse>(www.downloadHandler.text);
                onSendReportFailure?.Invoke(errorResponse);
                return;
            }

            var response = JsonUtility.FromJson<SuccessResponse>(www.downloadHandler.text);
            onSendReportSuccess?.Invoke(response);
            _uploadAttachments(response.links.upload_attachment);
        }

        private void _uploadAttachments(RefAndMethod uploadLink) {
            // TODO: Some kind of wait group to log when all the attachments succeed
            // and remove their references.
            foreach (var filename in _attachments) {
                UnityWebRequestAsyncOperation wwwOp = _uploadAttachment(filename, uploadLink);
                // TODO: Add logging if failed.
                wwwOp.completed += (AsyncOperation op) => {
                    var uwrOp = (UnityWebRequestAsyncOperation)op;
                    var www = uwrOp.webRequest;
                    if (_config.Verbose) {
                        UnityEngine.Debug.LogWarning($"Upload attachment response: {www.downloadHandler.text}");
                    }
                    
                };
            }

            // Remove attachments.
            _attachments = new List<string>();
        }

        private UnityWebRequestAsyncOperation _uploadAttachment(string path, RefAndMethod uploadLink) {
            var filename = Path.GetFileName(path);
            byte[] data = File.ReadAllBytes(path);
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormFileSection("attachment_file", data, filename, ""));

            APIProperties apiProperties = new APIProperties(_config.ConnectionURI);
            var url = apiProperties.BaseURI + uploadLink.href;
            var www = UnityWebRequest.Post(url, formData);
            // 60 Seconds timeout.
            www.timeout = 60;
            www.downloadHandler = new DownloadHandlerBuffer();
            UnityWebRequestAsyncOperation wwwOp = www.SendWebRequest();
            return wwwOp;
        }
    }
}
