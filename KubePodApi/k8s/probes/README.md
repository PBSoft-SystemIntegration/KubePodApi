# Øvelse: startup, readiness og liveness

## Det får I, og det skal I selv lave

API'et har tre healthchecks på `/healthz/startup`, `/healthz/readiness` og
`/healthz/liveness`. De returnerer 200 (Healthy) eller 503 (Unhealthy).
Startup kræver det antal kald, som `CALLS_BEFORE_SUCCESS` angiver.
To POST-endpoints i Scalar kan sætte readiness og liveness til true/false.
Det er en simulation af fejl, kun til undervisningen. En genstart nulstiller
begge værdier til true og startup-tælleren til 0.

Dette er løsningen på branchen `probes-solution`. Alle tre probes i
ProbePod.yaml er udfyldt. Følg forsøgene nedenfor for at kontrollere resultatet.
Elevens startpunkt uden udfyldte probes findes på branchen `probes-startup`.

## 1. Byg og start (fra mappen med KubePodApi.slnx)

```powershell
docker build -t probe-pod:exercise-v1 ./KubePodApi
docker save probe-pod:exercise-v1 -o "$env:TEMP\probe-exercise.tar"
docker cp "$env:TEMP\probe-exercise.tar" desktop-control-plane:/probe-exercise.tar
docker exec desktop-control-plane ctr -n k8s.io images import /probe-exercise.tar
kubectl apply -f ./KubePodApi/k8s/probes/ProbePod.yaml
kubectl apply -f ./KubePodApi/k8s/probes/ProbeService.yaml
```

Det indlæser imaget i jeres ene kind-node. Har I allerede importeret samme tag
før en kodeændring, så brug et nyt tag både ved build/import og i pod-filen.
Alternativt kan I pushe et nyt tag til eget Docker Hub-repository og ændre
`image` samt `imagePullPolicy` til `IfNotPresent` i pod-filen.

## 2. Startup: lykkes og fejler

Tilføj startupProbe med httpGet til `/healthz/startup`, port 80,
periodSeconds 5 og failureThreshold 5. Behold CALLS_BEFORE_SUCCESS = "4".
Pod'en skal genoprettes, hver gang I ændrer miljøvariabler eller probes:

```powershell
kubectl delete pod probes-exercise
kubectl apply -f ./KubePodApi/k8s/probes/ProbePod.yaml
kubectl get pod probes-exercise -w
```

Ctrl+C stopper kun visningen. Forvent READY 0/1 under opstart og derefter 1/1.
Skift værdien fra "4" til "6", genopret og se RESTARTS stige: checket kan ikke nå
at lykkes inden fem fejl. Sæt værdien tilbage til "4" og genopret inden næste del.
Se Events med `kubectl describe pod probes-exercise` og appens output med
`kubectl logs probes-exercise`. Manuelle kald til startup-endpointet tæller også med.

## 3. Readiness: stop og genoptag trafik uden genstart

Tilføj readinessProbe til `/healthz/readiness`, port 80, periodSeconds 5 og
failureThreshold 1. Behold startup-proben og genopret pod'en.
Vent på READY 1/1. Start derefter en direkte tunnel i en separat terminal:

```powershell
kubectl port-forward pod/probes-exercise 9000:80
```

Åbn **http://localhost:9000/scalar**. Under POST `/demo/readiness/{healthy}`
indtastes false, og kaldet udføres. Efter næste probe bliver READY 0/1.

Test Servicen fra en anden terminal (ny forbindelse og ingen browsercache):

```powershell
curl.exe --max-time 5 -H "Connection: close" http://localhost:8085/Info
```

Kaldet virker normalt før fejlen, men fejler eller får timeout, når pod'en ikke
er Ready. Servicen kan bruge nogle sekunder på at opdatere trafikfordelingen.
Sæt true gennem **den direkte tunnel**, vent på READY 1/1 og gentag kaldet.
Svaret skal vende tilbage, og RESTARTS skal være uændret.

Servicen er en almindelig LoadBalancer-Service; den matcher pod'ens label.
Port 8085 på Servicen fører til appens port 80. Der kræves ingen speciel
probe-konfiguration på Servicen. En direkte pod-tunnel omgår trafikfordelingen,
så brug aldrig tunnelen som bevis for, at readiness stopper trafik.
Hvis localhost:8085 ikke virker allerede før forsøget, kontrollér
`kubectl get service probes-exercise`; løs den eksterne adgang først.

## 4. Liveness: genstart containeren

Tilføj livenessProbe til `/healthz/liveness`, port 80, periodSeconds 5 og
failureThreshold 3. Behold de to andre probes og genopret pod'en.
Genåbn tunnelen og Scalar, når pod'en er Ready.
Sæt false med POST `/demo/liveness/{healthy}`. Lad readiness forblive true.

Efter tre fejl påbegyndes nedlukning. Appen kan få op til 30 sekunder til at stoppe.
Følg `kubectl get pod probes-exercise -w` og Events. RESTARTS skal stige,
podnavnet skal være det samme, og startup-proben skal lykkes på ny.
Tunnelen kan afbrydes ved genstart; start den igen. Liveness er nu automatisk
Healthy, fordi processen er genstartet.

## Færdig når

- Alle tre probes er udfyldt sammen i jeres pod-YAML.
- I har vist startup-succes og genstart efter startup-fejl.
- I har vist, at readiness stopper/genoptager Service-trafik uden genstart.
- I har vist, at liveness øger RESTARTS og nulstiller demo-tilstanden.
- I kan forklare forskellen på de tre reaktioner.

Ryd kun øvelsens ressourcer op:

```powershell
kubectl delete -f ./KubePodApi/k8s/probes/ProbePod.yaml
kubectl delete -f ./KubePodApi/k8s/probes/ProbeService.yaml
```
