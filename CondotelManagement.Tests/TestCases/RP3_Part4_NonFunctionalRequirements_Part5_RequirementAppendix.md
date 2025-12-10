# RP3 - Condotel Management System
## Requirements Document

---

## PHẦN 4: NON-FUNCTIONAL REQUIREMENTS

### 4.1. Performance Requirements

#### 4.1.1. Response Time
| Chức năng | Yêu cầu Response Time | Ghi chú |
|-----------|----------------------|---------|
| **API Endpoints** | < 500ms (95th percentile) | Cho các operations thông thường |
| **Search & Filter** | < 1s (95th percentile) | Tìm kiếm condotel với nhiều filters |
| **Database Queries** | < 200ms | Cho single query operations |
| **Image Upload** | < 3s | Upload ảnh lên Cloudinary |
| **Payment Processing** | < 2s | Tạo payment link PayOS |
| **Real-time Chat** | < 100ms | Latency cho SignalR messages |

#### 4.1.2. Throughput
- **Concurrent Users**: Hệ thống phải hỗ trợ tối thiểu **500 concurrent users**
- **API Requests**: Hỗ trợ tối thiểu **1000 requests/second** cho các endpoints thông thường
- **Database Connections**: Tối đa **100 concurrent database connections**
- **SignalR Connections**: Hỗ trợ tối thiểu **200 concurrent SignalR connections** cho chat

#### 4.1.3. Scalability
- **Horizontal Scaling**: Hệ thống phải có khả năng scale horizontally bằng cách thêm nhiều server instances
- **Database Scaling**: Hỗ trợ read replicas cho các query operations
- **Caching Strategy**: Implement caching cho:
  - Master data (Locations, Resorts, Utilities)
  - Blog posts và categories
  - Promotions (active promotions)
  - User sessions

#### 4.1.4. Resource Utilization
- **CPU Usage**: Trung bình < 70% dưới normal load
- **Memory Usage**: < 80% của allocated memory
- **Database Storage**: Hỗ trợ tối thiểu **100GB** database storage với khả năng mở rộng
- **File Storage**: Sử dụng Cloudinary cho image storage (unlimited với paid plan)

### 4.2. Security Requirements

#### 4.2.1. Authentication & Authorization
- **JWT Token-based Authentication**: 
  - Token expiration: 24 hours
  - Refresh token mechanism (nếu cần)
  - Secure token storage (HttpOnly cookies hoặc secure localStorage)
  
- **Role-Based Access Control (RBAC)**:
  - **Public**: Không cần authentication (view condotels, blog posts)
  - **Authorized**: Cần đăng nhập (bất kỳ role nào)
  - **Tenant**: Chỉ Tenant/Customer
  - **Host**: Chỉ Host/Owner
  - **Admin**: Chỉ Admin

- **Password Security**:
  - Minimum 8 characters
  - Hash bằng BCrypt với salt rounds >= 10
  - Không lưu plain text passwords

- **OAuth Integration**:
  - Google OAuth 2.0 support
  - Secure token validation
  - User profile mapping

#### 4.2.2. Data Protection
- **Data Encryption**:
  - HTTPS/TLS 1.2+ cho tất cả communications
  - Encrypted database connections (TrustServerCertificate)
  - Sensitive data encryption at rest (passwords, payment info)

- **SQL Injection Prevention**:
  - Sử dụng Entity Framework Core parameterized queries
  - Không sử dụng raw SQL queries trừ khi cần thiết
  - Input validation và sanitization

- **XSS (Cross-Site Scripting) Prevention**:
  - Input validation và encoding
  - Content Security Policy headers
  - Sanitize user inputs trước khi lưu database

- **CSRF (Cross-Site Request Forgery) Protection**:
  - CORS configuration với whitelist origins
  - Token-based CSRF protection cho state-changing operations

#### 4.2.3. API Security
- **API Authentication**:
  - Bearer token trong Authorization header
  - Token validation trên mỗi request
  - Rate limiting để prevent brute force attacks

