FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY KubePodApi.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=80
EXPOSE 80
USER $APP_UID
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "KubePodApi.dll"]

