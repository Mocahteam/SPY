using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System;
using System.Text;

[Serializable]
struct BasicAuthConfig
{
    [SerializeField] public string username;
    [SerializeField] public string password;
}
[Serializable]
struct OAuth2Config
{
    [SerializeField] public string tokenEndpoint;   
    [SerializeField] public string clientId;
    [SerializeField] public string clientSecret;
}

[Serializable]
public abstract class LrsConfig
{
    [SerializeField] private string xApiEndpoint;
    public string GetEndpoint() { return xApiEndpoint; }
    abstract public Awaitable<AuthenticationHeaderValue> GetAuthHeader();
    public LrsConfig(string a_XApiEnpoint)
    {
        this.xApiEndpoint = a_XApiEnpoint;
    }
}

[Serializable]
class LrsBasicAuthConfig : LrsConfig
{
    [SerializeField]
    private BasicAuthConfig m_AuthConfig;

    public LrsBasicAuthConfig(string a_XApiEnpoint, BasicAuthConfig authConfig) : base(a_XApiEnpoint)
    {
        m_AuthConfig = authConfig;
    }

    public override Awaitable<AuthenticationHeaderValue> GetAuthHeader()
    {
        return AwaitableUtils.FromResult( new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(m_AuthConfig.username + ":" + m_AuthConfig.password))));
    }
}

[System.Serializable]
class LrsOAuth2Config : LrsConfig
{
    [SerializeField]
    private OAuth2Config m_AuthConfig;

    [NonSerialized]
    private string m_CachedToken = null;
    [NonSerialized]
    private DateTime m_CachedTokenExpiry = DateTime.UnixEpoch;

    public LrsOAuth2Config(string a_XApiEnpoint, OAuth2Config authConfig) : base(a_XApiEnpoint)
    {
        m_AuthConfig = authConfig;
    }

    public override async Awaitable<AuthenticationHeaderValue> GetAuthHeader()
    {
        var authHeader = new AuthenticationHeaderValue("Bearer", await GetAuthToken());
        return authHeader;
    }

    struct OAuthTokenResponse
    {
        public string access_token;
        public int expires_in;
    }

/// <summary>
/// 
/// This application is authenticate using the Client Secret Basic method.
/// That is, the client id and secrets are base64-encoded
/// and put in a Basic Auth Authorization Header.
/// You must configure your IdP accordingly.
/// </summary>
/// <returns></returns>
    private async Task<string> GetAuthToken()
    {
        if (m_CachedToken != null && m_CachedTokenExpiry <= DateTime.Now.AddMinutes(-1))
        {
            return m_CachedToken;
        }
        using (UnityWebRequest tokenRequest = new UnityWebRequest(m_AuthConfig.tokenEndpoint, "POST"))
        {
            tokenRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            tokenRequest.downloadHandler = new DownloadHandlerBuffer();
            string scopes = "openid";
            var formData = string.Format("grant_type=client_credentials&scope={0}", scopes);
            string authHeader = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(m_AuthConfig.clientId + ":" + m_AuthConfig.clientSecret));
            tokenRequest.SetRequestHeader("Authorization", authHeader);
            tokenRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(formData));
            await tokenRequest.SendWebRequest();
            if (tokenRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to retrieve OAuth2 token for authentication with LRS: " + tokenRequest.error);
                m_CachedToken= null;
                m_CachedTokenExpiry = DateTime.UnixEpoch;
                return null;
            }
            var res = JsonUtility.FromJson<OAuthTokenResponse>(tokenRequest.downloadHandler.text);
            Debug.LogWarning(string.Format("Got token: {0}, expires at {1}", res.access_token, DateTime.Now.AddSeconds(res.expires_in)));
            Debug.LogWarning(string.Format("It is currently {0}", DateTime.Now));
            m_CachedToken= res.access_token;
            m_CachedTokenExpiry = DateTime.Now.AddSeconds(res.expires_in);
        }
        return m_CachedToken;
    }
}