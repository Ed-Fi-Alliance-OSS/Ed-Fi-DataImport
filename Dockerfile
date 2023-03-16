# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

#tag 6.0-alpine 
FROM mcr.microsoft.com/dotnet/aspnet@sha256:5d7911e8485a58ac50eefa09e2cea8f3d59268fd7f1501f72324e37e29d9d6ee
LABEL maintainer="Ed-Fi Alliance, LLC and Contributors <techsupport@ed-fi.org>"
ENV VERSION="2.0.1-alpha.0.15"
ENV TZ=${TIME_ZONE}

# Alpine image does not contain Globalization Cultures library so we need to install ICU library to get fopr LINQ expression to work
# Disable the globaliztion invariant mode (set in base image)
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

WORKDIR /app

RUN apk --no-cache add unzip=~6.0 dos2unix=~7.4 bash=~5.1 gettext=~0.21 postgresql-client=~13.10-r0 jq=~1.6 icu=~67.1 gcompat tzdata && \
    wget -O /app/DataImport.zip https://pkgs.dev.azure.com/ed-fi-alliance/Ed-Fi-Alliance-OSS/_apis/packaging/feeds/EdFi/nuget/packages/DataImport.Web/versions/${VERSION}/content && \
    unzip /app/DataImport.zip -d /app/DataImport && \
    cp -r /app/DataImport/DataImport.Web/. /app/DataImport.Web && \
    cp -r /app/DataImport/DataImport.Server.TransformLoad/. /app/DataImport.Server.TransformLoad && \
    chmod 755 /app/DataImport.Server.TransformLoad/DataImport.Server.TransformLoad -- ** && \  
    rm -r /app/DataImport && \ 
    rm -f /app/DataImport.zip

COPY Compose/pgsql/run.sh /app/DataImport.Web/run.sh
RUN dos2unix /app/DataImport.Web/*.json && \
    dos2unix /app/DataImport.Web/*.sh && \
    dos2unix /app/DataImport.Server.TransformLoad/*.json && \
    chmod 700 /app/DataImport.Web/*.sh -- **

EXPOSE 80

WORKDIR /app/DataImport.Web
ENTRYPOINT [ "/app/DataImport.Web/run.sh" ]
