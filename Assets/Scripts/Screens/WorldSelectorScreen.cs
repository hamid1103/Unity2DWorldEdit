using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Utils;

namespace DefaultNamespace.Screens
{
    public class WorldSelectorScreen : MonoBehaviour
    {
        public GameManager gameManager;
        public GameObject loadingScreen;
        public List<WorldSelectButton> worldButtons;
        
        //At this point, user should already be logged in.
        public void Start()
        {
            StartCoroutine(GetWorlds());
        }

        public IEnumerator GetWorlds()
        {
            loadingScreen.SetActive(true);
            UnityWebRequest getWorlds = UnityWebRequest.Get($"{GameManager.BaseURL}/environments");
            getWorlds.SetRequestHeader("Authorization", gameManager.GetSecurityToken());
            getWorlds.certificateHandler = new BypassCertificate();

            yield return getWorlds.SendWebRequest();

            if (getWorlds.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Got user worlds.");
                string jsonResponse = getWorlds.downloadHandler.text;
                Debug.Log(jsonResponse);
                APIWorldList worlds = JsonUtility.FromJson<APIWorldList>(jsonResponse);

                int worldIndex = 0;
                
                //Reset buttons to empty first
                foreach (WorldSelectButton btn in worldButtons)
                {
                    btn.EmptyHolder.SetActive(true);
                    btn.FilledHolder.SetActive(false);
                }
                
                foreach (var world in worlds.worlds)
                {
                    WorldSelectButton worldButton = worldButtons[worldIndex];
                    worldButton.PreviewURL = world.previewURL;
                    worldButton.WorldId = world.id;
                    worldButton.WorldName = world.name;
                    worldButton.LoadButton();
                    worldIndex++;
                }
                loadingScreen.SetActive(false);
                Debug.Log($"WorldIndex: {worldIndex}, Worlds count: {worlds.worlds.Count}");
                
            }
            else
            {
                Debug.Log("Sumn' went wrong when  requesting user worlds. Code" + getWorlds.responseCode);
                if (getWorlds.responseCode == 401)
                {
                    Debug.Log(gameManager.GetSecurityToken());
                }
                Debug.Log(getWorlds.result);
            }
        }
        
    }
}