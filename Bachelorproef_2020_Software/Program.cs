using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using System.Text;

namespace Bachelorproef_2020_Software {
	static class Program {
		
		static string subscriptionKey = Environment.GetEnvironmentVariable("COMPUTER_VISION_SUBSCRIPTION_KEY");
		
		static string endpoint = Environment.GetEnvironmentVariable("COMPUTER_VISION_ENDPOINT");
		// the Batch Read method endpoint
		static string uriBase = endpoint + "/vision/v3.0-preview/read/analyze";

		static void Main(string[] args) {

			if (string.IsNullOrEmpty(subscriptionKey) || string.IsNullOrEmpty(endpoint)) {
				Console.Error.WriteLine("Please set environment variables COMPUTER_VISION_ENDPOINT and COMPUTER_VISION_SUBSCRIPTION_KEY.");
				return;
			}

			string imageFilePath;
			string language;
			if (args.Length == 0) {
				Console.Write(
					 "Enter the path to an image (bmp/jpg/png/tiff) or PDF with text you wish to read: ");
				imageFilePath = Console.ReadLine();
			}
			else {
				imageFilePath = args[0];
			}

			if (args.Length <= 1) {
				Console.Write(
					 "Enter the language to read: \"en\" or \"es\": ");
				language = Console.ReadLine();
			}
			else {
				language = args[1];
			}

			Console.WriteLine($"Endpoint:     [{endpoint}]");
			Console.WriteLine($"Subscription: [{subscriptionKey}]");
			Console.WriteLine($"URL:          [{uriBase}]");

			if (File.Exists(imageFilePath)) {
				// Call the REST API method.
				Console.WriteLine("\nWait a moment for the results to appear.\n");
				ReadText(imageFilePath, language).Wait();
			}
			else {
				Console.WriteLine("\nInvalid file path");
			}
			Console.WriteLine("\nPress Enter to exit...");
			Console.ReadLine();
		}

		static async Task ReadText(string imageFilePath, string language) {
			try {
				HttpClient client = new HttpClient();

				// Request headers.
				client.DefaultRequestHeaders.Add(
					 "Ocp-Apim-Subscription-Key", subscriptionKey);

				var builder = new UriBuilder(uriBase);
				builder.Port = -1;
				var query = HttpUtility.ParseQueryString(builder.Query);
				query["language"] = language;
				builder.Query = query.ToString();
				string url = builder.ToString();

				HttpResponseMessage response;

				string operationLocation;

				byte[] byteData = GetImageAsByteArray(imageFilePath);

				// Adds the byte array as an octet stream to the request body.
				using (ByteArrayContent content = new ByteArrayContent(byteData)) {
					content.Headers.ContentType =
						 new MediaTypeHeaderValue("application/octet-stream");

					// The first REST API method, Batch Read, starts
					// the async process to analyze the written text in the image.
					response = await client.PostAsync(url, content);
				}

				if (response.IsSuccessStatusCode)
					operationLocation =
						 response.Headers.GetValues("Operation-Location").FirstOrDefault();
				else {
					// Display the JSON error data.
					string errorString = await response.Content.ReadAsStringAsync();
					Console.WriteLine("\n\nResponse:\n{0}\n",
						 JToken.Parse(errorString).ToString());
					return;
				}

				string contentString;
				int i = 0;
				do {
					System.Threading.Thread.Sleep(1000);
					response = await client.GetAsync(operationLocation);
					contentString = await response.Content.ReadAsStringAsync();
					++i;
				}
				while (i < 60 && contentString.IndexOf("\"status\":\"succeeded\"") == -1);

				if (i == 60 && contentString.IndexOf("\"status\":\"succeeded\"") == -1) {
					Console.WriteLine("\nTimeout error.\n");
					return;
				}
				StringBuilder sb = new StringBuilder();
	

				var jsonresult  = JToken.Parse(contentString);
				var x = jsonresult.SelectToken("analyzeResult").SelectToken("readResults").First().SelectToken("lines");

				Console.WriteLine(x.ToString());

				foreach (var w in x) {
					sb.Append("\n")
					.Append(w.SelectToken("text").ToString())
					.Append("\n")
					.Append("\n");
					foreach (var z in w.SelectToken("words")) {
						sb.Append(String.Format(" '{0}' ", z.SelectToken("text")))
							.Append(" ")
							.Append("detected with a confidence level of")
							.Append(" ")
							.Append(String.Format("[{0}]", z.SelectToken("confidence")));
						sb.Append("\n");
					}

				}

				Console.WriteLine(sb.ToString());

			   Console.WriteLine("\nResponse:\n\n{0}\n",
			   sb.ToString());;
			}
			catch (Exception e) {
				Console.WriteLine("\n" + e.Message);
			}
		}

		static byte[] GetImageAsByteArray(string imageFilePath) {
			// Open a read-only file stream for the specified file.
			using (FileStream fileStream =
				 new FileStream(imageFilePath, FileMode.Open, FileAccess.Read)) {
				// Read the file's contents into a byte array.
				BinaryReader binaryReader = new BinaryReader(fileStream);
				return binaryReader.ReadBytes((int)fileStream.Length);
			}
		}


	}
}

