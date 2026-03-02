using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace.Screens;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Tilemaps;
using Utils;

public class GameManager : MonoBehaviour
{
    public Grid grid;
    public GridEditor gridEditor;
    public Tile selectedTile;
    public int selectedTileIndex;
    public CameraMovement camera;
    public GameObject WorldSelector;
    public GameObject LoadingScreen;
    public GameObject GameUI;
    public static string BaseURL = "https://localhost:7222";

    public string worldName;
    public string worldUUID;
    
    public SaveIcon saveIcon;
    public RenderTexture worldCaptureTexture;
    
    
    [SerializeField] private string securityToken;
    [SerializeField] private string refreshToken;

    public GameObject tileSelector;
    
    private bool Started = false;

    public void setTile(Tile tile)
    {
        selectedTile = tile;
        //I want to double check. Might add more checks later.
        if (gridEditor.tile != tile)
        {
            gridEditor.tile = tile;
            gridEditor.tileIndex = selectedTileIndex;
        }
    }

    public void InitialCreateWorld(string worldName)
    {
        Debug.Log("InitialCreateWorld called");
        StartCoroutine(CreateWorldAtServer(worldName));
    }

    IEnumerator CreateWorldAtServer(string worldName)
    {
        Debug.Log("IEnumerator Create World At Server called");
        EnvUpload envUpload = new();
        envUpload.name = worldName;
        UnityWebRequest wwwCreateWorld = UnityWebRequest.Post($"{BaseURL}/environments", JsonUtility.ToJson(envUpload), "application/json");
        wwwCreateWorld.SetRequestHeader("Authorization", securityToken);
        
        wwwCreateWorld.certificateHandler = new BypassCertificate();
        yield return wwwCreateWorld.SendWebRequest();

        if (wwwCreateWorld.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = wwwCreateWorld.downloadHandler.text;
            //Get texture url response, save world info
            APIEnvironment2D createReturn = JsonUtility.FromJson<APIEnvironment2D>(jsonResponse);

            worldUUID = createReturn.id;
            this.worldName = createReturn.name;
            //In a new world, there is no saved data. Just enable the editor. 
            WorldSelector.SetActive(false);
            StartGame();

        }
        else
        {
            Debug.Log("Sumn' went wrong when creating the world.");
            Debug.Log(envUpload);
            Debug.LogError(wwwCreateWorld.error);
            
        }
    }

    public void LoadWorld(string uuid)
    {
        LoadingScreen.SetActive(true);
        tileSelector.SetActive(true);
        
        StartCoroutine(EnvironmentUtils.fetchWorldByID(uuid, securityToken, true, (env2) =>
        {
            worldUUID = env2.id;
            worldName = env2.name;
            
            //Populate Tilemap data
            foreach (Object2D obj in env2.objects)
            {
                Vector3Int vector3Int = new Vector3Int(obj.posX, obj.posY, 0);
                Debug.Log($"adding {obj.posX} {obj.posY} with TileId {obj.tileId}");
                gridEditor.tilemap.Add(vector3Int, obj.tileId);
            }
            
            Debug.Log(env2.objects.Count);
            Debug.Log("World should've loaded by now.");
            gridEditor.gameObject.SetActive(true);
            gridEditor.loadMap();
            LoadingScreen.SetActive(false);
            WorldSelector.SetActive(false);
            StartGame();
        }));
    }

    public string GetSecurityToken()
    {
        return this.securityToken;
    }

    public void SaveWorld()
    {
        Started = false;
        gridEditor.Started = false;
        StartCoroutine(SaveWorldToServer());
    }

