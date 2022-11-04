#!/bin/bash
# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

set -e
set -x

if [ ! -z "${DATABASEENGINE}" ] && [ "${DATABASEENGINE,,}" == "postgresql" ]; then

until PGPASSWORD=$POSTGRES_PASSWORD psql -h $POSTGRES_HOST -p $POSTGRES_PORT -U $POSTGRES_USER -c '\q';
do
  >&2 echo "DataImport Postgres is unavailable - sleeping"
  sleep 10
done

>&2 echo "Postgres is up - executing command"
exec $cmd

fi

dotnet DataImport.Web.dll