using System.Text.Json;
using System.Text.Json.Nodes;
using HtmlAgilityPack;


namespace UScrape
{
    public static class Scraper
    {
        static JsonNode? GetJson(string url)
        {
            HttpClient client = new HttpClient();
            string response = client.GetStringAsync(url).Result;
            JsonNode? result = JsonSerializer.Deserialize<JsonNode>(response);
            return result;
        }

        public static List<ISavable> ScrapeEvenko(ProgressTracker progress)
        {
            List<ISavable> events = new List<ISavable>();
            Dictionary<string, string> adresses = new Dictionary<string, string>();

            progress.Message = "Fetching data";
            JsonNode? data = GetJson("https://evenko.ca/api/search?body=eyJwYXJhbXMiOltudWxsLHsicGFnZSI6MCwiaGl0c1BlclBhZ2UiOjUwMCwiZmlsdGVycyI6ImVudGl0eV90eXBlOmV2ZW5rb19zaG93IEFORCBzdGF0dXM6MSBBTkQgc2hvd190aW1lIDw9IDE3ODkxNzg3NjYifV0sImxhbmciOiJlbiJ9");

            if (data is null)
            {
                progress.Data = "Failed to get data";
                progress.Status = ProgressTracker.Statuses.Error;
                return events;
            }

            progress.Message = "Parsing data";
            JsonArray jsonEvents = data["hits"].AsArray();

            for(int i = 0; i < jsonEvents.Count; i++)
            {
                if (progress.Status == ProgressTracker.Statuses.Aborted) 
                    return events;

                progress.Progression = (double)i / jsonEvents.Count;

                try
                {
                    // Get venue adress
                    JsonNode venue = jsonEvents[i]["venue"];
                    string venueId = venue["slug"].GetValue<string>();

                    if (!adresses.ContainsKey(venueId))
                    {
                        string venueUrl = $"https://evenko.ca/en/venues/{venueId}";
                        var web = new HtmlWeb();
                        HtmlDocument doc = web.Load(venueUrl);

                        HtmlNode adressNode = doc.DocumentNode.SelectSingleNode("/html/body/div[1]/div/div[1]");
                        adressNode.RemoveChild(adressNode.SelectSingleNode("div"));
                        adressNode.RemoveChild(adressNode.SelectSingleNode("h1"));
                        string fullAdress = adressNode.InnerHtml;

                        adresses[venueId] = fullAdress.Split('<')[0];
                    }

                    // Get datetime from unix
                    double unix = jsonEvents[i]["show_time"].GetValue<double>();
                    DateTime showTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    showTime = showTime.AddSeconds(unix);

                    // Modelize event
                    string name = jsonEvents[i]["title"].GetValue<string>();
                    string description = "";
                    string category = jsonEvents[i]["category"][0].GetValue<string>();
                    string city = venue["city"].GetValue<string>();
                    string adress = adresses[venueId];
                    DateTime date = showTime;
                    string price = "Paid";
                    string image = jsonEvents[i]["thumbnail"].GetValue<string>();

                    Event @event = new Event(name, description, category,  city, adress, date, price, image);
                    events.Add(@event);
                    progress.Data = @event.ToJSON();
                } catch (Exception e)
                {
                    progress.Data = $"Failed to parse event {i}: {e.Message}";
                }
            }

            progress.Status = ProgressTracker.Statuses.Completed;
            return events;
        }
    }
}
