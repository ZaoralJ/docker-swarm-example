FROM microsoft/dotnet:2.2-aspnetcore-runtime-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk-stretch AS build
WORKDIR /src
COPY ["MachineDataApi/MachineDataApi.csproj", "MachineDataApi/"]
RUN dotnet restore "MachineDataApi/MachineDataApi.csproj"
COPY . .
WORKDIR "/src/MachineDataApi"
RUN dotnet build "MachineDataApi.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "MachineDataApi.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MachineDataApi.dll"]