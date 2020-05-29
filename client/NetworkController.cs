using UnityEngine;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;
using System;

public class UdpState
{
	public IPEndPoint e;
	public UdpClient u;
}

public class NetworkController : MonoBehaviour
{
	public enum CommandType : byte
	{
		Connect = 0x00,
		SyncPosition,
		Disconnect,
		OnKeyDownEvent,
		OnKeyUpEvent,
		Spawn,
		Shoot,
		Hit,
		Death,
		SpawnWeapon,
		PickWeapon,
		ServerInfo,
		SpawnFlag,
		GrabFlag,
		DropFlag,
		ShipFlag,
		RoundFinish,
		RoundStart,
		ServerConfig
	}
	
	public string _host = "charmander.jumbledevs.net";
	public int _port = 11212;
	public static NetworkController instance;
	
	public bool _showDebugPackets = true;
	public bool _showSyncPackets = true;
	
	private UdpClient _udp;
	private IPEndPoint _ep;
	private bool _isWaitingMsg = false;
	private Game _game;
	
	private Queue<byte[]> _recvQueue;
	private Queue<byte[]> _sendQueue;
	
	
	public void SetGame(Game g)
	{
		_game = g;
	}
	
	public void Connect(string name = "")
	{
		byte[] msg = new byte[5];
		msg[0] = (byte)CommandType.Connect;
		
		if (BitConverter.IsLittleEndian)
		{
			msg[1] = 0x00;
		}
		else
		{
			msg[1] = 0x01;
		}
		
		if (!String.IsNullOrEmpty(name) && name.Length <= 3)
		{
			byte[] asciiName = ASCIIEncoding.ASCII.GetBytes(name);
			Array.Copy(asciiName, 0, msg, 2, 3);
		}
		
		_sendQueue.Enqueue(msg);
	}
	
	public void SyncPosition(byte id, float x, float y, Game.GameTeam team, byte rotation, PlayerController.WeaponType wep)
	{
		byte[] msg = new byte[13];
		msg[0] = (byte)CommandType.SyncPosition;
		msg[1] = id;
		
		byte[] bx = BitConverter.GetBytes(x);
		byte[] by = BitConverter.GetBytes(y);
		
		Array.Copy(bx, 0, msg, 2, 4);
		Array.Copy(by, 0, msg, 6, 4);
		
		msg[10] = (byte)team;
		msg[11] = rotation;
		msg[12] = (byte)wep;
		
		_sendQueue.Enqueue(msg);
	}
	
	public void Disconnect()
	{
		byte[] msg = new byte[5];
		msg[0] = (byte)CommandType.Disconnect;
		_sendQueue.Enqueue(msg);
	}
	
	public void KeyDownEvent(byte id, KeyCode c)
	{
		byte[] msg = new byte[5];
		msg[0] = (byte)CommandType.OnKeyDownEvent;
		msg[1] = id;
		msg[2] = (byte)c;
		_sendQueue.Enqueue(msg);
	}
	
	public void KeyUpEvent(byte id, KeyCode c)
	{
		byte[] msg = new byte[5];
		msg[0] = (byte)CommandType.OnKeyUpEvent;
		msg[1] = id;
		msg[2] = (byte)c;
		_sendQueue.Enqueue(msg);
	}
	
	public void ShootEvent(byte id, Vector3 dir, PlayerController.WeaponType weapon)
	{
		byte[] msg = new byte[11];
		msg[0] = (byte)CommandType.Shoot;
		msg[1] = id;
		
		byte[] bx = BitConverter.GetBytes(dir.x);
		byte[] by = BitConverter.GetBytes(dir.y);
		Array.Copy(bx, 0, msg, 2, 4);
		Array.Copy(by, 0, msg, 6, 4);
		
		msg[10] = (byte)weapon;
		
		_sendQueue.Enqueue(msg);
	}
	
