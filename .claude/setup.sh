#!/bin/bash
# Install .NET 10 SDK for Claude Code cloud sessions
set -e

apt-get update -q
apt-get install -y dotnet-sdk-10.0

dotnet --version
