using CondotelManagement.Data;
using CondotelManagement.DTOs;
using CondotelManagement.DTOs.Host;
using CondotelManagement.DTOs.Payment;
using CondotelManagement.Models;
using CondotelManagement.Repositories;
using CondotelManagement.Services.Interfaces;
using CondotelManagement.Services.Interfaces.Cloudinary;
using CondotelManagement.Services.Interfaces.OCR;
using CondotelManagement.Services.Interfaces.Payment;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HostModel = CondotelManagement.Models.Host;

// Sử dụng tiền tố đầy đủ để tránh lỗi "ambiguous reference" và loại bỏ 'using CondotelManagement.Models;'

namespace CondotelManagement.Services
{
    public class HostService : IHostService
    {
        private readonly IHostRepository _hostRepo;
        private readonly CondotelDbVer1Context _context;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IDeepSeekOCRService _ocrService;
        private readonly IVietQRService _vietQRService;

        // SỬA LỖI: Chỉ giữ lại CondotelDbVer1Context vì các repo đã bị xóa
        public HostService(CondotelDbVer1Context context, IHostRepository hostRepo, ICloudinaryService cloudinaryService, IDeepSeekOCRService ocrService, IVietQRService vietQRService)
        {
            _context = context;
            _hostRepo = hostRepo;
            _cloudinaryService = cloudinaryService;
            _ocrService = ocrService;
            _vietQRService = vietQRService;
        }

        public async Task<HostRegistrationResponseDto> RegisterHostAsync(int userId, HostRegisterRequestDto dto)
        {
            // 0. KHỞI TẠO TRANSACTION (Bảo vệ dữ liệu toàn vẹn)
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Tìm User
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new Exception("Không tìm thấy người dùng (UserID từ Token không tồn tại).");
                }

                // 2. Tìm Host và Wallet hiện tại
                var existingHost = await _context.Hosts
                    .Include(h => h.Wallet)
                    .FirstOrDefaultAsync(h => h.UserId == userId);

                // --- Khai báo biến ---
                HostModel hostToProcess;
                Wallet walletToProcess = null;
                bool isNewHost = existingHost == null;
                bool isNewWallet = false;

                // --- LOGIC: USER MỚI (Chưa từng là Host) ---
                if (isNewHost)
                {
                    // 3. Tạo Host
                    hostToProcess = new HostModel
                    {
                        UserId = userId,
                        PhoneContact = dto.PhoneContact,
                        Address = dto.Address,
                        CompanyName = dto.CompanyName,
                        Status = "Active", // Host mới thì Active luôn
                                           // Khởi tạo List rỗng để tránh lỗi Null Reference sau này
                        Condotels = new List<Condotel>(),
                        HostPackages = new List<HostPackage>()
                    };

                    _context.Hosts.Add(hostToProcess);

                    // 4. Tạo Wallet (Chưa gán HostId ngay được)
                    walletToProcess = new Wallet
                    {
                        BankName = dto.BankName,
                        AccountNumber = dto.AccountNumber,
                        AccountHolderName = dto.AccountHolderName,

                        // --- FIX THEO SQL CỦA BẠN ---
                        IsDefault = true,   // Ví đầu tiên nên mặc định là True
                        Status = "Active"   // Gán trạng thái hoạt động
                                            // Bỏ UserId để tránh conflict constraint (vì đây là ví của Host)
                    };

                    isNewWallet = true;

                    // 5. Nâng quyền User
                    user.RoleId = 3;
                }
                // --- LOGIC: USER ĐÃ LÀ HOST (Update thông tin) ---
                else
                {
                    hostToProcess = existingHost;

                    // Update info Host
                    hostToProcess.PhoneContact = dto.PhoneContact;
                    hostToProcess.Address = dto.Address;
                    hostToProcess.CompanyName = dto.CompanyName;

                    var existingWallet = existingHost.Wallet;

                    if (existingWallet != null)
                    {
                        // Update Wallet có sẵn
                        walletToProcess = existingWallet;
                        walletToProcess.BankName = dto.BankName;
                        walletToProcess.AccountNumber = dto.AccountNumber;
                        walletToProcess.AccountHolderName = dto.AccountHolderName;
                        // Không cần sửa Status hay IsDefault khi update
                    }
                    else
                    {
                        // Tạo Wallet mới cho Host cũ (nếu bị thiếu)
                        walletToProcess = new Wallet
                        {
                            HostId = existingHost.HostId, // Đã có ID rồi
                            BankName = dto.BankName,
                            AccountNumber = dto.AccountNumber,
                            AccountHolderName = dto.AccountHolderName,

                            // --- FIX THEO SQL ---
                            IsDefault = true,
                            Status = "Active"
                        };
                        isNewWallet = true;
                    }
                }

