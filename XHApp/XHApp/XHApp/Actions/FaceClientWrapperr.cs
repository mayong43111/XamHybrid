using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Plugin.Media.Abstractions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using XHApp.Models;

namespace XHApp.Actions
{
    public class FaceClientWrapperr
    {
        public static FaceClientWrapperr Instace = new FaceClientWrapperr();

        private const string subscriptionKey = "fef8c74367ee4d39bf4bfe37a87193b5";//免费的，一分钟只能请求20次
        private const string personGroupId = "xd_developer_2";

        private const string detectionModel = "detection_02";
        private const string recognitionModel = "recognition_02";

        private const string faceEndpoint = "https://api.cognitive.azure.cn";
        private readonly IFaceClient faceClient = new FaceClient(new ApiKeyServiceClientCredentials(subscriptionKey), new DelegatingHandler[] { })
        {
            Endpoint = faceEndpoint
        };

        public async Task<ObservableCollection<FaceClientResultModel>> Detection(MediaFile file)
        {
            ObservableCollection<FaceClientResultModel> result = new ObservableCollection<FaceClientResultModel>();

            IList<FaceAttributeType> faceAttributes =
                new FaceAttributeType[]
                {
                                FaceAttributeType.Gender, FaceAttributeType.Age,
                                FaceAttributeType.Smile, FaceAttributeType.Emotion,
                                FaceAttributeType.Glasses, FaceAttributeType.Hair
                };

            IList<DetectedFace> faceList;
            IList<IdentifyResult> identifyResult = new List<IdentifyResult>();

            try
            {
                faceList = await faceClient.Face.DetectWithStreamAsync(file.GetStream(), true, false, recognitionModel: recognitionModel, detectionModel: detectionModel);
                var faceIds = faceList.Where(face => face.FaceId.HasValue).Select(face => face.FaceId.Value).ToList();
                var groupId = await EnsurePersonGroup(personGroupId);

                if (!string.IsNullOrEmpty(groupId))
                {
                    //一次最大10人
                    for (int i = 0; i < faceIds.Count; i += 10)
                    {
                        var rs = await faceClient.Face.IdentifyAsync(faceIds.Skip(i).Take(10).ToList(), personGroupId);
                        rs.ForEach(r => identifyResult.Add(r));
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            foreach (var face in faceList)
            {
                var item = new FaceClientResultModel()
                {
                    FaceID = face.FaceId.ToString(),
                    Name = await GetPersonNameAsync(identifyResult, face.FaceId),
                    ImageData = CaptureImage(
                                 file.GetStream(),
                                 face.FaceRectangle.Left,
                                 face.FaceRectangle.Top,
                                 face.FaceRectangle.Width,
                                 face.FaceRectangle.Height)
                };

                item.Image = ImageSource.FromStream(() => item.ImageData.AsStream());
                result.Add(item);
            }

            return result;
        }


        public async Task Training(FaceClientResultModel input)
        {
            // TODO：这里先不考虑超出调用限制的问题
            //https://docs.azure.cn/zh-cn/cognitive-services/face/face-api-how-to-topics/how-to-add-faces

            //这是个临时处理
            var personList = await faceClient.PersonGroupPerson.ListAsync(personGroupId);

            Person person = personList.FirstOrDefault(p => p.Name == input.Name);

            //添加人员，这里需要考虑当前用户是否已经添加用用户了，如果已添加过则在当前用户下添加照片
            if (person == null)
            {
                person = await faceClient.PersonGroupPerson.CreateAsync(personGroupId, input.Name);
            }

            await faceClient.PersonGroupPerson.AddFaceFromStreamAsync(personGroupId, person.PersonId, input.ImageData.AsStream(), detectionModel: detectionModel);

            //必须要训练
            await faceClient.PersonGroup.TrainAsync(personGroupId);
        }

        private Stream Base64StringToStream(string strbase64)
        {
            byte[] arr = Convert.FromBase64String(strbase64);
            MemoryStream ms = new MemoryStream(arr);

            return ms;
        }

        private async Task<string> EnsurePersonGroup(string personGroupId)
        {
            try
            {
                var personGroup = await faceClient.PersonGroup.GetAsync(personGroupId);
            }
            catch (APIErrorException ex)
            {
                if (ex.Response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    //await faceClient.PersonGroup.CreateAsync(personGroupId, "XD Developer"); 
                    await faceClient.PersonGroup.CreateAsync(personGroupId, "XD Developer 2", recognitionModel: recognitionModel);//recognition_02
                }
                else
                {
                    throw ex;
                }
            }

            TrainingStatus trainingStatus = null;
            try
            {
                trainingStatus = await faceClient.PersonGroup.GetTrainingStatusAsync(personGroupId);
            }
            catch { }

            return trainingStatus != null && trainingStatus.Status == TrainingStatusType.Succeeded ? personGroupId : string.Empty;
        }

        private async Task<string> GetPersonNameAsync(IList<IdentifyResult> identifyResults, Guid? faceID)
        {
            if (!faceID.HasValue)
            {
                return string.Empty;
            }

            var candidateIds = identifyResults.FirstOrDefault(id => id.FaceId == faceID.Value)?.Candidates;

            if (candidateIds != null && candidateIds.Any())
            {
                var person = await faceClient.PersonGroupPerson.GetAsync(personGroupId, candidateIds[0].PersonId);

                return person.Name;
            }

            return string.Empty;
        }

        private SKData CaptureImage(Stream stream, int offsetX, int offsetY, int width, int height)
        {
            using (var bitmap = SKBitmap.Decode(stream))
            {
                int expand = Convert.ToInt32(width * 0.1);

                //扩大截取的范围
                offsetX = offsetX > expand ? offsetX - expand : 0;
                offsetY = offsetY > expand ? offsetY - expand : 0;

                width = width + 2 * expand;
                width = bitmap.Width > width + offsetX ? width : bitmap.Width - offsetX;

                height = height + 2 * expand;
                height = bitmap.Height > height + offsetY ? height : bitmap.Height - offsetY;

                SKBitmap croppedBitmap = new SKBitmap(width, height);

                SKRect dest = new SKRect(0, 0, width, height);
                SKRect source = new SKRect(offsetX, offsetY, offsetX + width, offsetY + height);

                using (SKCanvas canvas = new SKCanvas(croppedBitmap))
                {
                    canvas.DrawBitmap(bitmap, source, dest);
                }

                var encoding = SKImage.FromBitmap(croppedBitmap).Encode();

                return encoding;
            }
        }
    }
}
