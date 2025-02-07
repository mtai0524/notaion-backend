using Microsoft.ML.Data;

namespace Notaion.Application.ModelsChatBot;

public class ChatData
{
        [LoadColumn(0)] 
        public string Question { get; set; } = string.Empty;

        [LoadColumn(1)]
        public string Response { get; set; } = string.Empty;
}