using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VjoyWrapper
{
    public class Logger : ILogger
    {
        public void WriteLog(string error)
        {
            Console.WriteLine(error);
        }
    }
}
