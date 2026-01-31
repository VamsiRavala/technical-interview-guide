# REST
- REST (Representational State Transfer) is an architectural style for designing networked applications.
- REST APIs use HTTP methods to perform CRUD operations (Create, Read, Update, Delete) on resources, which are typically represented as URLs.

# Core Principles of REST API Design
 ## 1. Resources and URIs
  - Everything in a REST API is a resource, represented by a URI (Uniform Resource Identifier).
  - Use nouns, not verbs.
  - Example:
       ```
        GET /users
        GET /users/123
        POST /users
        PUT /users/123
        DELETE /users/123
          ```
      - Avoid: /getUsers, /createUser, etc.

  ## 2. HTTP Methods
  - Use the appropriate HTTP verbs for operations:
      
    | HTTP | Method	 | Operation Description|
    |----|----|--------|
    | GET  | 	Read	 | Retrieve data|
    | POST	 | Create	 | Add a new resource|
    | PUT	 | Update	 | Replace an existing resource|
    | PATCH	 | Partial Update	 | Update part of a resource|
    | DELETE	 | Delete	 | Remove a resource|

   ##  3. Status Codes
   - Always return proper HTTP status codes:
     
     | Code  | Meaning  | Example|
     |----|----|--------|
     | 200 | OK	 Successful read | 	GET completed| 
     | 201 | Created	Resource created	| POST successful| 
     | 204 | No Content	Deleted successfully | 	DELETE successful| 
     | 400 | Bad Request	| Invalid input	
     | 401 | Unauthorized	| Authentication required	| 
     | 404 | Not Found	| Resource doesn’t exist	
     | 500 | Internal Server Error| 	Server failure	|

  ## 4. Request and Response Format
  - Prefer JSON for data exchange.
  - Use consistent field naming (camelCase or snake_case).
  - Example response:
      ```json
      {
      "id": 123,
      "name": "John Doe",
      "email": "john.doe@example.com"
      }
      ```

  ## 5. Versioning
  - Always version your API to avoid breaking clients when updating:
    ```bash
    /api/v1/users
    /api/v2/users
    ```

  ## 6. Filtering, Sorting, and Pagination
  - Support query parameters for efficiency:
    ```bash
    GET /users?role=admin&sort=createdAt&page=2&limit=20
    ```

  ## 7. Authentication and Security
  - Use OAuth2 or JWT (JSON Web Token) for authentication.
  - Always use HTTPS.
  - Avoid sending sensitive data in URLs.

  ## 8. Error Handling
  - Return clear and structured error responses:
    ```json
      {
        "error": {
        "code": 400,
        "message": "Email is required"
        }
      }
    ```

  ## 10. HATEOAS (Optional for Discoverability)
  - Include links to related resources:
```json
{
  "id": 1,
  "name": "John",
  "links": {
    "self": "/users/1",
    "orders": "/users/1/orders"
  }
}
```

  ## 11. Documentation
  - Use OpenAPI (Swagger) to document endpoints.
  - Clearly describe methods, parameters, and example responses.


    
      
