namespace KelBurgAPI;

public class LogService
{
    private string GetTimeStamp()
    {
        return DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
    }
    
    public void LogMessageWithFrame(string message)
    {
        Console.WriteLine("----------------------------------------------------------------------");
        Console.WriteLine($"{GetTimeStamp()} [INFO] {message}");
        Console.WriteLine("----------------------------------------------------------------------");
    }
    
    public void LogMessage(string message)
    {
        Console.WriteLine($"{GetTimeStamp()} [INFO] {message}");
    }
    
    public void LogErrorWithFrame(string error)
    {
        Console.WriteLine("----------------------------------------------------------------------");
        Console.WriteLine($"{GetTimeStamp()} [ERROR] {error}");
        Console.WriteLine("----------------------------------------------------------------------");
    }

    public void LogError(string error)
    {
        Console.WriteLine($"{GetTimeStamp()} [ERROR] {error}");
    }

}