using System;
using System.Collections;
using DefaultNamespace;
using DefaultNamespace.Screens;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Utils;

public class LoginScreen : MonoBehaviour
{
    public GameManager gameManager;
    public TMP_InputField usernameField;
    public TMP_InputField passwordField;
    public TMP_Text messageText;
    public WorldSelectorScreen worldSelectorScreen;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (messageText.enabled)
        {
            messageText.enabled = false;
        }
    }

    private void Update()
    {
        if (string.IsNullOrEmpty(messageText.text))
        {
            
        }
        else
        {
            
        }
    }

    IEnumerator Register()
    {
        //Make register request
        LoginData ld = new LoginData();
        ld.email = usernameField.text;
        ld.password = passwordField.text;
        string jsonData = JsonUtility.ToJson(ld);
        
        UnityWebRequest wwwReg = UnityWebRequest.Post("https://localhost:7222/account/register", jsonData,"application/json");
        wwwReg.certificateHandler = new BypassCertificate();
        yield return wwwReg.SendWebRequest();

        if (wwwReg.result == UnityWebRequest.Result.Success)
        {
            //if successful 
            yield return StartCoroutine(Login());
        }
        else
        {
            //Display Errors
            Debug.Log(wwwReg.error);
            Debug.Log(jsonData);
            messageText.text = wwwReg.error;
        }
    }
    
    IEnumerator Login()
    {
        LoginData ld = new LoginData();
        ld.email = usernameField.text;
        ld.password = passwordField.text;
        string jsonData = JsonUtility.ToJson(ld);
        Debug.Log(usernameField.text);
        Debug.Log(passwordField.text);
        Debug.Log(ld.email);
        Debug.Log(ld.password);
        Debug.Log(jsonData);
        UnityWebRequest www = UnityWebRequest.Post("https://localhost:7222/account/login", jsonData,"application/json");
        www.certificateHandler = new BypassCertificate();
        
        yield return www.SendWebRequest();
        
        if (www.result != UnityWebRequest.Result.Success)
        {
            messageText.text = www.error;
        }
        else
        {
            string jsonResponse = www.downloadHandler.text;
            ApiLoginReturn loginReturn = JsonUtility.FromJson<ApiLoginReturn>(jsonResponse);
            gameManager.SetSecurityToken($"Bearer {loginReturn.accessToken}");
            gameManager.SetRefreshToken(loginReturn.refreshToken);
            Debug.Log($"Login user {usernameField.text}");
            this.gameObject.SetActive(false);
            worldSelectorScreen.gameObject.SetActive(true);
        }
    }

    public void AttemptRegister()
    {
        StartCoroutine(Register());
    }
    
    public void AttemptLogin()
    {
        StartCoroutine(Login());
    }
    
}