- **Input Validation**:
  - Model validation với Data Annotations
  - Custom validation logic cho business rules
  - Reject malformed requests

- **Error Handling**:
  - Không expose sensitive information trong error messages
  - Generic error messages cho users
  - Detailed errors chỉ trong development environment

#### 4.2.4. Payment Security
- **PayOS Integration**:
  - Secure API key storage (không hardcode)
  - Checksum validation cho payment callbacks
  - Webhook signature verification
  - Payment status verification trước khi confirm booking

- **Financial Data**:
  - Không lưu credit card information
  - Log payment transactions với audit trail
  - Secure payment link generation

### 4.3. Reliability & Availability Requirements

#### 4.3.1. Availability
- **Uptime Target**: 99.5% (tương đương ~3.6 hours downtime/month)
- **Scheduled Maintenance**: Thông báo trước ít nhất 24 hours
- **Backup Strategy**:
  - Database backup hàng ngày (full backup)
  - Transaction log backup mỗi 6 hours
  - Backup retention: 30 days
  - Test restore procedures hàng tháng

#### 4.3.2. Fault Tolerance
- **Error Handling**:
  - Graceful degradation khi services bên ngoài fail
  - Retry logic cho external API calls (PayOS, Cloudinary, Email)
  - Circuit breaker pattern cho third-party services
  - Fallback mechanisms khi có thể

- **Data Integrity**:
  - Database transactions cho critical operations
  - Rollback mechanism khi có lỗi
  - Data validation trước khi commit

- **Service Dependencies**:
  - **Cloudinary**: Hệ thống vẫn hoạt động nếu Cloudinary down (images sẽ fail nhưng không crash)
  - **PayOS**: Payment operations sẽ fail gracefully với error message
  - **Email Service**: Non-critical, có thể retry sau

#### 4.3.3. Disaster Recovery
- **Recovery Time Objective (RTO)**: < 4 hours
- **Recovery Point Objective (RPO)**: < 1 hour (data loss)
- **Disaster Recovery Plan**:
  - Regular database backups
  - Code repository backup (Git)
  - Configuration files backup
  - Documentation of recovery procedures

### 4.4. Usability Requirements

#### 4.4.1. User Interface
- **Responsive Design**: Hỗ trợ desktop, tablet, mobile
- **Browser Compatibility**: 
  - Chrome (latest 2 versions)
  - Firefox (latest 2 versions)
  - Safari (latest 2 versions)
  - Edge (latest 2 versions)

- **Accessibility**:
  - WCAG 2.1 Level AA compliance (nếu có frontend)
  - Keyboard navigation support
  - Screen reader compatibility

#### 4.4.2. API Usability
- **RESTful API Design**:
  - Consistent naming conventions (camelCase)
  - Standard HTTP methods (GET, POST, PUT, DELETE, PATCH)
  - Meaningful HTTP status codes
  - Clear error messages

- **API Documentation**:
  - Swagger/OpenAPI documentation
  - Endpoint descriptions
  - Request/response examples
  - Authentication requirements

- **Response Format**:
  - Consistent JSON structure
  - Standardized error response format
  - Pagination support cho list endpoints

### 4.5. Maintainability Requirements

#### 4.5.1. Code Quality
- **Architecture**:
  - Clean Architecture principles
  - Separation of concerns (Controllers, Services, Repositories)
  - Dependency Injection pattern
  - Repository pattern cho data access

- **Code Standards**:
  - C# coding conventions
  - Meaningful variable và method names
  - Code comments cho complex logic
  - XML documentation cho public APIs

- **Testing**:
  - Unit test coverage > 80%
  - Integration tests cho critical flows
  - API endpoint testing
  - Test data management

#### 4.5.2. Documentation
- **Technical Documentation**:
  - Architecture diagrams
  - Database schema documentation
  - API documentation (Swagger)
  - Deployment procedures

- **Code Documentation**:
  - XML comments cho public methods
  - README files cho setup instructions
  - Configuration guide
  - Troubleshooting guide

