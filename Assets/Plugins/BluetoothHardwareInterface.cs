using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using AOT;

public class BluetoothLEHardwareInterface
{
	public enum CBCharacteristicProperties
	{
		CBCharacteristicPropertyBroadcast = 0x01,
		CBCharacteristicPropertyRead = 0x02,
		CBCharacteristicPropertyWriteWithoutResponse = 0x04,
		CBCharacteristicPropertyWrite = 0x08,
		CBCharacteristicPropertyNotify = 0x10,
		CBCharacteristicPropertyIndicate = 0x20,
		CBCharacteristicPropertyAuthenticatedSignedWrites = 0x40,
		CBCharacteristicPropertyExtendedProperties = 0x80,
		CBCharacteristicPropertyNotifyEncryptionRequired = 0x100,
		CBCharacteristicPropertyIndicateEncryptionRequired = 0x200,
	};

	public  enum CBAttributePermissions
	{
		CBAttributePermissionsReadable = 0x01,
		CBAttributePermissionsWriteable = 0x02,
		CBAttributePermissionsReadEncryptionRequired = 0x04,
		CBAttributePermissionsWriteEncryptionRequired = 0x08,
	};

#if UNITY_IPHONE && !UNITY_EDITOR
    private const string DllName = "__Internal";
#else
    private const string DllName = "bluetoothle";
#endif

#if UNITY_IPHONE || UNITY_STANDALONE_OSX
	[DllImport (DllName)]
	private static extern void _iOSBluetoothLELog (string message);
	
	[DllImport (DllName)]
	private static extern void _iOSBluetoothLEInitialize (bool asCentral, bool asPeripheral);
	
	[DllImport (DllName)]
	private static extern void _iOSBluetoothLEDeInitialize ();
	
	[DllImport (DllName)]
	private static extern void _iOSBluetoothLEScanForPeripheralsWithServices (string serviceUUIDsString, bool allowDuplicates, bool rssiOnly);
	
	[DllImport (DllName)]
	private static extern void _iOSBluetoothLERetrieveListOfPeripheralsWithServices (string serviceUUIDsString);

    [DllImport (DllName)]
    private static extern void _iOSBluetoothLERetrievePeripheralsWithIdentifiers (string identifiersString);

	[DllImport (DllName)]
	private static extern void _iOSBluetoothLEStopScan ();
	
	[DllImport (DllName)]
	private static extern void _iOSBluetoothLEConnectToPeripheral (string name);
	
	[DllImport (DllName)]
	private static extern void _iOSBluetoothLEDisconnectPeripheral (string name);
	
	[DllImport (DllName)]
	private static extern void _iOSBluetoothLEReadCharacteristic (string name, string service, string characteristic);

	[DllImport (DllName)]
	private static extern void _iOSBluetoothLEWriteCharacteristic (string name, string service, string characteristic, byte[] data, int length, bool withResponse);
	
	[DllImport (DllName)]
	private static extern void _iOSBluetoothLESubscribeCharacteristic (string name, string service, string characteristic);
	
	[DllImport (DllName)]
	private static extern void _iOSBluetoothLEUnSubscribeCharacteristic (string name, string service, string characteristic);

	[DllImport (DllName)]
	private static extern void _iOSBluetoothLEPeripheralName (string newName);

	[DllImport (DllName)]
	private static extern void _iOSBluetoothLECreateService (string uuid, bool primary);
	
	[DllImport (DllName)]
	private static extern void _iOSBluetoothLERemoveService (string uuid);
	
	[DllImport (DllName)]
	private static extern void _iOSBluetoothLERemoveServices ();

	[DllImport (DllName)]
	private static extern void _iOSBluetoothLECreateCharacteristic (string uuid, int properties, int permissions, byte[] data, int length);
	
	[DllImport (DllName)]
	private static extern void _iOSBluetoothLERemoveCharacteristic (string uuid);
	
	[DllImport (DllName)]
	private static extern void _iOSBluetoothLERemoveCharacteristics ();

	[DllImport (DllName)]
	private static extern void _iOSBluetoothLEStartAdvertising ();
	
