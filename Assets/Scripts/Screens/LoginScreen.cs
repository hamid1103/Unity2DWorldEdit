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
    
    public void HideMessage()
    {
        messageText.gameObject.SetActive(false);
    }

    public void ShowMessage()
    {
        messageText.gameObject.SetActive(true);
    }

    IEnumerator Register()
    {
        //Make register request
        LoginData ld = new LoginData();
        ld.email = usernameField.text;
        ld.password = passwordField.text;
        string jsonData = JsonUtility.ToJson(ld);
        
        UnityWebRequest wwwReg = UnityWebRequest.Post($"{GameManager.BaseURL}/account/register", jsonData,"application/json");
        wwwReg.certificateHandler = new BypassCertificate();
        yield return wwwReg.SendWebRequest();

        if (wwwReg.result == UnityWebRequest.Result.Success)
        {
            //if successful 
            yield return StartCoroutine(Login());
        }
        else
        {
            RegisterErrorResponse regErs = JsonUtility.FromJson<RegisterErrorResponse>(wwwReg.downloadHandler.text);
            if (regErs.errors.DuplicateEmail != null)
            {
                messageText.text = "Email is already taken";
            }
            else if (regErs.errors.PasswordRequiresUpper != null)
            {
                messageText.text = "Password requires one uppercase letter, one lowercase letter, one digit and one non-alpha numeral";
            }
            else
            {
                messageText.text = wwwReg.error;
            }
            
            //Display Errors
            ShowMessage();
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
        UnityWebRequest www = UnityWebRequest.Post($"{GameManager.BaseURL}/account/login", jsonData,"application/json");
        www.certificateHandler = new BypassCertificate();
        
        yield return www.SendWebRequest();
        
        if (www.result != UnityWebRequest.Result.Success)
        {
            ShowMessage();
            if (www.responseCode == 401)
            {
                //Idk why it gives a 401 error if there is no matching account. I can't edit that.
                messageText.text = "Password and Email combination not found.";
            }
            else
            {
            messageText.text = www.error;
            }
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
