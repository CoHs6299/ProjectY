using System;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Project.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Composition;
using Microsoft.Graphics.Canvas.Effects;
using Windows.UI;
using Windows.Devices.I2c;

namespace ProjectY
{
    public sealed partial class MainPage : Page
    {
        private DispatcherTimer timer;
        private I2cDevice device;
        private DatagramSocket listener;
        private DatagramSocket controllersListener;
        private DatagramSocket controllersBrightnessListener;

        string raspberrysOriginalIPAddress = "172.20.10.9";
        string pcsOriginalIPAddress = "172.20.10.4";

        Class1 helper = new Class1();
        Rooms bedroom = new Rooms();
        Rooms kitchen = new Rooms();

        public MainPage()
        {
            this.InitializeComponent();
            bedroom.brightness = 50;
            bedroom.RoomID = 1;
            bedroom.temperature = 50;
            kitchen.RoomID = 2;
            kitchen.brightness = 50;
            kitchen.temperature = 50;
            slider.Value = bedroom.brightness;
            temperatureSlider.Value = bedroom.temperature;
            kitchenslider.Value = kitchen.brightness;
            KitchenTemperatureSlider.Value = kitchen.temperature;
            if (helper.determin())
            {
                IPBox.PlaceholderText = "请输入PC或手机的IP地址";
                DatagramSocketConnect();
                helper.Serial();
            }
            else
            {
                IPBox.PlaceholderText = "请输入Raspberry Pi的IP地址";
                ControllerDatagramSocketConnect();
                ControllersBrightnessDatagramSocketConnect();
            }
        }

        public async void I2C()
        {
            var settings = new I2cConnectionSettings(0x23);
            settings.BusSpeed = I2cBusSpeed.FastMode;
            var controller = await I2cController.GetDefaultAsync();
            device = controller.GetDevice(settings);
            byte[] writeBuf = { 0x01 };
            byte[] dataBuf = new byte[2];
            try
            {
                device.Write(writeBuf);
            }
            catch (Exception e)
            {
                lightSensorText.Text = "传感器故障或未连接，错误信息："+e.Message;
                return;
            }
        }



        private async void DatagramSocketConnect()
        {
            listener = new DatagramSocket();
            listener.MessageReceived += Listener_MessageReceived;
            await listener.BindServiceNameAsync("2000");
        }

        private async void ControllerDatagramSocketConnect()
        {
            controllersListener = new DatagramSocket();
            controllersListener.MessageReceived += ControllersListener_MessageReceived;
            await controllersListener.BindServiceNameAsync("2213");
        }

        private async void ControllersBrightnessDatagramSocketConnect()
        {
            controllersBrightnessListener = new DatagramSocket();
            controllersBrightnessListener.MessageReceived += ControllersBrightnessListener_MessageReceived;
            await controllersBrightnessListener.BindServiceNameAsync("2214");
        }

