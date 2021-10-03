using System.Collections;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Crittr.UI
{
    [Serializable]
    public class ClearScreensEvent : UnityEvent { };

    [HelpURL("https://docs.crittr.co/#/unity-sdk")]
    public class CrittrUI : MonoBehaviour, ICancelHandler
    {
        [Header("Panels")]
        [Tooltip("The main panel of the report window.")]
        public GameObject panel;
        [Tooltip("")]
        public GameObject formScreen;
        [Tooltip("When the report has been sent successfully, this GameObject will be enabled.")]
        public GameObject successScreen;
        [Tooltip("When the report can't be sent, this GameObject will be enabled.")]
        public GameObject failureScreen;
        [Header("Configuration")]
        public CrittrSDK _crittrSDK;
        public GameObject successQRCode;

        [SerializeField]
        public ClearScreensEvent OnClearedScreens;


        [NonSerialized]
        private Report _currentReport = null;
        private string _reportLocation;
        private RawImage _qrCodeRawImage;
        //If you want to use TextMeshPro, add TMP_ before Dropdown.
        private Dropdown _categoryDropdown;

        void Awake()
        {
            //Just as a double-check, set the current report to null.
            _currentReport = null;
            //Get the raw image of the gameobject
            _qrCodeRawImage = successQRCode.GetComponent<RawImage>();

            //If the SDK was not sent, then try to find it automatically
            if(_crittrSDK == null)
                _crittrSDK = FindObjectOfType<CrittrSDK>();
            //If the SDK is still null (I know for sure there's a better way to do this) then disable the script since it might cause problems.
            if (_crittrSDK == null)
            {
                Debug.LogError("The CrittrSDK was not present in the scene!");
                this.enabled = false;
            }
                

            //Get the category chooser from the formscreen
            _categoryDropdown = formScreen.GetComponentInChildren<Dropdown>();
            _categoryDropdown.onValueChanged.AddListener(delegate {
                HandleDropdownChange(_categoryDropdown);
            });
        }

        public void HandleShowForm(Report report)
        {
            //Set info
            _currentReport = report;
            _currentReport.category = _categoryDropdown.options[_categoryDropdown.value].text;
            //Screenshot
            StartCoroutine(ScreenShotAndDisplayScreen(report));
        }

        public void HandleTitleChange(string value)
        {
            if (_currentReport == null) return;
            _currentReport.title = value;
        }

        public void HandleDescriptionChange(string value)
        {
            if (_currentReport == null) return;
            _currentReport.description = value;
        }

        private void HandleDropdownChange(Dropdown change)
        {
            if (_currentReport == null) return;
            _currentReport.category = change.options[change.value].text;
        }

        private IEnumerator ScreenShotAndDisplayScreen(Report report)
        {
            // We want to capture the screen before we display the report screen.
            yield return CrittrRuntime.Instance.CaptureScreenshot(report);
            ShowScreen(formScreen);
        }

        public void HandleSendReport()
        {
            if (_currentReport == null) return;
            _crittrSDK.SendReport(_currentReport, false);
            ClearScreens();
        }

        public void HandleShowSuccess(Report report, SuccessResponse successResponse)
        {
            if (report.category != "error")
            {
                StartCoroutine(SetQRCodeImage(successResponse.qr_code_location));
                //Show the success screen
                ShowScreen(successScreen);
                _reportLocation = successResponse.location;
            }
        }

        private IEnumerator SetQRCodeImage(string qrCodeLocation)
        {
            UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(qrCodeLocation);
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log(request.error);
                yield return null;
            }

            _qrCodeRawImage.texture = ((UnityEngine.Networking.DownloadHandlerTexture)request.downloadHandler).texture;
        }

        public void HandleReportLinkClick()
        {
            Application.OpenURL(_reportLocation);
        }

        public void HandleShowFailure(Report _, ErrorResponse errorResponse)
        {
            var text = failureScreen.GetComponentInChildren<Text>();
            if (errorResponse.errors.Length > 0)
            {
                text.text = errorResponse.errors[0].message;
            }
            ShowScreen(failureScreen);
        }

        private void ShowScreen(GameObject screen)
        {
            EventSystem.current.SetSelectedGameObject(this.gameObject);
            panel.SetActive(true);
            screen.SetActive(true);
        }

        public void ClearScreens()
        {
            // Disable all screens
            panel.SetActive(false);
            formScreen.SetActive(false);
            successScreen.SetActive(false);
            failureScreen.SetActive(false);
            _currentReport = null;
            _qrCodeRawImage.texture = null;
            // Trigger event.
            OnClearedScreens?.Invoke();
        }

        public void OnCancel(BaseEventData _)
        {
            ClearScreens();
        }
    }
}
