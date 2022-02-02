FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
COPY . /build
WORKDIR /build
RUN dotnet publish -c release -o publish

FROM mcr.microsoft.com/dotnet/aspnet:3.1-buster-slim
EXPOSE 80
COPY --from=build /build/publish /App/
WORKDIR /App
ENV COMPlus_DbgEnableMiniDump 1
ENV COMPlus_DbgMiniDumpType 1
ENV COMPlus_DbgMiniDumpName /OOMRepro.<pid>.dmp
ENTRYPOINT [ "dotnet", "OOMRepro.dll" ]
