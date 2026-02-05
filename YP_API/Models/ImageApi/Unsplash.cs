namespace YP_API.Models.ImageApi
{
    public class UnsplashSearchResponse
    {
        public int Total { get; set; }
        public List<UnsplashPhoto> Results { get; set; } = new();
    }

    public class UnsplashPhoto
    {
        public string Id { get; set; } = "";
        public string Description { get; set; } = "";
        public UnsplashUrls Urls { get; set; } = new();
    }

    public class UnsplashUrls
    {
        public string Raw { get; set; } = "";
        public string Full { get; set; } = "";
        public string Regular { get; set; } = ""; 
        public string Small { get; set; } = "";
    }
}
