FROM mcr.microsoft.com/dotnet/sdk:5.0.102-ca-patch-buster-slim-amd64 as build
WORKDIR /app

COPY ./cli/dug.csproj .
RUN dotnet restore
COPY ./cli .
RUN dotnet publish -r linux-x64 -c Release -p:PublishSingleFile=true -p:PublishTrimmed=true -p:PublishReadyToRun=true --self-contained true -o publish

FROM mcr.microsoft.com/dotnet/runtime:5.0 as runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT [ "./dug" ]
