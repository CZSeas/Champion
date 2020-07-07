using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib.Unity;
using TwitchLib.Client.Models;
using UnityEngine; 

public class TwitchClient : MonoBehaviour
{
    public Client client;
    private string channel_name = "adeptthebest";
    private string bot_name = "chatchampionbot";
    GunController gunController;
    PlayerController playerController;

    private void Start()
    {
        gunController = gameObject.GetComponent<GunController>();
        playerController = gameObject.GetComponent<PlayerController>();

        ConnectionCredentials credentials = new ConnectionCredentials(bot_name, Scrts.access_token);
        client = new Client();
        client.Initialize(credentials, channel_name);
        client.OnMessageReceived += MsgReceivedFunc;
        client.Connect();
    }

    private void MsgReceivedFunc(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
    {
        //if (e.ChatMessage.Message == "PogU") {
        //    print(e.ChatMessage.Message);
        //    StartCoroutine(playerController.Dash());
        //}
        gunController.Shoot();
    }

}
