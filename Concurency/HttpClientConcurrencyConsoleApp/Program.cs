internal class Program
{
    private static async Task Main(string[] args)
    {
        List<string> urls = [
            "https://sales.timejob-online.dev/preview-1284/api/v1/document-signing/request/df177ad4-3f9e-4b4b-a2b2-367d9d3a890f/multiple-parties/sign?placeholderName=signature1&tenantId=8939457b-1175-4094-9401-9d994a36ced8&exp=07%2f16%2f2025+03%3a43%3a13&scp=read%2csign&sig=UrI8d%2bWm2oyaG1f40Qf%2b0%2bJ2MaWMKNlrgE8hOfH4e8M%3d",
            "https://sales.timejob-online.dev/preview-1284/api/v1/document-signing/request/df177ad4-3f9e-4b4b-a2b2-367d9d3a890f/multiple-parties/sign?placeholderName=signature2&tenantId=8939457b-1175-4094-9401-9d994a36ced8&exp=07%2f16%2f2025+03%3a43%3a13&scp=read%2csign&sig=FjEyQaMajaKSdH3X5fUY9dgIefAzJFc%2bPOeACpnHnuk%3d"
        ];

        var tasks = new List<Task>();

        foreach (var item in urls)
        {
            tasks.Add(PostSignSignature(item));
        }

        await Task.WhenAll(tasks);

        Console.WriteLine("All requests completed");
    }

    private static async Task PostSignSignature(string url)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("tenant-id", "8939457b-1175-4094-9401-9d994a36ced8");
        request.Headers.Add("Accept", "application/json");
        var content = new MultipartFormDataContent();
        content.Add(new StreamContent(File.OpenRead("/Users/anhtran/Downloads/signature.png")), "signature", "/Users/anhtran/Downloads/signature.png");
        content.Add(new StringContent("ä, ö, ü"), "signingPartyName");
        request.Content = content;
        var response = await client.SendAsync(request);
        Console.WriteLine(response.StatusCode);
        Console.WriteLine(await response.Content.ReadAsStringAsync());

    }
}