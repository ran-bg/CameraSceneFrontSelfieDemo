using System;

namespace DigitsSystemVolume
{
	public enum AudioDeviceType
	{
		UNKNOWN,
		AUX_LINE, //A device type describing the auxiliary line-level connectors. 
		BLUETOOTH_A2DP, //A device type describing a Bluetooth device supporting the A2DP profile. 
		BLUETOOTH_SCO, //A device type describing a Bluetooth device typically used for telephony. 
		BUILTIN_EARPIECE, //A device type describing the attached earphone speaker. 
		BUILTIN_MIC, //A device type describing the microphone(s) built in a device. 
		BUILTIN_SPEAKER, //A device type describing the speaker system (i.e. a mono speaker or stereo speakers) built in a device. 
		DOCK, //A device type describing the audio device associated with a dock. 
		FM, //A device type associated with the transmission of audio signals over FM. 
		FM_TUNER, //A device type for accessing the audio content transmitted over FM. 
		HDMI, //A device type describing an HDMI connection. 
		HDMI_ARC, //A device type describing the Audio Return Channel of an HDMI connection. 
		IP, //A device type connected over IP. 
		LINE_ANALOG, //A device type describing an analog line-level connection. 
		LINE_DIGITAL, //A device type describing a digital line connection (e.g. SPDIF). 
		TELEPHONY, //A device type describing the transmission of audio signals over the telephony network. 
		TV_TUNER, //A device type for accessing the audio content transmitted over the TV tuner system. 
		USB_ACCESSORY, //A device type describing a USB audio device in accessory mode. 
		USB_DEVICE, //A device type describing a USB audio device. 
		WIRED_HEADPHONES, //A device type describing a pair of wired headphones. 
		WIRED_HEADSET, //A device type describing a headset, which is the combination of a headphones and microphone. 
		AIRPLAY, //Output to a remote device over AirPlay.
		BLUETOOTH_HFP, //Output to a Bluetooth Hands-Free Profile device.
		BLUETOOTH_LE, //Output to a Bluetooth Low Energy (LE) peripheral.
		CAR_AUDIO //Output via Car Audio.
	}

	[Serializable]
	public class AudioOutputDevice
	{
		public string id;
		public string name;
		public AudioDeviceType type;
	}
}
