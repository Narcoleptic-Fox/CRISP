---
applyTo: "**/*api*.md,**/*api*.yaml,**/*swagger*.json,**/*openapi*.*"
---

# API Design Principles

## Overview

This document defines standards and best practices for designing consistent, user-friendly, and maintainable APIs. Following these principles ensures that APIs are easy to understand, integrate with, and evolve over time.

## General Design Principles

- **API First**: Design APIs before implementing them
- **Consistency**: Follow consistent patterns throughout all APIs
- **Simplicity**: Make common tasks simple and complex tasks possible
- **Evolvability**: Design APIs that can evolve without breaking existing clients
- **Documentation**: Provide comprehensive, accurate documentation
- **Security**: Design with security as a fundamental requirement

## REST API Design Guidelines

### URI Design

- Use nouns, not verbs for resources
- Use plural nouns for collections
- Use concrete names, not abstract concepts
- Organize resources hierarchically

```
✅ Good: /users/123/orders
❌ Bad: /getOrdersForUser/123
```

- Keep URIs simple, intuitive, and consistent
- Use kebab-case for multi-word resource names

```
✅ Good: /order-items
❌ Bad: /orderItems or /order_items
```

- Avoid deep nesting (not more than 3 levels)

```
✅ Good: /orders/123/items
❌ Bad: /customers/456/orders/123/items/789/options
```

### HTTP Methods

- **GET**: Retrieve a resource or collection
- **POST**: Create a new resource or perform a complex operation
- **PUT**: Replace a resource completely
- **PATCH**: Update a resource partially
- **DELETE**: Remove a resource

### Status Codes

Use appropriate HTTP status codes:

| Code | Description | Usage |
|------|-------------|-------|
| 200 | OK | Successful GET, PUT, PATCH or DELETE |
| 201 | Created | Successful resource creation via POST |
| 204 | No Content | Successful operation with no response body |
| 400 | Bad Request | Invalid request format or parameters |
| 401 | Unauthorized | Authentication required |
| 403 | Forbidden | Authentication succeeded but insufficient permissions |
| 404 | Not Found | Resource not found |
| 409 | Conflict | Request conflicts with resource state |
| 422 | Unprocessable Entity | Validation errors |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Server Error | Server error |

### Query Parameters

- Use for filtering, sorting, pagination, and field selection
- Use consistent parameter naming conventions

```
GET /users?role=admin&status=active      # Filtering
GET /products?sort=price:asc,name:desc   # Sorting
GET /orders?page=2&per_page=10           # Pagination
GET /users?fields=id,name,email          # Field selection
```

### Request Body Format

- Use JSON for request and response bodies
- Use camelCase for property names
- Validate request bodies against a schema
- Include required fields validation

Example request body:
```json
{
  "firstName": "Jane",
  "lastName": "Smith",
  "emailAddress": "jane.smith@example.com",
  "preferences": {
    "marketingEmails": false,
    "theme": "dark"
  }
}
```

### Response Format

- Return consistent response formats
- Include appropriate metadata
- Use envelope only when necessary
- Include pagination metadata for collections

Example response for a collection:
```json
{
  "data": [
    {
      "id": "123",
      "name": "Product A",
      "price": 19.99
    },
    {
      "id": "456",
      "name": "Product B",
      "price": 29.99
    }
  ],
  "pagination": {
    "totalItems": 57,
    "totalPages": 6,
    "currentPage": 1,
    "itemsPerPage": 10
  }
}
```

Example response for a single resource:
```json
{
  "id": "123",
  "name": "Product A",
  "description": "A detailed product description.",
  "price": 19.99,
  "category": "electronics",
  "createdAt": "2023-05-01T12:00:00Z",
  "updatedAt": "2023-05-10T15:30:00Z"
}
```

### Error Handling

Return consistent error formats:

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid request parameters",
    "details": [
      {
        "field": "email",
        "message": "Must be a valid email address"
      },
      {
        "field": "password",
        "message": "Must be at least 8 characters long"
      }
    ],
    "requestId": "request-123"
  }
}
```

- Include useful error messages
- Add field-level validation details when applicable
- Include request ID for troubleshooting
- Document all possible error codes

### Versioning

- Include version in the URL path
- Use major versions only (v1, v2)
- Maintain backward compatibility within a major version

```
✅ Good: /v1/users/123
```

- Document version deprecation policy
- Provide migration guides between versions

### Filtering, Sorting, and Pagination

- Implement consistent query parameters for filtering

```
GET /products?category=electronics&price[gte]=100&price[lte]=500
```

- Support sorting on multiple fields

```
GET /users?sort=lastName:asc,firstName:asc
```

- Implement pagination for all collection endpoints

```
GET /articles?page=2&per_page=25
```

- Return pagination metadata in responses
- Use cursor-based pagination for large datasets or real-time data

```
GET /events?after=ev_123&limit=100
```

### HATEOAS (Hypertext As The Engine Of Application State)

Consider including related resource links:

```json
{
  "id": "123",
  "name": "Product A",
  "price": 19.99,
  "_links": {
    "self": {
      "href": "/v1/products/123"
    },
    "reviews": {
      "href": "/v1/products/123/reviews"
    },
    "category": {
      "href": "/v1/categories/electronics"
    }
  }
}
```

## GraphQL API Design Guidelines

### Schema Design

- Follow naming conventions (CamelCase for types, camelCase for fields)
- Design schema around business domains
- Define clear object relationships
- Make root queries focused and specific

```graphql
type User {
  id: ID!
  firstName: String!
  lastName: String!
  email: String!
  orders(limit: Int, offset: Int): [Order!]!
  reviews: [Review!]!
}

