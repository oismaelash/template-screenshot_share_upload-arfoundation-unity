using System;
using Proyecto26;
using UnityEngine;

namespace IsmaelNascimento
{
    public class ApiManager : MonoBehaviour
    {
        #region VARIABLES

        private static ApiManager instance;
        public static ApiManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new GameObject().AddComponent<ApiManager>();

                return instance;
            }
        }

        #endregion

        #region PUBLIC_METHODS

        public void UploadFileToS3(string payload, Action<string, string> result)
        {
            RestClient.Post(Constants.ENDPOINT_UPLOAD, payload, (err, res) =>
            {
                if (err != null)
                {
                    result?.Invoke(err.Message, null);
                }
                else
                {
                    result?.Invoke(null, res.Text);
                }
            });
        }

        #endregion
    }
}