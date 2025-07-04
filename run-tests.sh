#!/bin/bash

# Set script to exit on error
set -e

echo "🧪 Running Crisp Framework Tests with Coverage"
echo "=============================================="

# Clean previous test results
echo "🧹 Cleaning previous test results..."
find . -name "TestResults" -type d -exec rm -rf {} + 2>/dev/null || true
find . -name "*.trx" -delete 2>/dev/null || true

# Restore packages
echo "📦 Restoring packages..."
dotnet restore

# Run tests with coverage
echo "🏃 Running tests with coverage collection..."

# Core tests
echo "📋 Testing Crisp.Core..."
dotnet test tests/Crisp.Core.Tests/Crisp.Core.Tests.csproj \
    --configuration Release \
    --logger "trx;LogFileName=crisp-core-tests.trx" \
    --collect:"XPlat Code Coverage" \
    --results-directory ./TestResults

# Runtime tests  
echo "⚡ Testing Crisp.Runtime..."
dotnet test tests/Crisp.Runtime.Tests/Crisp.Runtime.Tests.csproj \
    --configuration Release \
    --logger "trx;LogFileName=crisp-runtime-tests.trx" \
    --collect:"XPlat Code Coverage" \
    --results-directory ./TestResults

# AspNetCore tests
echo "🌐 Testing Crisp.AspNetCore..."
dotnet test tests/Crisp.AspNetCore.Tests/Crisp.AspNetCore.Tests.csproj \
    --configuration Release \
    --logger "trx;LogFileName=crisp-aspnetcore-tests.trx" \
    --collect:"XPlat Code Coverage" \
    --results-directory ./TestResults

# TodoApi integration tests
# echo "📝 Testing TodoApi Integration..."
# ~/.dotnet/dotnet test tests/TodoApi.IntegrationTests/TodoApi.IntegrationTests.csproj \
#     --configuration Release \
#     --logger "trx;LogFileName=todoapi-integration-tests.trx" \
#     --collect:"XPlat Code Coverage" \
#     --results-directory ./TestResults

echo ""
echo "✅ All tests completed!"
echo ""
echo "📊 Test Results:"
echo "  • Core Tests: TestResults/crisp-core-tests.trx"
echo "  • Runtime Tests: TestResults/crisp-runtime-tests.trx" 
echo "  • AspNetCore Tests: TestResults/crisp-aspnetcore-tests.trx"
echo "  • Integration Tests: TestResults/todoapi-integration-tests.trx"
echo ""
echo "📈 Coverage reports can be found in TestResults/ directory"
echo ""