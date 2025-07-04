# Crisp.AspNetCore Test Coverage Implementation Plan

This document provides step-by-step instructions for increasing Crisp.AspNetCore test coverage from 15.52% to 85%+.

## Current Coverage Status
- **Current Coverage**: 15.52% line coverage, 7.44% branch coverage
- **Target Coverage**: 85%+ line coverage, 80%+ branch coverage
- **Tested Components**: Builder (✅), ServiceCollectionExtensions (✅), HealthChecks (✅)
- **Untested Components**: Endpoints, Extensions, Security, Metadata, Middleware, OpenApi

## Phase 1: Core Endpoints (Priority: Critical)

### 1.1 CommandEndpoint Tests
**File**: `tests/Crisp.AspNetCore.Tests/Endpoints/CommandEndpointTests.cs`

**Key Classes to Test**:
```csharp
CommandEndpoint<TCommand, TResponse>
VoidCommandEndpoint<TCommand>
```

**Test Scenarios**:
```csharp
[Fact] public void Constructor_ShouldSetDefaultPatternAndMethod()
[Fact] public void Constructor_WithCustomValues_ShouldUseProvided()
[Fact] public void Map_ShouldRegisterEndpointWithCorrectConfiguration()
[Theory] public async Task Handle_GetOrDelete_ShouldBindFromRoute(string httpMethod)
[Theory] public async Task Handle_PostPutPatch_ShouldBindFromBody(string httpMethod)
[Fact] public async Task Handle_InvalidBody_ShouldThrowBadHttpRequest()
[Fact] public async Task Handle_PostCommand_ShouldReturnCreated()
[Fact] public async Task Handle_DeleteCommand_ShouldReturnNoContent()
```

### 1.2 QueryEndpoint Tests
**File**: `tests/Crisp.AspNetCore.Tests/Endpoints/QueryEndpointTests.cs`

**Key Class**: `QueryEndpoint<TQuery, TResponse>`

**Test Scenarios**:
```csharp
[Fact] public void Constructor_ShouldSetPropertiesCorrectly()
[Fact] public void Constructor_WithCustomPattern_ShouldUseProvided()
[Fact] public void Map_ShouldRegisterGetEndpoint()
[Fact] public async Task Handle_ShouldBindFromRouteAndQuery()
[Fact] public async Task Handle_ShouldReturnOkWithResponse()
```

### 1.3 EndpointMapper Tests
**File**: `tests/Crisp.AspNetCore.Tests/Endpoints/EndpointMapperTests.cs`

**Key Methods**:
```csharp
MapCommandEndpoint<TCommand, TResponse>()
MapQueryEndpoint<TQuery, TResponse>()
MapEndpoints()
```

### 1.4 EndpointConventions Tests
**File**: `tests/Crisp.AspNetCore.Tests/Endpoints/EndpointConventionsTests.cs`

**Key Methods**:
```csharp
DetermineRoutePattern(Type commandType)
DetermineHttpMethod(Type commandType)
BindFromRoute<T>(HttpContext context)
BindFromRouteAndQuery<T>(HttpContext context)
GetSummary(Type type)
ExtractTag(Type type)
```

## Phase 2: Extensions & Integrations (Priority: High)

### 2.1 Application Builder Extensions Tests
**File**: `tests/Crisp.AspNetCore.Tests/Extensions/CrispApplicationBuilderExtensionsTests.cs`

**Key Methods**:
```csharp
UseCrisp(IApplicationBuilder app)
UseCrispExceptionHandling(IApplicationBuilder app)
```

### 2.2 Endpoint Route Builder Extensions Tests
**File**: `tests/Crisp.AspNetCore.Tests/Extensions/EndpointRouteBuilderExtensionsTests.cs`

**Key Methods**:
```csharp
MapCrisp(IEndpointRouteBuilder endpoints)
MapCommand<TCommand, TResponse>()
MapQuery<TQuery, TResponse>()
```

### 2.3 Security Extensions Tests
**File**: `tests/Crisp.AspNetCore.Tests/Extensions/SecurityExtensionsTests.cs`

**Key Methods**:
```csharp
AddCrispSecurity(IServiceCollection services)
RequireAuthorization()
RequireRateLimit()
```

### 2.4 Swagger Extensions Tests
**File**: `tests/Crisp.AspNetCore.Tests/Extensions/SwaggerExtensionsTests.cs`

**Key Methods**:
```csharp
AddCrispSwagger(IServiceCollection services)
UseCrispSwagger(IApplicationBuilder app)
```

## Phase 3: Security & Behaviors (Priority: High)

### 3.1 Authorization Behavior Tests
**File**: `tests/Crisp.AspNetCore.Tests/Security/AuthorizationBehaviorTests.cs`

**Key Scenarios**:
```csharp
[Fact] public async Task Handle_AuthorizedUser_CallsNext()
[Fact] public async Task Handle_UnauthorizedUser_ThrowsUnauthorized()
[Fact] public async Task Handle_NoAuthRequirement_CallsNext()
[Fact] public async Task Handle_PolicyBased_ValidatesPolicy()
```

### 3.2 Input Sanitization Behavior Tests
**File**: `tests/Crisp.AspNetCore.Tests/Security/InputSanitizationBehaviorTests.cs`

**Key Scenarios**:
```csharp
[Fact] public async Task Handle_CleanInput_PassesThrough()
[Fact] public async Task Handle_MaliciousInput_SanitizesInput()
[Fact] public async Task Handle_XssAttempt_RemovesScripts()
[Fact] public async Task Handle_SqlInjection_EscapesInput()
```

### 3.3 Rate Limiting Behavior Tests
**File**: `tests/Crisp.AspNetCore.Tests/Security/RateLimitingBehaviorTests.cs`

