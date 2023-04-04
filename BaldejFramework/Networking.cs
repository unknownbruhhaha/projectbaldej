using System.Globalization;
using LiteNetLib;
using LiteNetLib.Utils;
//using NLua;
//using NLua.Method;

namespace BaldejFramework;

public static class Networking
{
	#region Client variables
	public static List<int> ConnectedPlayers
	{
		get
		{
			List<int> ids = new();
			foreach (NetPeer peer in _client.ConnectedPeerList)
			{
				ids.Add(peer.Id);
			}

			return ids;
		}
	}
    private static NetManager _client;
    private static NetPeer _serverPeer;

 // TODO: FIX   public static LuaFunction? OnConnectionSuccessful;
	#endregion

	#region Server variables
	private static NetManager _server;
	private static int _maxPlayers = 0;
	#endregion

	#region Shared variables
	// TODO: FIX   public static List<LuaFunction?> OnDataReceived = new (); // dataName, data, senderID, sentToID
	// TODO: FIX   public static List<LuaFunction?> OnPlayerConnected = new(); // playerID
	// TODO: FIX   public static List<LuaFunction?> OnPlayerDisconnected = new(); // playerID
	
	public enum NetworkTypes { Server, Client, None }

	public static NetworkTypes CurrentType = NetworkTypes.None;
    private static EventBasedNetListener _listener;
    private static NetDataWriter _writer = new();
    private static NetDataReader _dataReader = new();
    #endregion

    public static void CreateServer(int maxPlayers = 16, int port = 7777)
    {
        _maxPlayers = maxPlayers;
        
        _listener = new EventBasedNetListener();
        
        #region Events
        _listener.ConnectionRequestEvent += request =>
        {
	        if(_server.ConnectedPeersCount < maxPlayers /* max connections */)
		        request.Accept();
        };
        
        _listener.PeerConnectedEvent += peer =>
        {
	        Console.WriteLine("We got connection from {0}; connection ID is {1}", peer.EndPoint, peer.Id);  // Show peer id and ip
				/* TODO: FIX   
	        foreach (LuaFunction func in OnPlayerConnected)
	        {
		        func?.Call(peer.Id);
	        } */
	        
	        _serverSendData(DeliveryMethod.ReliableSequenced,@"playerConnected|/-1|/" + peer.Id.ToString(), -1, peer.Id);
        };
        
        _listener.PeerDisconnectedEvent += (peer, info) =>
        {
	        Console.WriteLine("Player with ID {0} disconnected, reason: {1}", peer.Id, info.Reason);
	        /* TODO: Fix
	        foreach (LuaFunction func in OnPlayerDisconnected)
	        {
		        func?.Call(peer.Id);
	        } */
	        
	        _serverSendData(DeliveryMethod.ReliableSequenced,@"playerDisconnected|/-1|/" + peer.Id.ToString(), -1, peer.Id);
        };
        
        _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
        {
	        string data = dataReader.GetString();
	        Console.WriteLine("Got data from {0}, data = {1}, delivery method is {2} ", fromPeer.Id, data, deliveryMethod);
	        _serverClientSendData(deliveryMethod, data, fromPeer.Id);
	        dataReader.Recycle();
        };
        #endregion

        #region Creating server
		_server = new NetManager(_listener);
        _server.Start(port);
        Console.WriteLine("Server created.");
	    #endregion
        
        CurrentType = NetworkTypes.Server;
    }

    public static void JoinServer(string ipAddress, int port = 7777)
    {
	    _listener = new EventBasedNetListener();
	    
	    #region Events
	    _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
	    {
		    string fullData = dataReader.GetString();
		    Console.WriteLine("Some data was received: {0}", fullData);

		    #region Splitted data
		    string[] stringParts = fullData.Split("|/");
		    string dataName = stringParts[1];
		    string data = stringParts[3];
		    int senderID = int.Parse(stringParts[0], NumberStyles.Any);
		    int sendToID = int.Parse(stringParts[2], NumberStyles.Any);
			#endregion

	        /* TODO: Fix
			if (dataName == "playerConnected")
				foreach (LuaFunction func in OnPlayerConnected) func?.Call(int.Parse(data));
			else if (dataName == "playerDisconnected")
				foreach (LuaFunction func in OnPlayerDisconnected) func?.Call(int.Parse(data));
			else
				foreach (LuaFunction func in OnDataReceived) func?.Call(dataName, data, senderID, sendToID);
				*/	
			dataReader.Recycle();
	    };
	    _listener.PeerConnectedEvent += (peer) =>
	    {
		    Console.WriteLine("Connected to server.");
		    //OnConnectionSuccessful?.Call();
		    _serverPeer = peer;
	    };
	    #endregion

	    #region Creating client
		_client = new NetManager(_listener);
	    _client.Start();
	    _client.Connect(ipAddress, port, "");
		#endregion
		
	    CurrentType = NetworkTypes.Client;
    }

    #region Shared functions to send data
    public static void SendDataReliable(string dataName, string data, int sentToID = -1)
    {
	    if (CurrentType == NetworkTypes.Server)
		    _serverSendData(DeliveryMethod.ReliableSequenced, dataName + "|/" + sentToID + "|/" + data);
	    if (CurrentType == NetworkTypes.Client)
	    {
		    _writer.Reset();
		    string packetData = dataName + "|/" + sentToID + "|/" + data;
		    _writer.Put(packetData);
		    _serverPeer.Send(_writer, DeliveryMethod.ReliableSequenced);
	    }
    }

    public static void SendData(string dataName, string data, int sentToID = -1)
    {
	    if (CurrentType == NetworkTypes.Server)
		    _serverSendData(DeliveryMethod.Unreliable, dataName + "|/" + sentToID + "|/" + data);
	    else if (CurrentType == NetworkTypes.Client)
	    {
		    _writer.Reset();
		    string packetData = dataName + "|/" + sentToID + "|/" + data;
		    _writer.Put(packetData);
		    _serverPeer.Send(_writer, DeliveryMethod.Unreliable);
	    }
    }
	#endregion


    #region Server functions for sending data
    // function to send data that was received from clients
    private static void _serverClientSendData(DeliveryMethod deliveryOptions, string data, int senderID)
    {
	    string[] dataParts = data.Split("|/");
	    if (int.Parse(dataParts[1]) == -1)
	    {
		    _writer.Reset();
		    _writer.Put(senderID + "|/" + data);
		    _server.SendToAll(_writer, deliveryOptions, _server.GetPeerById(senderID));
	    }
	    else
	    {
		    _writer.Reset();
		    _writer.Put(senderID + "|/" + data);
		    _server.GetPeerById(int.Parse(dataParts[1])).Send(_writer, deliveryOptions);
	    }
    }
    
    // function to send other data
    private static void _serverSendData(DeliveryMethod deliveryOptions, string data, int sendTo = -1, int exceptPeer = -1)
    {
	    if (sendTo == -1)
	    {
		    _writer.Reset();
		    _writer.Put("-1|/" + data);
		    if (exceptPeer == -1)
				_server.SendToAll(_writer, deliveryOptions);
		    else
				_server.SendToAll(_writer, deliveryOptions, _server.GetPeerById(exceptPeer));
	    }
	    else
	    {
		    _writer.Reset();
		    _writer.Put("-1|/" + data);
		    _server.GetPeerById(sendTo).Send(_writer, deliveryOptions);
	    }
    }
    #endregion

    public static void Tick()
    {
	    if (CurrentType == NetworkTypes.Server)
	    {
		    _server.PollEvents();
	    }
	    else if (CurrentType == NetworkTypes.Client)
	    {
		    _client.PollEvents();
	    }
    }
}
