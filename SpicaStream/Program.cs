using YoutubeExplode;

string outputPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads\\SpicaStream");

Console.WriteLine("Paste the Youtube Video Link Here to Download:");

string videoUrl = Console.ReadLine()!;

try
{
    await DownloadVideoAsync(videoUrl, outputPath);
}
catch (Exception ex)
{
    Console.WriteLine("An error occurred while downloading the videos: " + ex.Message);
}

static async Task DownloadVideoAsync(string videoUrl, string outputPath)
{
    if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);
 
    var youtube = new YoutubeClient();
    var video = await youtube.Videos.GetAsync(videoUrl);

    string sanitizedTitle = string.Join("_", video.Title.Split(Path.GetInvalidFileNameChars()));

    var streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
    var muxedStreams = streamManifest.GetMuxedStreams()
        .OrderByDescending(s => s.VideoQuality)
        .ToList();

    if (muxedStreams.Any())
    {
        var streamInfo = muxedStreams.First();
        using var httpClient = new HttpClient();
        var stream = await httpClient.GetStreamAsync(streamInfo.Url);

        string outputFilePath = Path.Combine(outputPath, $"{sanitizedTitle}.{streamInfo.Container}");
        using var outputStream = File.Create(outputFilePath);
        await stream.CopyToAsync(outputStream);

        Console.WriteLine("Download Completed!");
        Console.WriteLine($"Video saved as: {outputFilePath}");
    }
    else
    {
        Console.WriteLine($"No suitable video stream found for {video.Title}.");
    }
}