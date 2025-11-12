// See https://aka.ms/new-console-template for more information

using Newtonsoft.Json;
using System.Net.Http.Headers;


Console.WriteLine(@" __      ___                                                       
 \ \    / (_)                                                      
  \ \  / / _ _________   _                                         
   \ \/ / | |_  /_  / | | |                                        
    \  /  | |/ / / /| |_| |                                        
     \/   |_/___/___|\__, |                                        
  _____        _      __/ |____                _                   
 |  __ \      | |    |___/  __ \              | |              _   
 | |  | | __ _| |_ __ _  | |__) |___  __ _  __| | ___ _ __   _| |_ 
 | |  | |/ _` | __/ _` | |  _  // _ \/ _` |/ _` |/ _ \ '__| |_   _|
 | |__| | (_| | || (_| | | | \ \  __/ (_| | (_| |  __/ |      |_|  
 |_____/ \__,_|\__\__,_| |_|  \_\___|\__,_|\__,_|\___|_|           
                                                                   
                                                                   


");

string token = null;
DateTime startDate = DateTime.MinValue;
DateTime endDate = DateTime.MinValue;
int utcOffset = -100;

for (int x = 0; x < args.Length; x++)
{
    if (args[x].ToLower() == "help" || args[x].ToLower() == "-help" || args[x].ToLower() == "h" || args[x] == "-h".ToLower()
     || args[x].ToLower() == "man" || args[x].ToLower() == "-man" || args[x].ToLower() == "m" || args[x] == "-m".ToLower())
    {
        playMan();
        return;
    }
}



// Check for optional token argument
for (int x = 0; x < args.Length - 1; x += 2)
    {
        if (args[x] == "-t")
        {
            token = args[x + 1];
        }
        else if (args[x] == "-s")
        {
            DateTime.TryParse(args[x + 1], out startDate);
        }
        else if (args[x] == "-e")
        {
            DateTime.TryParse(args[x + 1], out endDate);
        }
        else if (args[x] == "-u")
        {
            Int32.TryParse(args[x + 1], out utcOffset);
        }
    }

if(!string.IsNullOrWhiteSpace(token))
{
    Console.WriteLine("Checking provided token...");
    var x = await getDevices(token);
    if(x == null)
    {
        Console.WriteLine("Provided token is invalid.");
        token = null;
    }
    else
    {
        Console.WriteLine("Provided token is valid.");
    }
}

while (string.IsNullOrWhiteSpace(token)) 
{
    Console.Write("Username: ");
    var username = Console.ReadLine();
    Console.Write("Password: ");
    var password = Console.ReadLine();

    HttpClient client = new HttpClient();
    token = await Login(client, username, password);

    if (string.IsNullOrWhiteSpace(token))
    {
        Console.WriteLine("Username or Password is incorrect!");
    }
    
}

if (startDate == DateTime.MinValue || endDate == DateTime.MinValue || utcOffset == -100)
{
    Console.WriteLine("Date Range or UTC Offset not provided, Starting date setup.");
    SetDateRange();

}
else if (startDate >= endDate)
{
    Console.WriteLine("Invalid Date Range provided via arguments, Starting date setup.");
    SetDateRange();
}
else
{
    Console.WriteLine();
    Console.WriteLine("Date Range and UTC Offset provided via arguments.");

}

bool continueBullshit = true;
while (continueBullshit)
{
    Console.WriteLine();
    Console.WriteLine($"Date Range Set: {startDate.ToString("yyyy-MM-dd")} to {endDate.ToString("yyyy-MM-dd")} with UTC Offset {utcOffset}");

 
    Console.WriteLine("\nMake Selection");
    Console.WriteLine("E: Extract Data");
    Console.WriteLine("L: GenerateTokenLogin");
    Console.WriteLine("D: Change Date Range");
    Console.WriteLine("Q: Quit");
    var key = Console.ReadKey();
    if (key.Key == ConsoleKey.Q)
    {
        continueBullshit = false;
    }
    else if (key.Key == ConsoleKey.E)
    {
        Console.Clear();
        await ExportData(token);
    }
    else if (key.Key == ConsoleKey.L)
    {
        Console.Clear();
        Console.WriteLine("Generate Token Login:");
        Console.WriteLine("Copy this argument list to the command line when starting the application.");
        string argumentString = $"-s {startDate.ToString("yyyy-MM-dd")} -e {endDate.ToString("yyyy-MM-dd")} -u {utcOffset} -t {token}";
        Console.WriteLine(argumentString);
        Console.WriteLine();

    }
    else if (key.Key == ConsoleKey.D)
    {
        Console.Clear();
        SetDateRange();
    }
}

async Task ExportData(string token)
{
    Console.WriteLine("Data Extraction Started...");
    var client = new HttpClient();
    var devices = await getDevices(token);
    if (devices == null || devices.Count == 0)
    {
        Console.WriteLine("No Devices Found!");
        return;
    }
    Console.WriteLine("Select Device to Extract (q) to quit:");
    var selectDevices = devices.Select(x => new SelectDevice { SelectIndex = (devices.IndexOf(x) + 1).ToString(), Name = x.Name, VizzyId = x.VizzyId, Device = x })
        .ToList();
    selectDevices.ForEach(x => Console.WriteLine($"{x.SelectIndex}:\t{x.Name.PadRight(40)}({x.VizzyId})"));

    SelectDevice selected = null;
    while (selected == null)
    {
        Console.Write("Selection: ");
        var selection = Console.ReadLine();
        if (selection.ToLower() == "q")
        {
            return;
        }
        selected = selectDevices.FirstOrDefault(x => x.SelectIndex == selection);
        if (selected == null)
        {
            Console.WriteLine("NO DEVICE SELECTED!");
            return;
        }
    }
    var deviceWithResources = await getFullDevice(selected.Device.Id, token);
    var deviceDef = await getDeviceDefinition(selected.Device.DeviceDefId, token);
    if (deviceWithResources == null || deviceDef == null)
    {
        Console.WriteLine("Failed To Get Device Information from Server.");
        return;
    }
    var selectedResources = deviceWithResources.Resources.Where(x =>
    {
        var def = deviceDef.ResourceDefs.FirstOrDefault(y => y.IoId == x.IoId);
        return def == null ? false : def.LogHistory && def.VisibleScript != "#0";
    }).Select((x, y) => new SelectedResource { SelectIndex = (y + 1).ToString(), Name = x.Name, Resource = x })
        .ToList();
    // Print all selected resources in 2 columns
    Console.WriteLine("\nAvailable Resources:");
    int colWidth = 40;
    for (int i = 0; i < selectedResources.Count; i += 2)
    {
        var left = $"{selectedResources[i].SelectIndex}: {selectedResources[i].Name}".PadRight(colWidth);
        string right = "";
        if (i + 1 < selectedResources.Count)
        {
            right = $"{selectedResources[i + 1].SelectIndex}: {selectedResources[i + 1].Name}";
        }
        Console.WriteLine(left + right);
    }
    SelectedResource selectedResource = null;
    while (selectedResource == null)
    {
        Console.Write("Select Resource to Extract (q to quit): ");
        var resourceSelection = Console.ReadLine();
        if (resourceSelection.ToLower() == "q")
        {
            return;
        }
        selectedResource = selectedResources.FirstOrDefault(x => x.SelectIndex == resourceSelection);
        if (selectedResource == null)
        {
            Console.WriteLine("NO RESOURCE SELECTED!");
        }
    }

    Console.WriteLine($"\nExtracting Data for Device: {selected.Name} ({selected.VizzyId})");
    Console.WriteLine($"Resource: {selectedResource.Name}");
    Console.WriteLine($"Date Range: {startDate.ToString("yyyy-MM-dd")} to {endDate.ToString("yyyy-MM-dd")}");
    Console.WriteLine($"UTC Offset: {utcOffset}");
    Console.WriteLine();
    Console.WriteLine("Please provide a file name for output: (.csv will be added to the end)");
    string fileName = "";
    while (string.IsNullOrWhiteSpace(fileName))
    {
        Console.Write("File Name: ");
        fileName = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            fileName = fileName.Trim();
            if (!fileName.EndsWith(".csv"))
            {
                fileName += ".csv";
            }
            Console.WriteLine($"Output will be saved to: {fileName}");
            Console.Write("Is this correct? (Y/N): ");
            if (Console.ReadKey().Key != ConsoleKey.Y)
            {
                fileName = "";
            }
            else
            {
                try
                {
                    using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (var writer = new StreamWriter(fileStream))
                    {
                        writer.WriteLine("TestOutput"); // Example header
                    }
                }
                catch
                {
                    Console.WriteLine($"Failed to open the file: {fileName}");
                    fileName = "";
                }
            }
                Console.WriteLine();
        }
    }
    await WriteFile(fileName, selectedResource.Resource, startDate, endDate, utcOffset, token);
    Console.WriteLine("Press Any Key to Contunue...");
    Console.ReadKey();

}

async Task WriteFile(string fileName, DeviceResourceDto resource, DateTime startDate, DateTime endDate, int utcOffset, string token)
{
    Console.WriteLine($"Writing data to {fileName}...");
    using (var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
    using (var writer = new StreamWriter(fileStream))
    {
        writer.WriteLine($"Resource: {resource.Name} from {startDate} to {endDate}");
        writer.WriteLine();
        writer.WriteLine("Timestamp,Value");
        var eventHistoryResult = new EventHistoryResultDto
        {
            ResourceId = resource.Id,
            EventHistory = new DeviceResourceEventDto[] { },
            EndEvent = null,
            PageToken = null
        };
        var adjustedEndDate = endDate.AddHours(utcOffset * -1);
        var events = await GetHistory(eventHistoryResult, token, adjustedEndDate);
        
        while (events != null && events.EventHistory != null && events.EventHistory.Length > 0)
        {
            bool breakLoop = false;
            foreach (var ev in events.EventHistory)
            {
                var filterDate = startDate.AddHours(utcOffset * -1);
                var adjustedTime = ev.Timestamp.AddHours(utcOffset);
                if (ev.Timestamp >= filterDate)
                {
                    writer.WriteLine($"{adjustedTime.ToString("yyyy-MM-ddTHH:mm:ss")},{ev.Value}");
                }
                else
                {
                    breakLoop = true;
                }
            }
            if (string.IsNullOrWhiteSpace(events.PageToken) || breakLoop)
            {
                break;
            }
            events = await GetHistory(events, token, endDate);
        }

    }
}

async Task<EventHistoryResultDto> GetHistory(EventHistoryResultDto eventHistoryResult, string token, DateTime endDate)
{
    var client = CreateHttpClient(token);
    var request = $"https://api.vizzy.site:443/event-history/{eventHistoryResult.ResourceId}?endDate={endDate.ToString("yyyy-MM-ddTHH:mm:ssZ")}&pageToken={eventHistoryResult.PageToken}";
    var response = await client.GetAsync(request);
    if (response.IsSuccessStatusCode)
    {
        var content = await response.Content.ReadAsStringAsync();
        var eventHistory = JsonConvert.DeserializeObject<DataWrapper<EventHistoryResultDto>>(content);
        return eventHistory.Data;
    }
    return null;
}

async Task<DeviceDefDto> getDeviceDefinition(string deviceDefId, string token)
{
    var client = CreateHttpClient(token);
    var result = await client.GetAsync($"https://api.vizzy.site:443/device-defs/{deviceDefId}");

    if (!result.IsSuccessStatusCode)
    {
        return null;
    }
    var content = await result.Content.ReadAsStringAsync();
    var deviceDef = JsonConvert.DeserializeObject<DataWrapper<DeviceDefDto>>(content);
    return deviceDef.Data;
}

async Task<DeviceDto> getFullDevice(string id, string token)
{
    var client = CreateHttpClient(token);
    var result = await client.GetAsync($"https://api.vizzy.site:443/devices/{id}");

    if (!result.IsSuccessStatusCode)
    {
        return null;
    }

    var content = await result.Content.ReadAsStringAsync();
    var devices = JsonConvert.DeserializeObject<DataWrapper<DeviceDto>>(content);
    return devices.Data;
}

async Task<IList<DeviceDto>> getDevices(string token)
{
    var client = CreateHttpClient(token);
    var result = await client.GetAsync("https://api.vizzy.site:443/devices");

    if (!result.IsSuccessStatusCode)
    {
        return null;
    }

    var content = await result.Content.ReadAsStringAsync();
    var devices = JsonConvert.DeserializeObject<DataWrapper<IList<DeviceDto>>>(content);
    return devices.Data;
}
void SetDateRange()
{
    startDate = DateTime.MinValue;
    endDate = DateTime.MinValue;
    utcOffset = -100;
    bool continueLoop = true;
    while (continueLoop)
    {
        while (startDate == DateTime.MinValue)
        {
            Console.Write("Enter Start Date (inclusive) (yyyy-MM-dd): ");
            var startInput = Console.ReadLine();
            DateTime.TryParse(startInput, out startDate);
            if(startDate != DateTime.MinValue)
            {
                startDate = startDate.Date;
            }
        }
        while (endDate == DateTime.MinValue)
        {
            Console.Write("Enter End Date (exclusive) (yyyy-MM-dd): ");
            var endInput = Console.ReadLine();
            DateTime.TryParse(endInput, out endDate);
            if(endDate != DateTime.MinValue)
            {
                endDate = endDate.Date;
            }
        }
        while (utcOffset == -100)
        {
            Console.Write("Enter UTC Offset (e.g., -5, 0, 2): ");
            var offsetInput = Console.ReadLine();
            Int32.TryParse(offsetInput, out utcOffset);
        }
        if(startDate >= endDate)
        {
            Console.WriteLine("Start Date must be before End Date. Please re-enter the dates.");
            startDate = DateTime.MinValue;
            endDate = DateTime.MinValue;
            utcOffset = -100;
        }
        else
        {
            continueLoop = false;
        }
    }
}
async Task<string> Login(HttpClient client, string? username, string? password)
{
    var loginRequest = new UserLoginRequestDto { Email = username, Password = password };
    var json = JsonConvert.SerializeObject(loginRequest);

    // Set String Content
    StringContent content = new StringContent(json);
    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
    content.Headers.Add("tenant-id", "vizzy");

    var result = await client.PostAsync($"https://api.vizzy.site:443/login", content);
    var resultContent = await result.Content.ReadAsStringAsync();
    var loginResult = JsonConvert.DeserializeObject<DataWrapper<AccessTokenInfo>>(resultContent);
    if(loginResult != null && loginResult.IsGood)
    {
        return loginResult.Data.AccessToken;
    }
    return "";
}
HttpClient CreateHttpClient(string token)
{
    var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    httpClient.DefaultRequestHeaders.Add("tenant-id", "vizzy");
    httpClient.Timeout = TimeSpan.FromMinutes(11);
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    return httpClient;
}

void playMan()
{ 
    Console.WriteLine("VizzyDataReaderPlus - Usage Manual");
    Console.WriteLine();
    Console.WriteLine("Arguments:");
    Console.WriteLine("  -t <token>      Optional. Provide an access token directly.");
    Console.WriteLine("  -s <startDate>  Optional. Specify the start date \"inclusive\" (format: yyyy-MM-dd or compatible).");
    Console.WriteLine("  -e <endDate>    Optional. Specify the end date \"exclusive\" (format: yyyy-MM-dd or compatible).");
    Console.WriteLine("  -u <utcOffset>  Optional. Specify the UTC offset as an integer (e.g., -5, 0, 2).");
    Console.WriteLine("                  -s, -e, and -u must be included together or, the date setup will still trigger.");
    Console.WriteLine();
    Console.WriteLine("Help:");
    Console.WriteLine("  help, -help, h, -h, man, -man, m, -m");
    Console.WriteLine("      Show this help message and exit.");
    Console.WriteLine();
    Console.WriteLine("Examples:");
    Console.WriteLine("  VizzyDataReaderPlus.exe -t <token> -s 2024-01-01 -e 2024-01-31 -u 0");
    Console.WriteLine("  VizzyDataReaderPlus.exe -s 2024-01-01 -e 2024-01-31");
    Console.WriteLine();
    Console.WriteLine("If no token is provided, you will be prompted for username and password.");
    Console.WriteLine();
}