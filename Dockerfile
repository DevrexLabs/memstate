FROM microsoft/dotnet:1.1.2-runtime

COPY ./Memstate.Host/bin/Debug/netcoreapp1.1/publish /memstate

#origodb/memstate wellknown port
EXPOSE 3001

ENTRYPOINT  ["dotnet", "/memstate/Memstate.Host.dll"]
