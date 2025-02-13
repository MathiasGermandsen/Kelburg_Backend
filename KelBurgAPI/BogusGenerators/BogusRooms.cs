using KelBurgAPI.Models;
using Bogus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace KelBurgAPI.BogusGenerators;

public class BogusRooms
{
    private static readonly Dictionary<int, List<string>> RoomCapacityMapping = new()
    {
        { 1, new List<string> { "Single room" } },
        { 2, new List<string> { "Double room", "Twin room", "Hollywood Twin", "Queen room" } },
        { 3, new List<string> { "Triple room" } },
        { 4, new List<string> { "Quad room" } },
        { 5, new List<string> { "Suite" } },
        { 6, new List<string> { "Deluxe Room" } },
        { 7, new List<string> { "Presidential Suites" } },
        { 8, new List<string> { "Presidential Suites" } },
        { 9, new List<string> { "Presidential Suites" } },
        { 10, new List<string> { "Presidential Suites" } },
        { 11, new List<string> { "Presidential Suites" } },
        { 12, new List<string> { "Presidential Suites" } }
    };

    public static List<RoomCreateDTO> GenerateRooms(int count, HotelPricing pricing)
    {
        Faker<RoomCreateDTO> faker = new Faker<RoomCreateDTO>()
            .RuleFor(r => r.Size, f => f.Random.Number(1, 12))
            .RuleFor(r => r.RoomType, (f, r) => RoomCapacityMapping[r.Size].First())
            .RuleFor(r => r.ViewType, f => pricing.ViewTypePrices.Keys.ToList()[f.Random.Int(0, pricing.ViewTypePrices.Count - 1)])
            .RuleFor(r => r.PricePrNight, (f, r) => CalculateRoomPrice(pricing, r.RoomType, r.ViewType, r.Size));
        

        return faker.Generate(count);
    }
    
    public static HotelPricing LoadHotelPricing(string filename)
    {
        string json = File.ReadAllText(filename);
        return JsonConvert.DeserializeObject<HotelPricing>(json);
    }

    public static int CalculateRoomPrice(HotelPricing pricing, string roomType, string viewType, int occupants)
    {
        if (!pricing.BaseRoomPrices.TryGetValue(roomType, out int roomPrice))
            throw new ArgumentException($"Invalid room type: {roomType}");

        if (!pricing.ViewTypePrices.TryGetValue(viewType, out int viewPrice))
            throw new ArgumentException($"Invalid view type: {viewType}");

        return roomPrice + viewPrice + (pricing.PerOccupantCharge * (occupants - 1));
    }
}
