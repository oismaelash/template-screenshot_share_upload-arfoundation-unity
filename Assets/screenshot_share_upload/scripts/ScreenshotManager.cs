using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NatSuite.Sharing;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

namespace IsmaelNascimento
{
    public class ScreenshotManager : MonoBehaviour
    {
        #region VARIABLES

        private static ScreenshotManager instance;
        public static ScreenshotManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new GameObject().AddComponent<ScreenshotManager>();

                return instance;
            }
        }

        [SerializeField] private List<GameObject> gameObjectsForDisable;
        public RawImage previewRawImage;
        public Texture2D screenshotTexture2D;

        #endregion

        #region MONOBEHAVIOUR_METHODS

        private void Start()
        {
            VerifyPermissions();
        }

        #endregion

        #region PUBLIC_METHODS

        public void OnButtonShareScreenshotClicked()
        {
            if (Application.isMobilePlatform)
            {
                SharePayload sharePayload = new SharePayload();
                sharePayload.AddImage(screenshotTexture2D);
                sharePayload.Commit();
            }
        }

        [ContextMenu("OnButtonScreenshotAndSaveClicked")]
        public void OnButtonScreenshotAndSaveClicked()
        {
            screenshotTexture2D = null;
            StartCoroutine(ScreenshotSystem_Coroutine(AfterScreenshot_Callback));
        }

        public string GetScreenshotTexture2DToBase64()
        {
            Texture2D tex = new Texture2D(Screen.width, Screen.height);
            string directory = Application.isEditor ? Directory.GetCurrentDirectory() : Application.persistentDataPath;
            byte[] image = File.ReadAllBytes($"{directory}/{GetNameFile()}");
            tex.LoadImage(image);
            tex.Apply();

            byte[] imageArray = tex.EncodeToPNG();
            string base64 = Convert.ToBase64String(imageArray);
            return base64;
        }

        #endregion

        #region PRIVATE_METHODS

        private void AfterScreenshot_Callback()
        {
            if (Application.isMobilePlatform)
            {
                var savePayload = new SavePayload();
                savePayload.AddImage(screenshotTexture2D);
                savePayload.Commit();
            }

            StartCoroutine(nameof(OpenPreview_Coroutine));
        }

        private void VerifyPermissions()
        {
#if PLATFORM_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Debug.Log("Ask permission for ExternalStorageWrite");
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            }

            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Debug.Log("Ask permission for Camera");
                Permission.RequestUserPermission(Permission.Camera);
            }
#endif
        }

        private int GetCount()
        {
            int count = PlayerPrefs.GetInt("photo-count");
            return count;
        }

        private string GetNameFile()
        {
            return $"ODS-{Constants.ODS_NUMBER}-foto-{GetCount()}.png";
        }

        #endregion

        #region COROUTINES

        private IEnumerator ScreenshotSystem_Coroutine(Action afterScreenshot = null)
        {
            gameObjectsForDisable.ForEach(gameObject => gameObject.SetActive(false));
            yield return new WaitForEndOfFrame();
            screenshotTexture2D = ScreenCapture.CaptureScreenshotAsTexture();
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(GetNameFile());
            yield return new WaitForSeconds(.1f);
            gameObjectsForDisable.ForEach(gameObject => gameObject.SetActive(true));
            yield return new WaitForSeconds(.1f);
            afterScreenshot?.Invoke();
        }

        private IEnumerator OpenPreview_Coroutine()
        {
            yield return new WaitForSeconds(.5f);
            previewRawImage.gameObject.SetActive(true);
            previewRawImage.texture = screenshotTexture2D;
        }

        #endregion
    }
}