static string subscriptionKey = 
        Environment.GetEnvironmentVariable
        ("COMPUTER_VISION_SUBSCRIPTION_KEY");

static string endpoint = 
        Environment.GetEnvironmentVariable
        ("COMPUTER_VISION_ENDPOINT");

static string uriBase = 
        endpoint + "/vision/v3.0-preview/read/analyze";