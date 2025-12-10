# RP3 - Condotel Management System
## Requirements Document

---

## 4. Non-Functional Requirements

### 4.1 External Interfaces

#### 4.1.1 User Interface (UI):

The system will provide a responsive, web-based interface accessible via modern browsers (Chrome, Firefox, Edge, Safari). The UI will be optimized with consistent design patterns across modules (Tenant, Host, Admin). The frontend application will communicate with the backend through RESTful APIs and real-time SignalR connections for chat functionality.

#### 4.1.2 Integration with External Systems:

**PayOS Payment Gateway**: Integrated for online payment transactions. The system uses PayOS API to create payment links, verify payment status, and process payment callbacks. Minimum payment amount is 10,000 VND.

**Cloudinary Image Service**: Integrated for image upload and management. The system uses Cloudinary API to upload, store, and optimize images for user profiles, condotel listings, and blog posts. Supports JPEG, PNG, and WebP formats with automatic compression.

**Google OAuth Service**: Integrated for single sign-on (SSO) authentication. Users can register and login using their Google accounts. The system validates Google tokens and maps user profile information.

**Email Service (via SMTP)**: Integrated for automated email notifications such as registration confirmation, OTP codes for email verification and password reset, payment receipts, booking confirmations, and system notifications.

**SignalR Real-time Communication**: Integrated for real-time chat functionality between users (Tenants and Hosts). Supports WebSocket connections for instant messaging, conversation management, and message delivery notifications.

#### 4.1.3 Peripheral Device Support:

The system will support basic peripheral devices such as:
- Standard Input Devices (keyboard, mouse, touchscreen)
- Web cameras for profile image capture (optional)
- Mobile devices for responsive web access

### 4.2 Quality Attributes

#### 4.2.1 User Interface (UI):

##### 4.2.1.1 User Training:

Minimal training will be required for Tenants and Hosts since the system provides intuitive navigation, contextual help, and clear error messages. Training documents and short tutorial videos will be provided for Admins handling advanced features such as user management, promotion management, and system configuration.

##### 4.2.1.2 User Experience:

The platform will follow a clean, modern design with easy navigation and minimal steps for key workflows (registration, condotel search, booking, payment, review submission). Accessibility guidelines (WCAG 2.1 Level AA) will be considered to ensure usability for all users. The system provides:
- Clear error messages with actionable guidance
- Success confirmations for completed actions
- Loading indicators for async operations
- Responsive design for mobile and desktop devices

#### 4.2.2 Performance

##### 4.2.2.1 Response Time:

The system should respond to user interactions within the following timeframes:
- **Page Loads**: < 2 seconds under normal network conditions
- **API Endpoints**: < 500ms for standard operations (95th percentile)
- **Search & Filter Operations**: < 1 second for condotel search with multiple filters
- **Image Upload**: < 3 seconds for standard image uploads to Cloudinary
- **Payment Processing**: < 2 seconds to generate payment link
- **Real-time Chat**: < 100ms latency for message delivery via SignalR
- **Database Queries**: < 200ms for single query operations

##### 4.2.2.2 Throughput:

The system will support at least **500 concurrent users** and scale horizontally to accommodate peak loads during booking periods and promotional campaigns. The system can handle:
- **API Requests**: Minimum 1000 requests/second for standard endpoints
- **Database Connections**: Maximum 100 concurrent connections
- **SignalR Connections**: Minimum 200 concurrent connections for real-time chat
- **Payment Transactions**: Support for high-volume payment processing during peak times

#### 4.2.3 Reliability

##### 4.2.3.1 System Availability:

The system will maintain **99.5% uptime** during operational hours. Scheduled maintenance will be performed during low-usage windows with prior notifications sent to users via email. The system implements:
- Graceful degradation when external services (Cloudinary, PayOS) are unavailable
- Retry logic for external API calls with exponential backoff
- Circuit breaker pattern for third-party service failures
- Database backup strategy with daily full backups and 6-hour transaction log backups

#### 4.2.4 Security

##### 4.2.4.1 Authentication & Authorization:

**Secure Authentication**:
- JWT (JSON Web Token) based authentication with 24-hour token expiration
- Password hashing using BCrypt with salt rounds >= 10
- Email verification via OTP (One-Time Password) required for account activation
- Google OAuth 2.0 integration for SSO

**Role-Based Access Control (RBAC)**:
- **Public**: No authentication required (view condotels, blog posts, promotions)
- **Authorized**: Requires login (any role) - view profile, manage bookings
- **Tenant**: Customer role - create bookings, make payments, write reviews
- **Host**: Owner role - manage condotels, view bookings, manage vouchers
- **Admin**: Administrator role - full system access, user management, system configuration