	[DllImport (DllName)]
	private static extern void _iOSBluetoothLEStopAdvertising ();
	
	[DllImport (DllName)]
	private static extern void _iOSBluetoothLEDisconnectAll ();

	[DllImport (DllName)]
	private static extern void _iOSBluetoothLEUpdateCharacteristicValue (string uuid, byte[] data, int length);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet=CharSet.Ansi)]
    private delegate void MessageCallback(string data);

	[DllImport (DllName)]
	private static extern void _iOSBluetoothLESetMessageCallback (MessageCallback cb);

	// keep a reference to the delegate to avoid being garbage collected
	private static readonly MessageCallback s_messageCallback;

	static BluetoothLEHardwareInterface()
	{
		s_messageCallback = OnMessage;
		_iOSBluetoothLESetMessageCallback(s_messageCallback);
	}

	private static readonly List<string> s_messages = new List<string>();

#elif UNITY_ANDROID && !UNITY_EDITOR
	static AndroidJavaObject _android = null;
#endif

	private static BluetoothDeviceScript bluetoothDeviceScript;

	public static void Log (string message)
	{
#if UNITY_IPHONE || UNITY_STANDALONE_OSX
		_iOSBluetoothLELog (message);
#elif UNITY_ANDROID && !UNITY_EDITOR
		if (_android != null)
			_android.Call ("androidBluetoothLog", message);
#endif
	}

	public static BluetoothDeviceScript Initialize (bool asCentral, bool asPeripheral, Action action, Action<string, string[]> errorAction, Action<bool> powerStateAction = null)
	{
		bluetoothDeviceScript = null;

		GameObject bluetoothLEReceiver = GameObject.Find("BluetoothLEReceiver");
		if (bluetoothLEReceiver == null)
		{
			bluetoothLEReceiver = new GameObject("BluetoothLEReceiver");

			bluetoothDeviceScript = bluetoothLEReceiver.AddComponent<BluetoothDeviceScript>();
			if (bluetoothDeviceScript != null)
			{
				bluetoothDeviceScript.InitializedAction = action;
				bluetoothDeviceScript.ErrorAction = errorAction;
                bluetoothDeviceScript.PowerStateUpdated = powerStateAction;
			}
		}

		GameObject.DontDestroyOnLoad (bluetoothLEReceiver);

#if UNITY_IPHONE || UNITY_STANDALONE_OSX
		_iOSBluetoothLEInitialize (asCentral, asPeripheral);
#elif UNITY_ANDROID && !UNITY_EDITOR
		if (_android == null)
		{
			AndroidJavaClass javaClass = new AndroidJavaClass ("com.shatalmic.unityandroidbluetoothlelib.UnityBluetoothLE");
			_android = javaClass.CallStatic<AndroidJavaObject> ("getInstance");
		}

		if (_android != null)
			_android.Call ("androidBluetoothInitialize", asCentral, asPeripheral);
#else
		if (bluetoothDeviceScript != null)
			bluetoothDeviceScript.SendMessage ("OnBluetoothMessage", "Initialized");
#endif

		return bluetoothDeviceScript;
	}
	
	public static void DeInitialize (Action action)
	{
		if (bluetoothDeviceScript != null)
			bluetoothDeviceScript.DeinitializedAction = action;

#if UNITY_IPHONE || UNITY_STANDALONE_OSX
		_iOSBluetoothLEDeInitialize ();
#elif UNITY_ANDROID && !UNITY_EDITOR
		if (_android != null)
			_android.Call ("androidBluetoothDeInitialize");
#else
		if (bluetoothDeviceScript != null)
			bluetoothDeviceScript.SendMessage ("OnBluetoothMessage", "DeInitialized");
#endif
	}

	public static void FinishDeInitialize ()
	{
		GameObject bluetoothLEReceiver = GameObject.Find("BluetoothLEReceiver");
		if (bluetoothLEReceiver != null)
			GameObject.Destroy(bluetoothLEReceiver);
	}

	public static bool ScanForPeripheralsWithServices (string[] serviceUUIDs, Action<string, string> action, Action<string, string, int, byte[]> actionAdvertisingInfo = null, bool rssiOnly = false)
	{
		if (bluetoothDeviceScript != null)
		{
			bluetoothDeviceScript.DiscoveredPeripheralAction = action;
			bluetoothDeviceScript.DiscoveredPeripheralWithAdvertisingInfoAction = actionAdvertisingInfo;

			if (bluetoothDeviceScript.DiscoveredDeviceList != null)
				bluetoothDeviceScript.DiscoveredDeviceList.Clear ();
		}

		string serviceUUIDsString = null;

		if (serviceUUIDs != null && serviceUUIDs.Length > 0)
		{
			serviceUUIDsString = "";

			foreach (string serviceUUID in serviceUUIDs)
				serviceUUIDsString += serviceUUID + "|";

			serviceUUIDsString = serviceUUIDsString.Substring (0, serviceUUIDsString.Length - 1);
		}

#if UNITY_IPHONE || UNITY_STANDALONE_OSX
		_iOSBluetoothLEScanForPeripheralsWithServices (serviceUUIDsString, (actionAdvertisingInfo != null), rssiOnly);
        return true;
#elif UNITY_ANDROID && !UNITY_EDITOR
		if (_android != null)
		{
			if (serviceUUIDsString == null)
				serviceUUIDsString = "";

			return _android.Call<bool> ("androidBluetoothScanForPeripheralsWithServices", serviceUUIDsString, rssiOnly);
		}
#endif
        return false;
	}
	
	public static void RetrieveListOfPeripheralsWithServices (string[] serviceUUIDs, Action<string, string> action)
	{
		if (bluetoothDeviceScript != null)
		{
			bluetoothDeviceScript.RetrievedConnectedPeripheralAction = action;
			
			if (bluetoothDeviceScript.DiscoveredDeviceList != null)
				bluetoothDeviceScript.DiscoveredDeviceList.Clear ();
		}
		
		string serviceUUIDsString = serviceUUIDs.Length > 0 ? "" : null;
		
		foreach (string serviceUUID in serviceUUIDs)
			serviceUUIDsString += serviceUUID + "|";
		
		// strip the last delimeter
		serviceUUIDsString = serviceUUIDsString.Substring (0, serviceUUIDsString.Length - 1);
		
#if UNITY_IPHONE || UNITY_STANDALONE_OSX
		_iOSBluetoothLERetrieveListOfPeripheralsWithServices (serviceUUIDsString);
#elif UNITY_ANDROID && !UNITY_EDITOR
		if (_android != null)
			_android.Call ("androidBluetoothRetrieveListOfPeripheralsWithServices", serviceUUIDsString);
#endif
	}

    public static void RetrievePeripheralsWithIdentifiers(string[] identifiers, Action action)
    {
		if (bluetoothDeviceScript != null)
		{
			bluetoothDeviceScript.RetrievedPeripheralsAction = action;
		}

		string identifiersString = null;
		if (identifiers.Length > 0)
		{
			for (int i = 0; i < identifiers.Length; ++i)
			{
				if (i > 0)
				{
					identifiersString += "|";
				}
				identifiersString += identifiers[i];
			}
		}
#if UNITY_IPHONE || UNITY_STANDALONE_OSX
		_iOSBluetoothLERetrievePeripheralsWithIdentifiers(identifiersString);
#elif UNITY_ANDROID && !UNITY_EDITOR
		if (_android != null)
			_android.Call ("androidRetrievePeripheralsWithIdentifiers", identifiers);
#endif
    }

	public static void StopScan ()
	{
#if UNITY_IPHONE || UNITY_STANDALONE_OSX
		_iOSBluetoothLEStopScan ();
#elif UNITY_ANDROID && !UNITY_EDITOR
		if (_android != null)
			_android.Call ("androidBluetoothStopScan");
#endif
	}

	public static void DisconnectAll ()
	{
#if UNITY_IPHONE || UNITY_STANDALONE_OSX
		_iOSBluetoothLEDisconnectAll ();
#elif UNITY_ANDROID && !UNITY_EDITOR
		if (_android != null)
			_android.Call ("androidBluetoothDisconnectAll");
#endif
	}

	public static void ConnectToPeripheral (string name, Action<string> connectAction, Action<string, string> serviceAction, Action<string, string, string> characteristicAction, Action<string> disconnectAction = null)
	{
		if (bluetoothDeviceScript != null)
		{
			bluetoothDeviceScript.ConnectedPeripheralAction = connectAction;
			bluetoothDeviceScript.DiscoveredServiceAction = serviceAction;
			bluetoothDeviceScript.DiscoveredCharacteristicAction = characteristicAction;
			bluetoothDeviceScript.ConnectedDisconnectPeripheralAction = disconnectAction;
            bluetoothDeviceScript.DisconnectedPeripheralAction = null;
		}

#if UNITY_IPHONE || UNITY_STANDALONE_OSX
		_iOSBluetoothLEConnectToPeripheral (name);
#elif UNITY_ANDROID && !UNITY_EDITOR
		if (_android != null)
			_android.Call ("androidBluetoothConnectToPeripheral", name);
#endif
	}
	
	public static void DisconnectPeripheral (string name, Action<string> action)
	{
		if (bluetoothDeviceScript != null)
        {
            bluetoothDeviceScript.ConnectedDisconnectPeripheralAction = null;
			bluetoothDeviceScript.DisconnectedPeripheralAction = action;
        }
		
#if UNITY_IPHONE || UNITY_STANDALONE_OSX
		_iOSBluetoothLEDisconnectPeripheral (name);
#elif UNITY_ANDROID && !UNITY_EDITOR
		if (_android != null)
			_android.Call ("androidBluetoothDisconnectPeripheral", name);
#endif
	}

	public static void ReadCharacteristic (string name, string service, string characteristic, Action<string, byte[]> action)
	{
		if (bluetoothDeviceScript != null)
		{
			if (!bluetoothDeviceScript.DidUpdateCharacteristicValueAction.ContainsKey (name))
				bluetoothDeviceScript.DidUpdateCharacteristicValueAction[name] = new Dictionary<string, Action<string, byte[]>>();

#if UNITY_IPHONE || UNITY_STANDALONE_OSX
			bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] [characteristic] = action;
#elif UNITY_ANDROID && !UNITY_EDITOR
			bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] [FullUUID (characteristic).ToLower ()] = action;