#### 4.5.3. Version Control
- **Git Workflow**:
  - Feature branch strategy
  - Code review requirements
  - Commit message conventions
  - Tag releases

### 4.6. Compatibility Requirements

#### 4.6.1. Technology Stack
- **Backend Framework**: .NET 8.0
- **Database**: SQL Server (2019+)
- **ORM**: Entity Framework Core 9.0
- **Authentication**: JWT Bearer Tokens
- **Real-time**: SignalR
- **Image Storage**: Cloudinary
- **Payment Gateway**: PayOS
- **Email Service**: SMTP (Gmail)

#### 4.6.2. Integration Requirements
- **Third-party Services**:
  - Cloudinary API (image upload/management)
  - PayOS API (payment processing)
  - Google OAuth API (authentication)
  - SMTP Server (email sending)

- **API Compatibility**:
  - RESTful API standards
  - JSON data format
  - HTTP/HTTPS protocols
  - CORS support cho frontend integration

### 4.7. Scalability Requirements

#### 4.7.1. Database Scalability
- **Connection Pooling**: Efficient connection management
- **Query Optimization**: 
  - Indexed columns cho frequently queried fields
  - Efficient LINQ queries
  - Avoid N+1 query problems
  - Pagination cho large datasets

- **Data Growth**:
  - Support cho millions of records
  - Archive strategy cho old data
  - Partitioning strategy nếu cần

#### 4.7.2. Application Scalability
- **Stateless Design**: 
  - JWT tokens (không lưu session trên server)
  - Stateless API design
  - Load balancer friendly

- **Caching Strategy**:
  - In-memory caching cho master data
  - Response caching cho static content
  - Cache invalidation strategy

- **Async Operations**:
  - Async/await pattern cho I/O operations
  - Background jobs cho heavy processing
  - Queue system cho long-running tasks (nếu cần)

### 4.8. Monitoring & Logging Requirements

#### 4.8.1. Logging
- **Log Levels**:
  - Information: Normal operations
  - Warning: Potential issues
  - Error: Errors that need attention
  - Critical: System failures

- **Log Information**:
  - Timestamp
  - User ID (nếu có)
  - Request/Response details
  - Error stack traces
  - Performance metrics

- **Log Storage**:
  - Centralized logging system
  - Log retention: 90 days
  - Log rotation để prevent disk space issues

#### 4.8.2. Monitoring
- **Application Monitoring**:
  - Response time tracking
  - Error rate monitoring
  - API endpoint health checks
  - Database connection pool status

- **Infrastructure Monitoring**:
  - CPU, Memory, Disk usage
  - Database performance metrics
  - Network latency
  - Service availability

- **Business Metrics**:
  - User registration rate
  - Booking conversion rate
  - Payment success rate
  - Active users count

### 4.9. Data Requirements

#### 4.9.1. Data Storage
- **Database**:
  - SQL Server relational database
  - Normalized schema design
  - Foreign key constraints
  - Index optimization

- **File Storage**:
  - Cloudinary cho images (unlimited với paid plan)
  - Image formats: JPEG, PNG, WebP
  - Maximum file size: 10MB per image
  - Image compression và optimization

#### 4.9.2. Data Retention
- **User Data**: Retained while account is active
- **Booking Data**: Retained indefinitely (legal/accounting requirements)
- **Payment Data**: Retained 7 years (tax/legal requirements)
- **Log Data**: Retained 90 days
- **Deleted Records**: Soft delete với flag, hard delete sau 30 days

#### 4.9.3. Data Privacy
- **GDPR Compliance** (nếu áp dụng):
  - Right to access data
  - Right to deletion
  - Data portability
  - Privacy policy

- **Personal Data Protection**:
  - Encrypt sensitive personal information
  - Access control cho personal data
  - Audit trail cho data access

### 4.10. Deployment Requirements

#### 4.10.1. Deployment Environment
- **Development**: Local development environment
- **Staging**: Pre-production testing environment
- **Production**: Live production environment

