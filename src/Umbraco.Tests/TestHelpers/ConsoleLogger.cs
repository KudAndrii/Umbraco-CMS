﻿using System;
using Umbraco.Core.Logging;

namespace Umbraco.Tests.TestHelpers
{
    public class ConsoleLogger : ILogger
    {
        public void Fatal(Type reporting, Exception exception, string message)
        {
            Console.WriteLine("FATAL {0} - {1}", reporting.Name, message);
            Console.WriteLine(exception);
        }

        public void Fatal(Type reporting, Exception exception)
        {
            Console.WriteLine("FATAL {0}", reporting.Name);
            Console.WriteLine(exception);
        }

        public void Fatal(Type reporting, string message)
        {
            Console.WriteLine("FATAL {0} - {1}", reporting.Name, message);
        }

        public void Fatal(Type reporting, Exception exception, string format, params object[] args)
        {
            Console.WriteLine("FATAL {0} - {1}", reporting.Name, MessageTemplates.Render(format, args));
            Console.WriteLine(exception);
        }

        public void Fatal(Type reporting, string format, params object[] args)
        {
            Console.WriteLine("FATAL {0} - {1}", reporting.Name, MessageTemplates.Render(format, args));
        }

        public void Error(Type reporting, Exception exception, string message)
        {
            Console.WriteLine("ERROR {0} - {1}", reporting.Name, message);
            Console.WriteLine(exception);
        }

        public void Error(Type reporting, Exception exception)
        {
            Console.WriteLine("ERROR {0}", reporting.Name);
            Console.WriteLine(exception);
        }

        public void Error(Type reporting, string message)
        {
            Console.WriteLine("ERROR {0} - {1}", reporting.Name, message);
        }

        public void Error(Type reporting, Exception exception, string format, params object[] args)
        {
            Console.WriteLine("ERROR {0} - {1}", reporting.Name, MessageTemplates.Render(format, args));
            Console.WriteLine(exception);
        }

        public void Error(Type reporting, string format, params object[] args)
        {
            Console.WriteLine("ERROR {0} - {1}", reporting.Name, MessageTemplates.Render(format, args));
        }

        public void Warn(Type reporting, string message)
        {
            Console.WriteLine("WARN {0} - {1}", reporting.Name, message);
        }

        public void Warn(Type reporting, string format, params object[] args)
        {
            Console.WriteLine("WARN {0} - {1}", reporting.Name, MessageTemplates.Render(format, args));
        }

        public void Warn(Type reporting, Exception exception, string message)
        {
            Console.WriteLine("WARN {0} - {1}", reporting.Name, message);
            Console.WriteLine(exception);
        }

        public void Warn(Type reporting, Exception exception, string format, params object[] args)
        {
            Console.WriteLine("WARN {0} - {1}", reporting.Name, MessageTemplates.Render(format, args));
            Console.WriteLine(exception);
        }

        public void Info(Type reporting, string format, params object[] args)
        {
            Console.WriteLine("INFO {0} - {1}", reporting.Name, MessageTemplates.Render(format, args));
        }

        public void Info(Type reporting, string message)
        {
            Console.WriteLine("INFO {0} - {1}", reporting.Name, message);
        }

        public void Debug(Type reporting, string message)
        {
            Console.WriteLine("DEBUG {0} - {1}", reporting.Name, message);
        }

        public void Debug(Type reporting, string format, params object[] args)
        {
            Console.WriteLine("DEBUG {0} - {1}", reporting.Name, MessageTemplates.Render(format, args));
        }

        public void Verbose(Type reporting, string message)
        {
            Console.WriteLine("VERBOSE {0} - {1}", reporting.Name, message);
        }

        public void Verbose(Type reporting, string format, params object[] args)
        {
            Console.WriteLine("VERBOSE {0} - {1}", reporting.Name, MessageTemplates.Render(format, args));
        }
    }
}
