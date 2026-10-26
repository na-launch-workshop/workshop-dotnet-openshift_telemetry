# 🚀 **Module: Using Red Hat build of OpenTelemetry**

**Technology Stack:**

- .NET 8 (ASP.NET Core)
- OpenTelemetry

---

## 🎯 **Scenario**

We will explore the Kubernetes Sidecar design pattern which deploys an agent sidecar container within an application pod that sends traces to a centralized Grafana Tempo. The application will send tracing data to a collector agent (sidecar) that offloads responsibility by sending the data to a storage backend, in our case, the central Grafana Tempo instance.

Instrumentation is the process of adding observability code to an application. Auto instrumentation creates this tracing framework without significant code changes by automatically injecting and configuring auto-instrumentation libraries in a variety of supported languages. Auto instrumentation of .NET sends data on port 4318 via the OTLP/HTTP protocol.

Inside your workspace is a .NET Application that listens at `/api/check` for web requests and returns an HTTP 200 or 500 status code based on the payload.

From the Terminal, deploy the .NET Application in your project using:

  ```sh
  oc new-build --binary --strategy=docker --name workshop-<your username>
  oc start-build workshop-<your username> --from-dir=.
  oc new-app workshop-<your username> --name workshop-<your username>
  ```

**NOTE: Please keep your application name unique from others in the workshop, otherwise you won’t be able to find your web requests in Jaeger UI.**

---

## 🐾 **Guided Walkthrough**

1. Deploy the sidecar with the OpenTelemetryCollector sidecar manifest:

  ```sh
  oc apply -f manifests/sidecar.yaml
  ```

2. Create an Instrumentation manifest in the project that will be used by the OpenTelemetry operator:

  ```sh
  oc apply -f manifests/instrumentation.yaml
  ```

3. Configure your deployment with annotations for .NET auto instrumentation:

  ```sh
  oc edit deployment workshop-<your username>

  spec:
    template:
      metadata:
        annotations:
          ..
          instrumentation.opentelemetry.io/inject-dotnet: "true"
          sidecar.opentelemetry.io/inject: sidecar
  ```

---

## 🧩 **Challenge**

- [ ] There is a cronjob at `curl-cron/cronjob.yaml` that will poll the application at service port 5000 every minute.
  - Modify the environment variable `ENDPOINT` to match the service name `workshop-<your username>` and deploy it with `oc apply -f curl-cron/cronjob.yaml`
- [ ] A centralized Grafana Tempo instance has been deployed for the workshop.  In OpenShift web console, navigate to the Applications Menu -> Jaeger UI to view the web requests.
- [ ] The cronjob polling the application returns an HTTP 500.  Fix the application so that it returns HTTP 200.
- [ ] (Optional) To generate additional web traffic, without waiting 1 minute for the cronjob to kickoff, we have provided a shell script that will randomly generate 20 HTTP POST requests to your service. To run this in the terminal of your Dev Space, run the following:

  ```sh
  sh traffic-generator.sh workshop-<your username>:5000
  ```

- [ ] You can create a new build of the application with the following command:

  ```sh
  oc start-build workshop-<your username> --from-dir=.
  ```

---

## 🥚 **Easter Eggs!**

- [ ] There is an easter egg in Jaeger

---

## 🛠️ **Running it locally**

```sh
dotnet run --project src/TelemetryWorkshop --urls http://0.0.0.0:5000
```

The application has **no OpenTelemetry NuGet dependencies**. `Activity` comes from
the shared framework, and the operator injects the tracing stack at deploy time —
which is the whole point of auto instrumentation.

| Path | Purpose |
|---|---|
| `src/TelemetryWorkshop/Program.cs` | Minimal API: `/` and `/api/check`, plus the custom span tags |
| `manifests/sidecar.yaml` | OpenTelemetryCollector running as a sidecar |
| `manifests/instrumentation.yaml` | Instrumentation CR consumed by the operator |
| `curl-cron/` | CronJob that polls `/api/check` every minute |
| `traffic-generator.sh` | Sends 20 random POSTs to `/api/check` |

---

## ✅ **Key Takeaways**

- Explored Kubernetes Sidecar design pattern
- Auto-instrumented application to collect tracing data
- Generated application traffic to produce tracing data
- Used tracing data to troubleshoot application
