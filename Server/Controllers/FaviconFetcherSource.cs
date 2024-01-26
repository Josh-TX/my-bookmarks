using FaviconFetcher;

//Because this project is intended for LAN web guis, and several of mine have self-signed SSL certificates, 
//the faviconFetcher doesn't work. Thankfully, I can provide a custom implementation of ISource that allows self-signed SSL
public class FaviconFetcherSource : ISource
{
    StreamReader ISource.DownloadText(Uri uri)
    {
        HttpClientHandler handler = new HttpClientHandler();
        // Allow all certificates, including self-signed ones
        handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
        HttpClient client = new HttpClient(handler);
        HttpResponseMessage response = client.GetAsync(uri).Result;
        Stream responseStream = response.Content.ReadAsStream();
        return new StreamReader(responseStream);
    }

    IEnumerable<IconImage> ISource.DownloadImages(Uri uri)
    {
        throw new NotImplementedException();
    }
}