	public void Hit(byte id, PlayerController.WeaponType wep)
	{
		byte[] msg = new byte[5];
		msg[0] = (byte)CommandType.Hit;
		msg[1] = id;
		msg[2] = (byte)wep;
		_sendQueue.Enqueue(msg);
	}
	
	public void Death(byte id)
	{
		byte[] msg = new byte[5];
		msg[0] = (byte)CommandType.Death;
		msg[1] = id;
		_sendQueue.Enqueue(msg);
	}
	
	public void PickWeapon(byte id, byte location, PlayerController.WeaponType type)
	{
		byte[] msg = new byte[5];
		msg[0] = (byte)CommandType.PickWeapon;
		msg[1] = id;
		msg[2] = location;
		msg[3] = (byte)type;
		_sendQueue.Enqueue(msg);
	}
	
	public void GetServerInfo()
	{
		byte[] msg = new byte[5];
		msg[0] = (byte)CommandType.ServerInfo;
		_sendQueue.Enqueue(msg);
	}
	
	public void GrabFlag(byte id, Game.GameTeam t)
	{
		byte[] msg = new byte[5];
		msg[0] = (byte)CommandType.GrabFlag;
		msg[1] = id;
		msg[2] = (byte)t;
		_sendQueue.Enqueue(msg);
	}
	
	public void DropFlag(float x, float y, byte id)
	{
		byte[] msg = new byte[5];
		msg[0] = (byte)CommandType.DropFlag;
		msg[1] = id;
		
		byte[] bx = BitConverter.GetBytes(x);
		byte[] by = BitConverter.GetBytes(y);
		
		Array.Copy(bx, 0, msg, 2, 4);
		Array.Copy(by, 0, msg, 6, 4);
		
		_sendQueue.Enqueue(msg);
	}
	
	public void ShipFlag(byte id)
	{
		byte[] msg = new byte[5];
		msg[0] = (byte)CommandType.ShipFlag;
		msg[1] = id;
		_sendQueue.Enqueue(msg);
	}
	
	void Start()
	{
		// Singleton
		if (instance == null) instance = this;
		
		_udp = new UdpClient(_host, _port);
		_ep = new IPEndPoint(IPAddress.Any, 0);
		_udp.Connect(_host, _port);
		
		_sendQueue = new Queue<byte[]>();
		_recvQueue = new Queue<byte[]>();
	}
	
	private void SendPacket(byte[] msg)
	{
		_udp.Send(msg, msg.Length);
		
		string smsg = "Send message (" + msg.Length + "): ";
		
		for (int i = 0; i < msg.Length; i++)
		{
			smsg += msg[i].ToString() + " ";
		}
		
		if (_showDebugPackets)
		{
			if (!_showSyncPackets)
			{
				if (msg[0] != (byte)CommandType.SyncPosition || 
					msg[0] != (byte)CommandType.ServerInfo)
				{
					Debug.Log(smsg);
				}
			}
			else
			{
				Debug.Log(smsg);
			}
		}
	}
	
