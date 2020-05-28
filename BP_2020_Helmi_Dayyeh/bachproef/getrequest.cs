string contentString;
int i = 0;
do {
	System.Threading.Thread.Sleep(1000);
	response = await client.GetAsync(operationLocation);
	contentString = await response.Content.ReadAsStringAsync();
	++i;
}
