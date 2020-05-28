
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

