---
applyTo: "**/*API*.md,**/*api*.yaml,**/*swagger*.json,**/*openapi*.*"
---
# REST API Documentation Guidelines

## Documentation Structure

### 1. Introduction
- **API Overview**: Describe the purpose and scope of the API.
- **Base URL**: Document the base URL for API requests.
- **Versioning Strategy**: Explain how API versioning works.
- **Authentication Methods**: Outline supported authentication methods.
- **Rate Limiting**: Document rate limits and throttling policies.
- **Common Response Formats**: Describe the standard response format.
- **Common Status Codes**: List common HTTP status codes and their meanings.

### 2. Getting Started
- **Prerequisites**: List what users need before using the API.
- **Authentication Setup**: Describe how to obtain and use credentials.
- **Basic Request Example**: Provide a simple example to get users started.
- **Client Libraries**: Link to official client libraries if available.
- **Sandbox Environment**: Instructions for accessing test environments.

### 3. Core Resources
For each resource:
- **Resource Description**: What the resource represents.
- **Resource URL Structure**: The URL pattern for the resource.
- **Available Methods**: HTTP methods supported (GET, POST, PUT, DELETE, etc.).
- **Query Parameters**: Parameters for filtering, sorting, pagination.
- **Request Headers**: Required and optional headers.
- **Request Body**: Expected format for request bodies where applicable.
- **Response Format**: Structure of the response.
- **Response Codes**: Specific status codes and their meanings.
- **Examples**: Request and response examples for each operation.

### 4. Common Patterns
- **Pagination**: How to navigate large result sets.
- **Filtering**: How to filter results.
- **Sorting**: How to sort results.
- **Search**: How to search across resources.
- **Error Handling**: Common error patterns and resolution strategies.
- **Partial Responses**: How to request only specific fields.
- **Batch Operations**: How to perform bulk operations if supported.

### 5. Webhooks (if applicable)
- **Webhook Setup**: How to configure webhook endpoints.
- **Event Types**: List of events that trigger webhooks.
- **Payload Format**: Structure of webhook payloads.
- **Security Considerations**: How to validate webhook authenticity.
- **Retry Logic**: How webhook delivery retries work.

### 6. SDK & Integration Examples
- **Code Examples**: In multiple languages (JavaScript, Python, Java, etc.).
- **Common Use Cases**: Examples of typical integration scenarios.
- **Integration Patterns**: Best practices for integration.

### 7. Reference
- **Complete Endpoint List**: Comprehensive list of all endpoints.
- **Schema Definitions**: Detailed data models and their relationships.
- **Error Codes**: Complete list of possible error codes.
- **Glossary**: Definitions of domain-specific terms.

## OpenAPI/Swagger Specifications

### Structure
- Use OpenAPI Specification (formerly Swagger) to document APIs.
- Organize endpoints by resource or functional area.
- Include detailed schemas for request and response bodies.
- Document all parameters with clear descriptions and constraints.

### Key Components
- **Info Object**: API title, description, version, contact info.
- **Servers**: URL endpoints where the API is available.
- **Paths**: Endpoints and their operations.
- **Components**: Reusable schemas, responses, parameters, examples.
- **Security Schemes**: Authentication methods.
- **Tags**: For grouping operations.

### Example OpenAPI Structure
```yaml
openapi: 3.0.0
info:
  title: Sample API
  description: Optional description
  version: 1.0.0
servers:
  - url: https://api.example.com/v1
    description: Production server
paths:
  /users:
    get:
      summary: Returns a list of users
      description: Optional detailed description
      parameters:
        - name: limit
          in: query
          description: Maximum number of users to return
          required: false
          schema:
            type: integer
            format: int32
      responses:
        '200':
          description: A JSON array of user objects
          content:
            application/json:
              schema:
                type: array
                items:
                  $ref: '#/components/schemas/User'
components:
  schemas:
    User:
      type: object
      properties:
        id:
          type: integer
          format: int64
        name:
          type: string
```

## Best Practices

### Consistency
- Use consistent naming conventions for endpoints, parameters, and properties.
- Follow REST conventions (use nouns for resources, HTTP methods for actions).
- Maintain consistent response structures across endpoints.
- Use standard HTTP status codes appropriately.

### Clarity
- Write clear, concise descriptions for all elements.
- Provide examples for complex request/response structures.
- Document edge cases and error scenarios.
- Explain domain-specific concepts.

### Completeness
- Document all possible response status codes.
- Specify all required and optional parameters.
- Include validation constraints (min/max values, patterns, etc.).
- Document rate limits and authentication requirements.

### Usability
- Organize documentation logically from simple to complex.
- Provide quickstart guides for common use cases.
- Include runnable examples (e.g., cURL, Postman collections).
- Include a change log to track API changes.

## Documentation Formats and Tools

### Documentation Formats
- **OpenAPI/Swagger**: Industry standard for REST API documentation.
- **RAML**: RESTful API Modeling Language.
- **API Blueprint**: Markdown-based API documentation format.
- **HAL**: Hypertext Application Language for hypermedia APIs.
- **JSON Schema**: For detailed data model documentation.

### Documentation Tools
- **Swagger UI**: Interactive documentation from OpenAPI specs.
- **ReDoc**: Alternative OpenAPI documentation renderer.
- **Postman**: API development and documentation platform.
- **Stoplight**: API design, documentation, and governance platform.
- **GitBook**: For comprehensive API guides.

## API Versioning

### Versioning Strategies
- **URL Path Versioning**: `/api/v1/resource`
- **Query Parameter**: `/api/resource?version=1`
- **Header-Based**: `Accept: application/vnd.company.v1+json`
- **Content Negotiation**: `Accept: application/json;version=1`

### Version Documentation
- Clearly mark deprecated features.
- Document breaking vs. non-breaking changes.
- Provide migration guides between versions.
- Indicate sunset dates for deprecated versions.

## Authentication Documentation

### Authentication Types
- **API Keys**: Where to obtain and how to include in requests.
- **OAuth 2.0**: Authorization flows, token endpoints, scopes.
- **JWT**: Token structure, claims, validation.
- **Basic Auth**: Usage and security considerations.

### Security Considerations
- Document HTTPS requirements.
- Explain token lifetimes and refresh procedures.
- Document permission models and scopes.
- Include security best practices for API consumers.

## Common HTTP Status Codes

- **200 OK**: The request was successful.
- **201 Created**: The resource was successfully created.
- **204 No Content**: The request was successful but returns no content.
- **400 Bad Request**: The request is malformed or invalid.
- **401 Unauthorized**: Authentication is required or failed.
- **403 Forbidden**: The client doesn't have permission to access the resource.
- **404 Not Found**: The requested resource doesn't exist.
- **405 Method Not Allowed**: The HTTP method is not supported for this resource.
- **409 Conflict**: The request conflicts with the current state of the resource.
- **422 Unprocessable Entity**: The request was well-formed but has semantic errors.
- **429 Too Many Requests**: The client has sent too many requests in a given time.
- **500 Internal Server Error**: A generic server error occurred.
- **503 Service Unavailable**: The server is temporarily unavailable or overloaded.

## Resources

- [OpenAPI Specification](https://swagger.io/specification/)
- [REST API Design Best Practices](https://restfulapi.net/)
- [JSON:API Specification](https://jsonapi.org/)
- [Microsoft REST API Guidelines](https://github.com/microsoft/api-guidelines)
- [Google API Design Guide](https://cloud.google.com/apis/design)
- [Zalando RESTful API Guidelines](https://opensource.zalando.com/restful-api-guidelines/)
