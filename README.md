# KubePodApi – dag 1

.NET 10, controllers og Scalar. Åbn KubePodApi.slnx i Visual Studio.
Kubernetes-filerne findes i projektets **k8s**-mappe.

## API'et fra slidesene

- `/Info`: viser POD_NAME, POD_IP og Version: 0.1.0.
- `/GetFromOtherService/{serviceUrlAndPort}`: kalder en anden apps `/Info`.
- `/scalar`: brugerflade til at afprøve de to endpoints.
- Ved opstart skrives podnavn og IP i loggen.

POD_NAME og POD_IP leveres af deployment-filerne via Downward API.
De er tomme ved almindelig lokal kørsel uden disse miljøvariabler.

## Lokal kørsel

Fra mappen med løsningen:

```powershell
dotnet run --project ./KubePodApi
```

Åbn http://localhost:5034/scalar.

## Byg og opret første Deployment

Fra mappen med løsningen:

```powershell
docker build -t kube_image_rest:0.1.0 ./KubePodApi
kubectl apply -f ./KubePodApi/k8s/deployment.yaml
kubectl get pods
```

Vælg et podnavn, og lav en tunnel:

```powershell
kubectl port-forward pod/PODNAVN 8000:8080
```

Åbn http://localhost:8000/Info eller http://localhost:8000/scalar.
Stop tunnelen med Ctrl+C. Deploymentet holder tre pods kørende og erstatter slettede pods.

## Kommunikation mellem to apps

```powershell
kubectl apply -f ./KubePodApi/k8s/deployment2.yaml
kubectl apply -f ./KubePodApi/k8s/service.yaml
kubectl apply -f ./KubePodApi/k8s/service2.yaml
kubectl get services
```

Åbn Scalar via den første Services eksterne adresse, eller lav en tunnel:

```powershell
kubectl port-forward service/pod-info-service 8000:80
```

Åbn http://localhost:8000/scalar. Afprøv GetFromOtherService med værdien
`pod-info-service2:80` (uden http://). Svaret kommer fra en pod i det andet Deployment.

## Navne og porte, der skal passe med slidesene

| Del | Værdi |
|---|---|
| Image i begge Deployments | kube_image_rest:0.1.0 |
| Første Deployment / label | pod-info-deployment / app: pod-info |
| Andet Deployment / label | pod-info-deployment2 / app: pod-info2 |
| Ekstern Service | pod-info-service, LoadBalancer, selector app: pod-info |
| Intern Service | pod-info-service2, ClusterIP, selector app: pod-info2 |
| Containerens lytteport | 8080 |
| Service port / targetPort | 80 / 8080 |

Containerporten er ændret fra de gamle slides' 80 til 8080. Ret pod-tunnels til
fx `8000:8080`, containerPort til 8080 og Service targetPort til 8080.
Service-navne og port 80 bruges stadig til intern kommunikation.

`imagePullPolicy: Never` forudsætter, at noden har adgang til det lokalt byggede image.
Det er endnu ikke afprøvet i Docker Desktop/kind herfra. Ved ErrImageNeverPull skal
image-tilgængeligheden undersøges; API'et og image-build alene bekræfter ikke dette.
Port-forward til en Service vælger en pod og demonstrerer ikke trafikfordeling.

## Oprydning

```powershell
kubectl delete -f ./KubePodApi/k8s
```

API'et er bevidst et lille undervisningseksempel. Der er ingen healthchecks endnu,
ingen generel URL-proxy og ingen ekstra validerings- eller fejlhåndteringskode.