#endif
		}

#if UNITY_IPHONE || UNITY_STANDALONE_OSX
		_iOSBluetoothLEReadCharacteristic (name, service, characteristic);
#elif UNITY_ANDROID && !UNITY_EDITOR
		if (_android != null)
			_android.Call ("androidReadCharacteristic", name, service, characteristic);
#endif
	}
	
	public static void WriteCharacteristic (string name, string service, string characteristic, byte[] data, int length, bool withResponse, Action<string> action)
	{
		if (bluetoothDeviceScript != null)
			bluetoothDeviceScript.DidWriteCharacteristicAction = action;
		
#if UNITY_IPHONE || UNITY_STANDALONE_OSX
		_iOSBluetoothLEWriteCharacteristic (name, service, characteristic, data, length, withResponse);
#elif UNITY_ANDROID && !UNITY_EDITOR
		if (_android != null)
			_android.Call ("androidWriteCharacteristic", name, service, characteristic, data, length, withResponse);
#endif
	}
	
	public static void WriteCharacteristicWithDeviceAddress (string name, string service, string characteristic, byte[] data, int length, bool withResponse, Action<string, string> action)
	{
		if (bluetoothDeviceScript != null)
			bluetoothDeviceScript.DidWriteCharacteristicWithDeviceAddressAction = action;
		
#if UNITY_IPHONE || UNITY_STANDALONE_OSX
		_iOSBluetoothLEWriteCharacteristic (name, service, characteristic, data, length, withResponse);
#elif UNITY_ANDROID && !UNITY_EDITOR
		if (_android != null)
			_android.Call ("androidWriteCharacteristic", name, service, characteristic, data, length, withResponse);
#endif
	}

	public static void SubscribeCharacteristic (string name, string service, string characteristic, Action<string> notificationAction, Action<string, byte[]> action)
	{
		if (bluetoothDeviceScript != null)
		{
			name = name.ToUpper ();
			service = service.ToUpper ();
			characteristic = characteristic.ToUpper ();
			
#if UNITY_IPHONE || UNITY_STANDALONE_OSX
			if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey (name))
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction [name] = new Dictionary<string, Action<string>> ();
			bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction [name] [characteristic] = notificationAction;

			if (!bluetoothDeviceScript.DidUpdateCharacteristicValueAction.ContainsKey (name))
				bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] = new Dictionary<string, Action<string, byte[]>> ();
			bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] [characteristic] = action;
