FROM registry.access.redhat.com/ubi8/dotnet-80:8.0 AS build
COPY --chown=1001:0 src/TelemetryWorkshop/ .
RUN dotnet publish -c Release -o /opt/app-root/publish

FROM registry.access.redhat.com/ubi8/dotnet-80-runtime:8.0
COPY --from=build /opt/app-root/publish .
EXPOSE 5000
ENV ASPNETCORE_URLS=http://0.0.0.0:5000
CMD ["dotnet", "TelemetryWorkshop.dll"]
