using Newtonsoft.Json;
using Plugin.AudioRecorder;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Xamarin.Forms;
using XHApp.Actions;
using XHApp.Models;

namespace XHApp.CustomViews
{
    public class EnglishAssistantView : WebView
    {
        readonly AudioRecorderService recorder;

        public EnglishAssistantView()
        {
            recorder = new AudioRecorderService
            {
                StopRecordingAfterTimeout = true,
                AudioSilenceTimeout = TimeSpan.FromSeconds(60),
                TotalAudioTimeout = TimeSpan.FromSeconds(60)
            };
        }

        public async Task<string> LogIn()
        {
            var requestParams = new NameValueCollection();
            requestParams.Add("grant_type", CommonDefine.EnglishAssistantGrantType);
            requestParams.Add("id", CommonDefine.EnglishAssistantID);
            requestParams.Add("secret", CommonDefine.EnglishAssistantSecret);

            var token = await PPTSHttpClient.PostAsync<EnglishAssistantAccessToken>(CommonDefine.EnglishAssistantOAuthUrl, requestParams);

            return JsonConvert.SerializeObject(token);
        }

        public async Task<string> StartRecord()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Microphone);
                if (status != PermissionStatus.Granted)
                {
                    status = (await CrossPermissions.Current.RequestPermissionsAsync(Permission.Microphone))[Permission.Microphone];
                }

                if (status == PermissionStatus.Granted)
                {
                    if (recorder.IsRecording)
                    {
                        await recorder.StopRecording();
                    }

                    await recorder.StartRecording();
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Message", ex.Message, "OK");
            }

            return "ok";
        }

        public async Task<string> StopRecord()
        {
            try
            {
                if (recorder.IsRecording)
                {
                    await recorder.StopRecording();

                    using (var stream = recorder.GetAudioFileStream())
                    {

                        byte[] bytes = new byte[stream.Length];
                        stream.Read(bytes, 0, bytes.Length);

                        return Convert.ToBase64String(bytes);
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Message", ex.Message, "OK");
            }

            return "error";
        }
    }
}
