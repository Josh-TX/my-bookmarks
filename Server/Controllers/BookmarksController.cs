using FaviconFetcher;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;
using System.Net;
using System.Text.Json;
using ImageMagick;
using System.Text.RegularExpressions;

[Route("api/bookmarks")]
public class BookmarksController : Controller
{
    private const int iconSize = 48;
    private const string filePath = "data/bookmarks.json";

    [HttpGet]
    [Route("can-edit")]
    public bool CanEdit([FromQuery] string passKey)
    {
        return _canEdit(passKey);
    }

    [HttpGet]
    [Route("")]
    public IEnumerable<Bookmark> GetBookmarks()
    {
        return _getBookmarks();
    }

    [HttpGet]
    [Route("{id:guid}/icon")]
    [ResponseCache(Duration = 31536000)]
    public async Task<ActionResult> GetIcon([FromRoute] Guid id, [FromQuery] string passKey)
    {
        var bookmarks = _getBookmarks();
        var bookmark = bookmarks.FirstOrDefault(z => z.Id == id);
        if (bookmark == null)
        {
            return NotFound();
        }
        var fileName = $"data/{id}.png";
        if (System.IO.File.Exists(fileName)){
            using (var stream = System.IO.File.OpenRead(fileName)){
                stream.Position = 0;
                return new FileStreamResult(stream, "image/png");
            }
        }

        HttpClientHandler handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
        using var httpClient = new HttpClient(handler);
        var urls = new List<string>(){bookmark.Url};
        try{
            var redirectUrl = (await httpClient.GetAsync(bookmark.Url)).RequestMessage!.RequestUri!.ToString();
        } catch(Exception){

        }
        //use FaviconFetcherSource because it can handle self-signed ssl certs.
        var scanner = new Scanner(new FaviconFetcherSource());
        var scanResults = new List<ScanResult>();
        foreach(var url in urls){
            var tempResults = scanner.Scan(new Uri(url));
            if (tempResults != null){
                scanResults.AddRange(tempResults);
            }
        }
        if (scanResults == null || !scanResults.Any())
        {
            return NotFound();
        }
        var idealResults = scanResults.Where(z => z.ExpectedSize.Width >= iconSize).OrderBy(z => z.ExpectedSize.Width)
            .Concat(scanResults.Where(z => z.ExpectedSize.Width < iconSize).OrderBy(z => z.ExpectedSize.Width)).ToList();
        foreach (var idealResult in idealResults)
        {
            try
            {
                MagickImage image;
                var settings = new MagickReadSettings();
                var base64Matches = new Regex("^data:image/([a-z]+);base64,(.+)$").Match(idealResult.Location.AbsoluteUri.ToString());
                if (base64Matches.Success){
                    if (base64Matches.Groups[1].Value == "ico"){
                        settings.Format = MagickFormat.Ico;
                    }
                    var bytes = Convert.FromBase64String(base64Matches.Groups[2].Value);
                    image = new MagickImage(bytes, settings);
                } else {
                    var httpStream = await httpClient.GetStreamAsync(idealResult.Location.AbsoluteUri);
                    if (idealResult.Location.AbsoluteUri.EndsWith("ico")){
                        settings.Format = MagickFormat.Ico;
                    }
                    image = new MagickImage(httpStream, settings);
                }
                image.Format = MagickFormat.Png;
                image.BackgroundColor = new MagickColor("#FF0000");
                image.Resize(iconSize, iconSize);
                var memoryStream = new MemoryStream();
                image.Write(memoryStream);
                using (var writeStream = System.IO.File.OpenWrite(fileName)){
                    memoryStream.WriteTo(writeStream);
                }
                memoryStream.Position = 0;
                return new FileStreamResult(memoryStream, "image/png");
            }
            catch (Exception)
            {

            }
        }
        return NotFound();
    }

    [HttpPut]
    [Route("")]
    public ActionResult SetBookmarks([FromQuery] string passKey, [FromBody] List<Bookmark> newBookmarks)
    {
        if (!_canEdit(passKey))
        {
            return StatusCode(403);
        }
        var existing = _getBookmarks().ToList();
        newBookmarks.ForEach(newBookmark =>
        {
            if (!newBookmark.Id.HasValue || !existing.Any(z => z.Url == newBookmark.Url))
            {
                //if the URL changes, the Id needs to update so that the icon refreshes
                newBookmark.Id = Guid.NewGuid();
            }
        });
        existing.ForEach(existing => {
            if (!newBookmarks.Any(z => z.Id == existing.Id)){
                try {
                    System.IO.File.Delete($"data/{existing.Id}.png");
                } catch(Exception){}
            }
        });
        string json = JsonSerializer.Serialize(newBookmarks);
        System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);
        fileInfo.Directory!.Create();
        System.IO.File.WriteAllText(fileInfo.FullName, json);
        return Ok();
    }

    private bool _canEdit(string providedPassKey)
    {
        var envPassKey = Environment.GetEnvironmentVariable("PassKey");
        if (string.IsNullOrEmpty(envPassKey))
        {
            return true;
        }
        return providedPassKey == envPassKey;
    }
    private IEnumerable<Bookmark> _getBookmarks()
    {
        if (!System.IO.File.Exists(filePath))
        {
            return Enumerable.Empty<Bookmark>();
        }
        var json = System.IO.File.ReadAllText(filePath);
        var bookmarks = JsonSerializer.Deserialize<IEnumerable<Bookmark>>(json)!;
        return bookmarks;
    }
}