**Key Scenarios**:
```csharp
[Fact] public async Task Handle_WithinLimit_CallsNext()
[Fact] public async Task Handle_ExceedsLimit_ThrowsTooManyRequests()
[Fact] public async Task Handle_ResetsAfterWindow_AllowsRequest()
```

### 3.4 Security Audit Service Tests
**File**: `tests/Crisp.AspNetCore.Tests/Security/SecurityAuditServiceTests.cs`

**Key Methods**:
```csharp
LogSecurityEvent(SecurityEvent securityEvent)
LogFailedAuthentication(string user, string reason)
LogUnauthorizedAccess(string user, string resource)
```

## Phase 4: Middleware & Support (Priority: Medium)

### 4.1 Exception Middleware Tests
**File**: `tests/Crisp.AspNetCore.Tests/Middleware/CrispExceptionMiddlewareTests.cs`

**Key Scenarios**:
```csharp
[Fact] public async Task InvokeAsync_NoException_CallsNext()
[Fact] public async Task InvokeAsync_ValidationException_Returns400()
[Fact] public async Task InvokeAsync_NotFoundException_Returns404()
[Fact] public async Task InvokeAsync_UnauthorizedException_Returns401()
[Fact] public async Task InvokeAsync_GenericException_Returns500()
[Fact] public async Task InvokeAsync_ProducesCorrectProblemDetails()
```

### 4.2 Metadata Tests
**File**: `tests/Crisp.AspNetCore.Tests/Metadata/`

**Files to Create**:
- `EndpointMetadataTests.cs`
- `HttpMethodAttributeTests.cs`
- `HttpMethodsTests.cs`
- `RouteAttributeTests.cs`

### 4.3 OpenApi Generator Tests
**File**: `tests/Crisp.AspNetCore.Tests/OpenApi/CrispOpenApiGeneratorTests.cs`

**Key Methods**:
```csharp
GenerateSchema(Type type)
GenerateOperationDescription(Type commandType)
AddSecurityRequirements(OpenApiOperation operation)
```

### 4.4 ProblemDetails Tests
**File**: `tests/Crisp.AspNetCore.Tests/ProblemDetailsTests.cs`

**Key Scenarios**:
```csharp
[Fact] public void Constructor_SetsPropertiesCorrectly()
[Fact] public void FromException_ValidationException_CreatesProblemDetails()
[Fact] public void FromException_NotFoundException_CreatesProblemDetails()
```

## Implementation Commands

### Create Test Directories
```bash
cd "D:\Projects\Visual Studios\Quetzal\Crisp\tests\Crisp.AspNetCore.Tests"

# Create missing directories
mkdir -p Endpoints Extensions Security Middleware Metadata OpenApi

# Verify structure
tree /f
```

### Test Infrastructure Setup

Update `Crisp.AspNetCore.Tests.csproj` dependencies:
```xml
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" />
<PackageReference Include="Microsoft.AspNetCore.TestHost" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" />
<PackageReference Include="Microsoft.Extensions.Hosting" />
```

### Base Test Classes

Create `TestBase.cs`:
```csharp
public abstract class TestBase
{
    protected IServiceProvider CreateServiceProvider()
    protected TestServer CreateTestServer()
    protected HttpClient CreateTestClient()
}
```

## Test Implementation Patterns

### Testing Endpoints
```csharp
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Add test services
            services.AddSingleton<ICommandDispatcher, TestCommandDispatcher>();
        });
    }
}
```

### Testing Behaviors
```csharp
[Fact]
public async Task Behavior_Scenario_ExpectedResult()
{
    // Arrange
    var next = Substitute.For<RequestHandlerDelegate<TResponse>>();
    var behavior = new AuthorizationBehavior<TRequest, TResponse>();
    
    // Act
    var result = await behavior.Handle(request, next, CancellationToken.None);
    
    // Assert
    result.Should().NotBeNull();
}
```

### Testing Middleware
```csharp
[Fact]
public async Task Middleware_Exception_ReturnsCorrectStatusCode()
{
    // Arrange
    var context = new DefaultHttpContext();
    var middleware = new CrispExceptionMiddleware(
        next: (ctx) => throw new ValidationException("Test"),
        logger: Mock.Of<ILogger<CrispExceptionMiddleware>>());
    
    // Act
    await middleware.InvokeAsync(context);
    
    // Assert
    context.Response.StatusCode.Should().Be(400);
}
```

## Coverage Verification

```bash
# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --results-directory TestResults

# Generate detailed coverage report
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage-report -reporttypes:Html

# Open coverage report
start coverage-report/index.html
```

## Success Criteria

**Phase 1 Complete**: 50%+ coverage (Endpoints tested)
**Phase 2 Complete**: 65%+ coverage (Extensions tested)  
**Phase 3 Complete**: 80%+ coverage (Security tested)
**Phase 4 Complete**: 85%+ coverage (All components tested)

## Priority Test Creation Order

1. **CommandEndpointTests.cs** - Highest impact
2. **QueryEndpointTests.cs** - Highest impact
3. **EndpointRouteBuilderExtensionsTests.cs** - High usage
4. **CrispExceptionMiddlewareTests.cs** - Critical path
5. **AuthorizationBehaviorTests.cs** - Security critical
6. **EndpointMapperTests.cs** - Core functionality
7. **EndpointConventionsTests.cs** - Core functionality
8. Remaining files in order of complexity

## Notes

- Focus on happy path scenarios first, then edge cases
- Use realistic test data reflecting actual usage patterns
- Mock external dependencies but test actual Crisp components
- Verify both success and failure scenarios
- Test middleware pipeline integration
- Validate security behaviors thoroughly
- Ensure OpenApi generation works correctly