	private void RecvPacket(IAsyncResult ar)
	{
		UdpClient u = (UdpClient)((UdpState)(ar.AsyncState)).u;
	    IPEndPoint e = (IPEndPoint)((UdpState)(ar.AsyncState)).e;
	    Byte[] receiveBytes = u.EndReceive(ar, ref e);
		
		// Enqueue the received data
		_recvQueue.Enqueue(receiveBytes);
		
		string smsg = "Recv message (" + receiveBytes.Length + "): ";
		
		for (int i = 0; i < receiveBytes.Length; i++)
		{
			smsg += receiveBytes[i].ToString() + " ";
		}
		
		if (_showDebugPackets)
		{
			if (!_showSyncPackets)
			{
				if (receiveBytes[0] != (byte)CommandType.SyncPosition || 
					receiveBytes[0] != (byte)CommandType.ServerInfo)
				{
					Debug.Log(smsg);
				}
			}
			else
			{
				Debug.Log(smsg);
			}
		}
		
		_isWaitingMsg = false;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (_udp != null)
		{
			// Check if there is anything to receive
			if (!_isWaitingMsg)
			{
				UdpState s = new UdpState();
				s.e = _ep;
				s.u = _udp;
			
				_udp.BeginReceive(new AsyncCallback(RecvPacket), s);
				_isWaitingMsg = true;
			}
		}
		
		// Check if there are any enqueued received
		// messages
		if (_recvQueue.Count > 0)
		{
			byte[] msg = _recvQueue.Dequeue();
			
			// Command type
			CommandType cmd = (CommandType)msg[0];
			
			switch (cmd)
			{
				case CommandType.Connect:
				{
					_game.OnPlayerConnect(msg[1]);
				}
				break;
				
				case CommandType.Spawn:
				{
                    _game.OnPlayerSpawn(msg[1], msg[2], (Game.GameTeam)msg[3], Vector3.zero);
				}
				break;
				
				case CommandType.SyncPosition:
				{
					_game.OnSyncPosition(msg[1], BitConverter.ToSingle(msg, 2), BitConverter.ToSingle(msg, 6), 
						(Game.GameTeam)msg[10], msg[11], (PlayerController.WeaponType)msg[12]);
				}
				break;
				
				case CommandType.Disconnect:
				{
					_game.OnPlayerDisconnect(msg[1]);
				}
				break;
				
				case CommandType.OnKeyDownEvent:
				{
					_game.OnKeyDownEvent(msg[1], (KeyCode)msg[2]);
				}
				break;
				
				case CommandType.OnKeyUpEvent:
				{
					_game.OnKeyUpEvent(msg[1], (KeyCode)msg[2]);
				}
				break;
				
				case CommandType.Shoot:
				{
					_game.OnShootEvent(msg[1], BitConverter.ToSingle(msg, 2), BitConverter.ToSingle(msg, 6), 
							(PlayerController.WeaponType)msg[10]);
					Debug.Log(msg[10]);
				}
				break;
				
				case CommandType.Hit:
				{
					_game.OnHitEvent(msg[1], (PlayerController.WeaponType)msg[2]);
				}
				break;
				
				case CommandType.Death:
				{
					_game.OnDeathEvent(msg[1]);
				}
				break;
				
				case CommandType.SpawnWeapon:
				{
					_game.OnWeaponSpawn(msg[1], (PlayerController.WeaponType)msg[2]);
				}
				break;
				
				case CommandType.PickWeapon:
				{
					_game.OnWeaponPick(msg[1], msg[2], (PlayerController.WeaponType)msg[3]);
				}
				break;
				
				case CommandType.ServerInfo:
				{
					_game.OnServerInfo(msg[1], msg[2], msg[3], msg[4], msg[5]);
				}
				break;
				
				case CommandType.SpawnFlag:
				{
					_game.OnSpawnFlag(BitConverter.ToSingle(msg, 1), BitConverter.ToSingle(msg, 5), (Game.GameTeam)msg[9]);
				}
				break;
				
				case CommandType.GrabFlag:
				{
					_game.OnGrabFlag(msg[1], (Game.GameTeam)msg[2]);
				}
				break;
				
				case CommandType.DropFlag:
				{
					_game.OnDropFlag(BitConverter.ToSingle(msg, 1), BitConverter.ToSingle(msg, 5), msg[9]);
				}
				break;
				
				case CommandType.ShipFlag:
				{
					_game.OnShipFlag(msg[1]);
				}
				break;
				
				case CommandType.RoundFinish:
				{
					_game.OnRoundFinish(msg[1], msg[2], msg[3]);
				}
				break;
				
				case CommandType.RoundStart:
				{
					_game.OnRoundStart();
				}
				break;
				
				case CommandType.ServerConfig:
				{
					_game.OnServerConfig(msg[1], msg[2]);
				}
				break;
			}
		}
		
		// Check if there are any enqueued messages
		// to send
		if (_sendQueue.Count > 0)
		{
			byte[] msg = _sendQueue.Dequeue();
			SendPacket(msg);
		}
	}
}