#### 4.10.2. Deployment Process
- **CI/CD Pipeline**:
  - Automated builds
  - Automated tests
  - Automated deployment
  - Rollback capability

- **Configuration Management**:
  - Environment-specific configuration files
  - Secrets management (không hardcode)
  - Connection strings management
  - Feature flags

#### 4.10.3. Deployment Frequency
- **Hotfixes**: As needed (critical bugs)
- **Regular Releases**: Bi-weekly hoặc monthly
- **Major Releases**: Quarterly

---

## PHẦN 5: REQUIREMENT APPENDIX

### 5.1. Glossary (Thuật ngữ)

| Thuật ngữ | Định nghĩa |
|-----------|-----------|
| **Condotel** | Condominium Hotel - Căn hộ dịch vụ có thể cho thuê ngắn hạn |
| **Host** | Chủ nhà/Chủ sở hữu condotel, người đăng ký và quản lý condotel |
| **Tenant** | Khách hàng thuê condotel, người đặt phòng |
| **Admin** | Quản trị viên hệ thống, có quyền quản lý toàn bộ hệ thống |
| **Booking** | Đặt phòng, giao dịch thuê condotel của Tenant |
| **Promotion** | Chương trình khuyến mãi, giảm giá cho booking |
| **Voucher** | Mã giảm giá do Host tạo cho condotel của mình |
| **Reward Points** | Điểm thưởng tích lũy từ các booking, có thể đổi thành discount |
| **Service Package** | Gói dịch vụ mà Host mua để sử dụng các tính năng premium |
| **Payout** | Thanh toán cho Host sau khi booking hoàn thành |
| **Refund** | Hoàn tiền cho Tenant khi hủy booking |
| **OTP** | One-Time Password - Mật khẩu một lần dùng để xác thực |
| **JWT** | JSON Web Token - Token xác thực dạng JSON |
| **SignalR** | Framework real-time communication của Microsoft |
| **Cloudinary** | Dịch vụ cloud storage và image management |
| **PayOS** | Payment gateway của Việt Nam |
| **Entity Framework Core** | ORM (Object-Relational Mapping) framework của .NET |
| **Repository Pattern** | Design pattern tách biệt data access logic |
| **DTO** | Data Transfer Object - Object chuyển dữ liệu giữa layers |
| **API** | Application Programming Interface - Giao diện lập trình ứng dụng |
| **REST** | Representational State Transfer - Kiến trúc API |
| **CORS** | Cross-Origin Resource Sharing - Chia sẻ tài nguyên cross-origin |
| **Swagger** | Tool tạo API documentation tự động |
| **Pagination** | Phân trang dữ liệu khi trả về danh sách lớn |

### 5.2. Acronyms (Viết tắt)

| Viết tắt | Ý nghĩa đầy đủ |
|----------|----------------|
| **API** | Application Programming Interface |
| **JWT** | JSON Web Token |
| **OTP** | One-Time Password |
| **RBAC** | Role-Based Access Control |
| **REST** | Representational State Transfer |
| **HTTP** | HyperText Transfer Protocol |
| **HTTPS** | HyperText Transfer Protocol Secure |
| **SQL** | Structured Query Language |
| **ORM** | Object-Relational Mapping |
| **DTO** | Data Transfer Object |
| **CORS** | Cross-Origin Resource Sharing |
| **XSS** | Cross-Site Scripting |
| **CSRF** | Cross-Site Request Forgery |
| **GDPR** | General Data Protection Regulation |
| **CI/CD** | Continuous Integration/Continuous Deployment |
| **RTO** | Recovery Time Objective |
| **RPO** | Recovery Point Objective |
| **WCAG** | Web Content Accessibility Guidelines |
| **SMTP** | Simple Mail Transfer Protocol |
| **OAuth** | Open Authorization |
| **EF Core** | Entity Framework Core |

### 5.3. References (Tài liệu tham khảo)

