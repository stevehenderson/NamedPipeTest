using System;

using System.IO;
using System.IO.Pipes;
using System.Threading;

//String NAMED_PIPE_HANDLE = "\\\\warhol\\pipe\\mynamedpipe";

namespace PipeApplication1
{
    class ProgramPipeTest
    {
       String NAMED_PIPE_HANDLE = "\\\\usmanbdf00018se\\pipe\\mynamedpipe";


        public void ThreadStartServer()
        {
            // Create a name pipe
            using (NamedPipeServerStream pipeStream = new NamedPipeServerStream(NAMED_PIPE_HANDLE))
            {
                Console.WriteLine("[Server] Pipe created {0}", pipeStream.GetHashCode());

                // Wait for a connection
                pipeStream.WaitForConnection();
                Console.WriteLine("[Server] Pipe connection established");

                using (StreamReader sr = new StreamReader(pipeStream))
                {
                    string temp;
                    // We read a line from the pipe and print it together with the current time
                    while ((temp = sr.ReadLine()) != null)
                    {
                        Console.WriteLine("{0}: {1}", DateTime.Now, temp);
                    }
                }
            }

            Console.WriteLine("Connection lost");
        }

        public void ThreadStartClient(object obj)
        {
            // Ensure that we only start the client after the server has created the pipe
            ManualResetEvent SyncClientServer = (ManualResetEvent)obj;

            // Only continue after the server was created -- otherwise we just fail badly
            // SyncClientServer.WaitOne();

            using (NamedPipeClientStream pipeStream = new NamedPipeClientStream(NAMED_PIPE_HANDLE))
            {
                // The connect function will indefinately wait for the pipe to become available
                // If that is not acceptable specify a maximum waiting time (in ms)
                pipeStream.Connect();

                Console.WriteLine("[Client] Pipe connection established");
                using (StreamWriter sw = new StreamWriter(pipeStream))
                {
                    sw.AutoFlush = true;
                    string temp;
                    Console.WriteLine("Please type a message and press [Enter], or type 'quit' to exit the program");
                    while ((temp = Console.ReadLine()) != null)
                    {
                        if (temp == "quit") break;
                        sw.WriteLine(temp);
                    }
                }
            }
        }

        static void Main(string[] args)
        {

            // To simplify debugging we are going to create just one process, and have two tasks
            // talk to each other. (Which is a bit like me sending an e-mail to my co-workers)

            ProgramPipeTest Server = new ProgramPipeTest();
            ProgramPipeTest Client = new ProgramPipeTest();

            Thread ServerThread = new Thread(Server.ThreadStartServer);
            Thread ClientThread = new Thread(Client.ThreadStartClient);

            ServerThread.Start();
            ClientThread.Start();
        }
    }
}
