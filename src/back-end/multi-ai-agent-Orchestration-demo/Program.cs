// See https://aka.ms/new-console-template for more information
var client=new HttpClient();
client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://www.bing.com/search?q=https://www.myntra.com/trollery-bag?rawQuery=trollery%20bag")).ContinueWith(responseTask =>
{
    
    var response=responseTask.Result;
    Console.WriteLine($"Response status code: {response.StatusCode}");
}).Wait();
