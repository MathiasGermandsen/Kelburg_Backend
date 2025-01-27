﻿namespace KelBurgAPI.Models;

public class Users : Common
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public int PostalCode { get; set; }
    public string Country { get; set; }
    public int CountryCode { get; set; }
    public int PhoneNumber { get; set; }
    public int BookingId { get; set; }
    public string AccountType { get; set; }
}

public class UserCreateDTO
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public int PostalCode { get; set; }
    public string Country { get; set; }
    public int CountryCode { get; set; }
    public int PhoneNumber { get; set; }
    public int BookingId { get; set; }
    public string AccountType { get; set; }
}