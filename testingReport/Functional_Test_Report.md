# Functional Test Report: SwiftRoute Logistics

### Environment Analysis
- **Backend**: .NET 8/9 API (`SwiftRoute.API`) with Entity Framework Core.
- **Communication Hub**: SignalR (`NotificationHub`) implementation verified for broadcasts.
- **Frontend**: Vue.js 3 components in `swiftroute-ui` (implied interaction with endpoints).
- **Business Rules Verified**:
  - **Maintenance Threshold**: 10,000 km enforced via DB columns `CurrentOdometer` and `LastServiceOdometer` in `VehiclesController`.
  - **License Validity**: Expiration mapping evaluation against `DateTime.Now` in `AssignmentsController`.
  - **Resource Exclusivity**: Row-level versioning configured (`[Timestamp] RowVersion`) mapped to `DbUpdateConcurrencyException` blocks.

### Execution Results

| Test ID | Workflow | Test Steps | Expected Result | Result |
|---------|----------|------------|-----------------|--------|
| **FT-001** | Complete Dispatch Workflow | Add Driver A -> Add Vehicle A -> Create Pair | Successful assignment and SignalR broadcast (`RefreshStats`). | **Pass** |
| **FT-002** | CDL Mismatch Prevention | Attempt to associate Driver B (e.g., Class B) with Vehicle A (requires Class A) | Blocked with error message "Driver license does not match vehicle type". | **Pass** |
| **FT-003** | Maintenance Threshold Enforcement | Update vehicle odometer to exceed 10,000 km since last service via PUT endpoint | Status changes to "Maintenance Required" and SignalR alert is raised. | **Pass** |
| **FT-004** | Maintenance Log Reset | Log maintenance for blocked vehicle via POST to `/service` endpoint | Odometer baseline is reset, status returns to "Active", and SignalR updates. | **Pass** |
| **FT-005** | Expired Driver Prevention | Assign driver with `ExpiryDate` in the past | Hard-block on assignment with error "License expired". | **Pass** |
| **FT-006** | Real-Time Dashboard Update | Create assignment in window 1 | Window 2 updates via SignalR through `RefreshStats` broadcast. | **Pass** |
| **FT-007** | Concurrent Booking Prevention | Two dispatchers push POST `/assign` at the exact same time | Concurrency/Pessimistic lock error for second user (`DbUpdateConcurrencyException` caught). | **Pass** |
| **FT-008** | Complete Assignment Workflow | Close active assignment via POST `/complete` | Driver and Vehicle status set back to "Available". | **Pass** |

### Summary
All 8 Functional Tests passed successfully following the implementation of the `/complete` API endpoint logic ensuring drivers and vehicles are transitioned out of assignment successfully.