**Password Security**:
- Minimum 6 characters (recommended 8+ with letters and numbers)
- Passwords stored securely (hashed & salted using BCrypt)
- Password reset via OTP sent to registered email
- Account lockout after multiple failed login attempts

##### 4.2.4.2 Content Protection:

**Data Protection**:
- Secure HTTPS communication (TLS 1.2 or higher) for all API requests
- SQL injection prevention through Entity Framework Core parameterized queries
- XSS (Cross-Site Scripting) prevention through input validation and encoding
- CSRF (Cross-Site Request Forgery) protection via CORS configuration with whitelist origins
- Sensitive data encryption at rest (passwords, payment information)

**Payment Security**:
- PayOS payment gateway integration with secure API key storage
- Checksum validation for payment callbacks and webhook signatures
- Payment status verification before confirming bookings
- No storage of credit card information
- Complete audit trail for all payment transactions

**Access Control**:
- Users can only access their own bookings, reviews, and profile data
- Hosts can only manage their own condotels and bookings
- Admins have full access with audit logging
- API endpoints protected by JWT token validation and role-based authorization

#### 4.2.5 Maintainability

##### 4.2.5.1 Modular Design:

The system follows a modular architecture separating:
- **Authentication Service**: User registration, login, password management
- **Booking Service**: Booking creation, updates, cancellation, availability checking
- **Payment Service**: Payment link generation, status checking, PayOS integration
- **Condotel Service**: Condotel CRUD operations, search, filtering
- **Review Service**: Review creation, updates, deletion, moderation
- **Chat Service**: Real-time messaging, conversation management
- **Admin Service**: User management, dashboard analytics, system configuration
- **Marketing Service**: Blog management, promotion management, voucher management

This modular design allows independent updates, scalability, and integration with future services.

##### 4.2.5.2 Developer Documentation:

Comprehensive developer documentation will be maintained, including:
- **API Documentation**: Swagger/OpenAPI documentation with endpoint descriptions, request/response examples, and authentication requirements
- **Architecture Documentation**: System design, database schema, service layer structure
- **Deployment Guides**: Setup instructions, configuration management, CI/CD procedures
- **Code Documentation**: XML comments for public APIs, inline comments for complex logic
- **Version Control**: Git repository with feature branch strategy, code review process, and commit message conventions

---

## 5. Requirement Appendix

### 5.1 Business Rules

