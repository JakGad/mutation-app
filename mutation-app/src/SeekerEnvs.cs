using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.environment;

namespace mutation_app.src
{
    internal class SeekerEnvs
    {
        public string SupervisorAddress { get; set; }
        public string QueueAddress { get; set; }
        public string QueueLogin { get; set; }
        public string QueuePassword { get; set; }
        public int QueuePort { get; set; }
        public int SupervisorPort { get; set; }
        public string OptlCollector { get; set; }

        private static EnvReader<SeekerEnvs>? _instance;

        public static SeekerEnvs GetEnvs()
        {
            return (_instance ??= new EnvReader<SeekerEnvs>()).Variables;
        }
    }
}
