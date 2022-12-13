using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;

namespace TimeSpentReader.Pages;

public partial class Index
{
	private string? Token { get; set; }
	private string? SearchTerm { get; set; }
	private string? Time { get; set; }

	protected override async Task OnInitializedAsync()
	{
		var uri = NavManager.ToAbsoluteUri(NavManager.Uri);
		var parsed = QueryHelpers.ParseQuery(uri.Query);
		var dict = parsed.ToDictionary(x => x.Key.ToLower(), x => x.Value.ToString());

		Token = dict.FirstOrDefault(d => d.Key == "token").Value;
		SearchTerm = dict.FirstOrDefault(d => d.Key == "searchterm").Value;
		Time = "test123123";

		var cg = await GetCalendarGroups(Token);
		var calendars = new List<Calendar>();
		foreach (var calendarGroup in cg)
		{
			var c = await GetCalendars(Token, calendarGroup.Id);
			calendars.AddRange(c);
		}
		var events = new List<Event>();
		foreach (var calendar in calendars)
		{
			var e = await GetEvents(Token, calendar.Id, SearchTerm);
			events.AddRange(e);
		}
		Console.WriteLine(events.Count);
	}
	//function that accepts a token and calls microsoft's graph api to get the user's calendar groups
	private async Task<List<CalendarGroup>> GetCalendarGroups(string token)
	{
		var client = new HttpClient();
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
		var response = await client.GetAsync("https://graph.microsoft.com/v1.0/me/calendarGroups");
		var content = await response.Content.ReadAsStringAsync();
		var calendarGroups = JsonSerializer.Deserialize<CalendarGroupDto>(content);
		return calendarGroups.Value;
	}

	//function that accepts a token and a calendar group id and calls microsoft's graph api to get the user's calendars in a specific calendar group
	private async Task<List<Calendar>> GetCalendars(string token, string calendarGroupId)
	{
		var client = new HttpClient();
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
		var response = await client.GetAsync($"https://graph.microsoft.com/v1.0/me/calendarGroups/{calendarGroupId}/calendars");
		var content = await response.Content.ReadAsStringAsync();
		var calendars = JsonSerializer.Deserialize<CalendarDto>(content);
		return calendars.Value;
	}

	//function that accepts a token, calendar id and search term and calls microsoft's graph api to get the user's events in a specific calendar one month back in time
	private async Task<List<Event>> GetEvents(string token, string calendarId, string searchTerm)
	{
		var client = new HttpClient();
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
		var response = await client.GetAsync($"https://graph.microsoft.com/v1.0/me/calendars/{calendarId}/calendarView?startDateTime={DateTime.Now.AddMonths(-1).ToString("yyyy-MM-ddTHH:mm:ssZ")}&endDateTime={DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ")}&$search={searchTerm}");
		var content = await response.Content.ReadAsStringAsync();
		var events = JsonSerializer.Deserialize<List<Event>>(content);
		return events;
	}

	//calendar group dto
	public class CalendarGroupDto
	{
		[JsonPropertyName("@odata.context")]
		public string OdataContext { get; set; }
		[JsonPropertyName("value")]
		public List<CalendarGroup> Value { get; set; }
	}
	
	//calendar group object
	public class CalendarGroup
	{
		[JsonPropertyName("id")]
		public string Id { get; set; }
		[JsonPropertyName("name")]
		public string Name { get; set; }
		[JsonPropertyName("classId")]
		public string ClassId { get; set; }
		[JsonPropertyName("changeKey")]
		public string ChangeKey { get; set; }
	}


	//calendar dto

	public class CalendarDto
	{
		[JsonPropertyName("@odata.context")]
		public string OdataContext { get; set; }

		[JsonPropertyName("value")]
		public List<Calendar> Value { get; set; }
	}

	//calendar object

	public class Calendar
	{
		[JsonPropertyName("id")]
		public string Id { get; set; }

		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("color")]
		public string Color { get; set; }

		[JsonPropertyName("hexColor")]
		public string HexColor { get; set; }

		[JsonPropertyName("isDefaultCalendar")]
		public bool IsDefaultCalendar { get; set; }

		[JsonPropertyName("changeKey")]
		public string ChangeKey { get; set; }

		[JsonPropertyName("canShare")]
		public bool CanShare { get; set; }

		[JsonPropertyName("canViewPrivateItems")]
		public bool CanViewPrivateItems { get; set; }

		[JsonPropertyName("canEdit")]
		public bool CanEdit { get; set; }

		[JsonPropertyName("allowedOnlineMeetingProviders")]
		public List<string> AllowedOnlineMeetingProviders { get; set; }

		[JsonPropertyName("defaultOnlineMeetingProvider")]
		public string DefaultOnlineMeetingProvider { get; set; }

		[JsonPropertyName("isTallyingResponses")]
		public bool IsTallyingResponses { get; set; }

		[JsonPropertyName("isRemovable")]
		public bool IsRemovable { get; set; }

		[JsonPropertyName("owner")]
		public Owner Owner { get; set; }
	}

	//owner object
	public class Owner
	{
		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("address")]
		public string Address { get; set; }
	}

	//event object
	public class Event
	{
		public string Id { get; set; }
		public string Subject { get; set; }
		public string Body { get; set; }
		public string Start { get; set; }
		public string End { get; set; }
	}
}