        private void ControllersBrightnessListener_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            DataReader ControllersBrightnessreader = args.GetDataReader();
            uint ControllerstringLength = ControllersBrightnessreader.UnconsumedBufferLength;
            var ignore = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var ControllersBrightnessresult = ControllersBrightnessreader.ReadString(ControllerstringLength);
                lightSensorText.Text = ControllersBrightnessresult;
            });
        }

        private void ControllersListener_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            DataReader Controllersreader = args.GetDataReader();
            uint ControllerstringLength = Controllersreader.UnconsumedBufferLength;
            var ignore = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var Controllersresult = Controllersreader.ReadString(ControllerstringLength);
                lampBrightnessText.Text = Controllersresult;
            });
        }

        private void Listener_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            DataReader reader = args.GetDataReader();
            uint stringLength = reader.UnconsumedBufferLength;
            var ignore = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var result = reader.ReadString(stringLength);
                if (result == "SwitchOn")
                    tog.IsOn = true;
                if (result == "SwitchOff")
                    tog.IsOn = false;
                else
                {
                    try
                    {
                        var resultArray = helper.ProcessUDPMessage(result);
                        switch (double.Parse(resultArray[0]))
                        {
                            case 1:
                                if (resultArray[1] == "210" || resultArray[1] == "211")
                                {
                                    switch (resultArray[1])
                                    {
                                        case "210":
                                            if (bedroom.brightness + 25 <= 100)
                                                bedroom.brightness += 25;
                                            break;
                                        case "211":
                                            if (bedroom.brightness - 25 >= 0)
                                                bedroom.brightness -= 25;
                                            break;
                                        default:
                                            break;
                                    }
                                    helper.SendSerialMess(253);
                                    helper.SendSerialMess(1);
                                    helper.SendSerialMess(bedroom.brightness);
                                    helper.SendSerialMess(bedroom.temperature);
                                    helper.SendSerialMess(254);
                                    lampBrightnessText.Text = "当前亮度为： " + bedroom.brightness + "%";
                                }
                                else
                                {
                                    bedroom.brightness = double.Parse(resultArray[1]);
                                    bedroom.temperature = double.Parse(resultArray[2]);
                                    helper.SendSerialMess(253);
                                    helper.SendSerialMess(1);
                                    helper.SendSerialMess(bedroom.brightness);
                                    helper.SendSerialMess(bedroom.temperature);
                                    helper.SendSerialMess(254);
                                    lampBrightnessText.Text = "当前亮度为： " + bedroom.brightness + "%";
                                }
                                break;
                            case 2:
                                if (resultArray[1] == "210" || resultArray[1] == "211")
                                {
                                    switch (resultArray[1])
                                    {
                                        case "210":
                                            if (kitchen.brightness + 25 <= 100)
                                                kitchen.brightness += 25;
                                            break;
                                        case "211":
                                            if (kitchen.brightness - 25 >= 0)
                                                kitchen.brightness -= 25;
                                            break;
                                        default:
                                            break;
                                    }
                                    helper.SendSerialMess(253);
                                    helper.SendSerialMess(2);
                                    helper.SendSerialMess(kitchen.brightness);
                                    helper.SendSerialMess(kitchen.temperature);
                                    helper.SendSerialMess(254);
                                    lampBrightnessText.Text = "当前亮度为： " + result + "%";
                                }
                                else
                                {
                                    kitchen.brightness = double.Parse(resultArray[1]);
                                    kitchen.temperature = double.Parse(resultArray[2]);
                                    helper.SendSerialMess(253);
                                    helper.SendSerialMess(2);
                                    helper.SendSerialMess(kitchen.brightness);
                                    helper.SendSerialMess(kitchen.temperature);
                                    helper.SendSerialMess(254);
                                    lampBrightnessText.Text = "当前亮度为： " + kitchen.brightness + "%";
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        return;
                    }

                }
            });
        }

        private void slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            bedroom.brightness = e.NewValue;
            if (tog.IsOn==true)
            {
                tog.IsOn = false;
            }
            if (helper.determin())
            {
                helper.SendSerialMess(253);
                helper.SendSerialMess(1);
                helper.SendSerialMess(bedroom.brightness);
                helper.SendSerialMess(bedroom.temperature);
                helper.SendSerialMess(254);
                lampBrightnessText.Text = "当前亮度为： " + bedroom.brightness.ToString() + "%";
                helper.SendLampBrightnessToPC("当前亮度为: " + bedroom.brightness.ToString() + "%",pcsOriginalIPAddress);
            }
            else
            {
                helper.SendMessagesToRasp("1" + "-" + bedroom.brightness.ToString() + "-" + bedroom.temperature.ToString(), raspberrysOriginalIPAddress);
                lampBrightnessText.Text = "当前亮度为： " + bedroom.brightness.ToString() + "%";
            }
        }

        private void temperatureSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            bedroom.temperature = e.NewValue;
            if (tog.IsOn == true)
            {
                tog.IsOn = false;
            }
            if (helper.determin())
            {
                helper.SendSerialMess(253);
                helper.SendSerialMess(1);
                helper.SendSerialMess(bedroom.brightness);
                helper.SendSerialMess(bedroom.temperature);
                helper.SendSerialMess(254);
            }
            else
            {
                helper.SendMessagesToRasp("1" + "-" + bedroom.brightness.ToString() + "-" + bedroom.temperature.ToString(), raspberrysOriginalIPAddress);
            }
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (helper.determin())
            {
                if (tog.IsOn == true)
                {
                    I2C();
                    timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromMilliseconds(100);
                    timer.Tick += Timer_Tick1;
                    timer.Start();
                }
                else
                {
                    if (timer != null)
                    {
                        timer.Stop();
                    }
                    device.Dispose();
                }
            }
            else
            {
                if (tog.IsOn == true)
                {
                    helper.SendMessagesToRasp("SwitchOn",raspberrysOriginalIPAddress);
                    helper.SendMessagesToRasp("1" + "-" + 30 + "-" + 80, raspberrysOriginalIPAddress);
                    helper.SendMessagesToRasp("2" + "-" + 30 + "-" + 80, raspberrysOriginalIPAddress);
                }
                else
                {
                    helper.SendMessagesToRasp("SwitchOff",raspberrysOriginalIPAddress);
                }
            }
        }

        private void Timer_Tick1(object sender, object e)
        {
            try
            {
                byte[] regAddrBuf = new byte[] { 0x23 };
                byte[] readBuf = new byte[2];
                device.WriteRead(regAddrBuf, readBuf);
                var valf = ((readBuf[0] << 8) | readBuf[1]) / 1.2;
                int a = (int)valf;
                lightSensorText.Text = "室内亮度为: " + a.ToString() + " lx";
                helper.SendSensorMessagesToPC("室内亮度为: " + a.ToString() + " lx",pcsOriginalIPAddress);
            }
            catch (Exception)
            {
                timer.Stop();
            }
            
        }

        private void upload_Click(object sender, RoutedEventArgs e)
        {
            if (helper.determin())
            {
                pcsOriginalIPAddress = IPBox.Text;
                IPBox.Text = pcsOriginalIPAddress;
            }
            else
            {
                raspberrysOriginalIPAddress = IPBox.Text;
                IPBox.Text = raspberrysOriginalIPAddress;
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (helper.determin())
            {
                pcsOriginalIPAddress = "172.20.10.4";
                IPBox.Text = pcsOriginalIPAddress;
            }
            else
            {
                raspberrysOriginalIPAddress = "172.20.10.9";
                IPBox.Text = raspberrysOriginalIPAddress;
            }
        }

        private void kitchenslider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            kitchen.brightness = e.NewValue;
            if (tog.IsOn == true)
            {
                tog.IsOn = false;
            }
            if (helper.determin())
            {
                helper.SendSerialMess(253);
                helper.SendSerialMess(2);
                helper.SendSerialMess(kitchen.brightness);
                helper.SendSerialMess(kitchen.temperature);
                helper.SendSerialMess(254);
                helper.SendLampBrightnessToPC("当前亮度为: " + kitchen.brightness.ToString() + "%", pcsOriginalIPAddress);
            }
            else
            {
                helper.SendMessagesToRasp("2" + "-" + kitchen.brightness.ToString() + "-" + kitchen.temperature.ToString(), raspberrysOriginalIPAddress);
            }
        }

        private void KitchenTemperatureSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            kitchen.temperature = e.NewValue;
            if (tog.IsOn == true)
            {
                tog.IsOn = false;
            }
            if (helper.determin())
            {
                helper.SendSerialMess(253);
                helper.SendSerialMess(2);
                helper.SendSerialMess(kitchen.brightness);
                helper.SendSerialMess(kitchen.temperature);
                helper.SendSerialMess(254);
            }
            else
            {
                helper.SendMessagesToRasp("2" + "-" + kitchen.brightness.ToString() + "-" + kitchen.temperature.ToString(), raspberrysOriginalIPAddress);
            }
        }

        private void HubSection_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            helper.SendMessagesToRasp("2"+"-"+"30"+"-"+"40",raspberrysOriginalIPAddress);
        }

        private void HubSection_Tapped_1(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            helper.SendMessagesToRasp("2" + "-" + "30" + "-" + "80", raspberrysOriginalIPAddress);

        }

        private void HubSection_Tapped_2(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            helper.SendMessagesToRasp("2" + "-" + "20" + "-" + "80", raspberrysOriginalIPAddress);
        }
    }
}
