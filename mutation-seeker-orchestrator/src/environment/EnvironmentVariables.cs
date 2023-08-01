using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.environment;

namespace mutation_seeker_orchestrator.src.environment;
internal class EnvironmentVariables
{
    public string MongoConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public string CollectionName { get; set; }
    public string QueueAddress { get; set; }
    public string QueueLogin { get; set; }
    public string QueuePassword { get; set; }
    public int QueuePort { get; set; }
    public string OptlCollector { get; set; }

    private static EnvReader<EnvironmentVariables>? _instance;

    public static EnvironmentVariables GetEnvs()
    { 
        return (_instance ??= new EnvReader<EnvironmentVariables>()).Variables;
    }
}

