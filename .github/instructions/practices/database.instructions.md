---
applyTo: "**/*database*.md,**/*schema*.sql,**/*data*.md,**/*db*.md"
---
# Database Design Documentation Guidelines

## Database Overview Documentation

### Database Architecture
- **Architecture Diagram**: Create a high-level database architecture diagram (.drawio.png format).
- **Database Type**: Document the database management system (RDBMS, NoSQL, etc.) and version.
- **Instance Types**: Document server/instance specifications for different environments.
- **High Availability Setup**: Document clustering, replication, and failover mechanisms.
- **Backup Strategy**: Document backup procedures, frequency, and retention policy.

### Database Environments
- **Environment List**: Document all database environments (development, test, staging, production).
- **Environment Configurations**: Document configuration differences between environments.
- **Data Synchronization**: Document procedures for refreshing non-production environments.
- **Access Controls**: Document who has access to each environment and permission levels.

## Schema Documentation

### Entity Relationship Diagrams
- **ERD Standards**: Create ERDs using Draw.io (.drawio.png format).
- **Diagram Organization**: Break down large schemas into logical diagram segments.
- **Notation Standards**: Use consistent notation (Crow's Foot, Chen, UML, etc.).
- **Entity Representations**: Include primary keys, foreign keys, and essential attributes.
- **Relationship Labels**: Clearly label relationships with cardinality and optionality.

### Table Documentation
For each table, document:
- **Purpose**: Brief description of what the table represents.
- **Primary Key**: Column(s) that form the primary key.
- **Foreign Keys**: References to other tables.
- **Columns**: For each column:
  - Name
  - Data type and constraints
  - Description
  - Default value (if any)
  - Whether NULL is allowed
- **Indexes**: List of indexes with included columns and purpose.
- **Triggers**: List of triggers with description of when/why they fire.
- **Partitioning**: Partitioning scheme if applicable.

### Schema Template
```markdown
# Table: [Table Name]

## Purpose
Brief description of what this table represents in the domain model.

## Primary Key
- Column(s) that form the primary key

## Foreign Keys
| Column | References | On Delete | On Update |
|--------|------------|-----------|-----------|
| column_name | table_name(column_name) | CASCADE/SET NULL/etc. | CASCADE/RESTRICT/etc. |

## Columns
| Name | Type | Description | Constraints | Default | Nullable |
|------|------|-------------|------------|---------|----------|
| column_name | data_type | Description | constraints | default value | Yes/No |

## Indexes
| Name | Type | Columns | Purpose |
|------|------|---------|---------|
| index_name | BTREE/HASH/etc. | column1, column2 | Performance for X queries |

## Triggers
| Name | Timing | Event | Purpose |
|------|--------|-------|---------|
| trigger_name | BEFORE/AFTER | INSERT/UPDATE/DELETE | Purpose description |
```

## Data Model Documentation

### Conceptual Data Model
- **Purpose**: Document high-level entity types and relationships.
- **Audience**: Business stakeholders and analysts.
- **Elements**: Major entities and relationships without implementation details.
- **Diagram**: Create conceptual diagram in Draw.io (.drawio.png format).

### Logical Data Model
- **Purpose**: Document the full data model independent of implementation.
- **Audience**: System architects and designers.
- **Elements**: All entities, attributes, relationships, and normalization.
- **Diagram**: Create logical model diagrams in Draw.io (.drawio.png format).

### Physical Data Model
- **Purpose**: Document the implementation-specific data model.
- **Audience**: Database developers and administrators.
- **Elements**: Tables, columns, data types, constraints, indexes, etc.
- **Diagram**: Create physical model diagrams in Draw.io (.drawio.png format).

## SQL Code Documentation

### SQL Scripts
- **Header Comments**: Include purpose, author, and date in script headers.
- **Section Comments**: Divide scripts into logical sections with comments.
- **Complex Logic**: Explain complex queries, joins, or conditions.
- **Performance Considerations**: Document expected performance characteristics.
- **Dependencies**: Note any dependencies on other objects.
- **Version Control**: Store scripts in version control.

### Stored Procedures and Functions
For each stored procedure or function, document:
- **Purpose**: What it does.
- **Parameters**: Input and output parameters with data types and descriptions.
- **Return Value**: What is returned and its meaning.
- **Business Rules**: Business logic implemented.
- **Error Handling**: How errors are handled.
- **Usage Example**: Sample call with parameters.

### Stored Procedure Template
```markdown
# Procedure: [Procedure Name]

## Purpose
Brief description of what this procedure does.

## Parameters
| Name | Direction | Type | Description | Default |
|------|-----------|------|-------------|---------|
| param_name | IN/OUT/INOUT | data_type | Description | default value |

## Return Value
Description of what is returned.

## Business Rules
- Rule 1
- Rule 2

## Error Handling
Description of how errors are handled.

## Usage Example
```sql
EXEC procedure_name @param1 = value1, @param2 = value2;
```

## Expected Results
Description of expected results.
```

## Database Migration Documentation

### Migration Strategy
- **Migration Approach**: Document the approach to schema migrations.
- **Migration Tools**: Document tools used (Flyway, Liquibase, custom scripts, etc.).
- **Version Control**: How migrations are versioned and tracked.
- **Testing Strategy**: How migrations are tested before production.
- **Rollback Procedures**: How to roll back failed migrations.

### Migration Script Standards
- **Naming Convention**: Standard format for migration script names.
- **Script Structure**: Content organization within scripts.
- **Transaction Handling**: Transaction boundaries and error handling.
- **Idempotence**: Ensuring scripts can be run multiple times safely.
- **Dependencies**: How to handle dependencies between migrations.

### Migration Log
- **Migration History**: Record of applied migrations.
- **Date and Time**: When migrations were applied.
- **Applied By**: Who applied each migration.
- **Status**: Success or failure status.
- **Duration**: How long migrations took to apply.
- **Errors**: Any errors encountered during migration.

## Performance Documentation

### Query Optimization
- **Slow Query Identification**: Process for identifying slow queries.
- **Query Plans**: How to capture and analyze execution plans.
- **Index Strategy**: Guidelines for index creation and maintenance.
- **Statistics**: Schedule for statistics updates.

### Performance Benchmarks
- **Key Queries**: Document performance expectations for key queries.
- **Load Testing**: Results from load testing the database.
- **Bottlenecks**: Identified bottlenecks and mitigation strategies.
- **Scaling Strategy**: Vertical and horizontal scaling options.

### Monitoring and Alerting
- **Monitoring Tools**: What tools are used to monitor database performance.
- **Key Metrics**: Which metrics are tracked and their baseline values.
- **Alerts**: Configured alerts and their thresholds.
- **Response Procedures**: How to respond to different types of alerts.

## Security Documentation

### Access Control
- **User Management**: How database users are managed.
- **Role Definitions**: Database roles and their permissions.
- **Permission Matrix**: What roles have access to what objects.
- **Authentication**: Authentication methods (password, certificate, integrated, etc.).
- **Principle of Least Privilege**: How least privilege is enforced.

### Data Protection
- **Encryption**: Data encryption at rest and in transit.
- **Sensitive Data**: How sensitive data is identified and protected.
- **Data Masking**: Masking rules for non-production environments.
- **Audit Logging**: What actions are logged and where logs are stored.

## Best Practices

### Naming Conventions
- **Tables**: Guidelines for table naming (singular/plural, prefixes, etc.).
- **Columns**: Guidelines for column naming.
- **Constraints**: Naming patterns for constraints (PK, FK, unique, check).
- **Indexes**: Naming patterns for indexes.
- **Stored Procedures**: Naming patterns for stored procedures.

### SQL Coding Standards
- **SQL Formatting**: Indentation, capitalization, and formatting rules.
- **Join Syntax**: Preferred join syntax (ANSI vs. older syntax).
- **NULL Handling**: Guidelines for handling NULL values.
- **Error Handling**: Standard error handling approaches.
- **Transactions**: Transaction management guidelines.

### Database Design Principles
- **Normalization**: Guidelines for appropriate normalization level.
- **Denormalization**: When and how to denormalize for performance.
- **Constraints**: When to use different types of constraints.
- **Referential Integrity**: How to enforce data relationships.
- **Soft Deletes**: When to use soft deletes vs. actual deletion.

## Tools and Resources

### Database Design Tools
- **Modeling Tools**: ER/Studio, MySQL Workbench, pgModeler, etc.
- **Diagramming**: Draw.io/diagrams.net (save as .drawio.png)
- **Script Generators**: SQL generation from models
- **Compare Tools**: Schema comparison tools

### Database Administration Tools
- **Monitoring**: Prometheus, Grafana, native monitoring tools
- **Backup Solutions**: Database-specific backup tools
- **Query Analysis**: Execution plan analyzers

### Documentation Resources
- [Database Design (C.J. Date)](https://www.pearson.com/en-us/subject-catalog/p/database-design-and-relational-theory-normal-forms-and-all-that-jazz/P200000009464)
- [SQL Style Guide](https://www.sqlstyle.guide/)
- [Database Normalization Explained](https://www.essentialsql.com/get-ready-to-learn-sql-database-normalization-explained-in-simple-english/)
- [Documentation System by Divio](https://documentation.divio.com/)

## Templates

### Data Dictionary Template
```markdown
# Data Dictionary: [Database Name]

## Tables Summary
| Table Name | Description | Estimated Row Count | Category |
|------------|-------------|---------------------|----------|
| table_name | Description | count | category |

## Detailed Table Information
(Include table documentation for each table)
```

### Database Change Request Template
```markdown
# Database Change Request

## Request Information
- **Request ID**: [ID]
- **Requested By**: [Name]
- **Requested Date**: [Date]
- **Priority**: [High/Medium/Low]

## Change Description
Brief description of the change.

## Justification
Why this change is needed.

## Impact Assessment
- **Tables Affected**: [List of tables]
- **Applications Affected**: [List of applications]
- **Performance Impact**: [None/Low/Medium/High]
- **Security Impact**: [None/Low/Medium/High]

## Implementation Plan
- **SQL Scripts**: [Link to scripts]
- **Rollback Plan**: [Rollback approach]
- **Testing Plan**: [How the change will be tested]

## Approval
- **Reviewed By**: [Name]
- **Approved By**: [Name]
- **Approval Date**: [Date]
```