#elif UNITY_ANDROID && !UNITY_EDITOR
			if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey (name))
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction [name] = new Dictionary<string, Action<string>> ();
			bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction [name] [FullUUID (characteristic).ToLower ()] = notificationAction;

			if (!bluetoothDeviceScript.DidUpdateCharacteristicValueAction.ContainsKey (name))
				bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] = new Dictionary<string, Action<string, byte[]>> ();
			bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] [FullUUID (characteristic).ToLower ()] = action;
#endif
		}

#if UNITY_IPHONE || UNITY_STANDALONE_OSX
		_iOSBluetoothLESubscribeCharacteristic (name, service, characteristic);
#elif UNITY_ANDROID && !UNITY_EDITOR
		if (_android != null)
			_android.Call ("androidSubscribeCharacteristic", name, service, characteristic);
#endif
	}
	
	public static void SubscribeCharacteristicWithDeviceAddress (string name, string service, string characteristic, Action<string, string> notificationAction, Action<string, string, byte[]> action)
	{
		if (bluetoothDeviceScript != null)
		{
			name = name.ToUpper ();
			service = service.ToUpper ();
			characteristic = characteristic.ToUpper ();

#if UNITY_IPHONE || UNITY_STANDALONE_OSX
			if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey (name))
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string>>();
			bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][characteristic] = notificationAction;

			if (!bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction.ContainsKey (name))
				bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string, byte[]>>();
			bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction[name][characteristic] = action;
