using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace TechTestBackend;

public static class SpotifyHelper
{
    public static Soptifysong[] GetTracks(string name)
    {
        var access_token = GetToken();

        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token.ToString());

        var requestParams = QueryString.Create(new[]
        { 
            new KeyValuePair<string, string>("q", name), 
            new KeyValuePair<string, string>("type", "track")
        });

        var response = client.GetAsync(string.Concat("https://api.spotify.com/v1/search", requestParams)).Result;
        dynamic objects = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);

        var songs = JsonConvert.DeserializeObject<Soptifysong[]>(objects.tracks.items.ToString());
        
        return songs;
    }

    private static string GetToken()
    {
        var client = new HttpClient();
        var c_id = SpotifyAuth.ClientId;
        var c_s = SpotifyAuth.ClientSecret;
        var e = Encoding.UTF8.GetBytes($"{c_id}:{c_s}");
        var base64 = Convert.ToBase64String(e);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64);

        var encodedContent = new FormUrlEncodedContent(new[] 
        { 
            new KeyValuePair<string, string>("grant_type", "client_credentials") 
        });

        var password = client.PostAsync("https://accounts.spotify.com/api/token", encodedContent).Result;
        dynamic Password_content = JsonConvert.DeserializeObject(password.Content.ReadAsStringAsync().Result);

        return Password_content.access_token;
    }

    public static Soptifysong GetTrack(string id)
    {
        var access_token = GetToken();

        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",access_token.ToString());

        var response = client.GetAsync($"https://api.spotify.com/v1/tracks/{id}").Result;
        dynamic objects = JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);

        var song = JsonConvert.DeserializeObject<Soptifysong>(objects.ToString());
        
        return song;
    }
}