# RP3 - Condotel Management System - Work Breakdown Structure (WBS)

## PHẦN 4: ƯỚC TÍNH EFFORT VÀ TIMELINE

### 4.1. Tổng quan Ước tính Effort

Dựa trên phân tích WBS, tổng effort ước tính cho dự án Condotel Management System là **134 effort points**.

### 4.2. Phân bổ Effort theo Iteration

| Iteration | Effort | Tỷ lệ | Mô tả |
|-----------|--------|-------|-------|
| **Iteration 1** | 45 | 33.6% | Các chức năng cơ bản và nền tảng |
| **Iteration 2** | 65 | 48.5% | Các chức năng nghiệp vụ chính |
| **Iteration 3** | 24 | 17.9% | Dashboard và các tính năng nâng cao |
| **TỔNG CỘNG** | **134** | **100%** | |

### 4.3. Chi tiết Effort theo Feature

#### 4.3.1. Iteration 1 (45 effort points)

| Feature | Sub Feature | Effort | Mô tả |
|---------|-------------|--------|-------|
| Account Management | Register | 2 | Đăng ký tài khoản (User + Host) |
| Account Management | Login | 2 | Đăng nhập (Email + Google OAuth) |
| Account Management | Verify Email | 1 | Xác thực email bằng OTP |
| Account Management | Forgot Password | 2 | Quên mật khẩu và reset |
| Account Management | View Profile | 3 | Xem và chỉnh sửa profile |
| Booking Management | Check Availability | 1 | Kiểm tra tính khả dụng |
| Condotel Management | CRUD Condotel | 7 | Tạo, cập nhật, quản lý condotel |
| Condotel Discovery | View Details | 1 | Xem chi tiết condotel |
| Condotel Discovery | Search/Filter | 5 | Tìm kiếm và lọc condotel |
| Communication Management | Review | 1 | Xem review |
| Marketing Management | Blog | 3 | Xem blog posts và categories |
| Marketing Management | Promotion | 5 | Xem promotions |
| Marketing Management | Voucher | 1 | Xem vouchers |
| Master Data Management | Location | 2 | Quản lý địa điểm |
| Master Data Management | Resort | 2 | Quản lý resort |
| Master Data Management | Utility | 2 | Quản lý tiện ích |
| Additional Features | Upload | 3 | Upload hình ảnh |

**Tổng Iteration 1: 45 effort points**

#### 4.3.2. Iteration 2 (65 effort points)

| Feature | Sub Feature | Effort | Mô tả |
|---------|-------------|--------|-------|
| Account Management | User Management | 6 | Quản lý người dùng (Admin) |
| Account Management | Reward Points | 4 | Quản lý điểm thưởng |
| Booking Management | Create Booking | 3 | Tạo booking |
| Booking Management | Update Booking | 1 | Cập nhật booking |
| Booking Management | Cancel Booking | 1 | Hủy booking |
| Booking Management | View Booking | 2 | Xem danh sách booking |
| Booking Management | Host Booking Management | 2 | Quản lý booking của Host |
| Booking Management | Payment | 5 | Quản lý thanh toán |
| Condotel Management | Customer Management | 1 | Quản lý khách hàng |
| Communication Management | Review | 4 | Quản lý review |
| Communication Management | Chat | 3 | Hệ thống chat/messaging |
| Marketing Management | Blog | 5 | Quản lý blog (Admin) |
| Marketing Management | Promotion | 4 | Quản lý promotion (Admin) |
| Marketing Management | Voucher | 4 | Quản lý voucher (Host) |
| Master Data Management | Location | 2 | CRUD Location |
| Master Data Management | Resort | 1 | CRUD Resort |
| Master Data Management | Utility | 2 | CRUD Utility |
| Service Management | Service Package | 12 | Quản lý gói dịch vụ |

**Tổng Iteration 2: 65 effort points**

#### 4.3.3. Iteration 3 (24 effort points)

| Feature | Sub Feature | Effort | Mô tả |
|---------|-------------|--------|-------|
| Dashboard Management | Admin Dashboard | 10 | Dashboard quản trị viên |
| Dashboard Management | Host Dashboard | 10 | Dashboard chủ nhà |
| Additional Features | Payout | 2 | Quản lý thanh toán cho Host |
| Additional Features | Refund | 2 | Quản lý hoàn tiền |

**Tổng Iteration 3: 24 effort points**

### 4.4. Timeline Ước tính

#### 4.4.1. Giả định
- 1 effort point = 1 ngày làm việc (8 giờ)
- Team size: 3-4 developers
- Parallel development: Có thể làm song song nhiều features

