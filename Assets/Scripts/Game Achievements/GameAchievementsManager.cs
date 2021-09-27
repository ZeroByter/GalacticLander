using Steamworks;
using System.Collections;
using System.Text;
using UnityEngine;

public class CoroutineWithData {
    public Coroutine coroutine { get; private set; }
    public object result;
    private IEnumerator target;

    public CoroutineWithData(MonoBehaviour owner, IEnumerator target) {
        this.target = target;
        this.coroutine = owner.StartCoroutine(Run());
    }

    private IEnumerator Run() {
        while (target.MoveNext()) {
            result = target.Current;
            yield return result;
        }
    }
}

public class GameAchievementsManager {
    private static HAuthTicket hAuthTicket;

    private static string getSteamAuthTicket(out HAuthTicket hAuthTicket) {
        byte[] ticketByteArray = new byte[1024];
        uint ticketSize;
        hAuthTicket = SteamUser.GetAuthSessionTicket(ticketByteArray, ticketByteArray.Length, out ticketSize);
        System.Array.Resize(ref ticketByteArray, (int)ticketSize);
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < ticketSize; i++) {
            sb.AppendFormat("{0:x2}", ticketByteArray[i]);
        }
        return sb.ToString();
    }

    public static IEnumerator GetGameAchievement(string gameAchievement) {
        string authTicket = getSteamAuthTicket(out hAuthTicket);

        using (WWW www = new WWW(string.Format("https://www.zerobyter.net/api/gameachievements/getgameachievement.php?ticket={0}&gameId={1}&achievementId={2}", authTicket, SteamUtils.GetAppID().m_AppId, gameAchievement))) {
            yield return www;

            if (www.text.StartsWith("true")) {
                yield return www.text;
            } else {
                yield return "false";
            }

            SteamUser.CancelAuthTicket(hAuthTicket);
        }
    }

    public static IEnumerator SetGameAchievement(string gameAchievement) {
        string authTicket = getSteamAuthTicket(out hAuthTicket);

        using (WWW www = new WWW(string.Format("https://www.zerobyter.net/api/gameachievements/setgameachievement.php?ticket={0}&gameId={1}&achievementId={2}", authTicket, SteamUtils.GetAppID().m_AppId, gameAchievement))) {
            yield return www;

            SteamUser.CancelAuthTicket(hAuthTicket);
        }
    }

    /*
     * this is how to get the result of GetGameAchievement
    private IEnumerator Start() {
        CoroutineWithData cd = new CoroutineWithData(this, GetGameAchievement("halloweenPack"));
        yield return cd.coroutine;
        print(cd.result);
    }
    */
}
