using Microsoft.SemanticKernel;
using System.ComponentModel;


namespace CustomCopilot.Plugins.FlightTrackerPlugin
{
    public class FlightTrackerPlugin(string apiKey)
    {
        readonly HttpClient client = new HttpClient();

        [KernelFunction, Description("Tracks the flight status of a provided source and destination")]
        [return: Description("Flight details and status")]
        public async Task<string> TrackFlightAsync(
        [Description("IATA code for the source location")] string source,
        [Description("IATA code for the designation location")] string destination,
        [Description("IATA code for the flight")] string flightNumber,
        [Description("Count of flights")] int limit)
        {
            string url = $"http://api.aviationstack.com/v1/flights?access_key={apiKey}&dep_iata={source}&arr_iata={destination}&limit={limit}&flight_iata={flightNumber}";

            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            return responseBody;
        }

    }
}