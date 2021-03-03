using System;

namespace message_sender
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting POC client");
            if(args.Length != 2)
            {
                Console.WriteLine("incorrect number of args, please provide queueName and service bus connection string");
            }
            PocRunner runner = new PocRunner(args[0], args[1]);
            runner.Run();
            Console.WriteLine("No test client running. Press any key to end...");
        }
    }
}
