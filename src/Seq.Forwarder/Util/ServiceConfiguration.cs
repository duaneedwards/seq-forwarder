﻿// Copyright 2016-2017 Datalust Pty Ltd
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Serilog;

namespace Seq.Forwarder.Util
{
    public static class ServiceConfiguration
    {
        public static bool GetServiceBinaryPath(ServiceController controller, TextWriter cout, out string path)
        {
            var sc = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "sc.exe");

            var config = new StringBuilder();
            if (0 != CaptiveProcess.Run(sc, "qc \"" + controller.ServiceName + "\"", l => config.AppendLine(l), cout.WriteLine))
            {
                cout.WriteLine("Could not query service path; ignoring.");
                path = null;
                return false;
            }

            var lines = config.ToString()
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim());

            var line = lines
                .SingleOrDefault(l => l.StartsWith("BINARY_PATH_NAME   : "));

            if (line == null)
            {
                cout.WriteLine("No existing binary path could be determined.");
                path = null;
                return false;
            }

            path = line.Replace("BINARY_PATH_NAME   : ", "");
            return true;
        }

        static bool GetServiceCommandLine(string serviceName, TextWriter cout, out string path)
        {
            if (serviceName == null) throw new ArgumentNullException(nameof(serviceName));
            if (cout == null) throw new ArgumentNullException(nameof(cout));

            var sc = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "sc.exe");

            var config = new StringBuilder();
            if (0 != CaptiveProcess.Run(sc, "qc \"" + serviceName + "\"", l => config.AppendLine(l), cout.WriteLine))
            {
                cout.WriteLine("Could not query service path; ignoring.");
                path = null;
                return false;
            }

            var lines = config.ToString()
                .Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim());

            var line = lines
                .SingleOrDefault(l => l.StartsWith("BINARY_PATH_NAME   : "));

            if (line == null)
            {
                cout.WriteLine("No existing binary path could be determined.");
                path = null;
                return false;
            }

            path = line.Replace("BINARY_PATH_NAME   : ", "");
            return true;
        }

        public static bool GetServiceStoragePath(string serviceName, StringWriter cout, out string storage)
        {
            if (serviceName == null) throw new ArgumentNullException(nameof(serviceName));
            if (cout == null) throw new ArgumentNullException(nameof(cout));

            string binpath;
            if (GetServiceCommandLine(serviceName, new StringWriter(), out binpath) &&
                binpath.Contains("--storage=\""))
            {
                var start = binpath.IndexOf("--storage=\"", StringComparison.Ordinal) + 11;
                var chop = binpath.Substring(start);
                storage = chop.Substring(0, chop.IndexOf('"'));
                return true;
            }

            storage = null;
            return false;
        }
    }
}