#elif UNITY_ANDROID && !UNITY_EDITOR
			if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey (name))
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string>>();
			bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][FullUUID (characteristic).ToLower ()] = notificationAction;
			
			if (!bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction.ContainsKey (name))
				bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string, byte[]>>();
			bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction[name][FullUUID (characteristic).ToLower ()] = action;
#endif
		}
		
#if UNITY_IPHONE || UNITY_STANDALONE_OSX
		_iOSBluetoothLESubscribeCharacteristic (name, service, characteristic);
#elif UNITY_ANDROID && !UNITY_EDITOR
		if (_android != null)
			_android.Call ("androidSubscribeCharacteristic", name, service, characteristic);
#endif
	}

	public static void UnSubscribeCharacteristic (string name, string service, string characteristic, Action<string> action)
	{
		if (bluetoothDeviceScript != null)
		{
			name = name.ToUpper ();
			service = service.ToUpper ();
			characteristic = characteristic.ToUpper ();
			
#if UNITY_IPHONE || UNITY_STANDALONE_OSX
			if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey (name))
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string>>();
			bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][characteristic] = null;

			if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey (name))
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name] = new Dictionary<string, Action<string>> ();
			bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name][characteristic] = null;
#elif UNITY_ANDROID && !UNITY_EDITOR
			if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey (name))
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string>>();
			bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][FullUUID (characteristic).ToLower ()] = null;
			
			if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey (name))
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name] = new Dictionary<string, Action<string>> ();
			bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name][FullUUID (characteristic).ToLower ()] = null;
#endif
		}

#if UNITY_IPHONE || UNITY_STANDALONE_OSX
		_iOSBluetoothLEUnSubscribeCharacteristic (name, service, characteristic);
#elif UNITY_ANDROID && !UNITY_EDITOR
		if (_android != null)
			_android.Call ("androidUnsubscribeCharacteristic", name, service, characteristic);
