using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS4390_ServerChat_Server
{
    class ChatHistory
    {
        string cIDA;
        string cIDB;
        string fileName;
        public ChatHistory(string clientA, string clientB)
        {
            cIDA = clientA;
            cIDB = clientB;
            if (cIDA.GetHashCode() < cIDB.GetHashCode())
            {
                fileName = cIDA + cIDB;
            }
            else
            {
                fileName = cIDB + cIDA;
            }
            fileName += ".txt";
        }

        public string chatHistory()
        {
            StreamReader streamReader = new StreamReader(fileName);
            string allData = streamReader.ReadToEnd();
            streamReader.Close();
            return allData;

        }

        public void addLine(string chatMessage)
        {
            string line = "";
            StreamWriter streamWriter = new StreamWriter(fileName, true);
            streamWriter.WriteLine(chatMessage);
            streamWriter.Close();
        }
    }
}
