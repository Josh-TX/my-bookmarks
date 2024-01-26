# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
WORKDIR /source

# copy csproj and restore as distinct layers
#COPY Server/*.csproj .
#RUN dotnet restore -a $TARGETARCH

# copy and publish app and libraries
COPY Server/. .
#RUN dotnet publish -a $TARGETARCH --no-restore -o /app
RUN dotnet publish -a $TARGETARCH -o /app


# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
RUN mkdir /app/data && chown $APP_UID /app/data
VOLUME /app/data
USER $APP_UID

ENTRYPOINT ["./MyBookmarks"]
