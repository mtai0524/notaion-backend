using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.ML;
using Microsoft.ML.Data;
using Notaion.Application.Common.Helpers;
using System.Globalization;
using System.Text.RegularExpressions;

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

        // Thay thế đường link trong câu hỏi bằng placeholder {URL}
        private static string ReplaceUrlsWithPlaceholderInQuestion(string input)
        {
            string pattern = @"http[^\s]+";
            return Regex.Replace(input, pattern, "{URL}");
        }

        // Đọc dữ liệu từ CSV và chỉ thay thế đường link trong câu hỏi, không thay đường link trong
        // câu trả lời
        private IEnumerable<ChatData> ReadCsvData(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true }))
            {
                var records = csv.GetRecords<ChatData>().ToList();

                // Thay thế đường link trong câu hỏi, nhưng không thay thế trong câu trả lời
                foreach (var record in records)
                {
                    record.Question = ReplaceUrlsWithPlaceholderInQuestion(record.Question);
                }

                return records;
            }
        }

        // Huấn luyện mô hình từ file CSV
        public async Task TrainModelFromCsvAsync(string filePath)
        {
            try
            {
                var mlContext = new MLContext();

                // Đọc dữ liệu từ tệp CSV
                var chatData = ReadCsvData(filePath);

                var data = mlContext.Data.LoadFromEnumerable(chatData);

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

                // Thay thế đường link trong câu hỏi (nếu có) bằng placeholder {URL}
                var inputQuestion = ReplaceUrlsWithPlaceholderInQuestion(userMessage);

                // Dự đoán phản hồi từ mô hình
                var prediction = predictionEngine.Predict(new ChatData { Question = inputQuestion });

                if (!string.IsNullOrWhiteSpace(prediction?.PredictedLabel))
                {
                    var predictedResponse = prediction.PredictedLabel;

                    if (predictedResponse.Contains("{current_time}"))
                    {
                        var currentTime = DateTimeHelper.GetVietnamTime().ToString("dd/MM/yyyy HH:mm:ss");
                        predictedResponse = predictedResponse.Replace("{current_time}", currentTime);
                    }

                    // Nếu có đường link trong câu hỏi, thay lại {URL} bằng URL thực tế
                    var match = Regex.Match(userMessage, @"http[^\s]+");
                    if (match.Success)
                    {
                        predictedResponse = predictedResponse.Replace("{URL}", match.Value);
                    }

                    // Trả về kết quả dự đoán
                    return predictedResponse;
                }

                return "Xin lỗi, tôi không thể trả lời câu hỏi này.";
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

    public class ChatPrediction
    {
        public string Question { get; set; }
        public string PredictedLabel { get; set; }
    }
}
