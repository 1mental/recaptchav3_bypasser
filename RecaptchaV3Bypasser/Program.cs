using RecaptchaV3Bypasser.Recaptcha;
using RecaptchaV3Bypasser.Server;


// This program is coded by Mental ^_*


Application.Main();


internal class Application
{
    public static void Main()
    {
        Console.WriteLine(AsciiArt());
        Console.ForegroundColor = ConsoleColor.White;


    TestingAnochrUrl:
        PrintInputMessage("Please enter the Anchor URL: ");
        try
        {
            RecaptchaV3.InitRecaptcha(Console.ReadLine());
        }
        catch (Exception ex) 
        {
            if (ex is NullUrlException)
                PrintErrorMessage($"NullUrlException: {ex.Message}");
            else if (ex is ParametersParsingException)
                PrintErrorMessage($"ParametersParsingException: {ex.Message}");
            else
                PrintErrorMessage($"Unhandled Exception : {ex.Message}");
            goto TestingAnochrUrl;
        }

        PrintSuccessMessage("Recaptcha Initialized Sucessfully!");
        PrintSuccessMessage("Testing Recaptcha Servers....");

        try
        {
            RecaptchaV3.ReloadRecaptcha();
        }catch (Exception ex) 
        {
            if (ex is TokenGenerationException)
                PrintErrorMessage($"Coudn't Generate a Token: {ex.Message}");
            else  if (ex is RecaptchaConnectionException)
            {
                PrintErrorMessage("Cannot connect to Google Servers...Wanna read the exception? (y/n)");
            TakingInput:
                char input = (char)Console.Read();
                switch(input)
                {
                    case 'y':
                        Console.WriteLine(ex.Message);
                        break;
                    case 'n':
                        PrintSuccessMessage("Exiting...");
                        Environment.Exit(-1);
                        break;
                    default:
                        PrintErrorMessage("Invalid Input!");
                        goto TakingInput;

                }
            }
            else
                PrintErrorMessage($"Unhandled Exception : {ex.Message}");

            PrintSuccessMessage("Press Anykey to exit..");
            Console.ReadKey();
            Environment.Exit(0);
        }

        PrintSuccessMessage("Connected to Google Servers Successfully!");

        PrintInputMessage("Enter the server port (80 is recommended) : ");

        int port;
        try
        {
            port = Convert.ToInt32(Console.ReadLine());
        }catch(Exception ex)
        {
            PrintErrorMessage(ex.Message);
            return;
        }

        Console.Clear();

        PrintSuccessMessage("Starting.. Http Server..");


         new HttpServer(port);


         PrintSuccessMessage($"HTTP Server Started Successfully!\n\n[ServerInfo] IP : localhost\n[ServerInfo] Endpoint : /generatetoken\n[ServerInfo] Port : {port}");


    }


    public static string AsciiArt()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        return @"

                ███╗   ███╗    ███████╗    ███╗   ██╗    ████████╗     █████╗ ██╗     
                ████╗ ████║    ██╔════╝    ████╗  ██║    ╚══██╔══╝    ██╔══██╗██║     
                ██╔████╔██║    █████╗      ██╔██╗ ██║       ██║       ███████║██║     
                ██║╚██╔╝██║    ██╔══╝      ██║╚██╗██║       ██║       ██╔══██║██║     
                ██║ ╚═╝ ██║    ███████╗    ██║ ╚████║       ██║       ██║  ██║███████╗
                ╚═╝     ╚═╝    ╚══════╝    ╚═╝  ╚═══╝       ╚═╝       ╚═╝  ╚═╝╚══════╝
       

                RecaptchaV3 Bypasser by Mental (@huvn on Instagram)
                
                Note: This program only bypasses RecaptchaV3!
      

";
    }

    public static void PrintErrorMessage(string message)
    {
        Console.Write("[");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("-");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("]");


        Console.WriteLine($" {message}");
    }


    public static void PrintSuccessMessage(string message)
    {
        Console.Write("[");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("+");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("]");


        Console.WriteLine($" {message}");

    }

    public static void PrintInputMessage(string message)
    {
        Console.Write("[");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("?");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("]");


        Console.Write($" {message}");

    }
}
