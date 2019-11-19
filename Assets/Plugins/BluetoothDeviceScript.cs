//#define BLE_DEBUG_LOG

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BluetoothDeviceScript : MonoBehaviour
{
	public List<string> DiscoveredDeviceList;

	public Action InitializedAction;
	public Action DeinitializedAction;
	public Action<string, string[]> ErrorAction;
	public Action<string> ServiceAddedAction;
	public Action StartedAdvertisingAction;
	public Action StoppedAdvertisingAction;
	public Action<string, string> DiscoveredPeripheralAction;
	public Action<string, string, int, byte[]> DiscoveredPeripheralWithAdvertisingInfoAction;
	public Action<string, string> RetrievedConnectedPeripheralAction;
    public Action RetrievedPeripheralsAction;
	public Action<string, byte[]> PeripheralReceivedWriteDataAction;
	public Action<string> ConnectedPeripheralAction;
	public Action<string> ConnectedDisconnectPeripheralAction;
	public Action<string> DisconnectedPeripheralAction;
	public Action<string, string> DiscoveredServiceAction;
	public Action<string, string, string> DiscoveredCharacteristicAction;
	public Action<string> DidWriteCharacteristicAction;
	public Action<string, string> DidWriteCharacteristicWithDeviceAddressAction;
    public Action<bool> PowerStateUpdated;

	public Dictionary<string, Dictionary<string, Action<string>>> DidUpdateNotificationStateForCharacteristicAction;
	public Dictionary<string, Dictionary<string, Action<string, string>>> DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction;
	public Dictionary<string, Dictionary<string, Action<string, byte[]>>> DidUpdateCharacteristicValueAction;
	public Dictionary<string, Dictionary<string, Action<string, string, byte[]>>> DidUpdateCharacteristicValueWithDeviceAddressAction;

#if UNITY_STANDALONE_OSX || UNITY_IPHONE
	private readonly List<string> messages = new List<string>();
#endif

	// Use this for initialization
	void Start ()
	{
		DiscoveredDeviceList = new List<string>();
		DidUpdateNotificationStateForCharacteristicAction = new Dictionary<string, Dictionary<string, Action<string>>>();
		DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction = new Dictionary<string, Dictionary<string, Action<string, string>>>();
		DidUpdateCharacteristicValueAction = new Dictionary<string, Dictionary<string, Action<string, byte[]>>>();
		DidUpdateCharacteristicValueWithDeviceAddressAction = new Dictionary<string, Dictionary<string, Action<string, string, byte[]>>>();
	}
	
#if UNITY_STANDALONE_OSX || UNITY_IPHONE
	// Update is called once per frame
	void Update ()
	{
		BluetoothLEHardwareInterface.GetMessages(messages);
		foreach (var msg in messages)
		{
			OnBluetoothMessage(msg);
		}
		messages.Clear();
	}
#endif
	
	const string deviceInitializedString = "Initialized";
	const string deviceDeInitializedString = "DeInitialized";
	const string deviceErrorString = "Error";
	const string deviceServiceAdded = "ServiceAdded";
	const string deviceStartedAdvertising = "StartedAdvertising";
	const string deviceStoppedAdvertising = "StoppedAdvertising";
	const string deviceDiscoveredPeripheral = "DiscoveredPeripheral";
	const string deviceRetrievedConnectedPeripheral = "RetrievedConnectedPeripheral";
    const string deviceRetrievedPeripherals = "RetrievedPeripherals";
	const string devicePeripheralReceivedWriteData = "PeripheralReceivedWriteData";
	const string deviceConnectedPeripheral = "ConnectedPeripheral";
	const string deviceDisconnectedPeripheral = "DisconnectedPeripheral";
	const string deviceDiscoveredService = "DiscoveredService";
	const string deviceDiscoveredCharacteristic = "DiscoveredCharacteristic";
	const string deviceDidWriteCharacteristic = "DidWriteCharacteristic";
	const string deviceDidUpdateNotificationStateForCharacteristic = "DidUpdateNotificationStateForCharacteristic";
	const string deviceDidUpdateValueForCharacteristic = "DidUpdateValueForCharacteristic";
    const string devicePowerStateUpdated = "PowerState";

    public const string ErrorBLENotAvailable = "BLE Not Available";
    public const string ErrorWriteCharacteristic = "WriteCharacteristic";
    public const string ErrorUpdateNotificationState = "UpdateNotificationState";
    public const string ErrorServiceDiscovery = "DiscoverService";

	public void OnBluetoothMessage (string message)
	{
		if (message != null)
		{
			char[] delim = new char[] { '~' };
			string[] parts = message.Split (delim);

#if BLE_DEBUG_LOG
			for (int i = 0; i < parts.Length; ++i)
				BluetoothLEHardwareInterface.Log(string.Format ("Part: {0} - {1}", i, parts[i]));
#endif

			if (message.StartsWith(deviceInitializedString))
			{
				if (InitializedAction != null)
					InitializedAction ();
			}
			else if (message.StartsWith(deviceDeInitializedString))
			{
				BluetoothLEHardwareInterface.FinishDeInitialize ();
				
				if (DeinitializedAction != null)
					DeinitializedAction ();
			}
			else if (message.StartsWith(deviceErrorString))
			{
				string error = "";

				if (parts.Length >= 2)
					error = parts[1];

                if (ErrorAction != null)
                    ErrorAction(error, parts.Skip(2).ToArray());
			}
			else if (message.StartsWith(deviceServiceAdded))
			{
				if (parts.Length >= 2)
				{
					if (ServiceAddedAction != null)
						ServiceAddedAction (parts[1]);
				}
			}
			else if (message.StartsWith(deviceStartedAdvertising))
			{
#if BLE_DEBUG_LOG
				BluetoothLEHardwareInterface.Log("Started Advertising");
#endif

				if (StartedAdvertisingAction != null)
					StartedAdvertisingAction ();
			}
			else if (message.StartsWith(deviceStoppedAdvertising))
			{
#if BLE_DEBUG_LOG
				BluetoothLEHardwareInterface.Log("Stopped Advertising");
#endif

				if (StoppedAdvertisingAction != null)
					StoppedAdvertisingAction ();
			}
			else if (message.StartsWith(deviceDiscoveredPeripheral))
			{
				if (parts.Length >= 3)
				{
					// the first callback will only get called the first time this device is seen
					// this is because it gets added to the a list in the DiscoveredDeviceList
					// after that only the second callback will get called and only if there is
					// advertising data available
					if (!DiscoveredDeviceList.Contains (parts[1]))
					{
						DiscoveredDeviceList.Add (parts[1]);

						if (DiscoveredPeripheralAction != null)
							DiscoveredPeripheralAction (parts[1], parts[2]);
					}
					
					if (parts.Length >= 5 && DiscoveredPeripheralWithAdvertisingInfoAction != null)
					{
						// get the rssi from the 4th value
						int rssi = 0;
						if (!int.TryParse (parts[3], out rssi))
							rssi = 0;
						
						// parse the base 64 encoded data that is the 5th value
						byte[] bytes = System.Convert.FromBase64String(parts[4]);
						
						DiscoveredPeripheralWithAdvertisingInfoAction(parts[1], parts[2], rssi, bytes);
					}
				}
			}
			else if (message.StartsWith(deviceRetrievedConnectedPeripheral))
			{
				if (parts.Length >= 3)
				{
					DiscoveredDeviceList.Add (parts[1]);
					
					if (RetrievedConnectedPeripheralAction != null)
						RetrievedConnectedPeripheralAction (parts[1], parts[2]);
				}
			}
			else if (message.StartsWith(devicePeripheralReceivedWriteData))
			{
				if (parts.Length >= 3)
					OnPeripheralData (parts[1], parts[2]);
			}
			else if (message.StartsWith(deviceConnectedPeripheral))
			{
				if (parts.Length >= 2 && ConnectedPeripheralAction != null)
					ConnectedPeripheralAction (parts[1]);
			}
			else if (message.StartsWith(deviceDisconnectedPeripheral))
			{
				if (parts.Length >= 2)
				{
					if (ConnectedDisconnectPeripheralAction != null)
						ConnectedDisconnectPeripheralAction (parts[1]);

					if (DisconnectedPeripheralAction != null)
						DisconnectedPeripheralAction (parts[1]);
				}
			}
			else if (message.StartsWith(deviceDiscoveredService))
			{
				if (parts.Length >= 3 && DiscoveredServiceAction != null)
					DiscoveredServiceAction (parts[1], parts[2]);
			}
			else if (message.StartsWith(deviceDiscoveredCharacteristic))
			{
				if (parts.Length >= 4 && DiscoveredCharacteristicAction != null)
					DiscoveredCharacteristicAction (parts[1], parts[2], parts[3]);
			}
			else if (message.StartsWith(deviceDidWriteCharacteristic))
			{
				if (parts.Length >= 2 && DidWriteCharacteristicAction != null)
					DidWriteCharacteristicAction (parts[1]);

				if (parts.Length >= 3 && DidWriteCharacteristicWithDeviceAddressAction != null)
					DidWriteCharacteristicWithDeviceAddressAction (parts[1], parts[2]);
			}
			else if (message.StartsWith(deviceDidUpdateNotificationStateForCharacteristic))
			{
				if (parts.Length >= 3)
				{
					if (DidUpdateNotificationStateForCharacteristicAction != null && DidUpdateNotificationStateForCharacteristicAction.ContainsKey (parts[1]))
				    {
						var characteristicAction = DidUpdateNotificationStateForCharacteristicAction[parts[1]];
						if (characteristicAction != null && characteristicAction.ContainsKey (parts[2]))
						{
							var action = characteristicAction[parts[2]];
							if (action != null)
								action (parts[2]);
						}
					}

					if (DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction != null && DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey (parts[1]))
					{
						var characteristicAction = DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[parts[1]];
						if (characteristicAction != null && characteristicAction.ContainsKey (parts[2]))
						{
							var action = characteristicAction[parts[2]];
							if (action != null)
								action (parts[1], parts[2]);
						}
					}
				}
			}
			else if (message.StartsWith(deviceDidUpdateValueForCharacteristic))
			{
				if (parts.Length >= 4)
					OnBluetoothData (parts[1], parts[2], parts[3]);
			}
            else if (message.StartsWith(deviceRetrievedPeripherals))
            {
                if (RetrievedPeripheralsAction != null)
                {
                    RetrievedPeripheralsAction();
                }
            }
            else if (message.StartsWith(devicePowerStateUpdated))
            {
                var poweredOn = int.Parse(parts[1]) != 0;
                if (PowerStateUpdated != null)
                {
                    PowerStateUpdated(poweredOn);
                }
            }
		}
	}

	public void OnBluetoothData (string base64Data)
	{
		OnBluetoothData ("", "", base64Data);
	}

	public void OnBluetoothData (string deviceAddress, string characteristic, string base64Data)
	{
		if (base64Data != null)
		{
			byte[] bytes = System.Convert.FromBase64String(base64Data);
			if (bytes.Length > 0)
			{
				deviceAddress = deviceAddress.ToUpper ();
#if UNITY_ANDROID
                characteristic = characteristic.ToLower();
#else
				characteristic = characteristic.ToUpper ();
#endif

#if BLE_DEBUG_LOG
				BluetoothLEHardwareInterface.Log("Device: " + deviceAddress + " Characteristic Received: " + characteristic);

				string byteString = "";
				foreach (byte b in bytes)
					byteString += string.Format("{0:X2}", b);

				BluetoothLEHardwareInterface.Log(byteString);
#endif

				if (DidUpdateCharacteristicValueAction != null && DidUpdateCharacteristicValueAction.ContainsKey (deviceAddress))
				{
					var characteristicAction = DidUpdateCharacteristicValueAction[deviceAddress];
					if (characteristicAction != null && characteristicAction.ContainsKey (characteristic))
					{
						var action = characteristicAction[characteristic];
						if (action != null)
							action (characteristic, bytes);
					}
				}
				
				if (DidUpdateCharacteristicValueWithDeviceAddressAction != null && DidUpdateCharacteristicValueWithDeviceAddressAction.ContainsKey (deviceAddress))
				{
					var characteristicAction = DidUpdateCharacteristicValueWithDeviceAddressAction[deviceAddress];
					if (characteristicAction != null && characteristicAction.ContainsKey (characteristic))
					{
						var action = characteristicAction[characteristic];
						if (action != null)
							action (deviceAddress, characteristic, bytes);
					}
				}
			}
		}
	}
	
	public void OnPeripheralData (string characteristic, string base64Data)
	{
		if (base64Data != null)
		{
			byte[] bytes = System.Convert.FromBase64String(base64Data);
			if (bytes.Length > 0)
			{
#if BLE_DEBUG_LOG
				BluetoothLEHardwareInterface.Log("Peripheral Received: " + characteristic);
				
				string byteString = "";
				foreach (byte b in bytes)
					byteString += string.Format("{0:X2}", b);
				
				BluetoothLEHardwareInterface.Log(byteString);
#endif
				
				if (PeripheralReceivedWriteDataAction != null)
					PeripheralReceivedWriteDataAction (characteristic, bytes);
			}
		}
	}
}
