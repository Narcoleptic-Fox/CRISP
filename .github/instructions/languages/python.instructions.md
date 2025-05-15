---
applyTo: "**/*.py"
---
# Python Programming Guidelines

## Code Style

- Follow [PEP 8](https://peps.python.org/pep-0008/) style guide.
- Use 4 spaces for indentation (no tabs).
- Limit lines to 79 characters (or 88 if using Black formatter).
- Use blank lines to separate functions and classes, and larger blocks of code inside functions.
- Use docstrings for all public modules, functions, classes, and methods.
- Use inline comments sparingly, only when necessary to explain complex logic.

## Naming Conventions

- **Packages and Modules**: Lowercase, short names, no underscores: `requests`, `numpy`.
- **Classes**: CapitalizedWords (PascalCase): `MyClass`, `NetworkManager`.
- **Functions and Variables**: lowercase_with_underscores (snake_case): `calculate_total`, `user_name`.
- **Constants**: UPPERCASE_WITH_UNDERSCORES: `MAX_CONNECTIONS`, `PI`.
- **Private Attributes/Methods**: Prefix with single underscore: `_internal_method`.
- **"Magic" Methods**: Surrounded by double underscores: `__init__`, `__str__`.
- **Type Variables**: CapitalizedWords with optional parameters: `T`, `AnyStr`, `UserType`.

## Best Practices

- Write function and variable names as descriptive as possible.
- Use list, dict, and set comprehensions instead of loops when appropriate.
- Use generator expressions for large data sets.
- Use context managers (`with` statement) for resource management.
- Use exception handling with specific exception classes.
- Use type hints for better tooling and documentation.
- Write functions that do one thing well.
- Return early to avoid deep nesting.

```python
# Good
def get_user(user_id):
    if not user_id:
        return None
    
    # Process user_id
    return user

# Avoid
def get_user(user_id):
    if user_id:
        # Process user_id
        return user
    else:
        return None
```

## Type Hints

- Use type annotations for function parameters and return values.
- Use `Optional[T]` for parameters that could be `None`.
- Use `Union[T1, T2]` for parameters that could be multiple types.
- Use `Any` sparingly.
- Use `TypedDict` for dictionaries with known structure.
- Use `Protocol` for structural subtyping.

```python
from typing import Optional, List, Dict, Union, Any

def process_data(data: List[Dict[str, Any]]) -> Optional[str]:
    if not data:
        return None
    
    # Process data
    return result
```

## Package Imports

- Group imports in this order: standard library, third-party packages, local modules.
- Separate import groups with a blank line.
- Use absolute imports over relative imports.
- Avoid wildcard imports (`from module import *`).
- Use aliases for long or conflicting import names.

```python
# Standard library
import os
import sys
from datetime import datetime

# Third-party
import numpy as np
import pandas as pd

# Local
from .helpers import format_data
from .constants import DEFAULT_TIMEOUT
```

## Classes

- Use classes for logical grouping of methods and data.
- Use dataclasses for data containers.
- Implement special methods as needed (`__str__`, `__repr__`, etc.).
- Follow the principle of composition over inheritance.
- Use properties instead of getter/setter methods when appropriate.

## Error Handling

- Use exceptions rather than return codes.
- Prefer specific exception classes over generic ones.
- Catch only exceptions you can handle.
- Use `finally` for cleanup code.
- Include context information in custom exceptions.

```python
try:
    process_file(filename)
except FileNotFoundError:
    logger.error(f"File {filename} not found")
except PermissionError:
    logger.error(f"Permission denied for file {filename}")
except Exception as e:
    logger.error(f"Unexpected error: {e}")
```

## Testing

- Write unit tests for all new functionality.
- Use pytest or unittest.
- Use fixtures and parameterized tests to reduce duplication.
- Mock external dependencies.
- Aim for high test coverage.

## Python-Specific Idioms

- Use `if x is not None` instead of `if not x is None`.
- Use `if x` instead of `if x is True` or `if x == True`.
- Use `enumerate()` for getting index while iterating.
- Use `zip()` to iterate over multiple sequences simultaneously.
- Use `dict.get(key, default)` instead of using explicit if-else for dictionary lookup.
- Use f-strings for string formatting (Python 3.6+).
- Use keyword arguments for better readability with many parameters.

## Resources

- [PEP 8 -- Style Guide for Python Code](https://peps.python.org/pep-0008/)
- [PEP 484 -- Type Hints](https://peps.python.org/pep-0484/)
- [Python Official Documentation](https://docs.python.org/3/)
- [The Hitchhiker's Guide to Python](https://docs.python-guide.org/)
- [Real Python](https://realpython.com/)
- [Python Cookbook by David Beazley and Brian K. Jones](https://www.oreilly.com/library/view/python-cookbook-3rd/9781449357337/)
