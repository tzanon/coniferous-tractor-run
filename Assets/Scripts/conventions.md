# Conventions

### Casing
- _camelCase with leading underscore for private fields
- camelCase for local variables/parameters
- PascalCase for property, method, and class names

### Variable accessibility
- Only use public fields if completely necessary. Use `[SerializeField]` to expose private field to inspector
- If some thing must be set from another class use an auto property instead of a public field
- If just need to get value from somewhere else use auto property with public get, private set