#### 5.3.1. Technical Documentation
- **.NET 8.0 Documentation**: https://learn.microsoft.com/en-us/dotnet/
- **Entity Framework Core 9.0**: https://learn.microsoft.com/en-us/ef/core/
- **ASP.NET Core**: https://learn.microsoft.com/en-us/aspnet/core/
- **SignalR Documentation**: https://learn.microsoft.com/en-us/aspnet/core/signalr/
- **JWT Authentication**: https://jwt.io/
- **Swagger/OpenAPI**: https://swagger.io/

#### 5.3.2. Third-party Services
- **Cloudinary Documentation**: https://cloudinary.com/documentation
- **PayOS API Documentation**: https://payos.vn/docs/
- **Google OAuth 2.0**: https://developers.google.com/identity/protocols/oauth2
- **BCrypt.NET**: https://github.com/BcryptNet/bcrypt.net

#### 5.3.3. Security Standards
- **OWASP Top 10**: https://owasp.org/www-project-top-ten/
- **CWE Top 25**: https://cwe.mitre.org/top25/
- **NIST Cybersecurity Framework**: https://www.nist.gov/cyberframework

#### 5.3.4. Best Practices
- **REST API Design**: https://restfulapi.net/
- **Clean Architecture**: https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html
- **Repository Pattern**: https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design

### 5.4. Assumptions (Giả định)

1. **Infrastructure**:
   - Server có đủ resources (CPU, Memory, Storage)
   - Stable internet connection
   - Database server có backup và recovery procedures

2. **Third-party Services**:
   - Cloudinary service available và stable
   - PayOS payment gateway available
   - Email service (SMTP) available
   - Google OAuth service available

3. **Users**:
   - Users có basic computer literacy
   - Users có email address để xác thực
   - Users có payment method (nếu muốn booking)

4. **Business**:
   - Business rules không thay đổi thường xuyên
   - Legal requirements được đáp ứng
   - Tax và accounting requirements được xử lý

### 5.5. Constraints (Ràng buộc)

#### 5.5.1. Technical Constraints
- **Technology Stack**: Phải sử dụng .NET và SQL Server (không thể thay đổi dễ dàng)
- **Legacy Code**: Một số code cũ có thể cần refactor
- **Database Schema**: Thay đổi schema cần migration và có thể ảnh hưởng đến data

#### 5.5.2. Business Constraints
- **Budget**: Giới hạn budget cho third-party services (Cloudinary, PayOS)
- **Timeline**: Deadline cho releases
- **Resources**: Số lượng developers và team members

#### 5.5.3. Regulatory Constraints
- **Data Privacy**: Phải tuân thủ data protection laws
- **Payment Regulations**: Phải tuân thủ payment processing regulations
- **Tax Requirements**: Phải lưu trữ financial data theo quy định

### 5.6. Dependencies (Phụ thuộc)

#### 5.6.1. External Dependencies
- **Cloudinary**: Image storage và management
- **PayOS**: Payment processing
- **Google OAuth**: Authentication
- **SMTP Server**: Email sending
- **SQL Server**: Database

#### 5.6.2. Internal Dependencies
- **Frontend Application**: Phụ thuộc vào backend API
- **Database**: Tất cả features phụ thuộc vào database
- **Authentication Service**: Nhiều features cần authentication
- **Email Service**: Registration, password reset phụ thuộc vào email

### 5.7. Change History (Lịch sử thay đổi)

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2024-01-XX | Development Team | Initial requirements document |
| 1.1 | 2024-XX-XX | Development Team | Added non-functional requirements |
| 1.2 | 2024-XX-XX | Development Team | Updated based on code analysis |

### 5.8. Approval (Phê duyệt)

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Project Manager | | | |
| Tech Lead | | | |
| Business Analyst | | | |
| Stakeholder | | | |

---

**Tài liệu này được tạo dựa trên phân tích code backend và các tài liệu hiện có của Condotel Management System.**

**Version**: 1.0  
**Last Updated**: 2024  
**Status**: Draft/Final