#### 4.4.2. Timeline theo Iteration

**Iteration 1: 6-8 tuần**
- Tuần 1-2: Account Management, Condotel CRUD cơ bản
- Tuần 3-4: Condotel Discovery, Search/Filter
- Tuần 5-6: Master Data Management, Upload
- Tuần 7-8: Marketing View, Testing & Bug fixing

**Iteration 2: 8-10 tuần**
- Tuần 1-2: Booking Management, Payment
- Tuần 3-4: User Management, Reward Points
- Tuần 5-6: Communication (Review, Chat)
- Tuần 7-8: Marketing Management (Blog, Promotion, Voucher)
- Tuần 9-10: Service Package, Testing & Integration

**Iteration 3: 3-4 tuần**
- Tuần 1-2: Dashboard Development (Admin & Host)
- Tuần 3: Payout & Refund Management
- Tuần 4: Final Testing, Bug Fixing, Documentation

**Tổng thời gian ước tính: 17-22 tuần (~4-5.5 tháng)**

### 4.5. Milestones

| Milestone | Timeline | Deliverables |
|-----------|----------|--------------|
| **M1: Foundation Complete** | Cuối Iteration 1 | Authentication, Condotel CRUD, Search/Filter hoạt động |
| **M2: Core Features Complete** | Cuối Iteration 2 | Booking, Payment, Review, Chat hoạt động đầy đủ |
| **M3: System Complete** | Cuối Iteration 3 | Dashboard, Payout, Refund hoàn thiện |

---

## PHẦN 5: PHÂN CÔNG NHÂN SỰ VÀ QUẢN LÝ RỦI RO

### 5.1. Phân công Nhân sự

#### 5.1.1. Team Structure

| Vai trò | Số lượng | Trách nhiệm chính |
|---------|----------|-------------------|
| **Project Manager** | 1 | Quản lý dự án, điều phối, tracking progress |
| **Tech Lead** | 1 | Kiến trúc hệ thống, code review, technical decisions |
| **Backend Developer** | 2-3 | Phát triển API, business logic, database |
| **Frontend Developer** | 1-2 | Phát triển UI/UX, tích hợp API |
| **QA/Tester** | 1 | Testing, viết test cases, bug tracking |
| **DevOps** | 0.5 (part-time) | CI/CD, deployment, infrastructure |

**Tổng team size: 6-8 người**

#### 5.1.2. Phân công theo Feature

**Iteration 1:**
- **Developer 1**: Account Management, Authentication
- **Developer 2**: Condotel CRUD, Condotel Discovery
- **Developer 3**: Master Data Management, Upload
- **Frontend**: UI cho tất cả features Iteration 1

**Iteration 2:**
- **Developer 1**: Booking Management, Payment
- **Developer 2**: User Management, Reward Points, Service Package
- **Developer 3**: Communication (Review, Chat), Marketing Management
- **Frontend**: UI cho tất cả features Iteration 2

**Iteration 3:**
- **Developer 1**: Admin Dashboard
- **Developer 2**: Host Dashboard, Payout, Refund
- **Frontend**: Dashboard UI, Financial Management UI

#### 5.1.3. Trách nhiệm cụ thể

**Tech Lead:**
- Thiết kế kiến trúc hệ thống
- Code review cho tất cả modules quan trọng
- Giải quyết các vấn đề kỹ thuật phức tạp
- Đảm bảo code quality và best practices

**Backend Developers:**
- Phát triển RESTful APIs
- Implement business logic
- Database design và optimization
- Unit testing và integration testing

**Frontend Developer:**
- Thiết kế và implement UI/UX
- Tích hợp với Backend APIs
- Responsive design
- User experience optimization

**QA/Tester:**
- Viết test cases dựa trên requirements
- Manual testing và automated testing
- Bug tracking và verification
- Test coverage reporting

### 5.2. Quản lý Rủi ro

#### 5.2.1. Rủi ro Kỹ thuật

| Rủi ro | Mức độ | Tác động | Biện pháp giảm thiểu |
|--------|--------|----------|---------------------|
| **Tích hợp Payment Gateway (PayOS)** | Cao | Delay payment features | - Nghiên cứu API PayOS sớm<br>- Tạo sandbox environment<br>- Có backup plan (stripe, vnpay) |
| **Real-time Chat (SignalR)** | Trung bình | Performance issues | - Load testing sớm<br>- Optimize message handling<br>- Consider Redis for scaling |
| **Image Upload & Storage (Cloudinary)** | Thấp | Cost, performance | - Implement image compression<br>- Set size limits<br>- Monitor usage |
| **Database Performance** | Trung bình | Slow queries | - Index optimization<br>- Query optimization<br>- Consider caching (Redis) |
| **Third-party API Integration** | Trung bình | API changes, downtime | - Implement retry logic<br>- Fallback mechanisms<br>- Monitor API health |

