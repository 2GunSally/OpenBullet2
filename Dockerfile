# Docker build arguments
ARG DOTNET_VERSION=8.0
ARG NODEJS_VERSION=20
ARG SOURCE_DIR=/app

# Build the web-server artifacts
FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION}-bookworm-slim as server

# Default environment variables
ENV DEBIAN_FRONTEND="noninteractive" \
    DOTNET_CLI_TELEMETRY_OPTOUT=1\
    LC_ALL="en_US.UTF-8" \
    LANG="en_US.UTF-8" \
    LANGUAGE="en_US:en"

# https://github.com/dlemstra/Magick.NET/issues/707#issuecomment-785351620
ENV MALLOC_TRIM_THRESHOLD_=131072

# Install dependencies:
RUN apt-get update --yes \
 && apt-get upgrade --yes \
 && apt-get install --no-install-recommends --yes \
    curl \
    unzip \ 
    python3 \
    python3-pip \
    build-essential \
    firefox-esr \
    chromium \
    jq

# Install nodejs:
# RUN mkdir -p /etc/apt/keyrings \
# && curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key | sudo gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg \
# && echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_$NODEJS_VERSION.x nodistro main" | sudo tee /etc/apt/sources.list.d/nodesource.list \
# && apt-get install --no-install-recommends --yes \
#    nodejs 
    
# Install webdrivermanager:
RUN pip3 install webdrivermanager || true
RUN webdrivermanager firefox chrome --linkpath /usr/local/bin || true

WORKDIR ${SOURCE_DIR}

# Install openbullet and clean up:
RUN curl -fsSLO $(curl -fsSL https://api.github.com/repos/WizzardHub/OpenBullet2/releases/latest | jq -r '.assets[] | select(.name == "OpenBullet2.zip") | .browser_download_url') \
 && unzip OpenBullet2.zip \
 && rm OpenBullet2.zip \
 && apt-get remove unzip --yes \
 && apt-get clean autoclean --yes \
 && apt-get autoremove --yes \
 && rm -rf /var/cache/apt/archives* /var/lib/apt/lists/*

EXPOSE 5000
CMD ["dotnet", "./OpenBullet2.dll", "--urls=http://*:5000"]