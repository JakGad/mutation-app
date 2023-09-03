if($args[0] -eq 'stop')
{
    kubectl scale --replicas=0 deployments  --all -n mutation-app
}

if($args[0] -eq 'start')
{
    kubectl scale --replicas=1 deployments --all -n mutation-app
    kubectl scale --replicas=3 deployments mutation-app -n mutation-app
}

if($args[0] -eq 'mongo')
{
    kubectl scale --replicas=1 deployments mongo -n mutation-app
}

if($args[0] -eq 'forward')
{
    kubectl port-forward service/rabbitmq -n mutation-app 15672:15672 & kubectl port-forward service/mongo -n mutation-app 27017:27017 & kubectl port-forward service/grafana -n mutation-app 3000:3000
}

if($args[0] -eq 'apply')
{
    kubectl apply -f .\mutation-orchestrator-deployment.yaml
    kubectl apply -f .\mutation-app-deployment.yaml
}