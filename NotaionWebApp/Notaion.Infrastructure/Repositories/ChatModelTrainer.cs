using Microsoft.ML;
using Microsoft.ML.Data;

namespace Notaion.Infrastructure.Repositories
{
    // Định nghĩa lớp chứa dữ liệu huấn luyện
    public class ChatData
    {
        [LoadColumn(0)]
        public string Question { get; set; }

        [LoadColumn(1)]
        public string Response { get; set; }
    }

    // Lớp huấn luyện mô hình
    public class ChatModelTrainer
    {
        private readonly string ModelPath = Path.Combine(Directory.GetCurrentDirectory(), "chatbot_model.zip");

        // Huấn luyện mô hình từ file CSV
        public async Task TrainModelFromCsvAsync(string filePath)
        {
            try
            {
                var mlContext = new MLContext();

                // Đọc dữ liệu từ tệp CSV
                var data = mlContext.Data.LoadFromTextFile<ChatData>(filePath, separatorChar: ',', hasHeader: true);

                var preview = data.Preview(10); // Hiển thị 10 dòng đầu tiên của dữ liệu
                foreach (var row in preview.RowView)
                {
                    Console.WriteLine($"Question: {row.Values[0].Value}, Response: {row.Values[1].Value}");
                }

                // Xây dựng pipeline huấn luyện
                var pipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(ChatData.Question))  // Tính năng từ câu hỏi
                    .Append(mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(ChatData.Response)))  // Câu trả lời là nhãn
                    .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))  // Huấn luyện phân loại
                    .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel", "PredictedLabel"));

                // Huấn luyện mô hình
                var model = pipeline.Fit(data);

                // Kiểm tra kết quả trước khi lưu mô hình
                var transformer = model.Transform(data);
                var predictions = mlContext.Data.CreateEnumerable<ChatPrediction>(transformer, reuseRowObject: false).ToList();

                foreach (var prediction in predictions)
                {
                    if (string.IsNullOrWhiteSpace(prediction.PredictedLabel))
                    {
                        Console.WriteLine("Dự đoán bị thiếu cho câu hỏi: " + prediction.Question);
                    }
                    else
                    {
                        Console.WriteLine($"Câu hỏi: {prediction.Question}, Dự đoán câu trả lời: {prediction.PredictedLabel}");
                    }
                }

                // Lưu mô hình vào file
                mlContext.Model.Save(model, data.Schema, ModelPath);
                Console.WriteLine($"Mô hình đã được lưu tại: {ModelPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi huấn luyện mô hình: {ex.Message}");
                throw;
            }
        }

        // Dự đoán câu trả lời từ câu hỏi của người dùng
        public async Task<string> PredictResponseAsync(string userMessage)
        {
            try
            {
                var mlContext = new MLContext();

                // Kiểm tra xem mô hình có tồn tại không
                if (!File.Exists(ModelPath))
                {
                    throw new FileNotFoundException("Mô hình không tồn tại tại: " + ModelPath);
                }

                // Load mô hình đã huấn luyện
                var model = mlContext.Model.Load(ModelPath, out var modelInputSchema);

                // Tạo prediction engine
                var predictionEngine = mlContext.Model.CreatePredictionEngine<ChatData, ChatPrediction>(model);

                // Sử dụng mô hình thực tế để dự đoán phản hồi
                var prediction = predictionEngine.Predict(new ChatData { Question = userMessage });

                // Kiểm tra kết quả dự đoán từ mô hình
                if (string.IsNullOrWhiteSpace(prediction?.PredictedLabel))
                {
                    return "Xin lỗi, tôi không thể trả lời câu hỏi này.";
                }

                // Trả về kết quả dự đoán
                return prediction.PredictedLabel;
            }
            catch (FileNotFoundException ex)
            {
                return "Không tìm thấy mô hình: " + ex.Message;
            }
            catch (Exception ex)
            {
                return "Đã xảy ra lỗi khi dự đoán: " + ex.Message;
            }
        }
    }


    // Dự đoán phản hồi cho một câu hỏi
    public class ChatPrediction
    {
        public string Question { get; set; } // Câu hỏi gốc
        public string PredictedLabel { get; set; }  // Câu trả lời dự đoán
    }
}