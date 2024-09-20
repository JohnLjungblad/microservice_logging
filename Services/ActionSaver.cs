public class ActionSaver
{
    private readonly MessageService messageService;

    DateTime now = DateTime.Now;

    private string filePath = @"C:\Users\ljung\OneDrive\Skrivbord\microservice_logging.txt"; 

    public ActionSaver(MessageService messageService)
    {
        this.messageService = messageService;
    }

    public void SaveActions(string action)
    {
        SaveActionToFile(action, filePath);
    }

    public void SaveActionToFile(string text, string filePath)
    {
        File.AppendAllText(filePath, text + " - " + now + Environment.NewLine);
        System.Console.WriteLine("Log file saved!");
    }
}