    IEnumerator SaveWorldToServer()
    {
        saveIcon.BeginAnimation();
        //Renew token just in case.
        
        //check token status
            
        //Get render texture and upload to server
        Texture2D WorldScreenshot = ConvertRenderTextureToTexture2D();
        byte[] pngData = WorldScreenshot.EncodeToPNG();
        WWWForm form = new WWWForm();
        form.AddBinaryData(
            "file",                 // must match server parameter name
            pngData,
            $"{worldName}_{DateTime.Now.Date.Day}-{DateTime.Now.Date.Month}-{DateTime.Now.Date.Year}_texture.png",          // file name
            "image/png"             // MIME type
        );

        UnityWebRequest wwwIMGUpload = UnityWebRequest.Post("https://localhost:7222/preview", form);
        wwwIMGUpload.certificateHandler = new BypassCertificate();
        wwwIMGUpload.SetRequestHeader("Authorization", securityToken);
        
        yield return wwwIMGUpload.SendWebRequest();
        string uploadedIMGUrl = "";
            
        if (wwwIMGUpload.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(wwwIMGUpload.error);
        }
        else
        {
            string jsonResponse = wwwIMGUpload.downloadHandler.text;
            //Get texture url response, save world info
            APIPreviewUploadReturn uploadReturn = JsonUtility.FromJson<APIPreviewUploadReturn>(jsonResponse);
            uploadedIMGUrl = uploadReturn.url;
            Debug.Log("Saving Image Success: " + uploadedIMGUrl);
            //Fetch world entry
            APIEnvironment2D env = null;
            yield return StartCoroutine(EnvironmentUtils.fetchWorldByID(worldUUID, securityToken, false, (env2d) =>
            {
                env = env2d;
                //Update world entry
                env.previewURL = uploadedIMGUrl;
            }));
            BulkObjectUpload upload = null;
            if (env != null)
            {
                yield return StartCoroutine(EnvironmentUtils.updateWorldByID(worldUUID, securityToken, env, () =>
                {
                    // - Save every placed object as Object2D (SO MANY POST REQUESTS!)
                
                    //Vector3 coords + tile index
                    //gridEditor.tilemap
                    List<Object2D> objects = new List<Object2D>();
                    foreach (var (k,v) in gridEditor.tilemap)
                    {
                        Debug.Log($"X: {k.x} {k.y} {k.ToString()}");
                        Object2D obj = new()
                        {
                            posX = k.x,
                            posY = k.y,
                            tileId = v,
                            Layer = "Default"
                        };
                        objects.Add(obj);
                    }
                
                    //To:do Look into finding a better saving method. Or find a way to post a bunch of objects in one post request
                    //Done - Created POST:/object2d/bulk
                    //Create POST Body: EnvironmentId + List<Object2D>
                    upload = new BulkObjectUpload();
                    upload.EnvironmentId = worldUUID;
                    upload.Objects = objects;
                
                    //Error handling

                    //End - Stop Animation
                }));
            }

            if (upload != null)
            {
                yield return StartCoroutine(BulkUploadObjects(upload));
            }
        }
        
    }

    IEnumerator BulkUploadObjects(BulkObjectUpload upload)
    { 
        UnityWebRequest wwwObjectPost = UnityWebRequest.Post("https://localhost:7222/object2d/bulk", JsonUtility.ToJson(upload),"application/json");
        wwwObjectPost.certificateHandler = new BypassCertificate();
        wwwObjectPost.SetRequestHeader("Authorization", securityToken);
        yield return wwwObjectPost.SendWebRequest();

        if (wwwObjectPost.result == UnityWebRequest.Result.Success)
        {
            saveIcon.StopAnimation();
            
            Started = true;
            gridEditor.Started =true;
        }
        else
        {
            Debug.Log(JsonUtility.ToJson(upload));
            foreach (var VARIABLE in upload.Objects)
            {
                Debug.Log($"{VARIABLE.posX} {VARIABLE.posY} {VARIABLE.tileId}");
            }
            Debug.LogError(wwwObjectPost.error);
        }
        
        
    }

    private Texture2D ConvertRenderTextureToTexture2D()
    {
        Texture2D tex = new Texture2D(worldCaptureTexture.width, worldCaptureTexture.height, TextureFormat.ARGB32, false, false);
        var oldRt = RenderTexture.active;
        RenderTexture.active = worldCaptureTexture;
        tex.ReadPixels(new Rect(0, 0, worldCaptureTexture.width, worldCaptureTexture.height), 0, 0);
        tex.Apply();
        RenderTexture.active = oldRt;
        return tex;
    }

    public void ReturnWorldScreen()
    {
        StopGame();
        //Reset grideditor
        gridEditor.Reset();
        
        WorldSelector.SetActive(true);
        StartCoroutine(WorldSelector.GetComponent<WorldSelectorScreen>().GetWorlds());
    }

    public void SetSecurityToken(string token)
    {
        this.securityToken = token;
    }

    public void SetRefreshToken(string token)
    {
        //set token
        this.refreshToken = token;
        //set refresh timer

    }

    public void Update()
    {
        gridEditor.tileIndex = selectedTileIndex;
        if ((Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S)) || (Input.GetKey(KeyCode.RightControl) && Input.GetKeyDown(KeyCode.S)) )
        {
            Debug.Log("Saving!!!");
            SaveWorld();
        }
    }

    public void StopGame()
    {
        GameUI.SetActive(false);
        Started = false;
        camera.locked = true;
        camera.ToCenter();
        tileSelector.SetActive(false);
        gridEditor.Started = false;
    }
    
    public void StartGame()
    {
        GameUI.SetActive(true);
        Started = true;
        camera.locked = false;
        gridEditor.Started = true;
        tileSelector.SetActive(true);
    }

    public void DeleteWorld(string worldId)
    {
        StartCoroutine(EnvironmentUtils.deleteWorldByID(worldId, securityToken, () =>
        {
            Debug.Log("World should be gone from db by now");
            //Reload world selector
            StartCoroutine(
                WorldSelector.GetComponent<WorldSelectorScreen>().GetWorlds());
        }));
    }
}

public class EnvUpload
{
    public string name;
}