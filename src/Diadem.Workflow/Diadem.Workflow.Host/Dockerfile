FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

# copy libraries => prepare pre-cached layers
COPY ./libraries ./libraries

# copy csproj and restore as distinct layers
COPY ./src ./src

# restore NuGet dependcies
WORKDIR /app/src/Diadem.Workflow/Diadem.Workflow.Host
RUN dotnet restore

# build workflow host
FROM build AS publish
WORKDIR /app/src/Diadem.Workflow/Diadem.Workflow.Host
RUN dotnet publish -c Debug -o /out
# RUN dotnet publish -c Release -o /out

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS final
WORKDIR /app

COPY --from=publish /out .
COPY ./libraries/assets ./libraries/assets


ENV CORE_ASSETSDIRECTORY=./libraries/assets/
ENV HOST_CONFIGURATIONDIRECTORY=../workflow-host/configuration
VOLUME ["/workflow-host"]


ENTRYPOINT ["dotnet", "Diadem.Workflow.Host.dll"]