using Newtonsoft.Json;
using System;
using UnityEngine;

namespace IsmaelNascimento
{
    public class AppManager : MonoBehaviour
    {
        #region VARIABLES

        private static AppManager instance;
        public static AppManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new GameObject().AddComponent<AppManager>();

                return instance;
            }
        }

        [Tooltip("directory/filename.extesion")]
        [SerializeField] private GameObject uploadingPanel;

        #endregion

        public void OnButtonUploadAwsS3Clicked()
        {
            Debug.Log("Start upload");
            uploadingPanel.SetActive(true);
            Debug.Log($"pathFileOnBucket: {GetNameFile()}");

            RequestUploadModel requestUploadModel = new RequestUploadModel
            {
                pathFileOnBucket = GetNameFile(),
                base64 = ScreenshotManager.Instance.GetScreenshotTexture2DToBase64()
            };

            string payload = JsonConvert.SerializeObject(requestUploadModel);

            Debug.Log($"Payload: {payload}");

            ApiManager.Instance.UploadFileToS3(payload, (error, response) =>
            {
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError(error);
                    uploadingPanel.SetActive(false);
                    return;
                }

                uploadingPanel.SetActive(false);
                PlayerPrefs.SetInt("photo-count", GetCount() + 1);
                Debug.Log("End upload");
            });
        }

        private int GetCount()
        {
            return PlayerPrefs.GetInt("photo-count");
        }

        private string GetNameFile()
        {
            string date = DateTime.Now.ToString("dd-MM-yyyy");
            string hour = DateTime.Now.ToString("hh:mm");
            return $"ODS-{Constants.ODS_NUMBER}-photo-{GetCount()}-{date}-{hour}.png";
        }
    }
}