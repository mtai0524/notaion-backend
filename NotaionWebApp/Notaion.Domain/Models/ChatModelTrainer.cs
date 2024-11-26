using Microsoft.ML;

namespace Notaion.Domain.Models
{
    public class ChatInput
    {
        public string Question { get; set; }
        public string Response { get; set; }
    }

    public class ChatPrediction
    {
        public string PredictedLabel { get; set; }
    }
    public class ChatModelTrainer
    {
        private const string ModelPath = "chatbot_model.zip";

        public void TrainModel(string trainingDataPath)
        {
            var mlContext = new MLContext();

            // Load dữ liệu huấn luyện
            var data = mlContext.Data.LoadFromTextFile<ChatInput>(
                path: trainingDataPath,
                hasHeader: true,
                separatorChar: ',');

            // Xây dựng pipeline huấn luyện
            var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(ChatInput.Question))
                .Append(mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(ChatInput.Response)))
                .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy())
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel", "Label"));

            // Huấn luyện mô hình
            var model = pipeline.Fit(data);

            // Lưu mô hình
            mlContext.Model.Save(model, data.Schema, ModelPath);
            Console.WriteLine($"Mô hình đã được lưu tại: {ModelPath}");
        }
    }
}