| ID | Rule Definition |
|----|----------------|
| **BR-01** | Only Tenants (customers) can create bookings; Hosts cannot book their own condotels |
| **BR-02** | A valid email address is required to register any account (Tenant, Host, Admin) |
| **BR-03** | Passwords must be at least 6 characters (recommended 8+ with letters and numbers) |
| **BR-04** | A user account with Status "Pending" cannot login until email is verified via OTP |
| **BR-05** | A locked/banned account (Status = "Inactive" or "Banned") cannot access any part of the system until reactivated by an Admin |
| **BR-06** | Google OAuth integration must be used for single sign-on (SSO) when users choose Google login |
| **BR-07** | A condotel must be created by a Host who has an active Host account |
| **BR-08** | Hosts can only create condotels up to the limit specified by their Service Package |
| **BR-09** | Only Hosts can create, update, and delete their own condotel listings |
| **BR-10** | Condotels with Status "Active" are visible to all users for booking |
| **BR-11** | A booking cannot be created for dates in the past |
| **BR-12** | A booking cannot be created if the condotel is already booked for the requested date range |
| **BR-13** | A booking must have valid start and end dates (end date must be after start date) |
| **BR-14** | Hosts cannot book their own condotels |
| **BR-15** | A booking with Status "Pending" requires payment before it can be confirmed |
| **BR-16** | Payments can only be processed through PayOS payment gateway |
| **BR-17** | Minimum payment amount is 10,000 VND (PayOS requirement) |
| **BR-18** | A payment transaction must return a confirmation from PayOS before booking status is updated to "Confirmed" |
| **BR-19** | All payment transactions must be logged with timestamp, payer, booking ID, and transaction ID |
| **BR-20** | Failed payment transactions must trigger an error message and not create any confirmed booking |
| **BR-21** | Bookings with Status "Pending" can be cancelled without refund |
| **BR-22** | Bookings with Status "Confirmed" or "Completed" require refund request when cancelled |
| **BR-23** | Refunds are only processed after Admin approval of refund request |
| **BR-24** | Hosts receive payout only after booking is completed for at least 15 days |
| **BR-25** | Hosts cannot receive payout if there is a pending or approved refund request for the booking |
| **BR-26** | A booking must be completed (Status = "Completed") before a Tenant can write a review |
| **BR-27** | A Tenant can only write one review per booking |
| **BR-28** | Reviews can only be edited or deleted within 7 days of creation |
| **BR-29** | Promotions can only be applied if booking dates fall within the promotion period |
| **BR-30** | Promotions must have Status "Active" to be applicable |
| **BR-31** | Promotions must belong to the condotel being booked |
| **BR-32** | Vouchers can only be created and managed by Hosts for their own condotels |
| **BR-33** | Reward points are earned after successful booking completion |
| **BR-34** | Reward points can be redeemed for discounts on future bookings |
| **BR-35** | Service Packages define the maximum number of condotels a Host can list |
| **BR-36** | Hosts must purchase a Service Package to create condotel listings |
| **BR-37** | An email notification must be sent to users upon successful registration with OTP code |
| **BR-38** | An email notification must be sent to users upon successful payment confirmation |
| **BR-39** | An email notification must be sent to Hosts when their condotel receives a new booking |
| **BR-40** | An email notification must be sent to Tenants when their booking status changes |
| **BR-41** | Admins can create, update, disable, or delete any user account except other Admin accounts |
| **BR-42** | Admins cannot create new Admin accounts (security restriction) |
| **BR-43** | All user activities (login, registration, booking, payment, review) must be logged for auditing |
| **BR-44** | Data retention for inactive accounts must be at least 3 years before archiving |
| **BR-45** | Deleted accounts must anonymize personal information but retain transaction and booking history for compliance |
| **BR-46** | All communications between client and server must use HTTPS with TLS 1.2 or higher |
| **BR-47** | The system must comply with local data protection regulations and payment processing laws |
| **BR-48** | Only Admins can configure system-wide settings (PayOS API keys, Cloudinary keys, Email service configuration) |
| **BR-49** | Backups must be performed daily and stored securely with restricted access |
| **BR-50** | Audit logs must not be editable by any role, including Admins |
| **BR-51** | Real-time chat messages are only accessible to participants in the conversation |
| **BR-52** | Blog posts with Status "Published" are visible to all users (public access) |
| **BR-53** | Only Admins can create, update, and delete blog posts and categories |
| **BR-54** | Promotions created by Admins can be applied to specific condotels or system-wide |

**Table 5.1. Business Rules**

### 5.2 System Messages

