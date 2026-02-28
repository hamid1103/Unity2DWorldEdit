using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Utils;

public class WorldSelectButton : MonoBehaviour
{
    public string WorldId { get; set; }
    public string WorldName { get; set; }
    public string PreviewURL { get; set; }
    
    public GameManager gameManager;
    public GameObject EmptyHolder;
    public GameObject FilledHolder;
    public RawImage PreviewImageHolder;
    public Image PreviewTempImage;
    public TMPro.TMP_Text WorldNameDisplay;
    public TMPro.TMP_InputField WorldNameInput;

    //Gets called if there is world data for this button
    public void LoadButton()
    {
        if (string.IsNullOrEmpty(PreviewURL))
        {
            PreviewImageHolder.gameObject.SetActive(false);
        }
        else
        {
            //Load image
            StartCoroutine(LoadPreviewImage());
        }
        EmptyHolder.SetActive(false);
        FilledHolder.SetActive(true);
        WorldNameDisplay.text = WorldName;
        
    }

    IEnumerator LoadPreviewImage()
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(PreviewURL))
        {
            uwr.certificateHandler = new BypassCertificate();
            uwr.SetRequestHeader("Authorization", gameManager.GetSecurityToken());
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(uwr.error);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                PreviewImageHolder.texture = texture;
                PreviewImageHolder.gameObject.SetActive(true);
                PreviewTempImage.gameObject.SetActive(false);
            }
        }
    }

    public void CreateNewWorld()
    {
        Debug.Log("Create button pressed");
        gameManager.InitialCreateWorld(WorldNameInput.text);
    }
    
    public void LoadWorld()
    {
        gameManager.LoadWorld(WorldId);
    }

    public void DeleteWorld()
    {
        gameManager.DeleteWorld(WorldId);
    }


}
