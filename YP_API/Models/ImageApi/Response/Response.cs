namespace YP_API.Models.ImageApi.Response
{
    public class Response
    {
        public class TokenResponse
        {
            public string access_token { get; set; }
            public string expired_at { get; set; }
        }

        public class ChatCompletionResponse
        {
            public List<Choice> choices { get; set; }
            public long created { get; set; }
            public string model { get; set; }
            public string @object { get; set; }
            public Usage usage { get; set; }
        }

        public class Choice
        {
            public Message message { get; set; }
            public int index { get; set; }
            public string finish_reason { get; set; }
        }

        public class Message
        {
            public string content { get; set; }
            public string role { get; set; }
        }

        public class Usage
        {
            public int prompt_tokens { get; set; }
            public int completion_tokens { get; set; }
            public int total_tokens { get; set; }
        }
    }
}
