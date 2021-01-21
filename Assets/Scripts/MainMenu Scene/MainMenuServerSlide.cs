using UnityEngine;
using TMPro;
public class MainMenuServerSlide : MonoBehaviour
{
    public TextMeshProUGUI slideName, slidePing, slideType, slideMode, slidePlayers;
    public Server storedServer;

    //Update Values of this Slide
    public void RefreshValues(Server server) 
    {
        slideName.text = server.server_name;
        slidePing.text = server.server_ping + "ms";
        slideType.text = server.server_description;
        slideMode.text = server.server_mode;
        slidePlayers.text = string.Format("({0}/{1})", server.server_players, server.server_maxPlayers);
        storedServer = server;
    }

    //Button: Join this Room
    public void JoinThisRoom() 
    {
        MusicManager.PlayUISound(0);
        if(ServerConnect.singleton != null) 
        {
            ServerConnect.singleton.ConnectToServer(storedServer.server_Ip, storedServer.server_Port);
        }
    }
}
