using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using SocketIO;
using UnityStandardAssets.Vehicles.Car;
using System;

public class CommandServer : MonoBehaviour
{
	[Tooltip("Number of frames between each telemetry emission")]
	public int frameInterval = 40;

	public CarRemoteControl CarRemoteControl;
	public Camera FrontFacingCamera;
	private SocketIOComponent _socket;
	private CarController _carController;
	private int _frameCount;
	private bool _isConnected;

	// Use this for initialization
	void Start()
	{
		_socket = GameObject.Find("SocketIO").GetComponent<SocketIOComponent>();
		_socket.On("open", OnOpen);
		_socket.On("close", OnClose);
		_socket.On("steer", OnSteer);
		_socket.On("manual", onManual);
		_carController = CarRemoteControl.GetComponent<CarController>();
		_frameCount = 0;
		_isConnected = false;
	}

	// Update is called once per frame
	void Update()
	{
		if (!_isConnected) return;

		_frameCount++;
		if (_frameCount >= frameInterval)
		{
			_frameCount = 0;
			EmitTelemetry();
		}
	}

	void OnOpen(SocketIOEvent obj)
	{
		Debug.Log("Connection Open");
		_isConnected = true;
	}

	void OnClose(SocketIOEvent obj)
	{
		Debug.Log("Connection Closed");
		_isConnected = false;
	}

	void onManual(SocketIOEvent obj)
	{
	}

	void OnSteer(SocketIOEvent obj)
	{
		JSONObject jsonObject = obj.data;
		CarRemoteControl.SteeringAngle = float.Parse(jsonObject.GetField("steering_angle").str);
		CarRemoteControl.Acceleration = float.Parse(jsonObject.GetField("throttle").str);
		// Note: Do not call EmitTelemetry(obj) here if you want telemetry sent strictly every couple of frames
	}

	void EmitTelemetry(SocketIOEvent obj = null)
	{
		UnityMainThreadDispatcher.Instance().Enqueue(() =>
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data["steering_angle"] = _carController.CurrentSteerAngle.ToString("N4");
			data["throttle"] = _carController.AccelInput.ToString("N4");
			data["speed"] = _carController.CurrentSpeed.ToString("N4");
			// data["image"] = Convert.ToBase64String(CameraHelper.CaptureFrame(FrontFacingCamera)).Substring(0, 10);
			data["image"] = Convert.ToBase64String(CameraHelper.CaptureFrame(FrontFacingCamera));
			_socket.Emit("telemetry", new JSONObject(data));
		});
	}
}