#endif
	}

	public static void PeripheralName (string newName)
	{
#if UNITY_IPHONE || UNITY_STANDALONE_OSX
		_iOSBluetoothLEPeripheralName (newName);
#endif
	}

	public static void CreateService (string uuid, bool primary, Action<string> action)
	{
		if (bluetoothDeviceScript != null)
			bluetoothDeviceScript.ServiceAddedAction = action;

#if UNITY_IPHONE || UNITY_STANDALONE_OSX
		_iOSBluetoothLECreateService (uuid, primary);
#endif
	}
	
	public static void RemoveService (string uuid)
	{
#if UNITY_IPHONE || UNITY_STANDALONE_OSX
		_iOSBluetoothLERemoveService (uuid);
#endif
	}

	public static void RemoveServices ()
	{
#if UNITY_IPHONE || UNITY_STANDALONE_OSX
		_iOSBluetoothLERemoveServices ();
#endif
	}

	public static void CreateCharacteristic (string uuid, CBCharacteristicProperties properties, CBAttributePermissions permissions, byte[] data, int length, Action<string, byte[]> action)
	{
		if (bluetoothDeviceScript != null)
			bluetoothDeviceScript.PeripheralReceivedWriteDataAction = action;

#if UNITY_IPHONE || UNITY_STANDALONE_OSX
		_iOSBluetoothLECreateCharacteristic (uuid, (int)properties, (int)permissions, data, length);
#endif
	}

	public static void RemoveCharacteristic (string uuid)
	{
		if (bluetoothDeviceScript != null)
			bluetoothDeviceScript.PeripheralReceivedWriteDataAction = null;

#if UNITY_IPHONE || UNITY_STANDALONE_OSX
		_iOSBluetoothLERemoveCharacteristic (uuid);
#endif
	}

	public static void RemoveCharacteristics ()
	{
#if UNITY_IPHONE || UNITY_STANDALONE_OSX
		_iOSBluetoothLERemoveCharacteristics ();
#endif
	}
	
	public static void StartAdvertising (Action action)
	{
		if (bluetoothDeviceScript != null)
			bluetoothDeviceScript.StartedAdvertisingAction = action;

#if UNITY_IPHONE || UNITY_STANDALONE_OSX
		_iOSBluetoothLEStartAdvertising ();
#endif
	}
	
	public static void StopAdvertising (Action action)
	{
		if (bluetoothDeviceScript != null)
			bluetoothDeviceScript.StoppedAdvertisingAction = action;

#if UNITY_IPHONE || UNITY_STANDALONE_OSX
		_iOSBluetoothLEStopAdvertising ();
#endif
	}
	
	public static void UpdateCharacteristicValue (string uuid, byte[] data, int length)
	{
#if UNITY_IPHONE || UNITY_STANDALONE_OSX
		_iOSBluetoothLEUpdateCharacteristicValue (uuid, data, length);
#endif
	}
	
	public static string FullUUID (string uuid)
	{
		if (uuid.Length == 4)
			return "0000" + uuid + "-0000-1000-8000-00805f9b34fb";
		return uuid;
	}

    // only works on android
    public static bool IsEnabled()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_android != null)
        {
            return _android.Call<bool>("androidBluetoothIsEnabled");
        }
#endif
        return false;
    }

    // only works on android
    public static bool EnableBluetooth(bool enabled)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
		if (_android != null)
		{
			return _android.Call<bool>("androidBluetoothEnable", enabled);
		}
#endif
		return false;
    }

    public static void SetPowerStateChangedAction(Action<bool> onPowerState)
    {
        if (bluetoothDeviceScript != null)
        {
            bluetoothDeviceScript.PowerStateUpdated = onPowerState;
        }
    }

#if UNITY_STANDALONE_OSX || UNITY_IPHONE
    [MonoPInvokeCallback(typeof(MessageCallback))]
	private static void OnMessage(string data)
	{
        //Debug.Log(data);
		lock (s_messages)
		{
			s_messages.Add(data);
		}
	}

	public static void GetMessages(List<string> messages)
	{
		lock (s_messages)
		{
			messages.AddRange(s_messages);
			s_messages.Clear();
		}
	}
#endif
}
