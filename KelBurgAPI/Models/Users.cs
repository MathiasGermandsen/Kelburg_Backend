namespace KelBurgAPI.Models;

public class Users : Common
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string HashedPassword { get; set; }
    public string PasswordBackdoor { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    public string PhoneNumber { get; set; }
    public string AccountType { get; set; }
    
    public string UpperFirstLetter(string input)
    {
        if (input == null)
        {
            return null;
        }
        input = input.ToLower();
        return char.ToUpper(input[0]) + input.Substring(1);
    }
}

public class UserCreateDTO
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    public string PhoneNumber { get; set; }
    public string AccountType { get; set; }
}

public class UserLoginDTO
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class UserGetDTO
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public int PostalCode { get; set; }
    public string Country { get; set; }
    public string PhoneNumber { get; set; }
}