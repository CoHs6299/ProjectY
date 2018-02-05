using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Project.Core;

namespace ProjectY.Services.Background
{
    public sealed class GeneralQueryVoiceCommandService : IBackgroundTask
    {
        VoiceCommandServiceConnection voiceCommandServiceConnection;
        BackgroundTaskDeferral serviceDeferral;
        Class1 helper1 = new Class1();
        string ipAddress = "172.20.10.9";
       
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            serviceDeferral = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskInstance_Canceled;
            var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            if (triggerDetails != null && triggerDetails.Name == "GeneralQueryVoiceCommandService")
            {
                try
                {
                  
                    voiceCommandServiceConnection = VoiceCommandServiceConnection.FromAppServiceTriggerDetails(triggerDetails);
                    voiceCommandServiceConnection.VoiceCommandCompleted += VoiceCommandServiceConnection_VoiceCommandCompleted;
                    VoiceCommand voiceCommand = await voiceCommandServiceConnection.GetVoiceCommandAsync();
                    
                    switch (voiceCommand.CommandName)
                    {
                        case "On":
                            await Class1.showProgressScreen(voiceCommandServiceConnection, "正在打开" + voiceCommand.Properties["rooms"][0] + "的灯");
                            var OnRoom = voiceCommand.Properties["rooms"][0];
                            switch (OnRoom)
                            {
                                case "卧室":
                                    helper1.SendMessagesToRasp("1-100-50", ipAddress);
                                    helper1.ReportSuccess(voiceCommandServiceConnection);
                                    break;
                                case "厨房":
                                    helper1.SendMessagesToRasp("2-100-50", ipAddress);
                                    helper1.ReportSuccess(voiceCommandServiceConnection);
                                    break;
                                default:
                                    helper1.ReportSuccess(voiceCommandServiceConnection);
                                    break;
                            }
                            break;
                        case "Off":
                            await Class1.showProgressScreen(voiceCommandServiceConnection, "正在关闭" + voiceCommand.Properties["rooms"][0] + "的灯");
                            var OffRoom = voiceCommand.Properties["rooms"][0];
                            switch (OffRoom)
                            {
                                case "卧室":
                                    helper1.SendMessagesToRasp("1-0-50", ipAddress);
                                    break;
                                case "厨房":
                                    helper1.SendMessagesToRasp("2-0-50", ipAddress);
                                    break;
                                default:
                                    break;
                            }
                            helper1.ReportSuccess(voiceCommandServiceConnection);
                            break;
                        case "Brighter":
                            await Class1.showProgressScreen(voiceCommandServiceConnection, "正在增加" + voiceCommand.Properties["rooms"][0] + "的亮度");
                            var BrighterRoom = voiceCommand.Properties["rooms"][0];
                            switch (BrighterRoom)
                            {
                                case "卧室":
                                    helper1.SendMessagesToRasp("1-210-50", ipAddress);
                                    helper1.ReportSuccess(voiceCommandServiceConnection);
                                    break;
                                case "厨房":
                                    helper1.SendMessagesToRasp("2-210-50", ipAddress);
                                    helper1.ReportSuccess(voiceCommandServiceConnection);
                                    break;
                                default:
                                    helper1.ReportSuccess(voiceCommandServiceConnection);
                                    break;
                            }
                            break;
                        case "Darker":
                            await Class1.showProgressScreen(voiceCommandServiceConnection, "正在降低" + voiceCommand.Properties["rooms"][0] + "的亮度");
                            var DarkerRoom = voiceCommand.Properties["rooms"][0];
                            switch (DarkerRoom)
                            {
                                case "卧室":
                                    helper1.SendMessagesToRasp("1-211-50", ipAddress);
                                    helper1.ReportSuccess(voiceCommandServiceConnection);
                                    break;
                                case "厨房":
                                    helper1.SendMessagesToRasp("2-211-50", ipAddress);
                                    helper1.ReportSuccess(voiceCommandServiceConnection);
                                    break;
                                default:
                                    helper1.ReportSuccess(voiceCommandServiceConnection);
                                    break;
                            }
                            break;
                        default:
                            helper1.ReportSuccess(voiceCommandServiceConnection);
                            break;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (this.serviceDeferral != null)
            {
                this.serviceDeferral.Complete();
            }
        }

        private void VoiceCommandServiceConnection_VoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            if (this.serviceDeferral != null)
            {
                this.serviceDeferral.Complete();
            }
        }
    }
}
