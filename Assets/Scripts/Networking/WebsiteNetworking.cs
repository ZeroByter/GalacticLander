using Steamworks;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class WebsiteNetworking : MonoBehaviour {
    private static WebsiteNetworking Singletron;

    public static WebsiteNetworking GetSingletron()
    {
        return Singletron;
    }

    private void Awake()
    {
        if(Singletron != null)
        {
            Destroy(gameObject);
            return;
        }

        Singletron = this;
        DontDestroyOnLoad(gameObject);
    }

    string GetSteamAuthTicket(out HAuthTicket hAuthTicket)
    {
        byte[] ticketByteArray = new byte[1024];
        uint ticketSize;
        hAuthTicket = SteamUser.GetAuthSessionTicket(ticketByteArray, ticketByteArray.Length, out ticketSize);
        System.Array.Resize(ref ticketByteArray, (int)ticketSize);
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < ticketSize; i++)
        {
            sb.AppendFormat("{0:x2}", ticketByteArray[i]);
        }
        return sb.ToString();
    }

    private IEnumerator Send(string name, string levelName, string value)
    {
        HAuthTicket authTicket;
        string ticket = GetSteamAuthTicket(out authTicket);

        WWWForm form = new WWWForm();
        form.AddField("ticket", ticket);
        form.AddField("gameId", "galacticlander");
        form.AddField("name", name);
        form.AddField("levelName", levelName);
        form.AddField("value", value);

        UnityWebRequest www = UnityWebRequest.Post("https://www.zerobyter.net/api/postStatistic.php", form);
        //UnityWebRequest www = UnityWebRequest.Post("http://localhost:91/api/postStatistic.php", form);
        yield return www.SendWebRequest();

        /*if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
        }*/

        SteamUser.CancelAuthTicket(authTicket);
    }

    public static void PostStatistic(string name, string levelName, string value)
    {
        if (!SteamManager.Initialized) return;

        Singletron.StartCoroutine(Singletron.Send(name, levelName, value));
    }

    public IEnumerator GetLevelBestStatisticsData(string levelName)
    {
        UnityWebRequest www = UnityWebRequest.Get($"https://www.zerobyter.net/api/getLevelBest.php?gameid=galacticlander&levelname={levelName}");
        yield return www.SendWebRequest();

        if (!www.isNetworkError && !www.isHttpError)
        {
            yield return www.downloadHandler.text;
        }
        else
        {
            yield return "false";
        }
    }
}
