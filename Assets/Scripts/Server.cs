#pragma warning disable 0649

using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class Server
{
    public static bool ServerReady;
    public static string BaseUrl;
    private static string _basicAuth;

    public static void SetCredentials(string username, string password)
    {
        var url = Application.absoluteURL;
        BaseUrl = url == "" ? "http://localhost" : url.Substring(0, url.IndexOf('/', 10));
        _basicAuth = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
    }

    [Serializable]
    public class Response
    {
        public int status;
        public string message;
        public bool exception;

        public static implicit operator bool(Response value)
        {
            return !value.exception;
        }
    }

    private static UnityWebRequest AddCommonHeaders(UnityWebRequest request)
    {
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", _basicAuth);
        return request;
    }

    public static UnityWebRequest GetRequest(string url)
    {
        var request = UnityWebRequest.Get(url);
        request = AddCommonHeaders(request);
        request.SendWebRequest();
        return request;
    }

    public static UnityWebRequest PostRequest<T>(string url, T data)
    {
        var request = new UnityWebRequest(url) {method = "POST"};
        var json = data is string s ? s : JsonUtility.ToJson(data);
        var uploader = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)) {contentType = "application/json"};
        request.uploadHandler = uploader;
        request = AddCommonHeaders(request);
        request.SendWebRequest();
        return request;
    }

    public static UnityWebRequest PutRequest<T>(string url, T data)
    {
        var request = new UnityWebRequest(url) {method = "PUT"};
        var json = data is string s ? s : JsonUtility.ToJson(data);
        var uploader = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)) {contentType = "application/json"};
        request.uploadHandler = uploader;
        request = AddCommonHeaders(request);
        request.SendWebRequest();
        return request;
    }

    public static UnityWebRequest DeleteRequest(string url)
    {
        var request = UnityWebRequest.Delete(url);
        request = AddCommonHeaders(request);
        request.SendWebRequest();
        return request;
    }

    public static T GetResponse<T>(UnityWebRequest request, bool raiseExceptions = true) where T : Response, new()
    {
        var serializable = new T {status = (int) request.responseCode};
        if (request.method == "POST") request.uploadHandler.Dispose();
        if (request.method == "PUT") request.uploadHandler.Dispose();
        if (request.method == "DELETE" && request.responseCode == 204) return serializable;
        
        var data = request.downloadHandler.data;
        if (data == null) throw new Exception($"[Server] no data");

        var jsonResponse = Encoding.Default.GetString(data);
        try
        {
            try
            {
                JsonUtility.FromJsonOverwrite(jsonResponse, serializable);
            }
            catch (ArgumentException)
            {
                throw new Exception($"[Server] JSON error: {jsonResponse}");
            }

            if (request.responseCode >= 400)
                throw new Exception($"[Server] Status {request.responseCode}. Error: {serializable.message}. " +
                                    $"Url: {request.url}. " +
                                    $"Response: {jsonResponse}");
            if (request.result.ToString() != "Success")
                throw new Exception($"[Server] Result: {request.result}. Error: {serializable.message}. " +
                                    $"Url: {request.url}. " +
                                    $"Response: {jsonResponse}");
            if (GameManager.Debug)
                Debug.Log($"[Server] Message: {serializable.message}. " +
                          $"Url: {request.url}. " +
                          $"Response: {jsonResponse}");
            return serializable;
        }
        catch (Exception e)
        {
            if (raiseExceptions) throw;

            if (GameManager.Debug) Debug.LogWarning(e.Message);

            serializable.exception = true;
            return serializable;
        }
    }
}