type Order {
  id: ID!
  createdAt: DateTime!
  items: [OrderItem!]!
  totalAmount: Float!
  status: OrderStatus!
}

enum OrderStatus {
  PENDING
  PROCESSING
  SHIPPED
  DELIVERED
  CANCELLED
}
```

### Query Design

- Design queries for specific use cases
- Implement pagination for collections
- Support filtering and sorting arguments
- Consider query complexity and depth limits

```graphql
type Query {
  user(id: ID!): User
  userByEmail(email: String!): User
  
  products(
    category: ID,
    minPrice: Float,
    maxPrice: Float,
    search: String,
    first: Int,
    after: String
  ): ProductConnection!
  
  product(id: ID!): Product
}
```

### Mutations

- Use descriptive names that indicate the action
- Return the modified resource and related objects
- Group related input fields in input types
- Validate inputs and return meaningful errors

```graphql
type Mutation {
  createUser(input: CreateUserInput!): CreateUserPayload!
  updateUser(input: UpdateUserInput!): UpdateUserPayload!
  deleteUser(id: ID!): DeleteUserPayload!
}

input CreateUserInput {
  firstName: String!
  lastName: String!
  email: String!
  password: String!
}

type CreateUserPayload {
  user: User
  errors: [Error!]
}
```

### Error Handling

- Use standard GraphQL error responses
- Implement custom error types for business errors
- Return partial results when possible
- Include error codes and user-friendly messages

```graphql
type Error {
  code: String!
  message: String!
  path: [String!]
}

type ValidationError implements Error {
  code: String!
  message: String!
  path: [String!]
  fieldErrors: [FieldError!]!
}

type FieldError {
  field: String!
  message: String!
}
```

### Performance Considerations

- Implement dataloaders for batching requests
- Use query complexity analysis
- Consider persisted queries for production
- Implement caching strategies

### Versioning

- Avoid breaking changes when possible
- Use deprecation annotations for evolving the schema
- Document deprecated fields with reason and alternatives
- Consider schema directives for versioning

```graphql
type User {
  id: ID!
  name: String! @deprecated(reason: "Use firstName and lastName instead")
  firstName: String!
  lastName: String!
}
```

## API Security Best Practices

### Authentication

- Use industry-standard protocols (OAuth 2.0, OpenID Connect)
- Issue short-lived access tokens and longer-lived refresh tokens
- Implement proper token validation
- Use secure storage for tokens
- Support multiple authentication methods when appropriate

### Authorization

- Implement role-based or attribute-based access control
- Check permissions for every operation
- Don't expose sensitive data in responses
- Implement resource-level authorization
- Document permission requirements for each endpoint

### Transport Security

- Require HTTPS for all API endpoints
- Configure proper TLS settings
- Implement HTTP security headers
- Use certificate pinning for mobile clients

### Input Validation

- Validate all input parameters
- Implement schema validation
- Sanitize inputs to prevent injection attacks
- Validate content types and request sizes
- Implement strict CORS policies

### Rate Limiting

- Apply rate limits to prevent abuse
- Use token bucket or leaky bucket algorithms
- Include rate limit headers in responses

```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1621880400
```

- Implement different limits for different endpoints
- Consider implementing application-level throttling

## API Documentation

### OpenAPI/Swagger

- Document APIs using OpenAPI Specification
- Include detailed descriptions for endpoints
- Document request parameters and response schemas
- Include examples for requests and responses
- Document error responses and codes

Example OpenAPI Path:
```yaml
paths:
  /users:
    get:
      summary: List all users
      description: Returns a paginated list of users
      parameters:
        - name: page
          in: query
          description: Page number
          schema:
            type: integer
            default: 1
        - name: per_page
          in: query
          description: Items per page
          schema:
            type: integer
            default: 10
      responses:
        '200':
          description: Successful operation
          content:
            application/json:
              schema:
                type: object
                properties:
                  data:
                    type: array
                    items:
                      $ref: '#/components/schemas/User'
                  pagination:
                    $ref: '#/components/schemas/Pagination'
```

### GraphQL Schema Documentation

- Include descriptions for all types and fields
- Document deprecated fields with reasons
- Provide usage examples
- Document directives and custom scalars

```graphql
"""
Represents a user in the system.
"""
type User {
  """
  Unique identifier for the user.
  """
  id: ID!
  
  """
  User's email address, which must be unique.
  """
  email: String!
}
```

### General Documentation Guidelines

- Keep documentation up to date
- Include authentication details
- Document rate limits and quotas
- Provide integration examples in multiple languages
- Include a getting started guide
- Document versioning and deprecation policies

## API Design Process

1. **Requirements Gathering**
   - Identify API consumers and their needs
   - Define use cases and user stories
   - Establish functional requirements
   - Determine non-functional requirements

2. **Resource Modeling**
   - Identify key resources and their relationships
   - Define resource attributes
   - Design resource hierarchy
   - Map business operations to API operations

3. **API Design Review**
   - Conduct internal design reviews
   - Verify against design principles
   - Check for consistency with existing APIs
   - Validate against use cases

4. **API Prototyping**
   - Create interactive API documentation
   - Implement mock responses
   - Gather feedback from stakeholders
   - Refine the design based on feedback

5. **Implementation Planning**
   - Define implementation milestones
   - Establish development standards
   - Plan for testing and validation
   - Prepare for deployment and monitoring

## API Governance

- Establish API style guidelines
- Implement design reviews for new APIs
- Use automated linting for API definitions
- Create reusable API patterns
- Monitor API usage and gather feedback
- Continuously improve API design