#### 5.2.2. Rủi ro Nghiệp vụ

| Rủi ro | Mức độ | Tác động | Biện pháp giảm thiểu |
|--------|--------|----------|---------------------|
| **Thay đổi Requirements** | Cao | Scope creep, delay | - Clear requirements upfront<br>- Change control process<br>- Regular stakeholder communication |
| **Phức tạp Business Logic** | Trung bình | Implementation delay | - Detailed analysis trước khi code<br>- Prototype cho complex features<br>- Regular review với business team |
| **Data Migration** | Trung bình | Data loss, downtime | - Backup strategy<br>- Migration scripts testing<br>- Rollback plan |

#### 5.2.3. Rủi ro Nhân sự

| Rủi ro | Mức độ | Tác động | Biện pháp giảm thiểu |
|--------|--------|----------|---------------------|
| **Key Person Dependency** | Cao | Project delay nếu mất người | - Knowledge sharing sessions<br>- Documentation đầy đủ<br>- Cross-training team members |
| **Skill Gap** | Trung bình | Quality issues | - Training programs<br>- Pair programming<br>- Code review |
| **Team Availability** | Trung bình | Schedule delay | - Buffer time trong planning<br>- Resource allocation flexible<br>- Backup resources |

#### 5.2.4. Rủi ro Timeline

| Rủi ro | Mức độ | Tác động | Biện pháp giảm thiểu |
|--------|--------|----------|---------------------|
| **Underestimation** | Cao | Delay delivery | - Add 20% buffer time<br>- Regular progress tracking<br>- Early warning system |
| **Scope Creep** | Cao | Feature bloat | - Strict change control<br>- Prioritize must-have features<br>- Defer nice-to-have |
| **Integration Issues** | Trung bình | Delay integration | - Early integration testing<br>- API contracts defined early<br>- Continuous integration |

### 5.3. Kế hoạch Giảm thiểu Rủi ro

#### 5.3.1. Phòng ngừa (Prevention)
- **Technical Spike**: Nghiên cứu các công nghệ mới trước khi implement
- **Proof of Concept**: Tạo POC cho các features phức tạp
- **Code Review**: Tất cả code phải được review trước khi merge
- **Automated Testing**: Maintain high test coverage (>80%)

#### 5.3.2. Giám sát (Monitoring)
- **Daily Standup**: Track progress và blockers hàng ngày
- **Weekly Review**: Review progress và risks hàng tuần
- **Burndown Chart**: Monitor effort consumption
- **Quality Metrics**: Track bug rate, test coverage

#### 5.3.3. Phản ứng (Response)
- **Escalation Process**: Rõ ràng khi nào cần escalate
- **Contingency Plan**: Có plan B cho các risks cao
- **Resource Reallocation**: Linh hoạt điều chỉnh resources
- **Scope Adjustment**: Sẵn sàng điều chỉnh scope nếu cần

### 5.4. Communication Plan

#### 5.4.1. Internal Communication
- **Daily Standup**: 15 phút mỗi ngày
- **Sprint Planning**: Đầu mỗi iteration
- **Sprint Review**: Cuối mỗi iteration
- **Retrospective**: Sau mỗi iteration

#### 5.4.2. Stakeholder Communication
- **Weekly Status Report**: Gửi stakeholders mỗi tuần
- **Demo Sessions**: Demo progress mỗi 2 tuần
- **Milestone Reviews**: Review tại mỗi milestone

### 5.5. Quality Assurance

#### 5.5.1. Code Quality
- **Code Review**: Bắt buộc cho tất cả PRs
- **Coding Standards**: Follow C# coding conventions
- **Static Analysis**: Sử dụng SonarQube hoặc tương tự
- **Technical Debt**: Track và manage technical debt

#### 5.5.2. Testing Strategy
- **Unit Tests**: >80% code coverage
- **Integration Tests**: Tất cả APIs phải có integration tests
- **E2E Tests**: Critical user flows
- **Performance Tests**: Load testing cho high-traffic endpoints

#### 5.5.3. Documentation
- **API Documentation**: Swagger/OpenAPI
- **Code Documentation**: XML comments cho public APIs
- **Architecture Documentation**: System design docs
- **User Documentation**: User guides và admin guides

---

**Tài liệu được tạo dựa trên phân tích WBS và dữ liệu từ Condotel Management System**












