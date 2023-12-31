﻿using Considition2023_Cs;
using System.Text.Json.Serialization;

const string apikey = "";


if (string.IsNullOrWhiteSpace(apikey))
{
    Console.WriteLine("Configure apiKey");
    return;
}

Console.WriteLine($"1: {MapNames.Stockholm}");
Console.WriteLine($"2: {MapNames.Goteborg}");
Console.WriteLine($"3: {MapNames.Malmo}");
Console.WriteLine($"4: {MapNames.Uppsala}");
Console.WriteLine($"5: {MapNames.Vasteras}");
Console.WriteLine($"6: {MapNames.Orebro}");
Console.WriteLine($"7: {MapNames.London}");
Console.WriteLine($"8: {MapNames.Linkoping}");
Console.WriteLine($"9: {MapNames.Berlin}");
Console.WriteLine($"10: {MapNames.GSandbox}");
Console.WriteLine($"11: {MapNames.SSandbox}");

Console.Write("Select the map you wish to play: ");
string option = Console.ReadLine();

var mapName = option switch
{
    "1" => MapNames.Stockholm,
    "2" => MapNames.Goteborg,
    "3" => MapNames.Malmo,
    "4" => MapNames.Uppsala,
    "5" => MapNames.Vasteras,
    "6" => MapNames.Orebro,
    "7" => MapNames.London,
    "8" => MapNames.Linkoping,
    "9" => MapNames.Berlin,
    "10" => MapNames.GSandbox,
    "11" => MapNames.SSandbox,
    _ => null
};

if (mapName is null)
{
    Console.WriteLine("Invalid map selected");
    return;
}
bool isHardcore = Scoring.SandBoxMaps.Contains(mapName.ToLower()); 
HttpClient client = new();
Api api = new(client);
MapData mapData = await api.GetMapDataAsync(mapName, apikey);
GeneralData generalData = await api.GetGeneralDataAsync();
SubmitSolution solution = new() 
{
    Locations = new()
};


if (isHardcore)
{
    var hotspot = mapData.Hotspots[1];
    var hotspot2 = mapData.Hotspots[2];

    solution.Locations.Add("location1", new PlacedLocations()
    {
        Freestyle9100Count = 1,
        Freestyle3100Count = 0,
        LocationType = generalData.LocationTypes["groceryStoreLarge"].Type,
        Longitude = hotspot.Longitude,
        Latitude = hotspot.Latitude
    });
    solution.Locations.Add("location2", new PlacedLocations()
    {
        Freestyle9100Count = 0,
        Freestyle3100Count = 1,
        LocationType = generalData.LocationTypes["groceryStore"].Type,
        Longitude = hotspot2.Longitude,
        Latitude = hotspot2.Latitude
    });
}
else
{
    foreach (KeyValuePair<string, StoreLocation> locationKeyPair in mapData.locations)
    {
        StoreLocation location = locationKeyPair.Value;
        //string name = locationKeyPair.Key;
        var salesVolume = location.SalesVolume;
        if (salesVolume > 100)
        {
            solution.Locations[location.LocationName] = new PlacedLocations()
            {
                Freestyle3100Count = 0,
                Freestyle9100Count = 1
            };
        }
    }
}

if (isHardcore)
{
    var hardcoreValidation = Scoring.SandboxValidation(mapName, solution, mapData);
    if (hardcoreValidation is not null)
    {
        throw new Exception("Hardcore validation failed");
    }
}

GameData score = new Scoring().CalculateScore(mapName, solution, mapData, generalData);
Console.WriteLine($"GameScore: {score.GameScore.Total}");
GameData prodScore = await api.SumbitAsync(mapName, solution, apikey);
Console.WriteLine($"GameId: {prodScore.Id}");
Console.WriteLine($"GameScore: {prodScore.GameScore.Total}");
Console.ReadLine();
