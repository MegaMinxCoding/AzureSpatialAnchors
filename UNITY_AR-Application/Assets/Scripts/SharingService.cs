using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Net;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.Http;

[Serializable]
public class IdentifierObject
{
    public String id;
    public String timeStamp;
}


public class SharingService : MonoBehaviour
{
    #region UNITY INSPECTOR VARIABLES
    [Header("WWW")]
    [SerializeField]
    [Tooltip("The ip adress of the webserver. (not empty)")]
    private string ipAdress;
    [SerializeField]
    [Tooltip("The portnumber of the webserver. (not empty)")]
    private string  portNumber;
    
    //Property which contains the ip adress of the destination
    public string fullAdress { get; set; }
  
    #endregion

    public void Start()
    { 
        fullAdress = ipAdress + ":" + portNumber;
    }

    
    /// <summary>
    /// Verschickt den Anker mittels eines <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="identifier">ID-String des Ankesr</param>
    /// <param name="logger">Wird genutzt um dem Nutzer </param>
    /// <returns></returns>
    public async Task<bool> postToAPIasync(string identifier, LoggerScript logger)
    {
        logger.Log("Sending anchor to the server...");
        //track the elapsed time
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        //make the post request
        HttpClient httpClient = new HttpClient();
        string url = $"http://{fullAdress}/addanchor";
        string jsonRequestBody = $"{{\"id\":\"{identifier}\"}}";
        using (var content = new StringContent(jsonRequestBody,Encoding.UTF8, "application/json"))
        {
            var result = await httpClient.PostAsync(url, content);
            stopwatch.Stop();
            if(result.StatusCode == HttpStatusCode.OK)
            {
                logger.Log($"Request succsessful finished in {stopwatch.ElapsedMilliseconds} ms.");
                return true;
            }
            else
            {
                logger.Log($"Request failed in {stopwatch.ElapsedMilliseconds} ms.", TextState.ERROR);
                await Task.Delay(200);
                return false;
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <returns></returns>
    public async Task<string> getFromAPIasync(LoggerScript logger)
    {
        logger.Log("Requesting the last anchor.");
        //track the elapsed time
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        //Send the request
        string url = $"http://{fullAdress}/getlastanchor";
        HttpClient httpClient = new HttpClient();
        HttpResponseMessage httpResponse = await httpClient.GetAsync(url);
        httpResponse.EnsureSuccessStatusCode();
        // work with the response
        string body = await httpResponse.Content.ReadAsStringAsync();
        IdentifierObject identifierObject = JsonConvert.DeserializeObject<IdentifierObject>(body);
        stopwatch.Stop();
        if(identifierObject.Equals(null) || String.IsNullOrEmpty(identifierObject.id))
        {
            logger.Log("Coould not receive any Identifiers..");
            return String.Empty;
        }
        logger.Log($"Received the last Anchor in {stopwatch.ElapsedMilliseconds} ms.");
        return identifierObject.id;
    }
    

}
