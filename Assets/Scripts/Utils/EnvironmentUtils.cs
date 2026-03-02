using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace Utils
{
    public class EnvironmentUtils
    {
        public static IEnumerator fetchWorldByID(string uuid, string authToken, bool? withObjects, Action<APIEnvironment2D> callback)
        {
            UnityWebRequest wwwFetch;
            wwwFetch = withObjects == true ? UnityWebRequest.Get($"{GameManager.BaseURL}/environments/{uuid}/2dobjects") : UnityWebRequest.Get($"https://localhost:7222/environments/{uuid}");
            wwwFetch.SetRequestHeader("Authorization", authToken);
            
            //Need to have this because of ssl/tsl certificate problems when running from terminal instead of rider/vs
            wwwFetch.certificateHandler = new BypassCertificate();
            
            Debug.Log(wwwFetch.url);
            
            yield return wwwFetch.SendWebRequest();

            if (wwwFetch.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(wwwFetch.error);
            }
            else
            {
                Debug.Log("Found world with id: " + uuid);
                Debug.Log(wwwFetch.downloadHandler);
                string jsonResponse = wwwFetch.downloadHandler.text;
                Debug.Log(wwwFetch.downloadHandler.text);
                APIEnvironment2D environment2D = JsonUtility.FromJson<APIEnvironment2D>(jsonResponse);
                callback(environment2D);
            }
        }

        public static IEnumerator deleteWorldByID(string uuid, string authToken, Action callback)
        {
            UnityWebRequest delete = UnityWebRequest.Delete($"{GameManager.BaseURL}/environments/{uuid}");
            delete.SetRequestHeader("Authorization", authToken);
            //Need to have this because of ssl certificate problems
            delete.certificateHandler = new BypassCertificate();

            yield return delete.SendWebRequest();
            
            if (delete.result == UnityWebRequest.Result.Success)
            {
                callback();
            }
            else
            {
                //Handle Errors
                Debug.Log(delete.error);
            }
        }

        public static IEnumerator updateWorldByID(string uuid, string authToken, APIEnvironment2D uploadData, Action callback)
        {
            APIUpdateData apiUpdateData = new();
            apiUpdateData.environmentId = uuid;
            apiUpdateData.environment2D = uploadData;
            string Data = JsonUtility.ToJson(apiUpdateData);
            UnityWebRequest wwwUpdate = UnityWebRequest.Put($"{GameManager.BaseURL}/environments", Data);
            Debug.Log(Data);
            wwwUpdate.SetRequestHeader("Authorization", authToken);
            //Need to have this because of ssl certificate problems
            wwwUpdate.certificateHandler = new BypassCertificate();
            wwwUpdate.SetRequestHeader("Content-Type", "application/json");
            yield return wwwUpdate.SendWebRequest();
            
            if (wwwUpdate.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(wwwUpdate.error);
            }
            else
            {
                callback();
            }
        }
    }

    [Serializable]
    public class APIEnvironment2D
    {
        [CanBeNull] public string id;
        [CanBeNull] public string name;
        [CanBeNull] public string previewURL;
        [CanBeNull] public string userId;
        [CanBeNull] public List<Object2D> objects;
    }

    public class APIUpdateData
    {
        public string environmentId;
        public APIEnvironment2D environment2D;
    }
    
}