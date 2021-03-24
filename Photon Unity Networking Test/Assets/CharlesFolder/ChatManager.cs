using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Photon.Chat;
using ExitGames.Client.Photon;
using AuthenticationValues = Photon.Chat.AuthenticationValues;

public class ChatManager : MonoBehaviour, IChatClientListener
{
    [SerializeField]
    private Text chat, StateText;

    public InputField InputFieldChat;
    public string UserName;

    protected internal ChatAppSettings chatAppSettings;
    ChatNetworkManager chatNetworkManager;
    ChatClient chatClient;

    public NewPlayerScript myCharacter;

    public void Start()
    {
        chatNetworkManager = FindObjectOfType<ChatNetworkManager>();
        this.chatAppSettings = PhotonNetwork.PhotonServerSettings.AppSettings.GetChatSettings();
        // In the C# SDKs, the callbacks are defined in the `IChatClientListener` interface.
        // In the demos, we instantiate and use the ChatClient class to implement the IChatClientListener interface.
        chatClient = new ChatClient(this);
        // Set your favourite region. "EU", "US", and "ASIA" are currently supported.
        chatClient.ChatRegion = "EU";
    }

    public void Update()
    {
        chatClient.Service();
    }

    public void OnEnterSend()
	{
		if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
		{
		    this.SendChatMessage(this.InputFieldChat.text);
			this.InputFieldChat.text = "";

            myCharacter.controllable = true;
        }
	}

    public void DisableInput()
    {
        myCharacter.controllable = false;
    }

    private void SendChatMessage(string inputLine)
    {
        chatClient.PublishMessage("Public", inputLine);
    }
    public void Connect(string name)
    {
        this.chatClient = new ChatClient(this);
        this.chatClient.AuthValues = new AuthenticationValues(name);
        this.chatClient.UseBackgroundWorkerForSending = true;
        this.chatClient.ConnectUsingSettings(this.chatAppSettings);

        Debug.Log("Connecting as: " + name);
    }

    public void ShowChannel(string channelName)
    {
        if (string.IsNullOrEmpty(channelName))
        {
            return;
        }

        ChatChannel channel = null;
        bool found = this.chatClient.TryGetChannel(channelName, out channel);
        if (!found)
        {
            Debug.Log("ShowChannel failed to find channel: " + channelName);
            return;
        }

        this.chat.text = channel.ToStringMessages();
        Debug.Log("ShowChannel: Public");

    }

    public void DebugReturn(DebugLevel level, string message)
    {
        if (level == DebugLevel.ERROR)
        {
            Debug.LogError(message);
        }
        else if (level == DebugLevel.WARNING)
        {
            Debug.LogWarning(message);
        }
        else
        {
            Debug.Log(message);
        }
    }

    public void OnChatStateChange(ChatState state)
    {
        if (state == ChatState.ConnectedToFrontEnd)
            this.StateText.text = "Connected to Public Channel";

        this.StateText.text = state.ToString();

       
    }

    public void OnConnected()
    {
        chatClient.Subscribe("Public", 0);
        InputFieldChat.gameObject.SetActive(true);
    }

    public void OnDisconnected()
    {
        throw new NotImplementedException();
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        if (channelName.Equals("Public"))
        {
            // update text
            this.ShowChannel("Public");
        }
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        throw new NotImplementedException();
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        Console.WriteLine("Status change for: {0} to: {1}", user, status);
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        Debug.Log("OnSubscribed: " + string.Join(", ", channels));
        this.ShowChannel(channels[0]);
    }

    public void OnUnsubscribed(string[] channels)
    {
        throw new NotImplementedException();
    }

    public void OnUserSubscribed(string channel, string user)
    {
        throw new NotImplementedException();
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        throw new NotImplementedException();
    }
}
