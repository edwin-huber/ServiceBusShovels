using System;
using System.Collections.Generic;
using System.Text;

namespace message_sender
{
    class PocRunner
    {
        private int MAX_MESSAGE_BATCH = 1000;
        private string sendQueue = "tenant_out";
        private string connectionString;

        public PocRunner(string sendQueue, string connectionString)
        {
            this.sendQueue = sendQueue;
            this.connectionString = connectionString;
        }
        public void Run()
        {
            var asbClient = new POCServiceBusClient(connectionString, sendQueue);

            bool finished = false;
            while (!finished)
            {

                Console.WriteLine("\nEnter Number of messages to send:");

                int numMessages = 1;
                try
                {
                    numMessages = int.Parse(Console.ReadLine());
                }
                catch
                {
                    Console.WriteLine("\nNot a number, sending 1.");
                }

                if (numMessages < 0 || numMessages > MAX_MESSAGE_BATCH)
                {
                    Console.WriteLine("Invalid number sending batch of " + MAX_MESSAGE_BATCH + ".");
                    numMessages = MAX_MESSAGE_BATCH;
                }

                for (int i = 0; i < numMessages; i++)
                {
                    var msg = PocMessageFactory.GetSampleMessage();
                    Console.WriteLine("Sending:");
                    Console.WriteLine(msg);
                    asbClient.SendMessage(msg);
                }

                Console.WriteLine("Again? Y or any key for no.");
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.Y:
                        break;
                    default:
                        finished = true;
                        break;

                }

            }
            
        }
    }
}