| # | Message code | Message Type | Context | Content |
|---|-------------|--------------|---------|---------|
| 1 | MSG-01 | Success | Account Registration | "User registration initiated. Please check your email for an OTP to verify your account." |
| 2 | MSG-02 | Error | Account Registration | "Email already exists and is activated." |
| 3 | MSG-03 | Success | Email Verification | "Email verified successfully. You can now log in." |
| 4 | MSG-04 | Error | Email Verification | "Invalid email, incorrect OTP, or OTP has expired." |
| 5 | MSG-05 | Success | Login | "Login successful. Welcome back!" |
| 6 | MSG-06 | Error | Login | "Invalid email or password, or account not activated." |
| 7 | MSG-07 | Warning | Login | "Your account has been disabled by the administrator." |
| 8 | MSG-08 | Error | Login | "Google authentication failed." |
| 9 | MSG-09 | Success | Password Reset | "If your email is registered, you will receive an OTP code." |
| 10 | MSG-10 | Success | Password Reset | "Password updated successfully" |
| 11 | MSG-11 | Error | Password Reset | "Failed to reset password. Invalid email, expired or incorrect OTP." |
| 12 | MSG-12 | Success | Booking Creation | "Booking created successfully." |
| 13 | MSG-13 | Error | Booking Creation | "Start date cannot be in the past." |
| 14 | MSG-14 | Error | Booking Creation | "End date cannot be in the past." |
| 15 | MSG-15 | Error | Booking Creation | "Invalid date range." |
| 16 | MSG-16 | Error | Booking Creation | "Condotel is not available in this period." |
| 17 | MSG-17 | Error | Booking Creation | "Host cannot book their own condotel." |
| 18 | MSG-18 | Error | Booking Creation | "Promotion does not belong to this condotel." |
| 19 | MSG-19 | Error | Booking Creation | "Promotion is not active." |
| 20 | MSG-20 | Error | Booking Creation | "Booking dates must be within promotion period." |
| 21 | MSG-21 | Error | Booking | "Booking not found" |
| 22 | MSG-22 | Error | Booking | "Access denied" |
| 23 | MSG-23 | Error | Payment | "Booking is not in a payable state. Current status: {status}" |
| 24 | MSG-24 | Error | Payment | "Amount must be at least 10,000 VND" |
| 25 | MSG-25 | Success | Payment | "Payment link created successfully" |
| 26 | MSG-26 | Error | Payment | "Payment failed. Please try again or contact support." |
| 27 | MSG-27 | Info | Payment | "Your payment is being processed. Please wait for confirmation." |
| 28 | MSG-28 | Success | Condotel Creation | "Condotel created successfully" |
| 29 | MSG-29 | Error | Condotel Creation | "Host not found. Please register as a host first." |
| 30 | MSG-30 | Error | Condotel Creation | "You have reached the maximum number of condotels allowed by your package." |
| 31 | MSG-31 | Error | Condotel | "Condotel not found" |
| 32 | MSG-32 | Error | Condotel | "You do not have permission to update this condotel" |
| 33 | MSG-33 | Success | Review Creation | "Review created successfully" |
| 34 | MSG-34 | Error | Review Creation | "Booking not found or does not belong to you" |
| 35 | MSG-35 | Error | Review Creation | "You can only review completed bookings" |
| 36 | MSG-36 | Error | Review Creation | "You have already reviewed this booking" |
| 37 | MSG-37 | Error | Review | "Review can only be edited or deleted within 7 days of creation" |
| 38 | MSG-38 | Success | Payout | "Payout processed successfully" |
| 39 | MSG-39 | Error | Payout | "Booking must be completed to process payout." |
| 40 | MSG-40 | Error | Payout | "Booking has already been paid to host." |
| 41 | MSG-41 | Error | Payout | "Booking must be completed for at least 15 days before payout." |
| 42 | MSG-42 | Error | Payout | "Cannot process payout. Booking has pending or approved refund request." |
| 43 | MSG-43 | Error | System | "An unexpected error occurred. Please contact the system administrator." |
| 44 | MSG-44 | Error | Validation | "One or more validation errors occurred. Please check the 'errors' property for details." |
| 45 | MSG-45 | Success | Logout | "Logout successful" |
| 46 | MSG-46 | Success | Admin Access | "Welcome, Admin!" |
| 47 | MSG-47 | Error | Location | "Location not found" |
| 48 | MSG-48 | Error | Package | "PackageId không hợp lệ." |
| 49 | MSG-49 | Warning | Package | "Host cannot cancel package. You can only upgrade to a higher package by purchasing a new one." |
| 50 | MSG-50 | Success | Package Payment | "Payment successful. You are now officially a Host!" |

**Table 5.2. System Messages**

### 5.3 Other Requirements

**Legal Compliance**: The system must comply with local data protection regulations (e.g., GDPR-equivalent in Vietnam if applicable). Personal data must be encrypted, and users have the right to access, modify, or delete their personal information.

**Scalability**: The system architecture must support future expansion for additional roles, payment gateways, or third-party integrations. The modular design allows for horizontal scaling and service separation.

**Audit Trail**: All key operations (payments, account changes, booking modifications, admin actions) must have a full audit trail with timestamps, user information, and action details. Audit logs are immutable and cannot be edited by any role.

**Version Control**: Code changes must be tracked using Git with feature branch strategy. All code must go through code review before merging to main branch. API versioning should be considered for future backward compatibility.

**Accessibility**: The system should adhere to WCAG 2.1 Level AA accessibility standards to ensure usability for users with disabilities. This includes keyboard navigation, screen reader compatibility, and proper color contrast.

**Data Backup and Recovery**: 
- Daily full database backups with 30-day retention
- Transaction log backups every 6 hours
- Test restore procedures monthly
- Recovery Time Objective (RTO): < 4 hours
- Recovery Point Objective (RPO): < 1 hour

**Monitoring and Logging**:
- Application performance monitoring (response times, error rates)
- Infrastructure monitoring (CPU, memory, disk usage)
- Business metrics tracking (user registrations, bookings, payments)
- Centralized logging system with 90-day retention
- Alert system for critical errors and system failures

**API Documentation**: Complete Swagger/OpenAPI documentation must be maintained with:
- All endpoint descriptions
- Request/response schemas
- Authentication requirements
- Example requests and responses
- Error code definitions

---

**Tài liệu này được tạo dựa trên phân tích code backend thực tế của Condotel Management System.**

**Version**: 1.0  
**Last Updated**: 2024  
**Status**: Final












