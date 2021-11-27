using System;
using System.Collections.Generic;
using System.Threading;

namespace Area_51
{
    class Program
    {
        static int floor;
        static int destinationFloor;
        static List<string> listAgents = new List<string>();
        static Semaphore _pool;
        static Random rand;
        static Thread elevatorThread;

        private static int _padding;

        public static void Main()
        {
            _pool = new Semaphore(0,2);
            rand = new Random();

            string[] agents = { "Confidential", "Secret", "Top-secret" };

            Console.Write("Agents count:");
            string n = Console.ReadLine();

            //Add agents
            for (int i = 0; i < int.Parse(n); i++)
            {
                string agent_to_add = agents[rand.Next(3)];
                Console.WriteLine(agent_to_add);
                listAgents.Add(agent_to_add);
                listAgents.Add((rand.Next(4) + 1).ToString());
            }

            elevatorThread = new Thread(Elevator);
            elevatorThread.Start();
               
            for (int j = 0; j < listAgents.Count; j++)
            {
                if (j % 2 != 0) continue;
                
                Thread t = new Thread(new ParameterizedThreadStart(Agents));

                t.Start(listAgents[j] + "," + listAgents[j + 1] + "," + j);
            }

            Thread.Sleep(1000);

            Console.WriteLine("Main thread calls Release().");
            _pool.Release(2);

            Console.WriteLine("Main thread exits.");
            Console.WriteLine("------------------------");
          
        }


        public static void Agents(object value)
        {
            string temp = (string)value;
            string[] array = temp.Split(",");

            Console.WriteLine("Agent {0} with index {1} with destination floor {2} begins " +
              "and waits for the elevator.", array[0], int.Parse(array[2]) / 2, array[1]);
            _pool.WaitOne();

            int padding = Interlocked.Add(ref _padding, 100);

            Console.WriteLine("Agent {0} with index {1} with destination floor {2} enters the semaphore.", array[0], int.Parse(array[2]) / 2, array[1]);

            destinationFloor = int.Parse(array[1]);

            bool isGoing = true;
            while (isGoing)
            {
                Thread.Sleep(1000);

                if (floor == destinationFloor)
                {
                    if (!ElevatorDoor(array[0], destinationFloor)) Console.WriteLine("New destination floor {0}", destinationFloor = (rand.Next(4) + 1));
                    else isGoing = false;
                }

            }
            Thread.Sleep(1000);
            listAgents[int.Parse(array[1])] = destinationFloor.ToString();
            Console.WriteLine("Agent {0} with index {1} releases the elevator on floor {2}.", array[0], int.Parse(array[2]) / 2, floor);
            Console.WriteLine("------------------------");
            _pool.Release();
            
        }
        public static void Elevator()
        {

            _pool.WaitOne();
            while (true)
            {
                while (floor != destinationFloor)
                {
                    if (floor > destinationFloor) { floor--; Console.WriteLine("Floor: " + floor + ". Destination floor: " + destinationFloor); Thread.Sleep(1000); }
                    if (floor < destinationFloor) { floor++; Console.WriteLine("Floor: " + floor + ". Destination floor: " + destinationFloor); Thread.Sleep(1000); }
                }
            }            
        }

        public static bool ElevatorDoor(string agent, int floor)
        {
            bool access = false;
            if (agent == "Confidential" && floor == 1) access = true;
            if (agent == "Secret" && floor <= 2) access = true;
            if (agent == "Top-secret" && floor <= 4) access = true;

            if (!access) Console.WriteLine("Access denied!");
            else Console.WriteLine("Access granted!");
            return access;
        }

    }
}
