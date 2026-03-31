# Security Testing Report: SwiftRoute Logistics

### Codebase Security Analysis

- **Backend (.NET 8/9 API)**:
  - **SQL Injection**: Prevented globally by the use of Entity Framework Core (`AppDbContext`), which automatically converts LINQ queries into safely parameterized SQL.
  - **Mass Assignment**: The API uses strict Data Transfer Objects (`VehicleDTO` and `DriverDTO`) in POST and PUT requests within `VehiclesController` and `DriversController`. This completely mitigates over-posting attacks by validating only allowed fields.
  - **Concurrency**: Properly implemented using EF Core Optimistic Concurrency Control. The `[Timestamp] byte[] RowVersion` attributes on the entities ensure that concurrent updates trigger a `DbUpdateConcurrencyException`, successfully terminating race conditions.
- **Frontend (Vue.js 3)**:
  - **Cross-Site Scripting (XSS)**: The frontend utilizes the Vue 3 Composition API (`<script setup>`) combined with native template interpolation (`{{ data }}`). Since `v-html` is entirely avoided, Vue inherently escapes malicious `<script>` injections.
- **Middleware**:
  - **CORS Policy**: Configured securely to allow only local frontend origins (`http://localhost:5173`) in `Program.cs`.
  - **Error-Handling**: An explicit global error-handling middleware block uses `app.UseExceptionHandler()` to intercept crashes, masking server exception details and preventing sensitive stack-trace logs from leaking publicly.

### Security Testing Results

| Test ID | Security Area | Threat Being Tested | Testing Approach | Expected Safeguard | Result |
|---------|---------------|---------------------|------------------|--------------------|--------|
| **ST-001** | Data Access | SQL Injection | Input `' OR '1'='1` in text fields / identifiers. | Application should map database sets securely using Entity Framework Core. | **Pass** |
| **ST-002** | Frontend Rendering | Cross-Site Scripting (XSS) | Input `<script>` tags in client-side text inputs. | Vue 3 Composition API and text-interpolation automatically escape inputs. | **Pass** |
| **ST-003** | Origin Control | CORS Policy | Unauthorized origin requests (e.g., executing fetch from random domains). | Explicit CORS Policy matching exactly `http://localhost:5173`. | **Pass** |
| **ST-004** | Model Binding | Mass Assignment | Sending extra fields like `IsAvailable` or `Status` in DTO mapping. | Usage of Data Transfer Objects (DTOs) restricting specific mapped properties on POST/PUT endpoints. | **Pass** |
| **ST-005** | Transaction Control| Concurrency / Race Condition | Simultaneous booking requests targeting the same vehicle/driver. | Attributes mapping `RowVersion` or explicit EF tracking to trigger concurrency blocks. | **Pass** |
| **ST-006** | Middleware Defense| Sensitive Data Exposure | Inspecting API error stack traces during 500 Internal Server Errors. | Global explicit error-handling middleware silencing sensitive stack-traces. | **Pass** |
