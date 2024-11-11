using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;

class OCRImageProcessor
{
    [DllImport("lib/libOCRImageProcessor.so", EntryPoint = "process_image_file")]
    public static extern IntPtr ProcessImageFile(string filePath);

    [DllImport("lib/libOCRImageProcessor.so", EntryPoint = "free_output")]
    public static extern void FreeOutput(IntPtr output);
}

class Program
{
    private static readonly HttpClient client = new HttpClient();
    static async Task Main(string[] args)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: dotnet run <image file path>");
            return;
        }

        string filePath = args[0];
        IntPtr output = OCRImageProcessor.ProcessImageFile(filePath);
        string outputString = Marshal.PtrToStringAnsi(output) ?? "Failed to process image";
        OCRImageProcessor.FreeOutput(output);

        var payload = new StringContent(outputString, Encoding.UTF8, "application/json");
        try
        {
            HttpResponseMessage response = await client.PostAsync("http://localhost:5000/predict", payload);

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Prediction Server Response: " + responseBody);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error occurred: " + ex.Message);
        }
        stopwatch.Stop();
        Console.WriteLine("Time elapsed: {0}", stopwatch.ElapsedMilliseconds);
    }
}