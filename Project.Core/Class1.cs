using System;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Networking;
using Windows.UI.Core;
using Windows.Storage.Streams;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Devices.Gpio;
using Windows.System.Profile;
using Windows.Devices.SerialCommunication;
using Windows.Devices.Enumeration;

namespace Project.Core
{
    public class Class1
    {
        private DatagramSocket sender;
        private DataWriter dataWriter;

        private DatagramSocket IoTSender;
        private DataWriter IoTDataWriter;

        private DatagramSocket IoTBrightnessSender;
        private DataWriter IoTBrightnessDataWriter;

        private SerialDevice serialPort;
        private DataWriter serialDataWriter;

        string raspberrysOriginalIPAddress = "172.20.10.9";
        string pcsOriginalIPAddress = "172.20.10.4";

        public async void Serial()
        {
            string aqs = SerialDevice.GetDeviceSelector("UART0");                   /* Find the selector string for the serial device   */
            var dis = await DeviceInformation.FindAllAsync(aqs, null);                    /* Find the serial device with our selector string  */
            if (dis.Count == 0) return;
            serialPort = await SerialDevice.FromIdAsync(dis[0].Id);    /* Create an serial device with our selected device */
            if (serialPort == null) return;

            serialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
            serialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
            serialPort.BaudRate = 38400;                                             /* mini UART: only standard baudrates */
            serialPort.Parity = SerialParity.None;                                  /* mini UART: no parities */
            serialPort.StopBits = SerialStopBitCount.One;                           /* mini UART: 1 stop bit */
            serialPort.DataBits = 8;
            serialPort.Handshake = SerialHandshake.None;
        }

        public static async Task showProgressScreen(VoiceCommandServiceConnection voiceServiceConnection, string message)
        {
            VoiceCommandUserMessage userProgressMessage = new VoiceCommandUserMessage();
            userProgressMessage.DisplayMessage = userProgressMessage.SpokenMessage = message;
            VoiceCommandResponse response = VoiceCommandResponse.CreateResponse(userProgressMessage);
            await voiceServiceConnection.ReportProgressAsync(response);
            return;
        }

        public async void SendLampBrightnessToPC(string message,string pcsIPAddress)//Send lamp's brightness info to pc
        {
            if (IoTSender == null || pcsIPAddress!=pcsOriginalIPAddress)
            {
                pcsOriginalIPAddress = pcsIPAddress;
                try
                {
                    HostName hostname = new HostName(pcsOriginalIPAddress);
                    IoTSender = new DatagramSocket();
                    await IoTSender.ConnectAsync(hostname, "2213");
                    IoTDataWriter = new DataWriter(IoTSender.OutputStream);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            try
            {
                IoTDataWriter.WriteString(message);
                await IoTDataWriter.StoreAsync();
            }
            catch (Exception)
            {
                return;
            }

        }

        public async void SendSensorMessagesToPC(string message,string pcsIPAddress) //send the light sensor's data to PC
        {
            if (IoTBrightnessSender == null || pcsIPAddress!=pcsOriginalIPAddress)
            {
                pcsOriginalIPAddress = pcsIPAddress;
                try
                {
                    HostName hostname = new HostName(pcsOriginalIPAddress);
                    IoTBrightnessSender = new DatagramSocket();
                    await IoTBrightnessSender.ConnectAsync(hostname, "2214");
                    IoTBrightnessDataWriter = new DataWriter(IoTBrightnessSender.OutputStream);
                }
                catch (Exception)
                {
                    return;
                }
            }
            try
            {
                IoTBrightnessDataWriter.WriteString(message);
                await IoTBrightnessDataWriter.StoreAsync();
            }
            catch (Exception)
            {
                return;
            }
        }

        public async void SendMessagesToRasp(string message,string ipAddress) //send command from PC or phone to RasPi
        {
            if (sender == null || ipAddress != raspberrysOriginalIPAddress)
            {
                raspberrysOriginalIPAddress = ipAddress;
                try
                {
                    HostName hostname = new HostName(ipAddress);
                    sender = new DatagramSocket();
                    await sender.ConnectAsync(hostname, "2000");
                    dataWriter = new DataWriter(sender.OutputStream);
                }
                catch (Exception)
                {
                    return;
                }
            }
            try
            {
                dataWriter.WriteString(message);
                await dataWriter.StoreAsync();
            }
            catch (Exception)
            {
                return;
            }
        }

        public bool determin()
        {

            AnalyticsVersionInfo info = AnalyticsInfo.VersionInfo;
            if (info.DeviceFamily == "Windows.IoT")
                return true;
            else
                return false;
        }

        public async void SendSerialMess(double messages)
        {
            if (serialPort != null)
            {
                if (serialDataWriter==null)
                {
                    serialDataWriter = new DataWriter(serialPort.OutputStream);
                }
                await WriteAsync(messages);
            }
        }

        private async Task WriteAsync(double messages)
        {
            Task<UInt32> storeAsyncTask;
            int testValue = int.Parse(messages.ToString());
            byte[] d = BitConverter.GetBytes(testValue);
            serialDataWriter.WriteByte(d[0]);
            storeAsyncTask = serialDataWriter.StoreAsync().AsTask();
            UInt32 bytesWritten = await storeAsyncTask;
        }

        public async void ReportSuccess(VoiceCommandServiceConnection voiceCommandServiceConnection)
        {
            VoiceCommandUserMessage userMessage = new VoiceCommandUserMessage();
            userMessage.SpokenMessage = userMessage.DisplayMessage = "已成功";
            VoiceCommandResponse response = VoiceCommandResponse.CreateResponse(userMessage);
            await voiceCommandServiceConnection.ReportSuccessAsync(response);
        }

        public string[] ProcessUDPMessage(string message)
        {
            string[] messageArray = message.Split('-');
            return messageArray;
        }
    }
    public class Rooms
    {
        public double RoomID { get; set; }
        public double brightness { get; set; }
        public double temperature { get; set; }
    }

}
