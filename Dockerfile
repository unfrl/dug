FROM mcr.microsoft.com/dotnet/sdk:6.0 as build
WORKDIR /app

COPY ./cli/dug.csproj .
RUN dotnet restore
COPY ./cli .
RUN dotnet publish -r linux-x64 -c Release -p:PublishSingleFile=true -p:PublishReadyToRun=true --self-contained true -o publish

FROM mcr.microsoft.com/dotnet/runtime:6.0 as runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT [ "./dug" ]