                // 6. LƯU XUỐNG DB (Xử lý việc gán ID)

                // TRƯỜNG HỢP A: HOST MỚI (Lưu 2 bước)
                if (isNewHost)
                {
                    // Bước 1: Lưu Host để DB sinh ra HostId
                    await _context.SaveChangesAsync();

                    // Bước 2: Lấy HostId vừa sinh ra gán cho Wallet
                    if (isNewWallet && walletToProcess != null)
                    {
                        walletToProcess.HostId = hostToProcess.HostId; // Quan trọng nhất
                        _context.Wallets.Add(walletToProcess);

                        // Lưu tiếp Wallet
                        await _context.SaveChangesAsync();
                    }
                }
                // TRƯỜNG HỢP B: HOST CŨ (Lưu 1 bước)
                else
                {
                    if (isNewWallet && walletToProcess != null)
                    {
                        // HostId đã có sẵn, chỉ cần Add ví
                        _context.Wallets.Add(walletToProcess);
                    }
                    // Update/Add xong xuôi thì Save 1 lần
                    await _context.SaveChangesAsync();
                }

                // 7. COMMIT TRANSACTION
                await transaction.CommitAsync();

                // 8. TRẢ VỀ
                return new HostRegistrationResponseDto
                {
                    HostId = hostToProcess.HostId,
                    Message = isNewHost ? "Đăng ký làm Host thành công!" : "Cập nhật thông tin thành công!"
                };
            }
            catch (Exception)
            {
                // Có lỗi thì hoàn tác sạch sẽ
                await transaction.RollbackAsync();
                throw;
            }
        }

        public HostModel GetByUserId(int userId)
        {
            return _hostRepo.GetByUserId(userId);
        }

        public Task<bool> CanHostUploadCondotel(int hostId)
        {
            throw new NotImplementedException();
        }

        public async Task<HostProfileDTO> GetHostProfileAsync(int userId)
        {
            var host = await _hostRepo.GetHostProfileAsync(userId);
            if (host == null) return null;

            return new HostProfileDTO
            {
                HostID = host.HostId,
                CompanyName = host.CompanyName,
                Address = host.Address,
                PhoneContact = host.PhoneContact,

                UserID = host.User.UserId,
                FullName = host.User.FullName,
                Email = host.User.Email,
                Phone = host.User.Phone,
                Gender = host.User.Gender,
                DateOfBirth = host.User.DateOfBirth,
                UserAddress = host.User.Address,
                ImageUrl = host.User.ImageUrl,

                Packages = host.HostPackages.Select(hp => new HostPackageDTO
                {
                    PackageID = hp.PackageId,
                    Name = hp.Package.Name,
                    StartDate = hp.StartDate,
                    EndDate = hp.EndDate
                }).ToList(),

                Wallet = new WalletDTO
                {
                    WalletID = host.Wallet.WalletId,
                    BankName = host.Wallet.BankName,
                    AccountNumber = host.Wallet.AccountNumber,
                    AccountHolderName = host.Wallet.AccountHolderName
                }
            };
        }

        public async Task<bool> UpdateHostProfileAsync(int userId, UpdateHostProfileDTO dto)
        {
            var host = await _hostRepo.GetHostProfileAsync(userId);
            if (host == null) return false;

            // Update HOST
            host.CompanyName = dto.CompanyName;
            host.Address = dto.Address;
            host.PhoneContact = dto.PhoneContact;

            // Update USER
            host.User.FullName = dto.FullName;
            host.User.Phone = dto.Phone;
            host.User.Gender = dto.Gender;
            host.User.DateOfBirth = dto.DateOfBirth;
            host.User.Address = dto.UserAddress;
            host.User.ImageUrl = dto.ImageUrl;

            // Update Wallet
            host.Wallet.BankName = dto.BankName;
            host.Wallet.AccountNumber = dto.AccountNumber;
            host.Wallet.AccountHolderName = dto.AccountHolderName;

            await _hostRepo.UpdateHostAsync(host);
            return true;
        }

        public async Task<HostVerificationResponseDTO> VerifyHostWithIdCardAsync(int userId, IFormFile idCardFront, IFormFile idCardBack)
        {
            var response = new HostVerificationResponseDTO { Success = false };

            try
            {
                // 1. Tìm host
                var host = await _context.Hosts.FirstOrDefaultAsync(h => h.UserId == userId);
                if (host == null)
                {
                    response.Message = "Không tìm thấy thông tin host.";
                    return response;
                }

                // 2. Upload ảnh lên Cloudinary
                var frontImageUrl = await _cloudinaryService.UploadImageAsync(idCardFront);
                var backImageUrl = await _cloudinaryService.UploadImageAsync(idCardBack);

                // 3. Lưu URL vào database
                host.IdCardFrontUrl = frontImageUrl;
                host.IdCardBackUrl = backImageUrl;
                host.VerificationStatus = "Pending";
                host.VerifiedAt = null;
                host.VerificationNote = null;

                // 4. Gọi OCR để đọc thông tin
                var frontOCRResult = await _ocrService.ExtractIdCardInfoAsync(frontImageUrl, isFront: true);
                var backOCRResult = await _ocrService.ExtractIdCardInfoAsync(backImageUrl, isFront: false);

                // 5. Map OCR results
                var frontInfo = new IdCardInfoDTO
                {
                    FullName = frontOCRResult.FullName,
                    IdNumber = frontOCRResult.IdNumber,
                    DateOfBirth = frontOCRResult.DateOfBirth,
                    Gender = frontOCRResult.Gender,
                    Nationality = frontOCRResult.Nationality,
                    Address = frontOCRResult.Address
                };

                var backInfo = new IdCardInfoDTO
                {
                    IssueDate = backOCRResult.IssueDate,
                    IssuePlace = backOCRResult.IssuePlace,
                    ExpiryDate = backOCRResult.ExpiryDate
                };

                // 6. Kiểm tra thông tin có hợp lệ không (có thể thêm validation logic ở đây)
                bool isValid = frontOCRResult.Success && backOCRResult.Success;
                
                if (isValid)
                {
                    // So sánh thông tin với thông tin user/host hiện tại (optional)
                    var user = await _context.Users.FindAsync(userId);
                    if (user != null)
                    {
                        // Có thể thêm logic so sánh tên, ngày sinh, etc.
                        // Nếu khớp thì set status = "Approved", không thì "Pending" để admin review
                        host.VerificationStatus = "Pending"; // Để admin review
                        host.VerificationNote = "Đang chờ admin xác minh";
                    }
                }
                else
                {
                    host.VerificationStatus = "Pending";
                    host.VerificationNote = "Không thể đọc thông tin từ ảnh. Vui lòng thử lại với ảnh rõ hơn.";
                }

                // 7. Lưu vào database
                await _context.SaveChangesAsync();

                // 8. Trả về response
                response.Success = true;
                response.Message = isValid 
                    ? "Upload ảnh CCCD thành công. Thông tin đã được đọc và đang chờ admin xác minh." 
                    : "Upload ảnh thành công nhưng không thể đọc thông tin. Vui lòng kiểm tra lại chất lượng ảnh.";
                response.VerificationStatus = host.VerificationStatus;
                response.FrontInfo = frontInfo;
                response.BackInfo = backInfo;
            }
            catch (Exception ex)
            {
                response.Message = $"Lỗi khi xác minh: {ex.Message}";
            }

            return response;
        }

        public async Task<ValidateIdCardResponseDTO> ValidateIdCardInfoAsync(int userId)
        {
            var response = new ValidateIdCardResponseDTO { IsValid = false };

            try
            {
                // 1. Tìm host và user
                var host = await _context.Hosts
                    .Include(h => h.User)
                    .FirstOrDefaultAsync(h => h.UserId == userId);

                if (host == null)
                {
                    response.Message = "Không tìm thấy thông tin host.";
                    return response;
                }

                // 2. Kiểm tra xem đã có ảnh CCCD chưa
                if (string.IsNullOrEmpty(host.IdCardFrontUrl))
                {
                    response.Message = "Chưa có ảnh CCCD. Vui lòng upload ảnh CCCD trước.";
                    return response;
                }

                // 3. Đọc lại thông tin từ ảnh CCCD bằng OCR
                var frontOCRResult = await _ocrService.ExtractIdCardInfoAsync(host.IdCardFrontUrl, isFront: true);

                if (!frontOCRResult.Success || string.IsNullOrEmpty(frontOCRResult.FullName) || string.IsNullOrEmpty(frontOCRResult.IdNumber))
                {
                    response.Message = "Không thể đọc thông tin từ ảnh CCCD. Vui lòng upload lại ảnh rõ hơn.";
                    return response;
                }

                // 4. So sánh thông tin với user
                var user = host.User;
                var details = new ValidationDetailsDTO
                {
                    UserFullName = user.FullName,
                    IdCardFullName = frontOCRResult.FullName,
                    IdCardNumber = frontOCRResult.IdNumber,
                    UserDateOfBirth = user.DateOfBirth?.ToString("dd/MM/yyyy"),
                    IdCardDateOfBirth = frontOCRResult.DateOfBirth
                };

                // 5. So sánh tên (loại bỏ dấu và chuyển về chữ hoa để so sánh)
                var normalizedUserName = NormalizeVietnameseName(user.FullName);
                var normalizedIdCardName = NormalizeVietnameseName(frontOCRResult.FullName);
                details.NameMatch = normalizedUserName.Equals(normalizedIdCardName, StringComparison.OrdinalIgnoreCase);

                // 6. So sánh số CCCD (nếu có lưu trong database)
                // Tạm thời chỉ kiểm tra xem có số CCCD hay không
                details.IdNumberMatch = !string.IsNullOrEmpty(frontOCRResult.IdNumber);

                // 7. So sánh ngày sinh (nếu có)
                if (user.DateOfBirth.HasValue && !string.IsNullOrEmpty(frontOCRResult.DateOfBirth))
                {
                    // Parse ngày sinh từ CCCD (format có thể là dd/MM/yyyy hoặc dd-MM-yyyy)
                    if (TryParseDate(frontOCRResult.DateOfBirth, out var idCardDate))
                    {
                        details.DateOfBirthMatch = user.DateOfBirth.Value == DateOnly.FromDateTime(idCardDate);
                    }
                }

                // 8. Gọi VietQR API để xác thực với hệ thống quốc gia
                bool vietQRVerified = false;
                string vietQRMessage = string.Empty;
                try
                {
                    // Chuẩn hóa tên để gửi lên VietQR (chuyển về chữ hoa, loại bỏ dấu)
                    var normalizedNameForVietQR = NormalizeVietnameseName(frontOCRResult.FullName).ToUpper();
                    var vietQRResult = await _vietQRService.VerifyCitizenAsync(frontOCRResult.IdNumber, normalizedNameForVietQR);
                    
                    // Code "00" nghĩa là thành công - số CCCD và tên khớp với hệ thống quốc gia
                    vietQRVerified = vietQRResult.Code == "00";
                    vietQRMessage = vietQRResult.Desc;
                    details.VietQRVerified = vietQRVerified;
                    details.VietQRMessage = vietQRMessage;
                }
                catch (Exception ex)
                {
                    // Nếu VietQR API lỗi, vẫn tiếp tục với validation nội bộ
                    vietQRMessage = $"Không thể xác thực qua VietQR: {ex.Message}";
                    details.VietQRVerified = false;
                    details.VietQRMessage = vietQRMessage;
                }

                // 9. Kết luận - Kết hợp validation nội bộ và VietQR
                // Nếu VietQR verify thành công, coi như valid
                // Nếu không, vẫn kiểm tra validation nội bộ
                bool internalValidation = details.NameMatch && details.IdNumberMatch;
                response.IsValid = vietQRVerified || internalValidation;
                response.Details = details;

                if (response.IsValid)
                {
                    if (vietQRVerified)
                    {
                        response.Message = "Thông tin CCCD đã được xác thực thành công qua hệ thống quốc gia.";
                    }
                    else
                    {
                        response.Message = "Thông tin CCCD khớp với thông tin tài khoản.";
                    }
                    
                    // Cập nhật status nếu chưa được approve
                    if (host.VerificationStatus != "Approved")
                    {
                        host.VerificationStatus = "Approved";
                        host.VerifiedAt = DateTime.UtcNow;
                        host.VerificationNote = vietQRVerified 
                            ? "Thông tin CCCD đã được xác thực qua VietQR API" 
                            : "Thông tin CCCD đã được xác thực tự động";
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    var mismatches = new List<string>();
                    if (!details.NameMatch) mismatches.Add("Tên không khớp");
                    if (!details.IdNumberMatch) mismatches.Add("Số CCCD không hợp lệ");
                    if (!details.DateOfBirthMatch && user.DateOfBirth.HasValue) mismatches.Add("Ngày sinh không khớp");
                    
                    if (!string.IsNullOrEmpty(vietQRMessage) && !vietQRVerified)
                    {
                        mismatches.Add($"VietQR: {vietQRMessage}");
                    }

                    response.Message = $"Thông tin CCCD không khớp: {string.Join(", ", mismatches)}. Vui lòng kiểm tra lại.";
                }
            }
            catch (Exception ex)
            {
                response.Message = $"Lỗi khi xác thực: {ex.Message}";
            }

            return response;
        }

        // Helper method để chuẩn hóa tên tiếng Việt (loại bỏ dấu)
        private string NormalizeVietnameseName(string name)
        {
            if (string.IsNullOrEmpty(name)) return string.Empty;

            // Loại bỏ khoảng trắng thừa và chuyển về chữ hoa
            name = name.Trim().ToUpper();
            name = System.Text.RegularExpressions.Regex.Replace(name, @"\s+", " ");

            // Loại bỏ dấu tiếng Việt
            var normalizedString = name.Normalize(System.Text.NormalizationForm.FormD);
            var stringBuilder = new System.Text.StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }

        public async Task<List<TopHostDTO>> GetTopHostsByRatingAsync(int topCount = 10)
        {
            // Lấy tất cả hosts active với thông tin user
            var hosts = await _context.Hosts
                .Include(h => h.User)
                .Where(h => h.Status == "Active")
                .ToListAsync();

            // Lấy tất cả condotels của các hosts này
            var hostIds = hosts.Select(h => h.HostId).ToList();
            var condotels = await _context.Condotels
                .Where(c => hostIds.Contains(c.HostId))
                .Select(c => new { c.HostId, c.CondotelId })
                .ToListAsync();

            // Lấy tất cả reviews Visible của các condotels này
            var condotelIds = condotels.Select(c => c.CondotelId).ToList();
            var reviews = await _context.Reviews
                .Where(r => condotelIds.Contains(r.CondotelId) && r.Status == "Visible")
                .Select(r => new { r.CondotelId, r.Rating })
                .ToListAsync();

            // Group reviews theo HostId thông qua CondotelId
            var hostReviewStats = condotels
                .GroupJoin(reviews,
                    condotel => condotel.CondotelId,
                    review => review.CondotelId,
                    (condotel, reviewGroup) => new
                    {
                        condotel.HostId,
                        Reviews = reviewGroup.ToList()
                    })
                .GroupBy(x => x.HostId)
                .Select(g => new
                {
                    HostId = g.Key,
                    AllReviews = g.SelectMany(x => x.Reviews).ToList()
                })
                .ToList();

            var hostDTOs = new List<TopHostDTO>();

            foreach (var host in hosts)
            {
                var stats = hostReviewStats.FirstOrDefault(s => s.HostId == host.HostId);
                if (stats != null && stats.AllReviews.Any())
                {
                    var averageRating = stats.AllReviews.Average(r => (double)r.Rating);
                    var totalReviews = stats.AllReviews.Count;
                    var totalCondotels = condotels.Count(c => c.HostId == host.HostId);

                    hostDTOs.Add(new TopHostDTO
                    {
                        HostId = host.HostId,
                        CompanyName = host.CompanyName ?? string.Empty,
                        FullName = host.User?.FullName,
                        AvatarUrl = host.User?.ImageUrl,
                        AverageRating = Math.Round(averageRating, 2),
                        TotalReviews = totalReviews,
                        TotalCondotels = totalCondotels,
                        Rank = 0 // Sẽ set sau khi sort
                    });
                }
            }

            // Sắp xếp theo AverageRating giảm dần, sau đó theo TotalReviews giảm dần
            var sortedHosts = hostDTOs
                .OrderByDescending(h => h.AverageRating)
                .ThenByDescending(h => h.TotalReviews)
                .Take(topCount)
                .ToList();

            // Set rank
            for (int i = 0; i < sortedHosts.Count; i++)
            {
                sortedHosts[i].Rank = i + 1;
            }

            return sortedHosts;
        }

        // Helper method để parse ngày từ string
        private bool TryParseDate(string dateString, out DateTime date)
        {
            date = DateTime.MinValue;
            if (string.IsNullOrEmpty(dateString)) return false;

            // Thử các format phổ biến
            var formats = new[] { "dd/MM/yyyy", "dd-MM-yyyy", "dd.MM.yyyy", "yyyy-MM-dd" };
            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(dateString, format, null, System.Globalization.DateTimeStyles.None, out date))
                {
                    return true;
                }
            }

            // Thử parse tự động
            return DateTime.TryParse(dateString, out date);
        }
    }
}