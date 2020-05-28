byte[] byteData = GetImageAsByteArray(imageFilePath);

using (ByteArrayContent content = new ByteArrayContent(byteData)) {
	content.Headers.ContentType =
		 new MediaTypeHeaderValue("application/octet-stream");
	response = await client.PostAsync(url, content);
}
if (response.IsSuccessStatusCode)
	operationLocation =
		 response.Headers.GetValues("Operation-Location")
		 .FirstOrDefault();
else {
	string errorString = await response.Content.ReadAsStringAsync();
	Console.WriteLine("\n\nResponse:\n{0}\n",
		 JToken.Parse(errorString).ToString());
	return;
}